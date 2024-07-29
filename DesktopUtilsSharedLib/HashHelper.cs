using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopUtilsSharedLib;

public static partial class HashHelper
{
    public static string ProcessHash(MemoryStream ms)
    {
        long originalPosition = ms.Position;

        ms.Position = 0;
        using HashAlgorithm hasher = Environment.OSVersion.Version.Major >= 11
                ? SHA3_512.Create()
                : SHA256.Create();
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

    public async static Task<string> ProcessHash(string path, SemaphoreSlim readSemaphore, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        MemoryStream ms = new();
        try
        {
            await readSemaphore.WaitAsync(ctoken);
            await using FileStream fs = new(path, FileMode.Open, FileAccess.Read);
            await fs.CopyToAsync(ms, ctoken);
        }
        finally { readSemaphore.Release(); }

        string hash = await Task.Run(() => ProcessHash(ms));
        Console.WriteLine("- " + hash + " : " + path);
        return hash;
    }

    public static async Task<ConcurrentDictionary<string, string>> HashFiles(HashSet<string> files, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        ConcurrentDictionary<string, string> result = [];
        await Parallel.ForEachAsync(files, ctoken, async (x, ct) => result.TryAdd(x, await ProcessHash(x, ct)));
        return result;
    }

    public static async Task<Dictionary<string, List<string>>> HashAndGroupFiles(HashSet<string> files, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        Dictionary<string, List<string>> result = [];
        SemaphoreSlim _sema = new(1);

        await Parallel.ForEachAsync(files, async (x, ct) =>
        {
            string hash = await ProcessHash(x, ct);
            try
            {
                await _sema.WaitAsync(ctoken);

                if (!result.ContainsKey(hash))
                    result[hash] = [];
                result[hash].Add(x);
            }
            finally
            { _sema.Release(); }
        });

        return result;
    }
}