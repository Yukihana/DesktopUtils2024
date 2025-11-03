using DesktopUtilsSharedLib;
using DesktopUtilsSharedLib.ConsoleHelpers;
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
        HashSet<string> filePaths = ConsoleInput.GetFilesInteractive(
            "Enter files or folders for analysis. Empty line to finish:");
        Console.WriteLine($"Found {filePaths.Count} files in the provided directories.");

        // Hash and group files.
        ConcurrentDictionary<string, string> hashes = await HashHelperSha2D256.HashFiles(filePaths);
        Dictionary<string, HashSet<string>> hashGroups = GroupHashes(hashes);

        // Skip groups that don't match content comparison.
        ConcurrentDictionary<string, bool> verifiedGroups = await FileComparison.CompareGroupsBinary(hashGroups, (int)ByteMaths.MebibytesToBytes(16), CancellationToken.None);
        foreach (var kvp in verifiedGroups)
        {
            if (!kvp.Value)
            {
                Console.WriteLine($"Hashed group failed to verify on binary comparison. Try a different hash algorithm.");
                ConsoleOutput.PrintListSection("Skipping failed hash: " + kvp.Key, hashGroups[kvp.Key]);
                hashGroups.Remove(kvp.Key);
            }
        }

        //  Get the speculated oldest file in each, if there's no conflict.
        Dictionary<string, string> oldestFiles = [];
        HashSet<string> removeList = [];

        Console.WriteLine("Comparing file metadata...");
        foreach (var kvp in hashGroups)
        {
            Console.WriteLine();
            Console.WriteLine($"Hash: {kvp.Key}");

            // Get oldest (if not by modified, then by created)
            Dictionary<FileInfo, string> fileInfos = kvp.Value.ToDictionary(x => new FileInfo(x), x => x); // *Reversed dictionary
            var oldest = FileMetadataComparison.GetOldestByModified(fileInfos.Keys);
            if (oldest.Count() > 1)
                oldest = FileMetadataComparison.GetOldestByCreation(oldest);

            bool multipleOld = oldest.Count() > 1;
            var oldestfis = oldest.ToDictionary(x => fileInfos[x], x => x); // *Restored dictionary (modified)
            var allfis = fileInfos.ToDictionary(x => x.Value, x => x.Key); // *Restore dictionary (all)

            // Display relevant files
            var consoleColorBkp = Console.ForegroundColor;
            foreach (string path in kvp.Value)
            {
                var oldcolor = multipleOld ? ConsoleColor.Yellow : ConsoleColor.Green;
                bool isOldest = oldestfis.ContainsKey(path);

                string prefix = isOldest ? "K" : "R";
                string created = allfis[path].CreationTime.ToString("yyyy-MM-dd_HH:mm:ss_fff");
                string modified = allfis[path].LastWriteTime.ToString("yyyy-MM-dd_HH:mm:ss_fff");

                Console.ForegroundColor = isOldest ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"{prefix} (M: {modified}, C: {created}): {path}");
            }
            Console.ForegroundColor = consoleColorBkp;

            // Otherwise add remaining files to remove list:
            Console.WriteLine("...adding newer files to removal list.");
            foreach (string path in kvp.Value.Where(x => !oldestfis.ContainsKey(x)))
                removeList.Add(path);
        }

        // Print files to be kept vs files to be deleted
        Console.WriteLine();
        ConsoleOutput.PrintListSection("Files to be be removed:", removeList);

        if (!ConsoleInput.GetYesNo("Confirm action?", defaultValue: false))
            return;

        // delete files but oldest
        foreach (string path in removeList)
            File.Delete(path);

        Console.WriteLine("...completed");
    }

    private static Dictionary<string, HashSet<string>> GroupHashes(
        ConcurrentDictionary<string, string> hashes)
    {
        var hashGroupsRaw = hashes.GroupBy(x => x.Value);
        int uniqueHashCount = hashGroupsRaw.Count();

        Dictionary<string, HashSet<string>> hashGroups = hashGroupsRaw
            .Where(x => x.Count() > 1)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Key).ToHashSet());
        int nonUniqueHashGroupsCount = hashGroups.Count;
        int duplicatesCount = hashGroups.Sum(x => x.Value.Count - 1);

        Console.WriteLine($"Preliminary hashing found {uniqueHashCount} unique hashes, of which {nonUniqueHashGroupsCount} have {duplicatesCount} total duplicates");

        return hashGroups;
    }
}