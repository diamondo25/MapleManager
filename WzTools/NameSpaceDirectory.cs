using System;
using System.Collections.Generic;
using System.Linq;

namespace MapleManager.WzTools
{
    class NameSpaceDirectory : NameSpaceNode
    {
        public List<NameSpaceDirectory> SubDirectories { get; set; } = new List<NameSpaceDirectory>();
        public List<NameSpaceFile> Files { get; set; } = new List<NameSpaceFile>();

        public void Add(NameSpaceNode node)
        {
            node.Parent = this;
            if (node is NameSpaceDirectory dir) SubDirectories.Add(dir);
            else if (node is NameSpaceFile file) Files.Add(file);
            else throw new Exception("Invalid NameSpaceNode");
        }

        public void AddDirectories(params NameSpaceDirectory[] directories) => AddDirectories(directories.AsEnumerable());

        public void AddDirectories(IEnumerable<NameSpaceDirectory> directories)
        {
            foreach (var dir in directories) Add(dir);
        }

        public void AddFiles(params NameSpaceFile[] files) => AddFiles(files.AsEnumerable());

        public void AddFiles(IEnumerable<NameSpaceFile> files)
        {
            foreach (var file in files) Add(file);
        }

        public override string ToString()
        {
            return "Dir: " + Name;
        }

        public override object GetChild(string key)
        {
            object ret = null;
            if (ret == null) ret = Files.Find(x => x.Name.ToLower() == key.ToLower());
            if (ret == null) ret = Files.Find(x => x.Name.ToLower() == (key + ".img").ToLower());
            if (ret == null) ret = SubDirectories.Find(x => x.Name.ToLower() == key.ToLower());

            if (ret is NameSpaceFile nsf) ret = nsf.Object;
            return ret;
        }
    }
}