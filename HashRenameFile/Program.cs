using DesktopUtilsSharedLib;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HashRenameFile;

internal class Program
{
    async static Task Main(string[] args)
    {
        string file = ConsoleHelper.GetInput("Enter file path: ");
        string newname = await HashHelper.ProcessHash(file) + Path.GetExtension(file);
        string dir = Path.GetDirectoryName(file) ?? throw new Exception();
        string newpath = Path.Combine(dir, newname);
        File.Move(file, newpath);
        Console.WriteLine($"File moved to {newpath}");
    }
}