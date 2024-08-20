using System;
using System.Collections.Generic;
using System.Linq;

namespace DesktopUtilsSharedLib;

public static partial class ConsoleOutput
{
    public static void WriteInSameLine(string message, ConsoleColor? color = null)
    {
        // Truncate
        message = message.Normalize().NormalizeMono();
        int width = Console.WindowWidth;
        if (message.Length > width)
            message = string.Concat(message.AsSpan(0, width - 3), "...");

        // Get pos
        Console.WriteLine();
        var row = Console.GetCursorPosition().Top - 1;

        // Clear
        Console.SetCursorPosition(0, row);
        Console.WriteLine(new string(' ', Console.WindowWidth));

        // Write to position
        Console.SetCursorPosition(0, row);
        var bkp = Console.ForegroundColor;
        if (color.HasValue)
            Console.ForegroundColor = color.Value;
        Console.WriteLine(message);

        // Restore
        Console.ForegroundColor = bkp;
        Console.SetCursorPosition(0, row);
    }

    // Sections

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