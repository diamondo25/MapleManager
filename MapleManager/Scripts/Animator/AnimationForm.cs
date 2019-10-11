
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AnimatedGif;
using MapleManager.WzTools.Objects;

namespace MapleManager.Scripts.Animator
{
    public partial class AnimationForm : Form
    {
        public AnimationForm()
        {
            InitializeComponent();
            ResizeRedraw = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer,
                true
            );

            vScrollBar1.Value = vScrollBar1.Maximum / 2;
            hScrollBar1.Value = hScrollBar1.Maximum / 2;
        }

        private Stopwatch sw = new Stopwatch();

        private List<FrameInfo> frames = new List<FrameInfo>();
        private int currentFrame = 0;
        private int totalDuration = 0;
        private int currentStart = 0;
        private int currentEnd = 0;

        private bool paused = false;

        internal void LoadFrames(List<FrameInfo> pictureFrames)
        {
            sw.Reset();

            var tmp = this.frames;
            for (var i = 0; i < tmp.Count; i++)
            {
                tmp[i].Dispose();
            }

            this.frames = pictureFrames;

            totalDuration = pictureFrames.Sum(x => x.delay);
            currentStart = 0;
            currentEnd = this.frames[0].delay;
            tbFrame.Minimum = 0;
            tbFrame.Maximum = this.frames.Count - 1;
            SetFrame(0);
            sw.Start();
        }


        private void SetFrameByTime(long currentTime)
        {
            var x = currentTime % totalDuration;

            if (x >= currentStart && x <= currentEnd) return;

            // Time went backwards.

            int y = 0;
            for (var i = 0; i < frames.Count; i++)
            {
                y += frames[i].delay;
                if (y > x)
                {
                    y -= frames[i].delay;
                    currentStart = y;
                    SetFrame(i);
                    break;
                }
            }
        }

        private const double TopOffsetPercentage = 0.75;

        private void SetFrame(int idx)
        {
            if (idx >= frames.Count) idx = 0;
            var frame = frames[idx];
            currentFrame = idx;
            tbFrame.Value = currentFrame;

            currentEnd = currentStart + frame.delay;
            this.Text = string.Format("Frame {0}, x {1} y {2} w {3} h {4}", currentFrame, frame.OffsetX, frame.OffsetY, frame.Width, frame.Height);
            if (paused) this.Text += " - PAUSED";

            this.Invalidate();
        }

        private int centerX;
        private int centerY;

        Point correctPoint(int x, int y)
        {
            return new Point(centerX + x, centerY + y);
        }

        private void AnimationForm_Paint(object sender, PaintEventArgs e)
        {
            var penBlack = new Pen(Color.Black);
            centerY = (int)(Height * TopOffsetPercentage);
            centerX = Width / 2;

            centerX += (hScrollBar1.Maximum / 2) - hScrollBar1.Value;
            centerY += (vScrollBar1.Maximum / 2) - vScrollBar1.Value;
            var graphics = e.Graphics;
            
            graphics.DrawLine(penBlack, 0, centerY, Width, centerY);

            graphics.DrawLine(penBlack, centerX, 0, centerX, Height);

            if (frames.Count > currentFrame)
            {
                var frame = frames[currentFrame];
                graphics.DrawImageUnscaled(frame.Tile, correctPoint(-frame.OffsetX, -frame.OffsetY));
                var head = frame.Data["head"] as WzVector2D;
                var lt = frame.Data["lt"] as WzVector2D;
                var rb = frame.Data["rb"] as WzVector2D;

                if (head != null)
                {
                    var penHead = new Pen(Color.Red, 3);
                    graphics.DrawLine(
                        penHead,
                        correctPoint(head.X + 3, head.Y + 3),
                        correctPoint(head.X - 3, head.Y - 3)
                    );

                    graphics.DrawLine(
                        penHead,
                        correctPoint(head.X - 3, head.Y + 3),
                        correctPoint(head.X + 3, head.Y - 3)
                    );
                }

                if (lt != null && rb != null)
                {
                    var penBorder = new Pen(Color.Blue);
                    graphics.DrawLines(
                        penBorder,
                        new []
                        {
                            correctPoint(lt.X, lt.Y),
                            correctPoint(rb.X, lt.Y),
                            correctPoint(rb.X, rb.Y),
                            correctPoint(lt.X, rb.Y),
                            correctPoint(lt.X, lt.Y),
                        }
                    );
                }
            }
        }

