using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopUtilsSharedLib;

public static partial class HashHelperSha2D256
{
    // Actual hashing internal (SHA2 256)

    private static string ProcessHash(Stream stream, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        long originalPosition = stream.Position;

        stream.Position = 0;
        using HashAlgorithm hasher = SHA256.Create();
        byte[] hash = hasher.ComputeHash(stream);

        stream.Position = originalPosition;
        return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private async static Task<string> ProcessHashAsync(Stream stream, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        long originalPosition = stream.Position;

        stream.Position = 0;
        using HashAlgorithm hasher = SHA256.Create();
        byte[] hash = await hasher.ComputeHashAsync(stream, ctoken);

        stream.Position = originalPosition;
        return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    // API: Single file

    public static async Task<string> HashFile(string path, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        string hash;

        await using (FileStream fs = new(path, FileMode.Open, FileAccess.Read))
        {
            long length = fs.Length;
            fs.Position = 0;    // remove this if not required

            if (length < int.MaxValue)
            {
                MemoryStream ms = new();
                await fs.CopyToAsync(ms, ctoken);
                ms.Position = 0;
                hash = await Task.Run(() => ProcessHash(ms, ctoken));
            }
            else
            {
                hash = await ProcessHashAsync(fs, ctoken);
            }
        }

        if (string.IsNullOrEmpty(hash))
            throw new Exception($"Error trying to hashing {path}");

        Console.WriteLine("- " + hash + " : " + path);
        return hash;
    }

    public static async Task<string> HashFile(string path, SemaphoreSlim readLock, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        MemoryStream ms = new();
        string hash = string.Empty;
        long length = 0;
        try
        {
            await readLock.WaitAsync(ctoken);
            await using FileStream fs = new(path, FileMode.Open, FileAccess.Read);
            length = fs.Length;
            if (length >= int.MaxValue)
                hash = await ProcessHashAsync(fs, ctoken);
            else
                await fs.CopyToAsync(ms, ctoken);
        }
        finally { readLock.Release(); }

        if (length < int.MaxValue)
            hash = await Task.Run(() => ProcessHash(ms, ctoken));

        if (string.IsNullOrEmpty(hash))
            throw new Exception($"Error trying to hashing {path}");

        Console.WriteLine("- " + hash + " : " + path);
        return hash;
    }

    // API: Multi file

    public static async Task<ConcurrentDictionary<string, string>> HashFiles(IEnumerable<string> files, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        HashSet<string> input = files.ToHashSet();
        ConcurrentDictionary<string, string> result = [];
        using SemaphoreSlim _semaphore = new(1);
        await Parallel.ForEachAsync(input, ctoken, async (x, ct) => result.TryAdd(x, await HashFile(x, _semaphore, ct)));
        return result;
    }
}