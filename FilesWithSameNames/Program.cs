using DesktopUtilsSharedLib;

namespace FilesWithSameNames;

internal class Program
{
    static void Main(string[] args)
    {
        string path = ConsoleHelper.GetInput("Enter path: ");

        Console.WriteLine("Processing...");
        Console.WriteLine();

        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        Dictionary<string, HashSet<string>> names = [];

        foreach (string file in files)
        {
            string filename = Path.GetFileName(file);
            if (!names.ContainsKey(filename))
                names.Add(filename, []);
            names[filename].Add(file);
        }

        int duplicateNames = 0;
        int duplicateFiles = 0;
        foreach (var kvp in names.Where(x => x.Value.Count > 1))
        {
            Console.WriteLine($"Name: {kvp.Key}");
            duplicateNames++;
            duplicateFiles += kvp.Value.Count;

            foreach (var filepath in kvp.Value)
                Console.WriteLine("- " + filepath);

            Console.WriteLine();
        }

        Console.WriteLine("...completed.");

        Console.WriteLine($"${duplicateNames} unique filenames had a total of {duplicateFiles} duplicates.");
    }
}