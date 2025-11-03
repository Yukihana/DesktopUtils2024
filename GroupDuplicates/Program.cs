using DesktopUtilsSharedLib;
using DesktopUtilsSharedLib.ConsoleHelpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GroupDuplicates;

internal class Program
{
    static async Task Main(string[] args)
    {
        string path = ConsoleInput.GetExistingFolder("Enter folder for analysis: ");
        bool recursive = ConsoleInput.GetYesNo("Process subfolders recursively? (y/n; default=n): ");

        string[] files = recursive
            ? Directory.GetFiles(path, "*", SearchOption.AllDirectories)
            : Directory.GetFiles(path);

        ConcurrentDictionary<string, string> hashes = await HashHelperSha2D256.HashFiles(files);

        // Group files by hash
        Dictionary<string, IEnumerable<string>> groups = hashes
            .GroupBy(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Key));

        foreach (var group in groups.Where(x => x.Value.Count() > 1))
        {
            string newdir = Path.Combine(path, group.Key);
            Directory.CreateDirectory(newdir);

            foreach (string file in group.Value)
            {
                string newFilePath = Path.Combine(newdir, Path.GetFileName(file));
                if (File.Exists(newFilePath))
                {
                    string newName
                        = Path.GetFileNameWithoutExtension(file)
                        + $"_{Guid.NewGuid():N}"
                        + Path.GetExtension(file);
                    newFilePath = Path.Combine(newdir, newName);
                }
                File.Move(file, newFilePath);
            }
        }
    }
}