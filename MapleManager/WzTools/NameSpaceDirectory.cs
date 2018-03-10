using System;
using System.Collections.Generic;

namespace MapleManager.WzTools
{
    class NameSpaceDirectory : NameSpaceNode
    {
        public List<NameSpaceDirectory> SubDirectories { get; set; } = new List<NameSpaceDirectory>();
        public List<NameSpaceFile> Files { get; set; } = new List<NameSpaceFile>();

        public void Add(NameSpaceNode node)
        {
            if (node is NameSpaceDirectory dir) SubDirectories.Add(dir);
            else if (node is NameSpaceFile file) Files.Add(file);
            else throw new Exception("Invalid PackageNode");
        }

        public override string ToString()
        {
            return "Dir: " + Name;
        }
    }
}
