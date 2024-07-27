using DesktopUtilsSharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DupliSha512;

internal class Program
{
    async static Task Main(string[] args)
    {
        string path = ConsoleHelper.GetInput("Enter path for analysis: ");
        Console.WriteLine("Processing...");
        Console.WriteLine();

        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        Dictionary<string, List<string>> hashList = [];
        SemaphoreSlim _lock = new(1);

        await Parallel.ForEachAsync(files, async (x, ctoken) =>
        {
            string hash = await HashHelper.ProcessHash(x);
            try
            {
                await _lock.WaitAsync(ctoken);

                if (!hashList.ContainsKey(hash))
                    hashList[hash] = [];
                hashList[hash].Add(x);
            }
            finally
            { _lock.Release(); }
        });

        int uniqueHashes = 0;
        int duplicateFiles = 0;
        foreach (var kvp in hashList.Where(x => x.Value.Count > 1))
        {
            Console.WriteLine($"Name: {kvp.Key}");
            uniqueHashes++;
            duplicateFiles += kvp.Value.Count;

            foreach (var filepath in kvp.Value)
                Console.WriteLine("- " + filepath);

            Console.WriteLine();
        }

        Console.WriteLine();
        Console.WriteLine("...completed.");

        Console.WriteLine($"${uniqueHashes} unique filenames had a total of {duplicateFiles} duplicates.");
    }
}