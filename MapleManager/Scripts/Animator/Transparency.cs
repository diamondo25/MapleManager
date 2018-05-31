// Converted through converter.telerik.com
// Original source: https://web.archive.org/web/20110917162002/http://www.nathansokalski.com/code/TransparencyClass.aspx
// Hint by: https://github.com/DataDink/Bumpkit/issues/11#issuecomment-292034801 

using System.Drawing;
using System.Drawing.Imaging;

namespace NathanSokalski
{
    public class Transparency
    {
        public static Bitmap MakeTransparent(Bitmap oldbmp, Color transparentcolor)
        {
            Bitmap bmp = new Bitmap(oldbmp.Width, oldbmp.Height, PixelFormat.Format8bppIndexed);
            ColorPalette palette = bmp.Palette;
            byte nextindex = 0;
            BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, oldbmp.Width, oldbmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            int index;
            for (int i = 0; i <= 255; i++)
                palette.Entries[i] = Color.Empty;
            for (int y = 0; y <= oldbmp.Height - 1; y++)
            {
                for (int x = 0; x <= oldbmp.Width - 1; x++)
                {
                    // Get the palette index of the current pixel
                    index = Transparency.InPalette(palette, nextindex - 1, oldbmp.GetPixel(x, y));
                    // If the color is not in the palette, add it at the next unused index
                    if (index == -1)
                    {
                        palette.Entries[nextindex] = oldbmp.GetPixel(x, y);
                        index = nextindex;
                        nextindex += System.Convert.ToByte(1);
                    }
                    // Set the pixel to the proper index
                    System.Runtime.InteropServices.Marshal.WriteByte(bmpdata.Scan0, y * bmpdata.Stride + x, System.Convert.ToByte(index));
                }
            }
            bmp.UnlockBits(bmpdata);
            // If the specified transparent color is included in the palette, change that color to transparent
            if (transparentcolor != Color.Empty && Transparency.InPalette(palette, nextindex - System.Convert.ToByte(1), transparentcolor) != -1)
                palette.Entries[Transparency.InPalette(palette, nextindex - System.Convert.ToByte(1), transparentcolor)] = Color.FromArgb(0, 0, 0, 0);
            bmp.Palette = palette;
            return bmp;
        }

        // Returns number of colors in bitmap
        public static int ColorCount(Bitmap bmp)
        {
            System.Collections.ObjectModel.Collection<int> palette = new System.Collections.ObjectModel.Collection<int>();
            int currcolor;
            for (int y = 0; y <= bmp.Height - 1; y++)
            {
                for (int x = 0; x <= bmp.Width - 1; x++)
                {
                    currcolor = bmp.GetPixel(x, y).ToArgb();
                    if (!palette.Contains(currcolor))
                        palette.Add(currcolor);
                }
            }
            return palette.Count;
        }

        // Returns index of color in palette or -1
        private static int InPalette(ColorPalette palette, int maxindex, Color colortofind)
        {
            for (int i = 0; i <= maxindex; i++)
            {
                if (palette.Entries[i].ToArgb() == colortofind.ToArgb())
                    return System.Convert.ToInt32(i);
            }
            return -1;
        }
    }
}
