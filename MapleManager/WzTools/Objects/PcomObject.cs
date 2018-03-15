using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MapleManager.Controls;
using MapleManager.WzTools.Helpers;

namespace MapleManager.WzTools.Objects
{
    public abstract class PcomObject
    {
        public PcomObject Parent = null;
        public WZTreeNode TreeNode = null;
        public string Name { get; set; }
        
        public PcomObject this[string key]
        {
            get => Get(key) as PcomObject;
            set => Set(key, value);
        }


        public static PcomObject LoadFromBlob(BinaryReader reader, int blobSize = 0)
        {
            var t = reader.ReadByte();
            if (t == '#')
            {
                throw new NotImplementedException("ASCII file");
                // TODO: Make WzProperty from ASCII file
            }

            if (t == 'A')
            {
                return null;
            }

            var type = reader.ReadString(t, 0x1B, 0x73, 0);
            PcomObject obj;
            switch (type)
            {
                case "Property": obj = new WzProperty(); break;
                case "List": obj = new WzList(); break;
                case "UOL": obj = new WzUOL(); break;
                case "Shape2D#Vector2D": obj = new WzVector2D(); break;
                case "Shape2D#Convex2D": obj = new WzConvex2D(); break;
                case "Sound_DX8": obj = new LazyPcomObject<WzSound>(reader); break;
                case "Canvas": obj = new WzImage(); break;
                default:
                    Console.WriteLine("Don't know how to read this proptype: {0}", type);
                    return null;
            }

            obj.Read(reader);
            return obj;
        }

        public static void WriteToBlob(ArchiveWriter writer, PcomObject obj)
        {
            void writeType(string type) => writer.Write(type, 0x1B, 0x73);
            switch (obj)
            {
                case WzConvex2D x: writeType("Shape2D#Convex2D"); break;
                case WzImage x: writeType("Canvas"); break;
                case WzProperty x: writeType("Property"); break;
                case WzList x: writeType("List"); break;
                case WzUOL x: writeType("UOL"); break;
                case WzVector2D x: writeType("Shape2D#Vector2D"); break;
                case LazyPcomObject<WzSound> x: writeType("Sound_DX8"); break;
                default: throw new NotImplementedException(obj.ToString());
            }

            obj.Write(writer);
        }

        public abstract void Read(BinaryReader reader);

        public abstract void Write(ArchiveWriter writer);

        public abstract void Set(string key, object value);
        public abstract object Get(string key);
        public abstract void Rename(string key, string newName);

        public string GetFullPath()
        {
            string ret = Name;
            var curParent = Parent;
            while (curParent != null)
            {
                ret = curParent.Name + "/" + ret;
                curParent = curParent.Parent;
            }

            return ret;
        }

        public override string ToString()
        {
            return base.ToString() + ", Path: " + GetFullPath();
        }

    }
}
