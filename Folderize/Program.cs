using DesktopUtilsSharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Folderize;

internal class Program
{
    static void Main(string[] args)
    {
        string path = ConsoleHelper.GetExistingFolder("Enter path: ");

        // List all files and folders
        string[] files = Directory.GetFiles(path).Select(x => Path.GetFileName(x)).ToArray();
        string[] folders = Directory.GetDirectories(path).Select(x => Path.GetFileName(x)).ToArray();

        // Prepare new folder names
        Dictionary<string, string> newFolders = files.ToDictionary(x => x, x => Path.GetFileNameWithoutExtension(x));

        // Test conflict
        HashSet<string> conflictor = [.. folders];
        foreach (var newfolder in newFolders)
        {
            if (!conflictor.Add(newfolder.Value) || !conflictor.Add(newfolder.Key))
            {
                Console.WriteLine($"Conflict detected for {newfolder}. Aborting...");
                return;
            }
        }

        // Ask for confirmation
        if (ConsoleHelper.GetInput("Confirm folderizing? (y):") != "y")
        {
            Console.WriteLine("Aborting due to cancellation...");
            return;
        }

        // Rename
        foreach (var newfolder in newFolders)
        {
            Directory.CreateDirectory(Path.Combine(path, newfolder.Value));
            File.Move(
                Path.Combine(path, newfolder.Key),
                Path.Combine(path, newfolder.Value, newfolder.Key));
        }

        Console.WriteLine();
        Console.WriteLine("...completed.");
    }
}