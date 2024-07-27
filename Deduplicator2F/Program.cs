using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Enter the first folder path:");
        string folderPath1 = Console.ReadLine();

        Console.WriteLine("Enter the second folder path:");
        string folderPath2 = Console.ReadLine();

        if (!Directory.Exists(folderPath1) || !Directory.Exists(folderPath2))
        {
            Console.WriteLine("One or both of the directories do not exist.");
            return;
        }

        var files1 = Directory.GetFiles(folderPath1);
        var files2 = Directory.GetFiles(folderPath2);

        var fileNames1 = files1.Select(Path.GetFileName);
        var fileNames2 = files2.Select(Path.GetFileName);

        var commonFileNames = fileNames1.Intersect(fileNames2);

        foreach (var fileName in commonFileNames)
        {
            string filePath1 = Path.Combine(folderPath1, fileName);
            string filePath2 = Path.Combine(folderPath2, fileName);

            if (FilesAreEqual(filePath1, filePath2))
            {
                Console.WriteLine($"Files {fileName} are equal. Deleting {filePath2}.");
                File.Delete(filePath2);
            }
        }

        Console.WriteLine("Operation completed.");
    }

    static bool FilesAreEqual(string filePath1, string filePath2)
    {
        const int bufferSize = 1024 * 1024; // 1MB buffer size

        using (var fs1 = new FileStream(filePath1, FileMode.Open, FileAccess.Read))
        using (var fs2 = new FileStream(filePath2, FileMode.Open, FileAccess.Read))
        {
            if (fs1.Length != fs2.Length)
                return false;

            byte[] buffer1 = new byte[bufferSize];
            byte[] buffer2 = new byte[bufferSize];

            while (true)
            {
                int bytesRead1 = fs1.Read(buffer1, 0, bufferSize);
                int bytesRead2 = fs2.Read(buffer2, 0, bufferSize);

                if (bytesRead1 != bytesRead2)
                    return false;

                if (bytesRead1 == 0) // End of file reached
                    break;

                if (!buffer1.Take(bytesRead1).SequenceEqual(buffer2.Take(bytesRead1)))
                    return false;
            }
        }

        return true;
    }
}