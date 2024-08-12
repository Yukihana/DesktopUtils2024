using DesktopUtilsSharedLib;
using System;
using System.Collections.Generic;
using System.IO;

namespace efix;

internal class Program
{
    static void Main(string[] args)
    {
        string path = ConsoleHelper.GetInput("Enter path: ");

        List<string> filesToIsolate = [];

        foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            DateTime writeTime = new FileInfo(file).LastWriteTime;

            string badname = writeTime.ToString("yyyyMMdd_hhMMss");
            string goodname = writeTime.ToString("yyyyMMdd_HHmmss");

            if (!Path.GetFileName(file).Contains(badname) ||
                goodname == badname)
                continue;

            string directory = Path.GetDirectoryName(file) ?? throw new Exception();
            string filename = Path.GetFileName(file);
            string newname = file.Replace(badname, goodname);
            File.Move(file, Path.Combine(directory, newname));

            Console.WriteLine($"- {filename} > {newname} ({directory})");
        }
    }
}