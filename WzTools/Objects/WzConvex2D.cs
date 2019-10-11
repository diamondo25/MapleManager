using System.IO;
using MapleManager.WzTools.Helpers;

namespace MapleManager.WzTools.Objects
{
    public class WzConvex2D : WzList
    {

        public override void Read(ArchiveReader reader)
        {
            var childCount = reader.ReadCompressedInt();
            var data = new object[childCount];

            for (int i = 0; i < childCount; i++)
            {
                data[i] = LoadFromBlob(reader);
                if (data[i] is PcomObject po) po.Parent = this;
            }

            _obj = data;
        }
    }
}
