using DesktopUtilsSharedLib;
using System;
using System.IO;
using System.Linq;

namespace EmptyDirs;

internal class Program
{
    static void Main(string[] args)
    {
        string path = ConsoleHelper.GetInput("Enter path for analysis:");
        Console.WriteLine();

        string[] dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

        string[] trulyEmpty = dirs.Where(x => Directory.GetFileSystemEntries(x).Length == 0).ToArray();
        string[] emptyStructures = dirs.Where(x => Directory.GetFiles(x, "*.*", SearchOption.AllDirectories).Length == 0 && Directory.GetDirectories(x, "*.*", SearchOption.AllDirectories).Length > 0).ToArray();
        string[] singleSub = dirs.Where(x => Directory.GetFiles(x).Length == 0 && Directory.GetDirectories(x).Length == 1).ToArray();
        string[] multiSub = dirs.Where(x => Directory.GetFiles(x).Length == 0 && Directory.GetDirectories(x).Length > 1).ToArray();

        ConsoleHelper.PrintListSection("Empty directories", trulyEmpty);
        ConsoleHelper.PrintListSection("Empty structures", emptyStructures);
        ConsoleHelper.PrintListSection("Single directory parents", singleSub);
        ConsoleHelper.PrintListSection("Multi directory parents", multiSub);
    }
}