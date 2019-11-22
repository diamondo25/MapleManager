using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using MapleManager.WzTools.Helpers;
using MapleManager.WzTools.Objects;

namespace MapleManager.WzTools
{
    static class ExtraTools
    {
        public static byte[] ApplyStringXor(this byte[] input, bool unicode)
        {
            var length = input.Length;
            if (unicode)
            {
                if ((length % 2) != 0) throw new Exception("Input string is not power of two");
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

        public static int PixelsPerYAxis(this WzPixFormat _this)
        {
            if (_this == WzPixFormat.DXT3 || _this == WzPixFormat.DXT5) return 4;
            return 1;
        }

        public static int BytesPerPixel(this WzPixFormat _this)
        {
            switch ((int)_this & 0xFF)
            {
                case 1: return 2;
                case 2: return 4;
                default: throw new Exception($"Unknow pixformat value: {(int)_this & 0xFF}");
            }
        }

        public static WzPixFormat ToWzPixFormat(this PixelFormat _this)
        {
            switch (_this)
            {
                case PixelFormat.Format32bppRgb: return WzPixFormat.A8R8G8B8;
                case PixelFormat.Format32bppArgb: return WzPixFormat.A8R8G8B8;
                case PixelFormat.Format16bppRgb565: return WzPixFormat.R5G6B5;
                default: throw new Exception($"Unusable PixelFormat: {_this}");
            }
        }
    }
}
