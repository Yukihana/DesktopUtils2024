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
    public async static Task<string> ProcessHash(string path, CancellationToken ctoken = default)
    {
        ctoken.ThrowIfCancellationRequested();

        using MemoryStream ms = new();
        using (FileStream fs = new(path, FileMode.Open, FileAccess.Read))
        {
            await fs.CopyToAsync(ms, ctoken);
            fs.Close();
        }
        ms.Position = 0;

        using HashAlgorithm hasher = Environment.OSVersion.Version.Major >= 11
                ? SHA3_512.Create()
                : SHA256.Create();

        byte[] hash = await hasher.ComputeHashAsync(ms, ctoken);
        string id = Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        Console.WriteLine("- " + id + " : " + path);
        return id;
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