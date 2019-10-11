using System;
using System.IO;
using MapleManager.WzTools.Helpers;

namespace MapleManager.WzTools.Objects
{
    public class WzSound : PcomObject
    {
        public byte[] Blob = null;
        public override void Read(ArchiveReader reader)
        {
            Blob = reader.ReadBytes(BlobSize);
        }

        public override void Write(ArchiveWriter writer)
        {
            writer.Write(Blob);
        }

        public override void Set(string key, object value)
        {
            return;
        }

        public override object Get(string key)
        {
            return null;
        }

        public override bool HasChild(string key)
        {
            throw new NotImplementedException();
        }
    }
}
