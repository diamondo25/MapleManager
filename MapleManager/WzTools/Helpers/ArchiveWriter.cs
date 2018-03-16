using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MapleManager.WzTools.Helpers
{
    public class ArchiveWriter : BinaryWriter
    {
        private Dictionary<string, int> _stringPool = new Dictionary<string, int>();

        public ArchiveWriter(Stream output) : base(output)
        {
        }

        public void Write(string value, byte existingId, byte newId)
        {
            if (_stringPool.TryGetValue(value, out var offset))
            {
                Write((byte)existingId);
                this.WriteCompressedInt(offset);
            }
            else
            {
                Write((byte)newId);
                var bytes = EncodeString(value, out var unicode);
                var actualLength = bytes.Length;
                if (unicode)
                {
                    actualLength /= 2;
                    if (actualLength >= 127)
                    {
                        Write((sbyte) 127);
                        Write((int) actualLength);
                    }
                    else
                    {
                        Write((sbyte) actualLength);
                    }
                }
                else
                {
                    if (actualLength >= 127)
                    {
                        Write((sbyte)-128);
                        Write((int)actualLength);
                    }
                    else
                    {
                        Write((sbyte)-actualLength);
                    }
                }

                Write(bytes);
            }
        }

        private byte[] EncodeString(string value, out bool unicode)
        {
            unicode = value.Any(x => x >= 0x80);

            byte[] bytes;

            if (unicode)
                bytes = Encoding.Unicode.GetBytes(value);
            else
                bytes = Encoding.ASCII.GetBytes(value);

            return bytes.ApplyStringXor(unicode);
        }
    }
}
