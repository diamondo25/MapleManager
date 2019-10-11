using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixNamingConflict
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("FixNamingConflict.exe <path>");
                return;
            }

            ProcessDirectory(new DirectoryInfo(args[0]));
        }

        static void ProcessDirectory(DirectoryInfo di)
        {
            foreach (var fileInfo in di.GetFiles("*.cs"))
            {
                FixFile(fileInfo.FullName);
            }

            foreach (var innerDir in di.GetDirectories())
            {
                ProcessDirectory(innerDir);
            }
        }

        static void FixFile(string path)
        {
            var contents = File.ReadAllText(path);

            var tmp = contents.Replace("MapleManager.Scripts", "MapleManager.Scripts.unconflicted");
            if (tmp != contents)
            {
                Console.WriteLine("Fixed {0}", path);
                contents = tmp;
            }

            File.WriteAllText(path, contents);
        }
    }
}
