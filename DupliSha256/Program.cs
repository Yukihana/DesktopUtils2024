using DesktopUtilsSharedLib;
using DesktopUtilsSharedLib.ConsoleHelpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

        List<string> paths = ConsoleInput.GetStrings("Enter paths for analysis (empty line to end): ");
        HashSet<string> files = paths.SelectMany(x => Directory.GetFiles(x, "*", SearchOption.AllDirectories)).ToHashSet();
        int fileCount = files.Count;
        Console.WriteLine();

        // Filter : Size

        long minSize = ConsoleInput.GetLong("Enter minimum size limit for files to be assessed: ");
        long maxSize = ConsoleInput.GetLong("Enter maximum size limit for files to be assessed: ");

        if (maxSize > 0 || minSize > 0)
            files = files.FilesWithin(minSize, maxSize);
        int filteredCount = files.Count;
        Console.WriteLine();

        // Hash

        Console.WriteLine("Processing...");
        Console.WriteLine();
        var stopwatch = Stopwatch.StartNew();
        ConcurrentDictionary<string, string> hashes = await HashHelperSha2D256.HashFiles(files, CancellationToken.None);
        stopwatch.Stop();
        TimeSpan hashTime = stopwatch.Elapsed;
        Console.WriteLine();

        // Group

        Console.WriteLine("Grouping...");
        Console.WriteLine();

        var hashGroups = hashes.GroupBy(x => x.Value).ToList();
        var totalUnique = hashGroups.Count;
        Dictionary<string, HashSet<string>> duplicateGroups = hashGroups
            .Where(x => x.Count() > 1)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Key).ToHashSet());

        // Display groups

        int uniqueHashes = 0;
        int duplicateCount = 0;
        foreach (var kvp in duplicateGroups)
        {
            ConsoleOutput.PrintListSection($"Hash: {kvp.Key}", kvp.Value);
            uniqueHashes++;
            duplicateCount += kvp.Value.Count;
        }
        Console.WriteLine();

        // Location grouping

        Console.WriteLine("Most relevant directories...");
        Console.WriteLine();

        Dictionary<string, int> dirGroups = duplicateGroups.SelectMany(x => x.Value).GroupBy(x => Path.GetDirectoryName(x) ?? string.Empty).ToDictionary(x => x.Key, x => x.Count());
        foreach (var kvp in dirGroups.OrderByDescending(x => x.Value))
            Console.WriteLine($"{kvp.Value} - {kvp.Key}");
        Console.WriteLine();

        // End

        Console.WriteLine("...completed.");
        Console.WriteLine();
        Console.WriteLine($"Method used: SHA2-256");
        Console.WriteLine($"Time taken: {hashTime}");
        Console.WriteLine($"Total files: {fileCount} (Size filtered to: {filteredCount})");
        Console.WriteLine();
        Console.WriteLine($"Total unique (in filtered files): {totalUnique}");
        Console.WriteLine($"Total duplicates: {duplicateCount}");
        Console.WriteLine($"Uniques in duplicates: {uniqueHashes}");
    }
}