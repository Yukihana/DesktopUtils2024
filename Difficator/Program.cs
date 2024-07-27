using DesktopUtilsSharedLib;
using System;
using System.IO;
using System.Linq;

namespace Difficator;

internal class Program
{
    static void Main(string[] args)
    {
        string src1 = ConsoleHelper.GetInput("Enter source 1: ");
        string src2 = ConsoleHelper.GetInput("Enter source 2: ");
        string trg1 = src1.TrimEnd(Path.PathSeparator) + "_diff";
        string trg2 = src2.TrimEnd(Path.PathSeparator) + "_diff";

        string[] files1 = [.. Directory.GetFiles(src1, "*.*", SearchOption.TopDirectoryOnly).Select(x => Path.GetFileName(x))];
        string[] files2 = [.. Directory.GetFiles(src2, "*.*", SearchOption.TopDirectoryOnly).Select(x => Path.GetFileName(x))];

        if (!files1.Intersect(files2).Any())
        {
            Console.WriteLine("There are no common files. Aborting...");
            return;
        }

        string[] diff1 = [.. files1.Where(x => !files2.Contains(x))];
        string[] diff2 = [.. files2.Where(x => !files1.Contains(x))];

        Directory.CreateDirectory(trg1);
        foreach (string file in diff1)
            File.Move(Path.Combine(src1, file), Path.Combine(trg1, file));

        Directory.CreateDirectory(trg2);
        foreach (string file in diff2)
            File.Move(Path.Combine(src2, file), Path.Combine(trg2, file));
    }
}