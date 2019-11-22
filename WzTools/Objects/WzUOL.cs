using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MapleManager.WzTools.Helpers;

namespace MapleManager.WzTools.Objects
{
    public class WzUOL : PcomObject
    {
        public string Path { get; set; }
        public bool Absolute { get; set; } = false;

        public override void Read(ArchiveReader reader)
        {
            if (reader.ReadByte() != 0) throw new Exception("Expected 0 is not zero");
            Path = reader.ReadString(1, 0);
        }

        public override void Write(ArchiveWriter writer)
        {
            writer.Write((byte)0);
            writer.Write(Path, 1, 0);
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

        public string ActualPath()
        {
            var stk = new Stack<string>();
            var elems = (this.Parent.GetFullPath() + "/" + Path).Split('\\', '/');
            foreach (var elem in elems)
            {
                if (elem == "..") stk.Pop();
                else if (elem == ".") continue;
                else stk.Push(elem);
            }
           
            return string.Join("/", stk.Reverse());
        }

        public object ActualObject(bool recursiveResolve = false)
        {
            // Go backwards
            var elements = Path.Split('/');
            object curNode = this.Parent;

            if (Absolute)
            {
                // Lets go all the way back to the root of the filesystem
                while (true)
                {
                    INameSpaceNode lastNonNullNode = (INameSpaceNode)curNode;

                    // PcomObject -> PcomObject -> WzFileProperty -> NameSpaceNode -> ... -> null

                    curNode = (INameSpaceNode)lastNonNullNode.GetParent();
                    if (curNode == null)
                    {
                        curNode = lastNonNullNode;
                        break;
                    }
                }
            }

            foreach (var element in elements)
            {
                if (element == ".") continue;
                if (element == "..")
                {
                    curNode = ((INameSpaceNode)curNode).GetParent();
                }
                else
                {
                    if (curNode is INameSpaceNode cn)
                    {
                        curNode = cn.GetChild(element);
                    }
                    else if (curNode == null)
                        return null;
                    else
                        throw new Exception($"Unable to traverse {curNode?.GetType().FullName ?? "null"} ({curNode}) at {element}");


                    if (curNode == null)
                    {
                        return null;
                    }
                }
            }

            if (recursiveResolve && curNode is WzUOL secondUol)
                curNode = secondUol.ActualObject();

            return curNode;
        }

        public override bool HasChild(string key) => (ActualObject(true) as INameSpaceNode)?.HasChild(key) ?? false;
    }
}
