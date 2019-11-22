using System;

namespace MapleManager.WzTools
{
    public class NameSpaceNode : INameSpaceNode, IDisposable
    {
        public int OffsetInFile { get; set; }
        public int BeginParsePos { get; set; }
        public int EndParsePos { get; set; }
        public int Checksum { get; set; }
        public int Size { get; set; }
        public string Name { get; set; }

        public NameSpaceNode Parent { get; set; }

        public string GetName() => Name;

        public virtual object GetParent() => Parent;

        public virtual object GetChild(string key) => throw new NotImplementedException();
        public virtual bool HasChild(string key) => GetChild(key) != null;

        public string NodePath
        {
            get
            {
                NameSpaceNode tmp = Parent;
                string ret = this.Name;
                while (tmp != null)
                {
                    ret = tmp.Name + "/" + ret;
                    tmp = tmp.Parent;
                }

                return ret;
            }
        }

        public virtual void Dispose()
        {
        }
    }
}