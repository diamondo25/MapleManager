
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MapleManager.Scripts.Animator
{
    public partial class AnimationForm : Form
    {
        public AnimationForm()
        {
            InitializeComponent();
            ResizeRedraw = true;
        }

        private Stopwatch sw = new Stopwatch();

        private List<FrameInfo> frames = new List<FrameInfo>();
        private int currentFrame = 0;
        private int totalDuration = 0;
        private int currentStart = 0;
        private int currentEnd = 0;

        internal void LoadFrames(List<FrameInfo> pictureFrames)
        {
            sw.Reset();
            this.frames = pictureFrames;

            totalDuration = pictureFrames.Sum(x => x.delay);
            currentStart = 0;
            currentEnd = this.frames[0].delay;
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

        private void SetFrame(int idx)
        {
            if (idx >= frames.Count) idx = 0;
            var frame = frames[idx];
            currentFrame = idx;

            currentEnd = currentStart + frame.delay;

            pictureBox1.Image = frame.Image.Tile;
            pictureBox1.Width = frame.Width;
            pictureBox1.Height = frame.Height;
            pictureBox1.Top = (int)((Height * 0.75) + -frame.Y);
            pictureBox1.Left = (Width / 2) + -frame.X;
        }


        private void AnimationForm_Paint(object sender, PaintEventArgs e)
        {
            var pen = new Pen(Color.Black);
            var y = (int)(Height * 0.75);
            e.Graphics.DrawLine(pen, 0, y, Width, y);

        }

        private void AnimationForm_Load(object sender, System.EventArgs e)
        {
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            if (frames.Count == 0) return;
            if (totalDuration == 0) return;
            SetFrameByTime(sw.ElapsedMilliseconds);

            

        }
    }
}
