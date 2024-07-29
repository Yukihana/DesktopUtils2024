using DesktopUtilsSharedLib;
using System;
using System.IO;

namespace RefolderByYear;

internal class Program
{
    static void Main(string[] args)
    {
        string source = ConsoleHelper.GetInput("Enter source path: ");
        string target = ConsoleHelper.GetInput("Enter target path: ");
        string suffix = ConsoleHelper.GetInput("Enter suffix: ");

        string[] files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            string filename = Path.GetFileName(file);
            FileInfo fi = new(file);

            var year = fi.LastWriteTime.Year;
            int index = 0;

            string newdir = Path.Combine(target, $"{year}{suffix}");

            while (true)
            {
                string newpath = Path.Combine(newdir, filename);

                try
                {
                    Directory.CreateDirectory(newdir);
                    File.Move(file, newpath);
                    Console.WriteLine($"Success: {file} > {newpath}");
                    break;
                }
                catch
                {
                    Console.WriteLine($"Failed: {file} > {newpath}");
                }

                index++;
                newdir = Path.Combine(target, $"{year}_{index}{suffix}");
            }
        }
        Console.WriteLine();
        Console.WriteLine("...completed");
    }
}