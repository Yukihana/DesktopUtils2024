using DesktopUtilsSharedLib;
using System;
using System.Collections.Concurrent;
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
        // Take input

        List<string> paths = ConsoleHelper.GetInputs("Enter paths for analysis (empty line to end): ");

        Console.WriteLine();
        Console.WriteLine("Processing...");

        List<string> files = paths.Select(x => Directory.GetFiles(x, "*", SearchOption.AllDirectories)).SelectMany(x => x).ToList();

        // Hash and group

        Console.WriteLine();
        ConcurrentDictionary<string, string> hashes = [];
        using (SemaphoreSlim se = new(1))
        {
            await Parallel.ForEachAsync(files, CancellationToken.None, async (file, ct) =>
            {
                hashes.TryAdd(file, await HashHelper.ProcessHash(file, se, ct));
            });
        }
        Dictionary<string, List<string>> hashGroups = hashes.GroupBy(x => x.Value).Where(x => x.Count() > 1).ToDictionary(x => x.Key, x => x.Select(x => x.Key).ToList());

        // Display grouping

        Console.WriteLine();
        Console.WriteLine("Grouping...");
        Console.WriteLine();

        int uniqueHashes = 0;
        int duplicateFiles = 0;
        foreach (var kvp in hashGroups)
        {
            ConsoleHelper.PrintListSection($"Hash: {kvp.Key}", kvp.Value);
            uniqueHashes++;
            duplicateFiles += kvp.Value.Count;
        }

        // Location grouping

        Console.WriteLine();
        Console.WriteLine("Most relevant directories...");
        Console.WriteLine();

        Dictionary<string, int> dirGroups = hashGroups.SelectMany(x => x.Value).GroupBy(x => Path.GetDirectoryName(x) ?? string.Empty).ToDictionary(x => x.Key, x => x.Count());
        foreach (var kvp in dirGroups.OrderByDescending(x => x.Value))
            Console.WriteLine($"{kvp.Value} - {kvp.Key}");
        Console.WriteLine();

        // End

        Console.WriteLine();
        Console.WriteLine("...completed.");

        Console.WriteLine($"${uniqueHashes} unique filenames had a total of {duplicateFiles} duplicates.");
    }
}