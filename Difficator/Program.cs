using DesktopUtilsSharedLib;
using System;
using System.IO;
using System.Linq;

namespace Difficator;

internal class Program
{
    static void Main(string[] args)
    {
        string src1 = ConsoleHelper.GetExistingFolder("Enter source 1: ");
        string src2 = ConsoleHelper.GetExistingFolder("Enter source 2: ");
        bool commonMode = ConsoleHelper.GetInput("Separate diff(default) or commons(y)?") == "y";
        string suffix = commonMode ? "_common" : "_diff";
        string trg1 = src1.TrimEnd(Path.PathSeparator) + suffix;
        string trg2 = src2.TrimEnd(Path.PathSeparator) + suffix;

        string[] files1 = [.. Directory.GetFiles(src1).Select(Path.GetFileName)];
        string[] files2 = [.. Directory.GetFiles(src2).Select(Path.GetFileName)];

        if (!files1.Intersect(files2).Any())
        {
            Console.WriteLine("There are no common files. Aborting...");
            return;
        }

        string[] sep1 = [.. files1.Where(x => files2.Contains(x) == commonMode)];
        string[] sep2 = [.. files2.Where(x => files1.Contains(x) == commonMode)];

        Console.WriteLine($"Moving {sep1.Length} out of {files1.Length} files to {Path.GetFileName(trg1)}");
        Directory.CreateDirectory(trg1);
        foreach (string file in sep1)
            File.Move(Path.Combine(src1, file), Path.Combine(trg1, file));

        Console.WriteLine($"Moving {sep2.Length} out of {files2.Length} files to {Path.GetFileName(trg2)}");
        Directory.CreateDirectory(trg2);
        foreach (string file in sep2)
            File.Move(Path.Combine(src2, file), Path.Combine(trg2, file));

        Console.WriteLine();
        Console.WriteLine("...completed.");
    }
}