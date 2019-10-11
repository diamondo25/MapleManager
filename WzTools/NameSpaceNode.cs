using System;

namespace MapleManager.WzTools
{
    public class NameSpaceNode : INameSpaceNode
    {
        public int OffsetInFile { get; set; }
        public int BeginParsePos { get; set; }
        public int EndParsePos { get; set; }
        public int Checksum { get; set; }
        public int Size { get; set; }
        public string Name { get; set; }

        public NameSpaceNode Parent { get; set; }
        

        public virtual object GetParent() => Parent;

        public virtual object GetChild(string key) => throw new NotImplementedException();
        public virtual bool HasChild(string key) => GetChild(key) != null;
    }
}