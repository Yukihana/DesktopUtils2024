using DesktopUtilsSharedLib;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HashRename;

internal class Program
{
    async static Task Main(string[] args)
    {
        string path = ConsoleHelper.GetInput("Enter path: ");
        string[] files = Directory.GetFiles(path);

        string option = ConsoleHelper.GetInput("Append original name(y): ");

        await Parallel.ForEachAsync(files, CancellationToken.None, async (file, ctoken) =>
        {
            string hash = await HashHelper.ProcessHash(file);
            string newname = option == "y"
                ? hash + "_" + Path.GetFileName(file)
                : hash + Path.GetExtension(file);
            string newPath = Path.Combine(Path.GetDirectoryName(file) ?? throw new Exception(), newname);
            File.Move(file, newPath);
            Console.WriteLine($"{file} > {newPath}");
        });
    }
}