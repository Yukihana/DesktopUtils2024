using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;

namespace DesktopUtilsSharedLib.ConsoleHelpers;

public static partial class ConsoleInput
{
    public static HashSet<string> GetFilesInteractive(
        string message)
    {
        WriteMessage(message, true);
        List<string> files = [];

        while (true)
        {
            string path = GetString("> ");

            if (string.IsNullOrEmpty(path))
                break;

            if (File.Exists(path))
            {
                files.Add(path);
                continue;
            }

            if (Directory.Exists(path))
            {
                List<string> directoriesToScan = [path];

                bool recursive = GetYesNo(
                    "Provided path is a directory. Scan for files recursively? (n):",
                    defaultValue: false);

                if (recursive)
                    directoriesToScan.AddRange(FileSystemHelper.SafeGetDirectoriesRecursive(path));

                foreach (var directory in directoriesToScan)
                    files.AddRange(FileSystemHelper.SafeGetFiles(directory));

                continue;
            }

            Console.WriteLine("Unknown path. Try again. To abort, enter an empty line.");
        }

        return files.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}