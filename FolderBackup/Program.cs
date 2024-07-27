using System;
using System.Diagnostics;
using System.IO;

internal sealed partial class Program
{
    private static void Main(string[] args)
    {
        // Config: Get the name from the executable.
        string exepath = Process.GetCurrentProcess().MainModule?.FileName
            ?? Process.GetCurrentProcess().ProcessName;
        string name = Path.GetFileNameWithoutExtension(exepath);

        // Config: set up
        string cwd = Environment.CurrentDirectory;
        string cfgfile = Path.Combine(cwd, name + ".bkpsrc");

        // Config: validate parameters
        if (!File.Exists(cfgfile))
        {
            using FileStream f = File.Create(cfgfile);
            Console.WriteLine($"Empty config created at {cfgfile}. Put the source path in here. No formatting necessary.");
            Console.ReadKey();
            return;
        }

        string source = File.ReadAllText(cfgfile);
        if (string.IsNullOrEmpty(source))
        {
            Console.WriteLine($"Unable to proceed. The file at {cfgfile} is empty. Put the source path in here. No formatting necessary.");
            Console.ReadKey();
            return;
        }

        if (!Directory.Exists(source))
        {
            Console.WriteLine($"Unable to proceed. The location provided in {cfgfile} can't be found.");
            Console.ReadKey();
            return;
        }
        Console.WriteLine("Found source: {0}", source);

        // Operation: Set up parameters
        DateTime current = DateTime.Now;
        string folderName = current.ToString("yyyyMMdd_HHmmss_fff");
        string dest = Path.Combine(cwd, "ManualBackups", folderName);
        Directory.CreateDirectory(dest);

        // Operation: Perform the backup
        string[] files = Directory.GetFiles(source);

        for (int i = 0; i < files.Length; i++)
        {
            string sourcefile = files[i];
            Console.WriteLine("{0}: Copying {1}", i + 1, sourcefile);
            string targetfile = Path.Combine(dest, Path.GetFileName(sourcefile));
            File.Copy(sourcefile, targetfile);
        }

        // Notify on finish
        Console.WriteLine("Files backed up to: {0}", dest);
        Console.ReadKey();
    }
}