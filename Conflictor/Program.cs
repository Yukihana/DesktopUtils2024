using DesktopUtilsSharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Conflictor;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Enter folders, one per entry:");
        HashSet<string> folders = [];
        while (true)
        {
            string line = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrEmpty(line))
                break;
            folders.Add(line);
        }

        Console.WriteLine("Analyzing...");
        Console.WriteLine();

        Dictionary<string, List<string>> map = [];
        foreach (string folder in folders)
        {
            string[] files = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                string filename = Path.GetFileName(file);
                if (!map.ContainsKey(filename))
                    map[filename] = [];
                map[filename].Add(file);
            }
        }

        foreach (var kvp in map.Where(x => x.Value.Count > 1))
            ConsoleHelper.PrintListSection(kvp.Key, [.. kvp.Value]);

        Console.WriteLine();
        Console.WriteLine("... completed");
    }
}