using System;
using System.Diagnostics;
using System.Globalization;
using MapleManager.WzTools.Objects;

namespace MapleManager.WzTools
{
    static class ExtraTools
    {
        public static int IMGNameToID(this WzProperty property)
        {
            return int.Parse(property.Name.Replace(".img", ""), NumberStyles.Number);
        }

        public static byte[] ApplyStringXor(this byte[] input, bool unicode)
        {
            var length = input.Length;
            if (unicode)
            {
                Debug.Assert((length % 2) == 0);
            }

            var bytes = new byte[length];
            Buffer.BlockCopy(input, 0, bytes, 0, length);
            if (unicode)
            {
                ushort mask = 0xAAAA;
                for (var i = 0; i < length; i += 2)
                {
                    bytes[i + 0] ^= (byte)(mask & 0xFF);
                    bytes[i + 1] ^= (byte)((mask >> 8) & 0xFF);
                    mask++;
                }
            }
            else
            {
                byte mask = 0xAA;

                for (var i = 0; i < length; i++)
                {
                    bytes[i] ^= mask;
                    mask++;
                }
            }

            return bytes;
        }
    }
}
