using System;
using System.Drawing;
using MapleManager.WzTools.Objects;

namespace MapleManager.Scripts.Animator
{
    internal class FrameInfo : IDisposable
    {
        public int OffsetX, OffsetY, Width, Height;
        public WzImage Data;
        public Image Tile;
        public int delay;
        public bool CustomImage;

        internal FrameInfo Clone()
        {
            return new FrameInfo()
            {
                Data = Data,
                Tile = Tile,
                OffsetX = OffsetX,
                OffsetY = OffsetY,
                Width = Width,
                Height = Height,
                delay = delay,
                CustomImage = CustomImage,
            };
        }

        public void Dispose()
        {
            if (CustomImage && Tile != null) Tile.Dispose();
        }
    }
}