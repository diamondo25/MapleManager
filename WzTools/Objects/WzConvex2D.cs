using System.IO;
using MapleManager.WzTools.Helpers;

namespace MapleManager.WzTools.Objects
{
    public class WzConvex2D : WzList
    {

        public override void Read(ArchiveReader reader)
        {
            var childCount = reader.ReadCompressedInt();
            var data = new PcomObject[childCount];
            ChildCount = childCount;

            for (int i = 0; i < childCount; i++)
            {
                var po = LoadFromBlob(reader);
                po.Parent = this;
                data[i] = po;
            }

            _obj = data;
        }

        public override void Write(ArchiveWriter writer)
        {
            writer.WriteCompressedInt(ChildCount);
            var data = (PcomObject[]) _obj;
            for (var i = 0; i < ChildCount; i++)
            {
                WriteToBlob(writer, data[i]);
            }
        }
    }
}
