using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using OpenVice.Dev;

namespace MapleManager.WzTools.Objects
{
    public class WzImage : WzProperty
    {
        public enum WzPixFormat
        {
            A4R4G4B4 = 1,
            A8R8G8B8 = 2,
            R5G6B5 = 0x201,
            DXT3 = 0x402,
            DXT5 = 0x802,
            UNKNOWN = 0
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int MagLevel { get; private set; }
        public WzPixFormat PixFormat { get; private set; }

        private int BytesForFormat(WzPixFormat format)
        {
            switch (format)
            {
                case WzPixFormat.A4R4G4B4: return 2;
                case WzPixFormat.A8R8G8B8: return 4;
                case WzPixFormat.R5G6B5: return 2;
                default: return -1;
            }
        }

        public int TileWidth => ((1 << MagLevel) + Width - 1) >> MagLevel;
        public int TileHeight => ((1 << MagLevel) + Height - 1) >> MagLevel;

        private Image _tile;
        private Image _linkedTile;

        public Image Tile
        {
            get
            {
                if (Width == 1 && Height == 1)
                {
                    if (_linkedTile != null) return _linkedTile;
                    if (HasKey("_outlink"))
                    {
                        // This one is absolute in whole data storage
                        var uol = new WzUOL()
                        {
                            Path = (string)this["_outlink"],
                            Parent = Parent,
                            Name = Name,
                            TreeNode = TreeNode,
                            Absolute = true
                        };
                        _linkedTile = (uol.ActualObject() as WzImage)?.Tile;
                        if (_linkedTile == null)
                        {
                            Console.WriteLine("Unable to load {0} image. Cur path: {1}", uol.Path, GetFullPath());
                            _linkedTile = _tile;
                        }

                        return _linkedTile;
                    }

                    if (HasKey("_inlink"))
                    {
                        // This one is relative to this img
                        PcomObject imgNode = this;
                        while (!imgNode.Name.EndsWith(".img")) imgNode = imgNode.Parent;

                        var uol = new WzUOL()
                        {
                            Path = (string)this["_inlink"],
                            Parent = imgNode,
                            Name = Name,
                            TreeNode = TreeNode,
                            Absolute = false
                        };
                        _linkedTile = (uol.ActualObject() as WzImage)?.Tile;
                        if (_linkedTile == null)
                        {
                            Console.WriteLine("Unable to load {0} image. Cur path: {1}", uol.Path, GetFullPath());
                            _linkedTile = _tile;
                        }

                        return _linkedTile;
                    }
                }
                return _tile;
            }
            private set
            {
                _tile = value;
            }
        }

        // (1 << x) === Math.pow(2, x)

        // AKA Stride
        public int Pitch => TileWidth << (byte)((int)PixFormat & 0xFF);

        public int ExpectedDataSize => Pitch * (TileHeight /
                                                ((PixFormat != WzPixFormat.DXT3 ? 0 : 3) + 1));

