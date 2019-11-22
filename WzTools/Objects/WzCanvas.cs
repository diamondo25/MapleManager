using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using MapleManager.WzTools.Helpers;
using MapleManager.WzTools.Package;
using OpenVice.Dev;

namespace MapleManager.WzTools.Objects
{
    public class WzCanvas : WzProperty
    {

        // Total image width
        public int Width { get; private set; }
        // Total image height
        public int Height { get; private set; }
        public int MagLevel { get; private set; }

        // Amount of times the Width and Height is divided in chunks like a checker board
        private int TilesPerAxis => (int)Math.Pow(2, MagLevel);

        public WzPixFormat PixFormat { get; private set; }

        public int BytesPerPixel => PixFormat.BytesPerPixel();

        public int CenterX { get; private set; }
        public int CenterY { get; private set; }

        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }

        private int WidthCount { get; set; }
        private int HeightCount { get; set; }

        private WzRawCanvas[] canvases;

        private int TileSize(int v) => (TilesPerAxis + v - 1) / TilesPerAxis;

        // Amount of bytes per line/stride/pitch
        // AKA Stride
        // See https://medium.com/@oleg.shipitko/what-does-stride-mean-in-image-processing-bba158a72bcd
        public int Pitch => TileWidth * BytesPerPixel;

        // DXT3 and DXT5 use 4x4 pixel lookup/compression
        // Others just use 1 pixel in Y axis
        public int ExpectedDataSize
        {
            get
            {
                return Pitch * (TileHeight / (PixFormat.PixelsPerYAxis() * PixFormat.BytesPerPixel()));
            }
        }

        private int GetCanvasIndex(int x, int y) => x / TileWidth + WidthCount * (y / TileHeight);

        public WzRawCanvas GetCanvas(int x, int y)
        {
            if (TileWidth == 0 || TileHeight == 0) return null;
            if (x >= Width || y >= Height) return null;
            if (canvases == null) return null;
            return canvases[GetCanvasIndex(x, y)];
        }

        // SetCanvas builds the canvases array if its not initialized yet
        public void SetCanvas(int x, int y, WzRawCanvas canvas)
        {
            if (TileWidth == 0 && TileHeight == 0)
            {
                if (x != 0 || y != 0)
                {
                    throw new Exception("WzCanvas is not yet initialized!");
                }

                var width = Math.Max(Width, canvas.Width);
                var height = Math.Max(Height, canvas.Height);
                TileWidth = width;
                TileHeight = height;

                WidthCount = (width + Width - 1) / width;
                HeightCount = (height + Height - 1) / height;

                canvases = new WzRawCanvas[WidthCount * HeightCount];
                for (var i = 0; i < canvases.Length; i++)
                {
                    canvases[i] = new WzRawCanvas();
                }
                PixFormat = canvas.PixFormat;
                MagLevel = canvas.MagLevel;
            }
            else
            {
                if (x >= Width || y >= Height) throw new Exception("Image out of bounds");
                if (x % Width != 0 || y % Height != 0) throw new Exception("Image is not a tile boundary");

                if (canvas.Width < TileWidth || canvas.Width < (Width - x)) throw new Exception("Canvas is too wide");
                if (canvas.Height < TileHeight || canvas.Height < (Height - y)) throw new Exception("Canvas is too long");
            }

            canvases[GetCanvasIndex(x, y)] = canvas;
        }

        public void Create(int width, int height, int magLevel, WzPixFormat pixFormat)
        {
            Width = width;
            Height = height;
            TileWidth = 0;
            TileHeight = 0;
            HeightCount = 0;
            WidthCount = 0;
            canvases = new WzRawCanvas[0];
            MagLevel = magLevel;

            if (pixFormat == 0)
            {
                // Initialize main empty canvas
                SetCanvas(0, 0, new WzRawCanvas
                {
                    Width = Width,
                    Height = Height,
                    PixFormat = PixFormat,
                    MagLevel = MagLevel,
                });
            }
            else
            {
                CenterX = Width / 2;
                CenterY = Height / 2;
            }
        }

        private WzCanvas _linkedTile;

