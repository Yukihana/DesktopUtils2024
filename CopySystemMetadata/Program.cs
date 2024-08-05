using DesktopUtilsSharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CopySystemMetadata;

internal class Program
{
    static void Main(string[] args)
    {
        string source = ConsoleHelper.GetExistingFolder("Enter source path:");
        string target = ConsoleHelper.GetExistingFolder("Enter target path:");
        bool matchExtensions = ConsoleHelper.GetInput("Match extensions too? (y):") == "y";
        bool matchCase = ConsoleHelper.GetInput("Match case? (y):") == "y";

        // Grab contents

        string[] sourceFiles = Directory.GetFiles(source);
        string[] targetFiles = Directory.GetFiles(target);

        Dictionary<string, string> sourceFilenames = matchExtensions
            ? sourceFiles.ToDictionary(x => Path.GetFileName(x), x => x)
            : sourceFiles.ToDictionary(x => Path.GetFileNameWithoutExtension(x), x => x);

        Dictionary<string, string> targetFilenames = matchExtensions
            ? targetFiles.ToDictionary(x => Path.GetFileName(x), x => x)
            : targetFiles.ToDictionary(x => Path.GetFileNameWithoutExtension(x), x => x);

        // Verify full folder contents. Warn if different compliment.

        var exceptsource = sourceFilenames.Keys.Except(targetFilenames.Keys);
        var excepttarget = targetFilenames.Keys.Except(sourceFilenames.Keys);
        var diff = exceptsource.Union(excepttarget);

        if (diff.Any() &&
            ConsoleHelper.GetInput($"Folder compliments don't match({diff.Count()}). Continue? (y):") != "y")
        {
            Console.WriteLine("...aborting (cancelled).");
            return;
        }

        foreach (var kvp in sourceFilenames)
        {
            foreach (var targetkvp in targetFilenames)
            {
                bool matched = matchCase
                    ? kvp.Key == targetkvp.Key
                    : kvp.Key.Equals(targetkvp.Key, StringComparison.OrdinalIgnoreCase);

                if (!matched)
                    continue;

                string sourcename = kvp.Value;
                string targetname = targetkvp.Value;

                FileInfo sourcefi = new(sourcename);
                FileInfo targetfi = new(targetname);

                Console.WriteLine($"- {kvp.Key} (Modified): {targetfi.LastWriteTime:yyyyMMdd_HHmmss_fff} > {sourcefi.LastWriteTime:yyyyMMdd_HHmmss_fff}");
                targetfi.LastWriteTime = sourcefi.LastWriteTime;
                Console.WriteLine($"- {kvp.Key} (Created): {targetfi.CreationTime:yyyyMMdd_HHmmss_fff} > {sourcefi.CreationTime:yyyyMMdd_HHmmss_fff}");
                targetfi.CreationTime = sourcefi.CreationTime;

                break;
            }
        }
        Console.WriteLine();
        Console.WriteLine("...completed.");
    }
}