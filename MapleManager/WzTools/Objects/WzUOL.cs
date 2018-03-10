using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleManager.WzTools.Objects
{
    public  class WzUOL : PcomObject
    {
        public string Path { get; set; }

        public override void Init(BinaryReader reader)
        {
            Debug.Assert(reader.ReadByte() == 0);
            Path = reader.ReadString(1, 0, false, 0);
        }

        public override void Set(string key, object value)
        {
            if (value is string x) Path = x;
            else throw new InvalidDataException();
        }

        public override object Get(string key)
        {
            var obj = ActualObject();
            if (obj is PcomObject po) return po.Get(key);
            return obj;
        }

        public override void Rename(string key, string newName)
        {
            throw new NotImplementedException();
        }

        public string ActualPath()
        {
            return System.IO.Path.GetFullPath(System.IO.Path.Combine("Z:/" + this.Parent.GetFullPath().Replace('/', '\\'), Path.Replace('/', '\\'))).Replace('\\', '/').Substring(3);
        }

        public object ActualObject(bool recursiveResolve = false)
        {
            // Go backwards
            var elements = Path.Split('/');
            PcomObject curNode = this.Parent;
            foreach (var element in elements)
            {
                if (element == "..") curNode = curNode.Parent;
                else if (element == ".") continue;
                else curNode = curNode.Get(element) as PcomObject;
            }

            if (recursiveResolve && curNode is WzUOL secondUol)
                curNode = secondUol.ActualObject() as PcomObject;

            return curNode;
        }
    }
}
