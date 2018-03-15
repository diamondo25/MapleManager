using System.IO;
using System.Linq;

namespace MapleManager.WzTools.FileSystem
{
    class WzFileSystem : WzNameSpace
    {
        private static void LoadDirectories(NameSpaceDirectory parentNode, DirectoryInfo folder)
        {
            var dirs = folder.GetDirectories().ToDictionary(x => x.Name, x => x);

            // Build structure out of dirs
            foreach (var kvp in dirs)
            {
                var name = kvp.Key;
                if (parentNode.SubDirectories.Exists(x => x.Name == name)) continue;

                var node = new FSDirectory();
                node.Size = 0;
                node.Name = name;
                node.OffsetInFile = -1;
                node.RealPath = folder.FullName;
                LoadDirectories(node, kvp.Value);

                parentNode.Add(node);
            }

            LoadFiles(parentNode, folder);
        }

        private static void LoadFiles(NameSpaceDirectory parentNode, DirectoryInfo folder)
        {
            var files = folder.GetFiles("*.img").ToDictionary(x => x.Name, x => x);

            foreach (var kvp in files)
            {
                var name = kvp.Key;
                if (parentNode.Files.Exists(x => x.Name == name)) continue;

                var node = new FSFile();
                node.Size = (int)kvp.Value.Length;
                node.Name = name;
                node.OffsetInFile = -1;
                node.RealPath = kvp.Value.FullName;
                parentNode.Add(node);
            }
        }

        public void Init(string folder)
        {
            LoadDirectories(this, new DirectoryInfo(folder));
        }
    }
}
