using DesktopUtilsSharedLib;
using DesktopUtilsSharedLib.ConsoleHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SearchByFilesize;

internal class Program
{
    static void Main(string[] args)
    {
        // Input

        string input = ConsoleHelper.GetInput("Enter path or size: ");
        if (!long.TryParse(input, out long size))
        {
            FileInfo fi = new(input);
            size = fi.Length;
        }
        long tolerance = ConsoleInput.GetLong("Tolerance: ");

        // Acquire paths

        HashSet<string> sources = [];
        foreach (var searchTarget in DriveInfo.GetDrives().Select(x => x.RootDirectory.FullName))
            sources.Add(searchTarget);

        // Acquire directories

        Console.WriteLine("Acquiring directories:");
        HashSet<string> directories = [.. sources];
        foreach (string dir in sources)
        {
            ConsoleOutput.WriteInSameLine(dir);
            foreach (string subdir in FileSystemHelper.SafeGetDirectoriesRecursive(dir))
                directories.Add(subdir);
        }
        ConsoleOutput.WriteInSameLine("...done");
        Console.WriteLine();
        Console.WriteLine();

        // Search per file per directory

        Console.WriteLine("Scanning files in directory:");
        Dictionary<string, long> files = [];
        string[] fileList = [];
        foreach (string dir in directories)
        {
            ConsoleOutput.WriteInSameLine($"{files.Count} + {dir}");

            // Get files

            try { fileList = FileSystemHelper.SafeGetFiles(dir); }
            catch { continue; }

            // Assess files

            foreach (var file in fileList)
            {
                try
                {
                    FileInfo fi = new(file);
                    long fsize = fi.Length;
                    var diff = Math.Abs(size - fsize);
                    if (diff < tolerance)
                        files.Add(file, fsize);
                }
                catch { /*ignore fails by permission*/ }
            }
        }

        // Show results

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Results: ");
        foreach (var file in files)
            Console.WriteLine($"{file.Value}: {file.Key}");

        Console.WriteLine();
        ConsoleHelper.Goodbye();
    }
}