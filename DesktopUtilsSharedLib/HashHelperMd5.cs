using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopUtilsSharedLib;

public static partial class HashHelperMd5
{
    public static string ProcessHash(MemoryStream ms, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        long originalPosition = ms.Position;

        ms.Position = 0;
        using HashAlgorithm hasher = MD5.Create();
        byte[] hash = hasher.ComputeHash(ms);

        ms.Position = originalPosition;
        return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public async static Task<string> ProcessHash(string path, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        MemoryStream ms = new();
        using (FileStream fs = new(path, FileMode.Open, FileAccess.Read))
        {
            await fs.CopyToAsync(ms, ctoken);
        }
        string hash = await Task.Run(() => ProcessHash(ms));

        Console.WriteLine("- " + hash + " : " + path);
        return hash;
    }

    public static async Task<string> HashFile(string path, SemaphoreSlim readLock, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        MemoryStream ms = new();
        try
        {
            await readLock.WaitAsync(ctoken);
            await using FileStream fs = new(path, FileMode.Open, FileAccess.Read);
            await fs.CopyToAsync(ms, ctoken);
        }
        finally { readLock.Release(); }

        string hash = await Task.Run(() => ProcessHash(ms));
        Console.WriteLine("- " + hash + " : " + path);
        return hash;
    }

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