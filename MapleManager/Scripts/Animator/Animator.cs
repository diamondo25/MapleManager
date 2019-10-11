using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using MapleManager.Controls;
using MapleManager.WzTools.Objects;

namespace MapleManager.Scripts.Animator
{
    class Animator : IScript
    {
        public string GetScriptName()
        {
            return "Animator";
        }

        private AnimationForm form = new AnimationForm();

        public void Start(ScriptNode mainScriptNode)
        {
            Program.MainForm.tvData.AfterSelect += treeView1_AfterSelect;
            Program.MainForm.Shown += putScreenToFront;
            if (form == null || form.IsDisposed) form = new AnimationForm();
            form.Show();
        }

        public void Stop()
        {
            Program.MainForm.tvData.AfterSelect -= treeView1_AfterSelect;
            form.Close();
        }


        private void putScreenToFront(object sender, EventArgs e)
        {
            if (form != null) form.BringToFront();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (form == null || form.IsDisposed)
            {
                form = null;
                Program.MainForm.tvData.AfterSelect -= treeView1_AfterSelect;
                return;
            }

            if (e.Node == null) return;
            if (!(e.Node is WZTreeNode)) return;

            try
            {
                //Program.MainForm.BeginTreeUpdate();

                var wtn = (WZTreeNode) e.Node;
                var tag = wtn.WzObject;

                if (tag is WzProperty && wtn.Name.EndsWith(".img"))
                {
                    var prop = (WzProperty) tag;
                    int tmp;
                    try_get_anim:
                    if (prop.HasKey("move")) tag = prop["move"];
                    else if (prop.HasKey("fly")) tag = prop["fly"];
                    else if (prop.HasKey("die")) tag = prop["die"];
                    else if (prop.HasKey("stand")) tag = prop["stand"];
                    else if (prop.HasKey("effect")) tag = prop["effect"];
                    else
                    {
                        if (prop.HasKey("info") && prop["info"] is WzProperty)
                        {
                            prop = (WzProperty) prop["info"];
                            if (prop.HasKey("link"))
                            {
                                var actualInfo = e.Node.Parent.Nodes[(string) prop["link"] + ".img"];
                                if (actualInfo is WZTreeNode)
                                {
                                    wtn = (actualInfo as WZTreeNode);
                                    Program.MainForm.TryLoadNode(wtn);
                                    prop = (WzProperty) wtn.WzObject;
                                    goto try_get_anim;
                                }
                            }
                        }

                        return;
                    }
                }

                if (tag != null && tag is PcomObject)
                {
                    var pco = (PcomObject) tag;
                    Trace.WriteLine("Using object: " + pco.Name);
                }

                object workingObject = tag;
                if (workingObject is WzUOL)
                {
                    var uol = (WzUOL) workingObject;
                    workingObject = uol.ActualObject(true);
                }


                // Magic code for animation
                if (workingObject is WzProperty)
                {
                    var prop = (WzProperty) workingObject;

                    bool indexesAreImageOrUOL = true;
                    bool foundAny = false;
                    var frames = new List<FrameInfo>();
                    for (var i = 0;; i++)
                    {
                        var p = prop[i.ToString()];
                        if (p == null) break;
                        foundAny = true;
                        if (!(p is WzImage || p is WzUOL))
                        {
                            if (i == 0) continue;
                            indexesAreImageOrUOL = false;
                            break;
                        }

                        var frame = new FrameInfo();

                        WzImage actualImage;

                        if (p is WzImage) actualImage = (WzImage) p;
                        else if (p is WzUOL)
                        {
                            var ao = ((WzUOL) p).ActualObject(true);
                            if (ao is WzImage)
                            {
                                actualImage = (WzImage) ao;
                            }
                            else continue;
                        }
                        else continue;

                        frame.Data = actualImage;
                        frame.Tile = frame.Data.Tile;

                        var originProp = frame.Data["origin"] as WzVector2D;
                        if (originProp != null)
                        {
                            frame.OffsetX = originProp.X;
                            frame.OffsetY = originProp.Y;
                        }

                        frame.delay = actualImage.HasKey("delay") ? actualImage.GetInt32("delay") : 100;
                        int a0 = actualImage.HasKey("a0") ? actualImage.GetInt32("a0") : 255;
                        int a1 = actualImage.HasKey("a1") ? actualImage.GetInt32("a1") : 255;

                        var tile = frame.Tile;
                        frame.Width = tile.Width;
                        frame.Height = tile.Height;

                        if (a0 != a1)
                        {
                            float diff = (1.0f * (a1 - a0));
                            // Delay used for each frame for the animation
                            // Try to make the diff as effienct as possible
                            float animationDelay = frame.delay / 256.0f;

                            float step = diff / (frame.delay / animationDelay);
                            for (float a = a0; (a0 > a1 ? a >= a1 : a <= a1); a += step)
                            {
                                var bm = new Bitmap(frame.Width, frame.Height);

                                using (var graphics = Graphics.FromImage(bm))
                                {
                                    var matrix = new ColorMatrix();
                                    matrix.Matrix33 = a / 256.0f;

                                    var imgAttr = new ImageAttributes();
                                    imgAttr.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                                    graphics.DrawImage(
                                        tile,
                                        new Rectangle(0, 0, frame.Width, frame.Height),
                                        0, 0, frame.Width, frame.Height,
                                        GraphicsUnit.Pixel,
                                        imgAttr
                                    );
                                }

                                var clone = frame.Clone();
                                clone.Tile = bm;
                                clone.CustomImage = true;
                                clone.delay = (int)animationDelay;
                                frames.Add(clone);
                            }
                        }
                        else
                        {
                            frames.Add(frame);
                        }
                    }

                    if (foundAny && indexesAreImageOrUOL)
                    {
                        if (prop.HasKey("zigzag") && prop.GetInt32("zigzag") == 1)
                        {
                            if (frames.Count > 2)
                            {
                                // Zigzag
                                var tmp = new List<FrameInfo>(frames);
                                tmp.RemoveAt(0);
                                tmp.RemoveAt(tmp.Count - 1);
                                tmp.Reverse();
                                frames.AddRange(tmp);
                            }
                        }


                        if (frames.Count > 0)
                        {
                            form.LoadFrames(frames);
                        }
                    }
                }
            }
            finally
            {
                // Program.MainForm.EndTreeUpdate();
            }
        }
    }
}