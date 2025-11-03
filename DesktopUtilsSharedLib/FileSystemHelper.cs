using System.Collections.Generic;
using System.IO;

namespace DesktopUtilsSharedLib;

public static partial class FileSystemHelper
{
    public static string[] SafeGetFiles(
        string path)
    {
        try { return Directory.GetFiles(path); }
        catch { return []; }
    }

    public static List<string> SafeGetDirectoriesRecursive(
        string path)
    {
        List<string> directories = [];
        try
        {
            var dirs = Directory.GetDirectories(path);
            directories.AddRange(dirs);
            foreach (var dir in dirs)
                directories.AddRange(SafeGetDirectoriesRecursive(dir));
            return directories;
        }
        catch { return []; }
    }
}