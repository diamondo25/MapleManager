using System;
using System.IO;
using MapleManager.WzTools.Helpers;

namespace MapleManager.WzTools.Objects
{
    public class WzVector2D : PcomObject
    {
        public int X { get; set; }
        public int Y { get; set; }

        public new int this[string key]
        {
            get => (int)Get(key);
            set => Set(key, value);
        }

        public override void Read(BinaryReader reader)
        {
            X = reader.ReadCompressedInt();
            Y = reader.ReadCompressedInt();
        }

        public override void Write(ArchiveWriter writer)
        {
            writer.WriteCompressedInt(X);
            writer.WriteCompressedInt(Y);
        }

        public override void Set(string key, object value)
        {
            if (value is int x)
            {
                switch (key)
                {
                    case "X":
                    case "x": X = x; return;
                    case "Y":
                    case "y": Y = x; return;
                }
            }
            throw new InvalidDataException();
        }

        public override object Get(string key)
        {
            switch (key)
            {
                case "X":
                case "x": return X;
                case "Y":
                case "y": return Y;
            }
            throw new InvalidDataException();
        }

        public override void Rename(string key, string newName)
        {
            throw new NotImplementedException();
        }
    }
}
