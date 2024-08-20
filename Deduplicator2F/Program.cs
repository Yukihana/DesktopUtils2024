using DesktopUtilsSharedLib;
using FSE = DesktopUtilsSharedLib.Extensions.FileSystemExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Deduplicator2F;

internal class Program
{
    async static Task Main(string[] args)
    {
        string sourceDir = ConsoleHelper.GetExistingFolder("Enter source folder: ");
        string targetDir = ConsoleHelper.GetExistingFolder("Enter target folder: ");

        // Acquire files and sizes
        var sourceNames = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
        var targetNames = Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories);
        Dictionary<string, long> sourceBySize = sourceNames.ToDictionary(x => x, x => new FileInfo(x).Length);
        Dictionary<string, long> targetBySize = targetNames.ToDictionary(x => x, x => new FileInfo(x).Length);

        // Create the compare list. target as key ensures multiple can be deleted by same source.
        Dictionary<string, List<string>> compareList = targetBySize.ToDictionary(
            keySelector: t => t.Key,
            elementSelector: t => sourceBySize.Where(s => s.Value == t.Value).Select(s => s.Key).ToList());

        // When comparing use a hash cache to improve speed
        Dictionary<string, string> hashCache = [];
        Dictionary<string, string> deletionList = [];

        // Do comparison
        foreach (var compareTask in compareList)
        {
            var targetHash = await HashCacheFile(compareTask.Key);
            foreach (var sourcefile in compareTask.Value)
            {
                // compare by hash first
                var sourceHash = await HashCacheFile(sourcefile);
                if (targetHash != sourceHash)
                    continue;

                // verify by actual content
                if (await FileComparison.CompareBinary(compareTask.Key, sourcefile))
                {
                    deletionList[compareTask.Key] = sourcefile;
                    break;
                }
            }
        }

        // Confirm and finish

        Console.WriteLine("List of files to delete: ");
        var bkp = Console.ForegroundColor;
        foreach (var deletionTask in deletionList)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(deletionTask.Key);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(deletionTask.Value);
            Console.WriteLine();
        }
        Console.ForegroundColor = bkp;

        if (ConsoleHelper.GetInput("Confirm deletion of files in Red? (y):") != "y")
        {
            Console.WriteLine("Aborting due to cancellation...");
            return;
        }

        foreach (var deletionTask in deletionList)
        {
            FSE.RecycleFile(deletionTask.Key);
            // File.Delete(deletionTask.Key); // for perma deletion
        }

        ConsoleHelper.Goodbye();
    }

    private static readonly Dictionary<string, string> _hashCache = [];

    static async Task<string> HashCacheFile(string filePath) // turn this into a class
    {
        if (!_hashCache.TryGetValue(filePath, out var hash))
        {
            hash = await HashHelperSha2D256.HashFile(filePath);
            _hashCache[filePath] = hash;
        }
        return hash;
    }
}