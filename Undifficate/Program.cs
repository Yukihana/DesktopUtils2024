using DesktopUtilsSharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Undifficate;

internal class Program
{
    static void Main(string[] args)
    {
        string target = ConsoleHelper.GetInput("Enter target directory prefix: ");

        string parent = Path.GetDirectoryName(target) ?? throw new Exception();
        string prefix = Path.GetFileName(target);

        string[] dirs = Directory.GetDirectories(parent, $"{prefix}*", SearchOption.TopDirectoryOnly);
        List<string> files = dirs.Where(x => x != target).SelectMany(Directory.GetFiles).ToList();

        foreach (string file in files)
        {
            string newpath = Path.Combine(target, Path.GetFileName(file));
            File.Move(file, newpath);
            Console.WriteLine($"{file} > {newpath}");
        }

        foreach (string dir in dirs)
        {
            if (dir == target)
                continue;
            if (Directory.GetFiles(dir).Length > 0)
                throw new Exception("Directory not empty, unable to delete.");
            Directory.Delete(dir);
        }

        Console.WriteLine();
        Console.WriteLine("...completed");
    }
}