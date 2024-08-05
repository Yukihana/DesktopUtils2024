using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

namespace DesktopUtilsSharedLib;

public static class ConsoleHelper
{
    // Inputs

    public static string GetInput(string message)
    {
        Console.Write(message);
        return Console.ReadLine() ?? string.Empty;
    }

    public static string GetExistingFolder(string message)
    {
        Console.Write(message);
        string directory = Console.ReadLine() ?? string.Empty;
        if (!Directory.Exists(directory))
        {
            if (File.Exists(directory))
                throw new InvalidDataException($"The provided path is a file, not a directory: {directory}");
            else
                throw new InvalidDataException($"The provided directory does not exist: {directory}");
        }
        return directory;
    }

    // Printing

    public static List<string> GetInputs(string message)
    {
        if (Console.CursorLeft != 0)
            Console.WriteLine();
        Console.WriteLine(message);

        List<string> inputs = [];
        while (true)
        {
            string? input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
                inputs.Add(input);
            else
                break;
        }

        return inputs;
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