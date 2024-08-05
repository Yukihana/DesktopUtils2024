using DesktopUtilsSharedLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RenameBySha2D256;

internal class Program
{
    async static Task Main(string[] args)
    {
        string path = ConsoleHelper.GetExistingFolder("Enter path: ");
        string prefix = ConsoleHelper.GetInput("prefix: ");
        string suffix = ConsoleHelper.GetInput("suffix: ");

        string[] files = Directory.GetFiles(path);

        ConcurrentDictionary<string, string> hashlist = await HashHelperSha2D256.HashFiles(files, CancellationToken.None);
        Dictionary<string, string> renameList = [];
        foreach (var kvp in hashlist)
            renameList[kvp.Key] = Path.Combine(path, $"{prefix}{kvp.Value}{suffix}{Path.GetExtension(kvp.Key)}");

        List<string> filenames = [];
        filenames.AddRange(renameList.Keys);
        filenames.AddRange(renameList.Values);

        if (filenames.Count != filenames.Distinct().Count())
        {
            var bkp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Hash or naming conflict.");
            Console.WriteLine("Check for duplicate files, overwrites, or try a different hashing algorithm.");
            Console.WriteLine(" Aborting...");
            Console.ForegroundColor = bkp;
            return;
        }

        if (ConsoleHelper.GetInput("Confirm renaming? (y):") != "y")
        {
            Console.WriteLine("Aborting due to cancellation...");
            return;
        }

        foreach (var kvp in renameList)
        {
            Console.WriteLine(Path.GetFileName(kvp.Key) + " > " + Path.GetFileName(kvp.Value));
            File.Move(kvp.Key, kvp.Value);
        }

        Console.WriteLine();
        Console.WriteLine("...completed.");
    }
}