        public WzCanvas GetActualImage()
        {
            // 1-by-1 pixel image should be loaded from somewhere else
            if (!(Width == 1 && Height == 1))
            {
                return this;
            }

            if (_linkedTile != null) return _linkedTile;

            if (HasChild("_outlink"))
            {
                // This one is absolute in whole data storage
                var uol = new WzUOL
                {
                    Path = GetString("_outlink"),
                    Parent = Parent,
                    Name = Name,
                    Absolute = true
                };

                _linkedTile = uol.ActualObject() as WzCanvas;

                if (_linkedTile == null)
                {
                    Console.WriteLine("Unable to load {0} image. Cur path: {1}", uol.Path, GetFullPath());
                }
                else
                {
                    return _linkedTile;
                }
            }

            if (HasChild("_inlink"))
            {
                // This one is relative to this img
                PcomObject imgNode = this;
                while (!imgNode.Name.EndsWith(".img")) imgNode = imgNode.Parent;

                var uol = new WzUOL
                {
                    Path = GetString("_inlink"),
                    Parent = imgNode,
                    Name = Name,
                    Absolute = false
                };

                _linkedTile = uol.ActualObject() as WzCanvas;
                if (_linkedTile == null)
                {
                    Console.WriteLine("Unable to load {0} image. Cur path: {1}", uol.Path, GetFullPath());
                }
                else
                {
                    return _linkedTile;
                }
            }


            return _linkedTile ?? this;
        }

        private Image GetImage()
        {
            if (canvases == null) return null;
            if (canvases.Length == 1) return canvases[0].Bitmap;

            // Build bitmap for the user
            Bitmap bmp = new Bitmap(Width, Height);
            using (var gr = Graphics.FromImage(bmp))
            {
                for (int x = 0; x < Width; x += TileWidth)
                {
                    for (int y = 0; y < Height; y += TileHeight)
                    {
                        var rc = GetCanvas(x, y);
                        gr.DrawImageUnscaled(rc.Bitmap, new Point(x, y));
                    }
                }
            }

            return bmp;
        }


        public Image Tile => GetActualImage().GetImage();

        struct RCINFO
        {
            public WzRawCanvas rc;
            public BitmapData data;
            public int length;
            public int pitch;
            public int currentOffset;
        }

        public override void Read(ArchiveReader reader)
        {
            // dont care
            if (reader.ReadByte() != 0) throw new Exception("Expected 0 is not zero");

            if (reader.ReadByte() != 0)
            {
                // Initialize prop
                base.Read(reader);

                if (Get("origin") is WzVector2D origin)
                {
                    CenterX = origin.X;
                    CenterY = origin.Y;
                }
            }
            else
            {
                base._objects = new Dictionary<string, object>();
            }

            Width = reader.ReadCompressedInt();
            if (Width >= 0x10000)
                throw new Exception($"Invalid Width: {Width}");

            Height = reader.ReadCompressedInt();
            if (Height >= 0x10000)
                throw new Exception($"Invalid Height: {Height}");

            PixFormat = (WzPixFormat)reader.ReadCompressedInt();

            if (!(
                PixFormat == WzPixFormat.A4R4G4B4 ||
                PixFormat == WzPixFormat.A8R8G8B8 ||
                PixFormat == WzPixFormat.R5G6B5 ||
                PixFormat == WzPixFormat.DXT3 ||
                PixFormat == WzPixFormat.DXT5
            ))
            {
                throw new Exception($"Invalid PixFormat: {PixFormat:D}");
            }

            MagLevel = reader.ReadCompressedInt();
            if (MagLevel < 0) throw new Exception("MagLevel is < 0");

            // Zeroes
            for (var i = 0; i < 4; i++)
                if (reader.ReadCompressedInt() != 0) throw new Exception("Expected 0 is not zero");


            var dataSize = reader.ReadInt32();

            if (reader.ReadByte() != 0) throw new Exception("Expected 0 is not zero");

            Create(Width, Height, MagLevel, (WzPixFormat)0);

            using (var outputStream = new MemoryStream(ExpectedDataSize))
            using (var inputStream = new MemoryStream(dataSize))
            {
                var isPlainZlibStream = reader.ReadByte() == 0x78;
                reader.BaseStream.Position -= 1;

                var blob = new byte[Math.Min(0x20000, dataSize)];

                if (reader.HasCurrentCrypto && !isPlainZlibStream)
                {
                    // Need to skip 1
                    for (var i = 1; i < dataSize;)
                    {
                        var blobSize = reader.ReadInt32();
                        i += 4;
                        Array.Resize(ref blob, blobSize);
                        reader.Read(blob, 0, blobSize);

                        reader.TryDecryptImage(blob);
                        inputStream.Write(blob, 0, blobSize);
                        i += blobSize;
                    }
                }
                else
                {
                    for (var i = 0; i < dataSize;)
                    {
                        var blobSize = Math.Min(blob.Length, dataSize - i);
                        reader.Read(blob, 0, blobSize);
                        inputStream.Write(blob, 0, blobSize);
                        i += blobSize;
                    }
                }


                inputStream.Position = 2;
                using (var deflate = new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    deflate.CopyTo(outputStream);
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
                    case WzPixFormat.DXT3:
                        arr = DXTDecoder.Decode(
                            TileWidth,
                            TileHeight,
                            outputStream.ToArray(),
                            PixFormat == WzPixFormat.DXT3 ?
                            DXTDecoder.CompressionType.DXT3 :
                            DXTDecoder.CompressionType.DXT5
                        );
                        break;
                    default:
                        arr = outputStream.ToArray();
                        break;
                }


                PixelFormat format;

                switch (PixFormat)
                {
                    case WzPixFormat.R5G6B5:
                        format = PixelFormat.Format16bppRgb565;
                        break;
                    case WzPixFormat.A4R4G4B4:
                    case WzPixFormat.A8R8G8B8:
                    default:
                        format = PixelFormat.Format32bppArgb;
                        break;
                }

                output = new Bitmap(TileWidth, TileHeight, format);
                var rect = new Rectangle(0, 0, output.Width, output.Height);
                var bmpData = output.LockBits(rect, ImageLockMode.ReadWrite, output.PixelFormat);

                var arrRowLength = rect.Width * (Image.GetPixelFormatSize(output.PixelFormat) / 8);
                var ptr = bmpData.Scan0;
                for (var i = 0; i < rect.Height; i++)
                {
                    Marshal.Copy(arr, i * arrRowLength, ptr, arrRowLength);
                    ptr += bmpData.Stride;
                }

                output.UnlockBits(bmpData);

                SetCanvas(0, 0, new WzRawCanvas
                {
                    Bitmap = output,
                    MagLevel = MagLevel,
                });

            }
        }

