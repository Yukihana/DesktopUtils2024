using DesktopUtilsSharedLib;
using DesktopUtilsSharedLib.ConsoleHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SearchByModified;

internal class Program
{
    static void Main(string[] args)
    {
        // Input

        List<string> paths = ConsoleHelper.GetInputs("Enter path (* = all): ");
        DateTime eventpoint = ConsoleInput.GetDateTime("Enter datetime, yyyyMMdd_HHmmss (optional _ grouping): ");
        TimeSpan tolerance = ConsoleInput.GetTimeSpan("Tolerance, [-][d'.']hh':'mm':'ss['.'fffffff]: ");

        // Acquire paths

        HashSet<string> sources = [.. paths];
        HashSet<string> directories = [.. sources];

        if (sources.Any(x => x == "*"))
        {
            foreach (var searchTarget in DriveInfo.GetDrives().Select(x => x.RootDirectory.FullName))
                sources.Add(searchTarget);
        }

        // Acquire directories

        Console.WriteLine("Acquiring directories:");
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
        Dictionary<string, DateTime> files = [];
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
                    DateTime lwt = fi.LastWriteTime;
                    var diff = Math.Abs(lwt.Ticks - eventpoint.Ticks);
                    if (diff < tolerance.Ticks)
                        files.Add(file, lwt);
                }
                catch { /*ignore fails by permission*/ }
            }
        }

        // Show results

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Results: ");
        foreach (var file in files)
            Console.WriteLine($"{file.Value:yyyy-MM-dd_HH:mm:ss_fff}: {file.Key}");

        Console.WriteLine();
        ConsoleHelper.Goodbye();
    }
}