using DesktopUtilsSharedLib;

namespace RefolderByYear;

internal class Program
{
    static void Main(string[] args)
    {
        string path = ConsoleHelper.GetInput("Enter path: ");

        if (Directory.GetDirectories(path).Length != 0)
        {
            Console.WriteLine("Make sure there are no subdirectories. Aborting...");
        }

        string[] files = Directory.GetFiles(path);
    }
}