        public override void Write(ArchiveWriter writer)
        {
            writer.Write((byte)0);
            if (_objects.Count > 0)
            {
                writer.Write((byte)1);
                base.Write(writer);
            }
            else
            {
                writer.Write((byte)0);
            }

            writer.WriteCompressedInt(Width);
            writer.WriteCompressedInt(Height);
            writer.WriteCompressedInt((int)PixFormat);
            writer.WriteCompressedInt(MagLevel);

            for (var i = 0; i < 4; i++)
                writer.WriteCompressedInt((int)0);

            int dataSize = 0;
            // Data Size
            writer.Write(dataSize);
            if (dataSize > 0)
            {
                // CWzCanvas::SerializeData
                writer.Write((byte)0);

                for (int y = 0; y < Height; y += TileHeight)
                {
                    RCINFO[] rcs = new RCINFO[WidthCount];
                    int i = 0;
                    for (int x = 0; x < Width; x += TileWidth)
                    {
                        var canvas = GetCanvas(x, y);
                        var rcinfo = new RCINFO
                        {
                            currentOffset = 0,
                            rc = canvas,

                        };

                        canvas.LockAddress(out rcinfo.pitch, out rcinfo.data);

                    }
                }

            }
        }

        private void FlushCanvases(Action<byte[]> onChunkReady)
        {

            for (int y = 0; y < Height; y += TileHeight)
            {
                RCINFO[] rcs = new RCINFO[WidthCount];
                int i = 0;
                for (int x = 0; x < Width; x += TileWidth)
                {
                    var canvas = GetCanvas(x, y);
                    var rcinfo = new RCINFO
                    {
                        currentOffset = 0,
                        rc = canvas,
                    };

                    canvas.LockAddress(out rcinfo.pitch, out rcinfo.data);
                    rcinfo.length = Math.Min(Width - x, TileWidth) * canvas.PixFormat.BytesPerPixel();
                }

                var tileHeightSize = TileSize(Math.Min(TileHeight, Height - y));
                var bytesPerWrite = PixFormat.PixelsPerYAxis();
                // We need to correct for DXT3/DXT5 images having multiple pixels in Y axis per written bytes
                // Additionally, MagLevel also applies to the canvases, so need to keep that 

            }

        }

        private byte[] ARGB16toARGB32(MemoryStream input, long inputLen)
        {
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

        private byte[] ARGB32toARGB16(MemoryStream input, long inputLen)
        {
            var output = new MemoryStream((int)(inputLen / 2));
            for (var i = 0; i < inputLen; i++)
            {
                var a = input.ReadByte();
                var b = input.ReadByte();

                byte c = 0;
                c |= (byte)((a / 0x11) << 4);
                c |= (byte)((b / 0x11) << 0);

                output.WriteByte(c);
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
