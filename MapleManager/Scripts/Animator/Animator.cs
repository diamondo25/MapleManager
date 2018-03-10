using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            if (form == null || form.IsDisposed) form = new AnimationForm();
            form.Show();
        }

        public void Stop()
        {
            Program.MainForm.tvData.AfterSelect -= treeView1_AfterSelect;
            form.Close();
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
            var wtn = (WZTreeNode)e.Node;
            var tag = wtn.WzObject;

            if (tag is WzProperty && wtn.Name.EndsWith(".img"))
            {
                var prop = (WzProperty) tag;
                
                try_get_anim:
                if (prop.HasKey("move")) tag = prop["move"];
                else if (prop.HasKey("fly")) tag = prop["fly"];
                else if (prop.HasKey("die")) tag = prop["die"];
                else if (prop.HasKey("stand")) tag = prop["stand"];
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
                                prop = (WzProperty)wtn.WzObject;
                                goto try_get_anim;

                            }
                        }
                    }
                    return;
                }
            }

            object workingObject = tag;
            if (workingObject is WzUOL)
            {
                var uol = (WzUOL)workingObject;
                workingObject = uol.ActualObject();
            }



            // Magic code for animation
            if (workingObject is WzProperty)
            {
                var prop = (WzProperty)workingObject;

                bool indexesAreImageOrUOL = true;
                bool foundAny = false;
                var frames = new List<FrameInfo>();
                for (var i = 0; ; i++)
                {
                    var p = prop[i.ToString()];
                    if (p == null) break;
                    foundAny = true;
                    if (!(p is WzImage || p is WzUOL))
                    {
                        indexesAreImageOrUOL = false;
                        break;
                    }

                    var frame = new FrameInfo();

                    WzImage actualImage;

                    if (p is WzImage) actualImage = (WzImage)p;
                    else if (p is WzUOL)
                    {
                        var ao = ((WzUOL)p).ActualObject(true);
                        if (ao is WzImage)
                        {
                            actualImage = (WzImage) ao;
                        }
                        else continue;
                    }
                    else continue;
                    
                    frame.Image = actualImage;

                    var originProp = actualImage["origin"] as WzVector2D;
                    if (originProp != null)
                    {
                        frame.X = originProp.X;
                        frame.Y = originProp.Y;
                    }

                    frame.delay = actualImage.HasKey("delay") ? actualImage.GetInt32("delay") : 100;
                    frame.a0 = actualImage.HasKey("a0") ? actualImage.GetInt32("a0") : 255;
                    frame.a1 = actualImage.HasKey("a1") ? actualImage.GetInt32("a1") : 255;

                    frame.Width = actualImage.Width;
                    frame.Height = actualImage.Height;

                    frames.Add(frame);
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

                    Trace.WriteLine("FRAMES");
                    
                    if (frames.Count > 0)
                    {
                        form.LoadFrames(frames);
                    }

                }
            }

        }

    }
}
