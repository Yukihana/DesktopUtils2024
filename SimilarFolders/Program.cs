using DesktopUtilsSharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimilarFolders;

internal class Program
{
    static void Main(string[] args)
    {
        string path = ConsoleHelper.GetInput("Enter path: ");
        string minSimS = ConsoleHelper.GetInput("Minimum similarity (0~1): ");

        Console.WriteLine();
        string[] directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
        if (!double.TryParse(minSimS, out double minSim))
            minSim = 0;

        Console.WriteLine($"Gathering info about {directories.Length} directories...");
        Dictionary<string, HashSet<string>> files = [];

        foreach (string directory in directories)
        {
            var contents = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);
            if (contents.Length == 0)
                continue;
            files[directory] = [.. contents.Select(x => Path.GetFileName(x))];
        }

        Console.WriteLine($"Comparing {files.Count} non empty directories...");
        Dictionary<string, double> comparison = [];
        HashSet<string> compared = [];

        foreach (var f1 in files)
        {
            foreach (var f2 in files.Where(x => !compared.Contains(x.Key) && x.Key != f1.Key))
            {
                double similarityIndex = CalculateJaccardSimilarity(f1.Value, f2.Value);
                comparison[$"{f1.Key}|{f2.Key}"] = similarityIndex;
            }
            compared.Add(f1.Key);
        }

        double[] indices = [.. comparison.Values.ToHashSet().ToArray().OrderByDescending(x => x)];

        Console.WriteLine();
        Console.WriteLine($"Showing results with at least {minSim}x similarity in descending order:");
        Console.WriteLine();

        int count = 0;
        foreach (var index in indices.Where(x => x != 0))
        {
            foreach (var kvp in comparison.Where(x => x.Value == index))
            {
                string[] dirs = kvp.Key.Split('|');
                Console.WriteLine($"{index}");
                Console.WriteLine("- " + dirs[0]);
                Console.WriteLine("- " + dirs[1]);
                Console.WriteLine();
                count++;
            }
        }

        Console.WriteLine("...completed.");

        Console.WriteLine($"Showing {count} out of {comparison.Count} comparisons. (Similarity at least {minSim}");
    }

    public static double CalculateJaccardSimilarity(HashSet<string> set1, HashSet<string> set2)
    {
        var intersection = new HashSet<string>(set1);
        intersection.IntersectWith(set2);

        var union = new HashSet<string>(set1);
        union.UnionWith(set2);

        return union.Count == 0 ? 0 : (double)intersection.Count / union.Count;
    }
}