        public override void Read(BinaryReader reader)
        {
            // dont care
            Debug.Assert(reader.ReadByte() == 0); // just zero

            if (reader.ReadByte() != 0)
            {
                // Initialize prop
                base.Read(reader);
            }
            else
            {
                base._objects = new Dictionary<string, object>();
            }

            Width = reader.ReadCompressedInt();
            Debug.Assert(Width < 0x10000);
            Height = reader.ReadCompressedInt();
            Debug.Assert(Height < 0x10000);

            PixFormat = (WzPixFormat)reader.ReadCompressedInt();

            Debug.Assert(
                PixFormat == WzPixFormat.A4R4G4B4 ||
                PixFormat == WzPixFormat.A8R8G8B8 ||
                PixFormat == WzPixFormat.R5G6B5 ||
                PixFormat == WzPixFormat.DXT3 ||
                PixFormat == WzPixFormat.DXT5
            );

            MagLevel = reader.ReadCompressedInt();
            Debug.Assert(MagLevel >= 0);

            // Zeroes
            for (var i = 0; i < 4; i++)
                Debug.Assert(reader.ReadCompressedInt() == 0);


            var dataSize = reader.ReadInt32();

            Debug.Assert(reader.ReadByte() == 0); // just zero

            using (var outputStream = new MemoryStream(ExpectedDataSize))
            using (var inputStream = new MemoryStream(dataSize))
            {
                // Skip header
                reader.BaseStream.Position += 2;

                var blob = new byte[Math.Min(0x2000, dataSize)];
                for (var i = 0; i < dataSize;)
                {
                    var blobSize = Math.Min(blob.Length, dataSize - i);
                    reader.Read(blob, 0, blobSize);
                    inputStream.Write(blob, 0, blobSize);
                    i += blobSize;
                }

                inputStream.Position = 0;
                using (var gzip = new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    gzip.CopyTo(outputStream);
                }


                var uncompressedSize = outputStream.Length;
                outputStream.Position = 0;

                Bitmap output = null;

                byte[] arr;
                switch (PixFormat)
                {
                    case WzPixFormat.A4R4G4B4:
                        arr = ARGB16toARGB32(outputStream, uncompressedSize);
                        break;
                    case WzPixFormat.DXT5:
                        arr = DXTDecoder.Decode(TileWidth, TileHeight, outputStream.ToArray(), DXTDecoder.CompressionType.DXT5);
                        break;
                    case WzPixFormat.DXT3:
                        arr = DXTDecoder.Decode(TileWidth, TileHeight, outputStream.ToArray(), DXTDecoder.CompressionType.DXT3);
                        break;
                    default:
                        arr = outputStream.ToArray();
                        break;
                }


                PixelFormat format;

                // TODO: Figure out why some images are not transparent
                switch (PixFormat)
                {
                    case WzPixFormat.R5G6B5: format = PixelFormat.Format16bppRgb565; break;
                    case WzPixFormat.A4R4G4B4:
                    case WzPixFormat.A8R8G8B8:
                    default: format = PixelFormat.Format32bppArgb; break;
                }

                output = new Bitmap(TileWidth, TileHeight, format);
                var rect = new Rectangle(0, 0, output.Width, output.Height);
                var bmpData = output.LockBits(rect, ImageLockMode.ReadWrite, output.PixelFormat);

                // Row-by-row copy
                var arrRowLength = rect.Width * Image.GetPixelFormatSize(output.PixelFormat) / 8;
                var ptr = bmpData.Scan0;
                for (var i = 0; i < rect.Height; i++)
                {
                    Marshal.Copy(arr, i * arrRowLength, ptr, arrRowLength);
                    ptr += bmpData.Stride;
                }

                output.UnlockBits(bmpData);

                if (PixFormat == WzPixFormat.DXT3 || PixFormat == WzPixFormat.DXT5)
                {
                    //output.Save(@"C:\Users\Erwin\Documents\visual studio 2017\Projects\MapleManager\" + PixFormat + ".png", ImageFormat.Png);
                }
                Tile = output;
            }

            // TODO: Check for compression bytes?
        }

        private byte[] ARGB16toARGB32(MemoryStream input, long inputLen)
        {
            // 16 bit colors to 32 bit colors...
            // every 4 bits multiply by 256/(1 << 4) = 64.


            var output = new MemoryStream((int)(inputLen * 2));
            for (var i = 0; i < inputLen; i++)
            {
                var a = input.ReadByte();

                var c_g = (byte)((a & 0x0F) * 0x11);
                var c_b = (byte)((a >> 4) * 0x11);
                output.WriteByte(c_g);
                output.WriteByte(c_b);
            }
            return output.ToArray();
        }

        private MemoryStream RGB565toARGB32(MemoryStream input, long inputLen)
        {
            // 16 bit colors to 32 bit colors...
            // 5 bit value = 256 / 32 = 8
            // 6 bit value = 256 / 64 = 4


            // 1011100101111100
            // |   |, shift right 3
            // 1011100101111100
            //      | xx |, byte 1 & 0x7 << 5 | byte 2 >> 5
            // 1011100101111100
            //            |   |, byte 2 & 0x

            const byte bit6_mask = 0x3F;
            const byte bit5_mask = 0x1F;

            var output = new MemoryStream((int)(inputLen * 2));
            for (var i = 0; i < inputLen; i++)
            {
                var low = input.ReadByte();
                var high = input.ReadByte();

                byte r = (byte)((low >> 3) & bit5_mask);
                r *= 8;

                byte g = (byte)((((low & 0x7) << 5 | high) >> 5) & bit6_mask);
                g *= 4;

                byte b = (byte)(high & bit5_mask);
                b *= 8;

                output.WriteByte(0xFF); // full alpha
                output.WriteByte(r);
                output.WriteByte(g);
                output.WriteByte(b);
            }

            return output;
        }
    }
}
