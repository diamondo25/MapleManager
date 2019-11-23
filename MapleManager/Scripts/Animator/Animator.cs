using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using MapleManager.Controls;
using MapleManager.WzTools;
using MapleManager.WzTools.Objects;

namespace MapleManager.Scripts.Animator
{
    class Animator : IScript
    {
        public string GetScriptName()
        {
            return "Animator";
        }

        private AnimationForm animationForm = new AnimationForm();
        private CharacterGenForm characterGenForm;
        private ScriptNode _mainScriptNode;

        public void Start(ScriptNode mainScriptNode)
        {
            _mainScriptNode = mainScriptNode;
            Program.MainForm.tvData.AfterSelect += treeView1_AfterSelect;
            Program.MainForm.Shown += putScreenToFront;
            if (animationForm == null || animationForm.IsDisposed) animationForm = new AnimationForm();
            animationForm.Show();

            if (characterGenForm == null || characterGenForm.IsDisposed) characterGenForm = new CharacterGenForm(mainScriptNode);
            characterGenForm.Show();
        }

        public void Stop()
        {
            Program.MainForm.tvData.AfterSelect -= treeView1_AfterSelect;
            Program.MainForm.Shown -= putScreenToFront;
            if (animationForm != null) animationForm.Close();
            if (characterGenForm != null) characterGenForm.Close();
        }


        private void putScreenToFront(object sender, EventArgs e)
        {
            if (animationForm != null) animationForm.BringToFront();
            if (characterGenForm != null) characterGenForm.BringToFront();
        }

        private static string GetFilenameForNode(ScriptNode node)
        {
            var iterableWorkingObject = (INameSpaceNode)node;
            var filename = iterableWorkingObject.GetName();
            {
                INameSpaceNode tmp = iterableWorkingObject;
                while (!tmp.GetName().EndsWith(".img"))
                {
                    tmp = (INameSpaceNode)tmp.GetParent();
                    filename = tmp.GetName().Replace(".img", "") + "_" + filename;
                }
            }
            return filename;
        }

        private static List<FrameInfo> TryRenderNode(ScriptNode node, out string filename)
        {
            filename = "??";

        resolveAgain:
            {
                // Try to resolve UOL
                while (node.Get() is WzUOL)
                {
                    var uol = node.Get() as WzUOL;
                    var path = uol.ActualPath();
                    var newNode = node.GetNode(path);
                    if (newNode == null) return null;
                    node = newNode;
                }
            }

            {
                // If its a single canvas, render that
                var canvas = node.GetCanvas();
                if (canvas != null)
                {
                    filename = GetFilenameForNode(node);
                    return new List<FrameInfo>(RenderFrame(canvas));
                }
            }

            {
                // If its a prop, figure out if we can render it
                var animateableProp = FindAnimateableProp(node);
                if (animateableProp == null)
                {
                    var info = node.GetNode("info");
                    if (info == null) return null;

                    // Maybe we can find a link node
                    var link = node.GetString("info/link");
                    if (link != null)
                    {
                        var path = "../" + link + ".img";
                        Console.WriteLine("Trying to get node @ {0}", path);
                        var tmp = node.GetNode(path);
                        if (tmp != null)
                        {
                            node = tmp;
                            goto resolveAgain;
                        }
                    }

                    // Figure out if the info node is any good
                    animateableProp = FindAnimateableProp(info);
                    if (animateableProp != null && animateableProp is ScriptNode)
                    {
                        node = animateableProp;
                        goto resolveAgain;
                    }
                }
                else
                {
                    node = animateableProp;
                }
            }

            {
                // Try to render elements
                var frames = new List<FrameInfo>();

                bool indexesAreImageOrUOL = true;
                bool foundAny = false;
                for (var i = 0; ; i++)
                {
                    var p = node.Get(i.ToString());
                    if (p == null) break;
                    foundAny = true;
                    if (!(p is WzCanvas || p is WzUOL))
                    {
                        if (i == 0) continue;
                        indexesAreImageOrUOL = false;
                        break;
                    }

                    frames.AddRange(RenderFrame(p));
                }

                if (foundAny && indexesAreImageOrUOL)
                {
                    if (node.GetInt32("zigzag", 0) == 1)
                    {
                        // Apply a zigzag
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
                }

                filename = GetFilenameForNode(node);
                return frames;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (animationForm == null || animationForm.IsDisposed)
            {
                animationForm = null;
                Program.MainForm.tvData.AfterSelect -= treeView1_AfterSelect;
                return;
            }

            if (e.Node == null) return;
            if (!(e.Node is WZTreeNode)) return;


            {
                var wtn = (WZTreeNode)e.Node;
                var scriptNode = _mainScriptNode.GetNode(wtn.GetFullPath());
                string filename;
                var frames = TryRenderNode(scriptNode, out filename);

                if (frames != null && frames.Count > 0)
                {
                    animationForm.FileName = filename;
                    animationForm.LoadFrames(frames);
                }
            }
        }

        public static ScriptNode FindAnimateableProp(ScriptNode prop)
        {
            if (prop.GetCanvas() != null) return prop;

            string[] names = new[]
            {
                "move", "fly", "die", "stand", "iconRaw", "icon"
                , "effect/default"
                , "effect"
            };

            // Find any node that is usable for rendering
            foreach (var name in names)
            {
                var x = prop.GetNode(name);
                if (x == null) continue;
                return x;
            }

            if (prop.HasChild("0")) return prop;

            return null;
        }


        public static IEnumerable<FrameInfo> RenderFrame(ScriptNode p)
        {
            return RenderFrame(p.Get());
        }


        public static IEnumerable<FrameInfo> RenderFrame(object p)
        {
            var frame = new FrameInfo();

            WzCanvas actualImage;

            if (p is WzCanvas) actualImage = (WzCanvas)p;
            else if (p is WzUOL)
            {
                var ao = ((WzUOL)p).ActualObject(true);
                if (ao is WzCanvas)
                {
                    actualImage = (WzCanvas)ao;
                }
                else yield break;
            }
            else yield break;

            frame.Data = actualImage;
            frame.Tile = frame.Data.Tile;

            frame.OffsetX = frame.Data.CenterX;
            frame.OffsetY = frame.Data.CenterY;

            frame.delay = actualImage.HasChild("delay") ? actualImage.GetInt32("delay") : 100;
            int a0 = actualImage.HasChild("a0") ? actualImage.GetInt32("a0") : 255;
            int a1 = actualImage.HasChild("a1") ? actualImage.GetInt32("a1") : 255;

            if (a1 == a0)
            {
                // No need to do animation
                yield return frame;
                yield break;
            }

            var tile = frame.Tile;
            frame.Width = tile.Width;
            frame.Height = tile.Height;

            float diff = (1.0f * (a1 - a0));
            // Delay used for each frame for the animation
            // Try to make the diff as effienct as possible
            float animationDelay = frame.delay / 256.0f;

            float step = diff / (frame.delay / animationDelay);

            Trace.WriteLine(string.Format("Need to fade from {0} to {1} (= {2} diff), step of {3}, delay of {4}!", a0, a1, diff, step, animationDelay));

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
                clone.delay = (int)Math.Ceiling(animationDelay);
                yield return clone;
            }
        }
    }
}