using System;
using System.IO;
using System.Text;
using MapleManager.WzTools.Helpers;

namespace MapleManager.WzTools.Objects
{
    public abstract class PcomObject : INameSpaceNode
    {
        public PcomObject Parent = null;
        
        public string Name { get; set; }

        public int BlobSize { get; set; }

        public PcomObject this[string key]
        {
            get => Get(key) as PcomObject;
            set => Set(key, value);
        }

        public static void PrepareEncryption(ArchiveReader reader)
        {
            var start = reader.BaseStream.Position;
            var t = reader.ReadByte();
            if (t == 'A' || t == '#')
            {
                // not needed
            }
            else
            {
                string type = reader.ReadString(t, 0x1B, 0x73, 0);
                switch (type)
                {
                    // Only a Property is valid on this level
                    case "Property":
                        /*
                    case "List": 
                    case "UOL": 
                    case "Shape2D#Vector2D": 
                    case "Shape2D#Convex2D": 
                    case "Sound_DX8": 
                    case "Canvas": 
                    */
                        reader.LockCurrentEncryption();
                        break;
                        
                    default:
                        throw new Exception($"Don't know how to read this proptype: {type}");
                }
            }
            reader.BaseStream.Position = start;
        }

        public static PcomObject LoadFromBlob(ArchiveReader reader, int blobSize = 0, string name = null, bool isFileProp = false)
        {
            var start = reader.BaseStream.Position;
            var t = reader.ReadByte();
            var type = "";

            if (t == 'A')
            {
                return null;
            }

            PcomObject obj;
            if (t == '#')
            {
                type = reader.ReadAndReturn<string>(() =>
                {
                    // Try to read #Property
                    var sr = new StringReader(Encoding.ASCII.GetString(reader.ReadBytes(Math.Min(100, blobSize))));
                    return sr.ReadLine();
                });

                reader.BaseStream.Position += type.Length + 2; // \r\n
            }
            else
            {
                type = reader.ReadString(t, 0x1B, 0x73, 0);
            }

            switch (type)
            {
                case "Property":
                    obj = isFileProp ? new WzFileProperty() : new WzProperty(); 
                    break;
                case "List": obj = new WzList(); break;
                case "UOL": obj = new WzUOL(); break;
                case "Shape2D#Vector2D": obj = new WzVector2D(); break;
                case "Shape2D#Convex2D": obj = new WzConvex2D(); break;
                case "Sound_DX8": obj = new WzSound(); break;
                case "Canvas": obj = new WzImage(); break;
                default:
                    Console.WriteLine("Don't know how to read this proptype: {0}", type);
                    return null;
            }

            if (t == '#' && !(obj is WzProperty))
            {
                // Unable to handle non-wzprops???
                return null;
            }

            obj.BlobSize = blobSize - (int)(reader.BaseStream.Position - start);
            obj.Name = name;
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
                case WzSound x: writeType("Sound_DX8"); break;
                default: throw new NotImplementedException(obj.ToString());
            }

            obj.Write(writer);
        }

        public abstract void Read(ArchiveReader reader);

        public abstract void Write(ArchiveWriter writer);

        public abstract void Set(string key, object value);
        public abstract object Get(string key);

        public virtual bool HasChild(string key) => Get(key) != null;

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

        public virtual object GetParent() => Parent;

        public object GetChild(string key) => Get(key);

    }
}
