using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DesktopUtilsSharedLib.ConsoleHelpers;

public static partial class ConsoleInput
{
    private static readonly string[] YES_VALUES = ["y", "yes", "1"];
    private static readonly string[] NO_VALUES = ["n", "no", "0"];
    // Path

    public static string GetExistingFile(string message)
    {
        Console.Write(message);
        string file = Console.ReadLine() ?? throw new Exception();
        if (!File.Exists(file))
        {
            if (Directory.Exists(file))
                throw new InvalidDataException($"The provided path is a directory, not a file: {file}");
            else
                throw new InvalidDataException($"The provided file does not exist: {file}");
        }
        return file;
    }

    public static string GetExistingFolder(string message)
    {
        Console.Write(message);
        string directory = Console.ReadLine() ?? throw new Exception();
        if (!Directory.Exists(directory))
        {
            if (File.Exists(directory))
                throw new InvalidDataException($"The provided path is a file, not a directory: {directory}");
            else
                throw new InvalidDataException($"The provided directory does not exist: {directory}");
        }
        return directory;
    }

    public static string[] GetFoldersUntilEmpty(string message)
    {
        WriteMessage(message, appendNewLine: true);
        List<string> folders = [];

        while (true)
        {
            if (!TryTakeFolderOrEmpty(out string path))
                break;
            folders.Add(path);
        }

        return [.. folders];
    }

    // String

    public static string GetString(string message)
    {
        Console.Write(message);
        return Console.ReadLine() ?? string.Empty;
    }

    public static List<string> GetStrings(string message)
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

    // Primitives

    public static long GetLong(string message)
    {
        if (Console.GetCursorPosition().Left != 0)
            Console.WriteLine();
        Console.Write(message);
        while (true)
        {
            try
            {
                var input = Console.ReadLine();
                input = input?.Replace("_", "");
                if (string.IsNullOrEmpty(input))
                    return 0;
                long result = long.Parse(input);
                return result;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }

    public static float GetFloat(string message)
    {
        if (Console.GetCursorPosition().Left != 0)
            Console.WriteLine();
        Console.Write(message);
        while (true)
        {
            try
            {
                string input = Console.ReadLine() ?? throw new Exception();
                float result = float.Parse(input);
                return result;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }

    // Misc

    public static DateTime GetDateTime(string message)
    {
        if (Console.GetCursorPosition().Left != 0)
            Console.WriteLine();
        Console.Write(message);
        while (true)
        {
            try
            {
                string input = Console.ReadLine() ?? throw new Exception();
                DateTime result = DateTime.ParseExact(input.Replace("_", ""), "yyyyMMddHHmmss", null);
                return result;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }

    public static TimeSpan GetTimeSpan(string message)
    {
        if (Console.GetCursorPosition().Left != 0)
            Console.WriteLine();
        Console.Write(message);
        while (true)
        {
            try
            {
                string input = Console.ReadLine() ?? throw new Exception();
                TimeSpan result = TimeSpan.ParseExact(input, "c", null);
                return result;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }

    public static bool GetYesNo(
        string message,
        bool defaultValue = false)
    {
        WriteMessage(message);
        while (true)
        {
            try
            {
                string input = Console.ReadLine() ?? throw new Exception();

                if (string.IsNullOrEmpty(input))
                    return defaultValue;

                if (NO_VALUES.Contains(input, StringComparer.OrdinalIgnoreCase))
                    return false;

                if (YES_VALUES.Contains(input, StringComparer.OrdinalIgnoreCase))
                    return true;

                throw new Exception("Please enter 'y' or 'n'.");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }

    private static void WriteMessage(
        string message,
        bool appendNewLine = false)
    {
        if (Console.GetCursorPosition().Left != 0)
            Console.WriteLine();

        if (appendNewLine)
            Console.WriteLine(message);
        else
            Console.Write(message);
    }

    private static bool TryTakeFolderOrEmpty(out string path)
    {
        while (true)
        {
            try
            {
                path = Console.ReadLine() ?? throw new Exception();
                if (string.IsNullOrEmpty(path))
                    return false;

                if (!Directory.Exists(path))
                    throw new InvalidDataException($"The provided directory does not exist: {path}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}