using DesktopUtilsSharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Apos;

internal class Program
{
    static void Main(string[] args)
    {
        string path = ConsoleHelper.GetInput("Enter path: ");
        string prefix = ConsoleHelper.GetInput("Enter prefix: ");
        string suffix = ConsoleHelper.GetInput("Enter suffix: ");
        string mode = ConsoleHelper.GetInput("Additional filter (f=files only, d=directories only): ");

        Dictionary<string, string> directories = [];
        if (mode is not "f")
        {
            foreach (string dir in Directory.GetDirectories(path))
            {
                string oldname = Path.GetFileName(dir);
                string newname = prefix + oldname + suffix;
                Console.WriteLine(oldname + " > " + newname);
                directories[dir] = Path.Combine(path, newname);
            }
        }

        Dictionary<string, string> files = [];
        if (mode is not "d")
        {
            foreach (string file in Directory.GetFiles(path))
            {
                string oldname = Path.GetFileNameWithoutExtension(file);
                string newname = prefix + oldname + suffix + Path.GetExtension(file);
                Console.WriteLine(oldname + " > " + newname);
                files[file] = Path.Combine(path, newname);
            }
        }

        // Assessing conflict
        IEnumerable<IEnumerable<string>> all = [directories.Keys, directories.Values, files.Keys, files.Values];
        HashSet<string> conflict = [];
        foreach (string s in all.SelectMany(x => x))
        {
            if (!conflict.Add(s))
            {
                Console.WriteLine($"Possible rename conflict at: {s}. Aborting...");
                return;
            }
        }
        if (ConsoleHelper.GetInput("No conflicts expected. Continue? (y):") != "y")
            return;

        foreach (var kvp in directories)
            Directory.Move(kvp.Key, kvp.Value);
        foreach (var kvp in files)
            File.Move(kvp.Key, kvp.Value);

        Console.WriteLine();
        Console.WriteLine("...completed");
    }
}