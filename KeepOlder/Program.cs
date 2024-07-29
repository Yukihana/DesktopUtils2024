using DesktopUtilsSharedLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KeepOlder;

internal class Program
{
    async static Task Main(string[] args)
    {
        string path1 = ConsoleHelper.GetInput("Enter path 1: ");
        string path2 = ConsoleHelper.GetInput("Enter path 2: ");

        ConcurrentDictionary<string, string> files1 = new(Directory.GetFiles(path1, "*", SearchOption.TopDirectoryOnly).ToDictionary(x => Path.GetFileName(x), x => x));
        ConcurrentDictionary<string, string> files2 = new(Directory.GetFiles(path2, "*", SearchOption.TopDirectoryOnly).ToDictionary(x => Path.GetFileName(x), x => x));

        HashSet<string> filenames = [.. files1.Keys.Select(Path.GetFileName)];
        HashSet<string> filenames2 = [.. files2.Keys.Select(Path.GetFileName)];

        if (!filenames.SetEquals(filenames2))
        {
            Console.WriteLine("Folders don't have the same files.");
            return;
        }

        // Run data comparison
        ConcurrentBag<bool> compareResults = [];
        ConcurrentDictionary<string, string> newerFiles = [];
        await Parallel.ForEachAsync(filenames, CancellationToken.None, async (filename, ctoken) =>
        {
            string file1 = Path.Combine(path1, filename);
            string file2 = Path.Combine(path2, filename);

            bool result = await FileComparison.CompareBinary(file1, file2, ctoken);
            compareResults.Add(result);
            Console.WriteLine($"Comparison {(result ? "passed" : "failed")} for {filename}");

            if (result)
            {
                FileInfo f1 = new(file1);
                FileInfo f2 = new(file2);

                if (f1.LastWriteTime != f2.LastWriteTime)
                {
                    var newer = f1.LastWriteTime > f2.LastWriteTime ? f1 : f2;
                    var older = f1.LastWriteTime > f2.LastWriteTime ? f2 : f1;
                    newerFiles.TryAdd(newer.FullName, $"Newer by modified ({newer.LastWriteTime} vs {older.LastWriteTime})");
                }
                else if (f1.CreationTime != f2.CreationTime)
                {
                    var newer = f1.CreationTime > f2.CreationTime ? f1 : f2;
                    var older = f1.CreationTime > f2.CreationTime ? f2 : f1;
                    newerFiles.TryAdd(newer.FullName, $"Newer by creation ({newer.CreationTime} vs {older.CreationTime})");
                }
            }
        });

        if (compareResults.Any(x => x == false))
        {
            Console.WriteLine("File contents don't match between folders");
            return;
        }

        ConsoleHelper.PrintListSection("Newer files:", [.. newerFiles.Select(kvp => $"{kvp.Key}: {kvp.Value}")]);
        string confirmation = ConsoleHelper.GetInput("Confirm deletion of newer files? (Enter y)");

        if (confirmation != "y")
            return;

        foreach (var kvp in newerFiles)
        {
            try
            {
                File.Delete(kvp.Key);
                // FileSystem.DeleteFile(kvp.Key, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        Console.WriteLine();
        Console.WriteLine("...completed");
    }
}