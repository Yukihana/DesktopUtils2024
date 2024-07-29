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
        SearchOption subdirs = ConsoleHelper.GetInput("Include files in subdirectories? (y): ") == "y" ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        // Acquire the files
        HashSet<string> filePaths = directories.SelectMany(x => Directory.GetFiles(x, "*", subdirs)).ToHashSet();
        Console.WriteLine($"Found {filePaths.Count} files in the provided directories.");

        // Hash group them
        ConcurrentDictionary<string, string> hashList = await HashHelper.HashFiles(filePaths);
        Console.WriteLine($"Preliminary hashing found {hashList.Count} unique files. i.e. duplicates: {filePaths.Count - hashList.Count}");

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
            Console.WriteLine();
            Console.WriteLine($"Hash: {kvp.Key}");

            // Get oldest (if not by modified, then by created)
            var oldest = FileMetadataComparison.GetOldestByModified(kvp.Value);
            if (oldest.Count() > 1)
                oldest = FileMetadataComparison.GetOldestByCreation(kvp.Value);

            if (oldest.Count() > 1)
            {
                Console.WriteLine("...Skipping. Too many identical files with identical metadata.");
                continue; // Skip files with the same metadata
            }

            string oldestOne = oldest.First();

            // Display relevant files
            foreach (string path in kvp.Value)
            {
                string prefix = path == oldestOne ? "K" : "R";
                Console.WriteLine($"{prefix}: {path}");
            }

            // Otherwise add remaining files to remove list:
            Console.WriteLine("...adding newer files to removal list.");
            foreach (string path in kvp.Value)
            {
                if (path != oldestOne)
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