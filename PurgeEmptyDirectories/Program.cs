using DesktopUtilsSharedLib;
using System;
using System.Collections.Generic;
using System.IO;

namespace PurgeEmptyDirectories;

internal class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            string path = ConsoleHelper.GetInput("Enter path: ");
            Console.WriteLine();

            List<string> empties = [];
            foreach (string dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
            {
                if (Directory.GetFileSystemEntries(dir, "*", SearchOption.TopDirectoryOnly).Length == 0)
                    empties.Add(dir);
            }

            ConsoleHelper.PrintListSection("Empty folders:", [.. empties]);
            if (ConsoleHelper.GetInput("Confirm deletion?") == "y")
            {
                foreach (var empty in empties)
                {
                    if (Directory.GetFileSystemEntries(empty, "*", SearchOption.AllDirectories).Length == 0)
                        Directory.Delete(empty);
                }
            }

            if (ConsoleHelper.GetInput("Retry?") != "y")
                break;
        }

        Console.WriteLine("...completed.");
    }
}