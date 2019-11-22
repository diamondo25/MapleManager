using System;
using System.Drawing;
using System.Drawing.Imaging;
using MapleManager.WzTools.Helpers;

namespace MapleManager.WzTools.Objects
{
    public class WzRawCanvas
    {
        private Bitmap _bitmap;

        public Bitmap Bitmap
        {
            get => _bitmap;
            set
            {
                _bitmap = value;
                Width = _bitmap.Width;
                Height = _bitmap.Height;
                PixFormat = Bitmap.PixelFormat.ToWzPixFormat();
            }
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public int MagLevel { get; set; }

        public int Pitch => Width * PixFormat.BytesPerPixel() * (int)Math.Pow(2, PixFormat.PixelsPerYAxis());

        public WzPixFormat PixFormat { get; set; }

        public void LockAddress(out int pitch, out BitmapData address)
        {
            pitch = Pitch;
            address = Bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, Bitmap.PixelFormat);
        }

        public void UnlockAddress(in BitmapData address)
        {
            Bitmap.UnlockBits(address);
        }
    }
}