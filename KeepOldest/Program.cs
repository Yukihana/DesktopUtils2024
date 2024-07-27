using DesktopUtilsSharedLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KeepOldest;

internal class Program
{
    async static Task Main(string[] args)
    {
        List<string> directories = [];
        string input = string.Empty;
        while (true)
        {
            input = ConsoleHelper.GetInput($"Enter path {directories.Count + 1} (empty line to finish input): ");
            if (string.IsNullOrEmpty(input))
                break;
            directories.Add(input);
        }

        // Acquire the files and hash them
        HashSet<string> filePaths = directories.SelectMany(Directory.GetFiles).ToHashSet();
        ConcurrentDictionary<string, string> hashList = await HashHelper.HashFiles(filePaths);

        // Group all files, ingore uniques, and then verify by content comparison
        Dictionary<string, IEnumerable<string>> hashGroups = hashList.GroupBy(g => g.Value).ToDictionary(group => group.Key, group => group.Select(kvp => kvp.Key).ToList() as IEnumerable<string>);
        hashGroups = new(hashGroups.Where(kvp => kvp.Value.Count() > 1));
        ConcurrentDictionary<string, bool> verifiedGroups = await FileComparison.CompareGroupsBinary(hashGroups, (int)ByteMaths.MebibytesToBytes(16), CancellationToken.None);

        // skip groups that don't match and get the speculated oldest file in each, if there's no conflict.
        hashGroups = new(hashGroups.Where(x => verifiedGroups[x.Key]));
        Dictionary<string, string> oldestFiles = [];
        HashSet<string> removeList = [];

        Console.WriteLine("Comparing file metadata...");
        foreach (var kvp in hashGroups)
        {
            // Compare oldest with demo
            Console.WriteLine();
            Console.WriteLine($"Hash: {kvp.Key}");
            var oldest = FileMetadataComparison.GetOldest(kvp.Value);
            foreach (string path in kvp.Value)
            {
                string prefix = oldest.Contains(path) ? "K" : "R";
                Console.WriteLine($"{prefix}: {path}");
            }

            // Conclude section
            if (oldest.Count() > 1)
            {
                Console.WriteLine("...Skipping");
                continue; // Skip files with the same metadata
            }

            // Otherwise add remaining files to remove list:
            Console.WriteLine("...adding newer files to removal list.");
            foreach (string path in kvp.Value)
            {
                if (!oldest.Contains(path))
                    removeList.Add(path);
            }
        }

        // Print files to be kept vs files to be deleted
        Console.WriteLine();
        ConsoleHelper.PrintListSection("Files to be be removed:", removeList);

        if (ConsoleHelper.GetInput("Confirm action?") != "y")
            return;

        // delete files but oldest
        foreach (string path in removeList)
            File.Delete(path);

        Console.WriteLine("...completed");
    }
}