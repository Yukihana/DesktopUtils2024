using System;
using System.Collections.Generic;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

namespace DesktopUtilsSharedLib;

public static class ConsoleHelper
{
    public static string GetInput(string message)
    {
        Console.Write(message);
        return Console.ReadLine() ?? string.Empty;
    }

    public static void PrintListSection(string section, string[] entries)
    {
        Console.WriteLine($"{section} ({entries.Length}):");
        foreach (string entry in entries)
            Console.WriteLine("- " + entry);
        Console.WriteLine();
    }

    public static void PrintListSection(string title, IEnumerable<string> items)
    {
        Console.WriteLine($"{title} ({items.Count()}):");
        foreach (string entry in items)
            Console.WriteLine("- " + entry);
        Console.WriteLine();
    }
}