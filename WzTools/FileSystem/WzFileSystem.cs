using System.IO;
using System.Linq;

namespace MapleManager.WzTools.FileSystem
{
    class WzFileSystem : WzNameSpace
    {
        private static void LoadDirectories(NameSpaceDirectory parentNode, DirectoryInfo folder)
        {
            // Build structure out of dirs
            var dirNodes = folder.GetDirectories()
                .Where(x => !parentNode.SubDirectories.Exists(y => y.Name == x.Name))
                .Select(dir =>

                    {
                        var name = dir.Name;

                        var node = new FSDirectory();
                        node.Size = 0;
                        node.Name = name;
                        node.OffsetInFile = 0;
                        node.RealPath = folder.FullName;
                        LoadDirectories(node, dir);

                        return node;
                    }
                );

            parentNode.AddDirectories(dirNodes);
            LoadFiles(parentNode, folder);
            
        }

        private static void LoadFiles(NameSpaceDirectory parentNode, DirectoryInfo folder)
        {
            var files = folder.GetFiles("*.img")
                .AsParallel()
                .Where(x => !parentNode.Files.Exists(y => y.Name == x.Name))
                .Select(file =>
                    {
                        var name = file.Name;

                        var node = new FSFile();
                        node.Size = (int) file.Length;
                        node.Name = name;
                        node.OffsetInFile = 0;
                        node.RealPath = file.FullName;
                        return node;
                    }
                );

            parentNode.AddFiles(files.ToList());
        }

        public void Init(string folder)
        {
            LoadDirectories(this, new DirectoryInfo(folder));
        }
    }
}
