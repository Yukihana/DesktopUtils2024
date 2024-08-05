using DesktopUtilsSharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RenameMd5;

internal class Program
{
    static void Main(string[] args)
    {
        string path = ConsoleHelper.GetExistingFolder("Enter path: ");
        string prefix = ConsoleHelper.GetInput("prefix: ");
        string suffix = ConsoleHelper.GetInput("suffix: ");
        string suffixmilliseconds = ConsoleHelper.GetInput("Suffix milliseconds? (y): ");

        string[] files = Directory.GetFiles(path);
        Dictionary<string, string> renameList = [];

        Console.WriteLine();
        Console.WriteLine("Evaluating...");
        Console.WriteLine();

        // Process new names

        foreach (string file in files)
        {
            string extension = Path.GetExtension(file);
            DateTime lastWriteTime = new FileInfo(file).LastWriteTime;

            string datetime = lastWriteTime.ToString("yyyyMMdd_hhMMss");
            string millsec = suffixmilliseconds == "y" ? $"_{lastWriteTime:fff}" : string.Empty;

            string oldName = Path.GetFileName(file);
            string newname = $"{prefix}{datetime}{millsec}{suffix}{extension}";

            renameList[oldName] = newname;
            Console.WriteLine($"- {oldName} > {newname}");
        }

        // Conflict check

        List<string> filenames = [];
        filenames.AddRange(renameList.Keys);
        filenames.AddRange(renameList.Values);

        if (filenames.Count != filenames.Distinct().Count())
        {
            var bkp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Possible conflict might occur on renaming.");
            Console.WriteLine("Check for naming format related conflicts.");
            Console.WriteLine("Aborting...");
            Console.ForegroundColor = bkp;
            return;
        }

        // Confirmation and finalize

        if (ConsoleHelper.GetInput("Confirm rename? (y):") != "y")
        {
            Console.WriteLine("...aborting (cancelled).");
            return;
        }

        foreach (var kvp in renameList)
        {
            Console.WriteLine(Path.GetFileName(kvp.Key) + " > " + Path.GetFileName(kvp.Value));
            File.Move(
                Path.Combine(path, kvp.Key),
                Path.Combine(path, kvp.Value));
        }

        Console.WriteLine();
        Console.WriteLine("...completed.");
    }
}