using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Subdirectories Sequential Rename
namespace SDSR
{
    class Program
    {
        static string guid;
        static void Main(string[] args)
        {
            Console.WriteLine("A program to rename all file-system-entries using unique incremental numbering.");
            Console.WriteLine("Target Path: ");
            var path = Console.ReadLine();

            ProcessDir(path);
        }
        static void ProcessDir(string path)
        {
            foreach (string f in Directory.GetFileSystemEntries(path))
            {
                guid = Guid.NewGuid().ToString();

                if (File.Exists(f))
                {
                    while(File.Exists(Path.Combine(Path.GetDirectoryName(f), guid + Path.GetExtension(f))))
                    {
                        guid = Guid.NewGuid().ToString();
                    }
                    File.Move(f, Path.Combine(Path.GetDirectoryName(f), guid + Path.GetExtension(f)));
                }
                else if(Directory.Exists(f))
                {
                    ProcessDir(f);
                    while(Directory.Exists(Path.Combine(Path.GetDirectoryName(f), guid)))
                    {
                        guid = Guid.NewGuid().ToString();
                    }
                    Directory.Move(f, Path.Combine(Path.GetDirectoryName(f), guid));
                }
            }
        }
    }
}
