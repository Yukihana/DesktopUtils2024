using DesktopUtilsSharedLib;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MoveDCIMFiles;

internal partial class Program
{
    static void Main(string[] args)
    {
        string sourceFolder = ConsoleHelper.GetInput("Enter source folder: ");
        string targetFolder = Path.Combine(Path.GetDirectoryName(sourceFolder) ?? throw new Exception(), Path.GetFileName(sourceFolder) + "_FilteredExtras");
        Directory.CreateDirectory(targetFolder);

        Regex regexJpg = DcimJpgRegex();
        Regex regexMp4 = DcimMp4Regex();

        int count = 0;

        foreach (string path in Directory.GetFiles(sourceFolder))
        {
            string filename = Path.GetFileName(path);
            if (!regexJpg.IsMatch(filename) && !regexMp4.IsMatch(filename))
            {
                Console.WriteLine($"Moving file: {filename}");
                File.Move(path, Path.Combine(targetFolder, filename));
                count++;
            }
        }
    }

    [GeneratedRegex(@"^\d{8}_\d{6}\.jpg$", RegexOptions.Compiled)]
    private static partial Regex DcimJpgRegex();

    [GeneratedRegex(@"^\d{8}_\d{6}\.mp4$", RegexOptions.Compiled)]
    private static partial Regex DcimMp4Regex();
}