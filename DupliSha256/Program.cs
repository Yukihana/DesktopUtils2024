using DesktopUtilsSharedLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DupliSha256;

internal class Program
{
    async static Task Main(string[] args)
    {
        // Take input

        List<string> paths = ConsoleHelper.GetInputs("Enter paths for analysis (empty line to end): ");
        HashSet<string> files = paths.SelectMany(x => Directory.GetFiles(x, "*", SearchOption.AllDirectories)).ToHashSet();
        Console.WriteLine($"Found {files.Count} files.");
        Console.WriteLine();

        // Filter by size

        string minSizeString = ConsoleHelper.GetInput("Enter minimum size limit for files to be assessed: ");
        if (!long.TryParse(minSizeString.Replace("_", ""), out var minSize))
            minSize = 0;
        string maxSizeString = ConsoleHelper.GetInput("Enter maximum size limit for files to be assessed: ");
        if (!long.TryParse(maxSizeString.Replace("_", ""), out var maxSize))
            maxSize = 0;

        if (maxSize > 0 || minSize > 0)
            files = files.FilesWithin(minSize, maxSize);
        Console.WriteLine($"Filtered down to {files.Count} files.");
        Console.WriteLine();

        // Hash and group

        Console.WriteLine("Processing...");
        Console.WriteLine();
        ConcurrentDictionary<string, string> hashes = await HashHelperSha2D256.HashFiles(files, CancellationToken.None);
        Console.WriteLine();

        // Display grouping

        Console.WriteLine("Grouping...");
        Console.WriteLine();

        Dictionary<string, HashSet<string>> hashGroups = hashes
            .GroupBy(x => x.Value)
            .Where(x => x.Count() > 1)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Key).ToHashSet());
        int uniqueHashes = 0;
        int duplicateFiles = 0;
        foreach (var kvp in hashGroups)
        {
            ConsoleHelper.PrintListSection($"Hash: {kvp.Key}", kvp.Value);
            uniqueHashes++;
            duplicateFiles += kvp.Value.Count;
        }
        Console.WriteLine();

        // Location grouping

        Console.WriteLine("Most relevant directories...");
        Console.WriteLine();

        Dictionary<string, int> dirGroups = hashGroups.SelectMany(x => x.Value).GroupBy(x => Path.GetDirectoryName(x) ?? string.Empty).ToDictionary(x => x.Key, x => x.Count());
        foreach (var kvp in dirGroups.OrderByDescending(x => x.Value))
            Console.WriteLine($"{kvp.Value} - {kvp.Key}");
        Console.WriteLine();

        // End

        Console.WriteLine("...completed.");
        Console.WriteLine($"${uniqueHashes} unique SHA2-256 hashes had a total of {duplicateFiles} duplicates.");
    }
}