        private void AnimationForm_Load(object sender, System.EventArgs e)
        {
            updateStartPauseButtonText();
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            if (frames.Count == 0) return;
            if (totalDuration == 0) return;
            if (paused) return;
            SetFrameByTime(sw.ElapsedMilliseconds);
        }

        class FrameBounds
        {
            // The GD orientation is left-top being 0,0
            // and right-bottom being R,B

            public int left, top, right, bottom;

            public void Move(int x, int y)
            {
                left += x;
                right += x;
                top += y;
                bottom += y;
            }

            public FrameInfo frame;

            public int Width
            {
                get { return right - left; }
            }

            public int Height
            {
                get { return bottom - top; }
            }

            public void MakeLTOrigin()
            {
                Move(left, top);
            }

            public void MakeRBOrigin()
            {
                Move(-right, -bottom);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="x">X point in image that specifies origin</param>
            /// <param name="y">Y point in image that specifies origin</param>
            /// <param name="width">Image width</param>
            /// <param name="height">Image height</param>
            /// <param name="frame"></param>
            public FrameBounds(int x, int y, int width, int height, FrameInfo frame)
            {
                left = 0;
                right = width;
                top = 0;
                bottom = height;
                this.frame = frame;
                Move(-x, -y);
            }

            public FrameBounds() { }

            public override string ToString()
            {
                return string.Format("Left {0} Top {1} Right {2} Bottom {3}, {4}x{5}", left, top, right, bottom, Width, Height);
            }
        }

        private void ToGif(string outputFile)
        {
            var bounds = new FrameBounds[frames.Count];

            int i = 0;
            foreach (var x in frames)
            {
                bounds[i++] = new FrameBounds(x.OffsetX, x.OffsetY, x.Width, x.Height, x);
                Console.WriteLine("{0} {1} {2} {3}", x.OffsetX, x.OffsetY, x.Width, x.Height);
            }


            var minLeft = bounds.Min(x => x.left);
            var minTop = bounds.Min(x => x.top);
            var totalBounds = new FrameBounds() { 
                left = bounds.Min(x => x.left),
                top = bounds.Min(x => x.top), // Top of the image
                right = bounds.Max(x => x.right),
                bottom = bounds.Max(x => x.bottom), // Bottom of the image
            };


            Console.WriteLine("{0} {1}", minLeft, minTop);

            foreach (var fb in bounds)
            {
                Console.WriteLine(fb.ToString());
                fb.Move(-minLeft, -minTop);
                Console.WriteLine(fb.ToString());
            }

            Console.WriteLine("{0} {1}", totalBounds.left, totalBounds.top);

            int w = totalBounds.Width;
            int h = totalBounds.Height;

            int minx = 0, miny = 0;
            
            using (var gif = new AnimatedGifCreator(outputFile, 0, 0))
            {
                foreach (var b in bounds)
                {
                    using (var bm = new Bitmap(w + 1, h + 1))
                    using (var g = Graphics.FromImage(bm))
                    {

                        int x = b.left;
                        int y = b.top;
                        Console.WriteLine("Drawing image {0} - {1}, {2} - {3}", x, x + b.Width, y, y + b.Height);

                        g.DrawImageUnscaled(
                            b.frame.Tile,
                            x,
                            y
                        );
                        
                        gif.AddFrame(
                            bm,
                            b.frame.delay,
                            GifQuality.Bit8
                        );
                    }
                }
            }
        }

        private void btnExportGif_Click(object sender, EventArgs e)
        {
            using (var fsd = new SaveFileDialog())
            {
                fsd.Filter = "GIF|*.gif";
                fsd.FileName = "Animation.gif";
                if (fsd.ShowDialog() == DialogResult.OK)
                    ToGif(fsd.FileName);
            }
        }

        private void tbFrame_ValueChanged(object sender, EventArgs e)
        {
            SetFrame(tbFrame.Value);
        }

        private void btnStartPause_Click(object sender, EventArgs e)
        {
            paused = !paused;
            updateStartPauseButtonText();
        }

        private void updateStartPauseButtonText()
        {
            btnStartPause.Text = paused ? "►" : "||";

        }

        private void AnimationForm_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
