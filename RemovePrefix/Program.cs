using DesktopUtilsSharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RemovePrefix;

internal class Program
{
    static void Main(string[] args)
    {
        string path = ConsoleHelper.GetInput("Enter path: ");
        string prefix = ConsoleHelper.GetInput("Enter prefix (case sensitive): ");

        string[] files = [.. Directory.GetFiles(path).Select(Path.GetFileName)];

        HashSet<string> existing = [];
        Dictionary<string, string> todo = [];

        foreach (string file in files)
        {
            string newname = file;

            if (file.StartsWith(prefix))
            {
                newname = file[prefix.Length..];
                todo.Add(file, newname);
            }

            if (!existing.Add(newname))
            {
                Console.WriteLine($"Naming conflict expected at {newname}. Aborting...");
                return;
            }
        }

        ConsoleHelper.PrintListSection("Files to rename:", todo.Select(kvp => $"{kvp.Key} > {kvp.Value}").ToArray());

        if (ConsoleHelper.GetInput("Confirm rename?") != "y")
            return;

        foreach (var kvp in todo)
            File.Move(Path.Combine(path, kvp.Key), Path.Combine(path, kvp.Value));

        Console.WriteLine("...completed");
    }
}