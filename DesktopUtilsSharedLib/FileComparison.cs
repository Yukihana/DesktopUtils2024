using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopUtilsSharedLib;

public static partial class FileComparison
{
    private const int DEFAULT_BUFFER_SIZE = 16 * 1024 * 1024;

    public async static Task<bool> CompareBinary(string file1, string file2, CancellationToken ctoken = default)
    {
        try
        {
            using MemoryStream m1 = new();
            using MemoryStream m2 = new();

            using (FileStream f1 = new(file1, FileMode.Open, FileAccess.Read))
            {
                await f1.CopyToAsync(m1, ctoken);
            }

            using (FileStream f2 = new(file2, FileMode.Open, FileAccess.Read))
            {
                await f2.CopyToAsync(m2, ctoken);
            }

            if (m1.Length != m2.Length)
                return false;

            m1.Position = 0;
            m2.Position = 0;

            const int bufferSize = 1024; // Adjust buffer size as needed
            byte[] buffer1 = new byte[bufferSize];
            byte[] buffer2 = new byte[bufferSize];

            while (m1.Position < m1.Length)
            {
                int bytesRead1 = m1.Read(buffer1, 0, bufferSize);
                int bytesRead2 = m2.Read(buffer2, 0, bufferSize);

                if (bytesRead1 != bytesRead2 || !buffer1.AsSpan(0, bytesRead1).SequenceEqual(buffer2.AsSpan(0, bytesRead2)))
                    return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

    // Compare Binary (Unrestricted File Count)

    public static bool CompareBuffers(IEnumerable<Memory<byte>> buffersToCompare)
    {
        // Snapshot and validate minimum operational quantity
        List<Memory<byte>> bufferList = buffersToCompare.ToList();
        if (bufferList.Count < 2)
            throw new InvalidOperationException("Need two or more buffers for comparison.");

        // Make sure the buffers are of same length and there's content to compare.
        Span<byte> first = bufferList[0].Span;
        int length = first.Length;
        if (bufferList.Any(x => x.Length != length))
            throw new InvalidOperationException("The buffers must be of the same size.");
        if (length == 0)
            return true;    // Mark matched if there's no content.

        // Compare content from first with the rest.
        for (int i = 1; i < bufferList.Count; i++)
        {
            if (!first.SequenceEqual(bufferList[i].Span))
                return false;
        }
        return true;
    }

    public async static Task<bool> CompareStreams(IEnumerable<Stream> streamsToCompare, SemaphoreSlim readLock, int bufferSize = 0, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        var streams = streamsToCompare.ToArray();
        if (streams.Length < 2)
            throw new InvalidOperationException("Need two or more items for comparison");

        // Validate all files are the same size.
        long length = streams[0].Length;
        if (streams.Any(x => x.Length != length))
            return false;

        // Prepare
        long position = 0;
        bufferSize = bufferSize < 1 ? DEFAULT_BUFFER_SIZE : bufferSize;
        ConcurrentBag<byte[]> bufferPool = [];
        ConcurrentBag<bool> results = [];
        List<Task> tasks = [];

        while (position < length)
        {
            if (results.Any(x => !x))
                return false;   // Early bail if there's any failed compare tasks.

            Dictionary<byte[], int> buffersToCompare = [];

            // Read data into buffers from pool, or create new
            try
            {
                await readLock.WaitAsync(ctoken);

                foreach (var fs in streams)
                {
                    fs.Position = position;
                    if (!bufferPool.TryTake(out byte[]? buffer))
                        buffer = new byte[bufferSize];

                    buffersToCompare[buffer] = await fs.ReadAsync(buffer, ctoken);
                }
            }
            finally { readLock.Release(); }

            // Verify buffer sizes match
            int readSize = buffersToCompare.First().Value;
            if (buffersToCompare.Values.Any(x => x != readSize))
                throw new Exception("Buffer read sizes don't match. Internal error.");
            position += readSize;

            // Queue task to compare buffers
            tasks.Add(Task.Run(() =>
            {
                Memory<byte>[] memories = buffersToCompare.Select(x => x.Key.AsMemory(0, x.Value)).ToArray();
                bool result = CompareBuffers(memories);

                // Add result to bag and return buffers to the pool.
                results.Add(result);
                foreach (var kvp in buffersToCompare)
                    bufferPool.Add(kvp.Key);
            }, ctoken));
        }

        await Task.WhenAll(tasks);

        return results.All(x => x == true);
    }

    public static async Task<bool> CompareFiles(IEnumerable<string> paths, SemaphoreSlim readLock, int bufferSize, CancellationToken ctoken = default)
    {
        List<FileStream> streams = [];

        try
        {
            foreach (string path in paths)
                streams.Add(new(path, FileMode.Open, FileAccess.Read));

            return await CompareStreams(streams, readLock, bufferSize, ctoken);
        }
        finally
        {
            await Parallel.ForEachAsync(streams, ctoken, async (x, ct) => await x.DisposeAsync());
        }
    }

    public async static Task<ConcurrentDictionary<string, bool>> CompareGroupsBinary(Dictionary<string, HashSet<string>> groups, int bufferSize = 0, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        using SemaphoreSlim readLock = new(1);
        ConcurrentDictionary<string, bool> result = [];
        await Parallel.ForEachAsync(groups, ctoken, async (group, ct) => result[group.Key] = await CompareFiles(group.Value, readLock, bufferSize, ct));
        return result;
    }
}