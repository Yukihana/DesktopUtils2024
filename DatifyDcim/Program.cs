using DesktopUtilsSharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

string path = ConsoleHelper.GetInput("Enter path: ");
string prefix = ConsoleHelper.GetInput("Enter prefix: ");
string suffixmilliseconds = ConsoleHelper.GetInput("Suffix milliseconds? (y): ");

string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
Dictionary<string, string> result = [];

foreach (string file in files)
{
    string directory = Path.GetDirectoryName(file) ?? throw new Exception();
    FileInfo fi = new(file);
    string newname = fi.LastWriteTime.ToString("yyyyMMdd_HHmmss");
    if (!string.IsNullOrEmpty(prefix))
        newname = prefix + newname;
    if (suffixmilliseconds == "y")
        newname = newname + "_" + fi.LastWriteTime.ToString("fff");
    result.Add(file, newname + Path.GetExtension(file));
}

if (result.Count != result.Values.ToHashSet().Count)
{
    Console.WriteLine("Duplication in modified time of images.");
    return;
}

ConsoleHelper.PrintListSection("Recommended renames:", [.. result.Select(x => $"{x.Value}: {x.Key}")]);

string confirmation = ConsoleHelper.GetInput("Continue with renaming?");

if (confirmation != "y")
    return;

foreach (var kvp in result)
{
    string directory = Path.GetDirectoryName(kvp.Key) ?? throw new Exception();
    string newname = Path.Combine(directory, kvp.Value);

    if (File.Exists(newname))
        throw new Exception("File already exists.");
    File.Move(kvp.Key, newname);
}

Console.WriteLine();
Console.WriteLine("...completed");