using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MapleManager.Controls;

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
            }
            else
            {
                var type = reader.ReadString(t, 0x1B, 0x73, false);
                switch (type)
                {
                    case "Property":
                        var prop = new WzProperty();
                        prop.Init(reader);
                        return prop;
                    case "List":
                        var list = new WzList();
                        list.Init(reader);
                        return list;
                    case "UOL":
                        var uol = new WzUOL();
                        uol.Init(reader);
                        return uol;
                    case "Shape2D#Vector2D":
                        var vector = new WzVector2D();
                        vector.Init(reader);
                        return vector;
                    case "Shape2D#Convex2D":
                        var convex = new WzConvex2D();
                        convex.Init(reader);
                        return convex;

                    case "Sound_DX8":
                        return new LazyPcomObject<WzSound>(reader);


                    case "Canvas":
                        var image = new WzImage();
                        image.Init(reader);
                        return image;
                    case "Canvas_":
                        return new LazyPcomObject<WzImage>(reader);
                }
            }

            return null;
        }

        public abstract void Init(BinaryReader reader);
        
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
