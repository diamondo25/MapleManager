using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapleManager.WzTools.FileSystem;
using MapleManager.WzTools.Objects;

namespace MapleManager.Controls
{
    public class WZTreeNode : TreeNode
    {
        public object WzObject { get; set; }
        public Dictionary<string, AdditionalInfoObject> AdditionalInfo { get; set; } = null;

        public class AdditionalInfoObject
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public bool ShowInText { get; set; }
        }

        public WZTreeNode() : base() { }

        public WZTreeNode(object obj, string name = null)
        {
            WzObject = obj;
            if (obj is PcomObject pcomObject)
            {
                pcomObject.TreeNode = this;
                UpdateData(); // Sets all the fields required
            }
            else
            {
                Name = name;
                Text = name;
                ToolTipText = "" + obj;
            }

        }


        public void AddNode(object obj, int idx = -1, string name = null)
        {
            var subNode = new WZTreeNode(obj, name);
            if (idx != -1)
                Nodes.Insert(idx, subNode);
            else
                Nodes.Add(subNode);
        }

        public void SetAdditionalInfo(string key, string text, bool showInText, bool updateText = true)
        {
            if (AdditionalInfo == null) AdditionalInfo = new Dictionary<string, AdditionalInfoObject>();
            AdditionalInfo[key] = new AdditionalInfoObject
            {
                Name = key,
                Text = text,
                ShowInText = showInText
            };
            if (updateText) UpdateText();
        }

        public void RemoveAdditionalInfo(string key, bool updateText = true)
        {
            if (AdditionalInfo == null) return;
            if (!AdditionalInfo.ContainsKey(key)) return;
            AdditionalInfo.Remove(key);
            if (AdditionalInfo.Count == 0) AdditionalInfo = null;

            if (updateText) UpdateText();
        }


        private const string DummyNodeName = "---DUMMYNODE---";

        public void SetNotLoaded()
        {
            WzObject = null;
            Nodes.Clear();
            Nodes.Add(DummyNodeName, DummyNodeName);
            SetNotLoadedTooltip();
        }

        public void SetNotLoadedTooltip()
        {
            ToolTipText = "-- not loaded --";
        }

        public bool IsNotLoaded()
        {
            if (Nodes.Count == 0) return false;
            if (Nodes[0].Name == DummyNodeName) return true;
            return false;
        }

        public void TryLoad(bool force)
        {
            if (!force && !IsNotLoaded()) return;
            var nsf = Tag as FSFile;
            var obj = nsf.Object;
            WzObject = obj;
            Nodes.Clear();

            UpdateData();
        }

        public void UpdateText()
        {
            void SetString(object x, out string y)
            {
                if (x is string g) y = g;
                else y = null;
            }

            ToolTipText = "";


            if (WzObject is PcomObject pcomObject)
            {
                pcomObject.TreeNode = this;

                var oldName = Name;
                this.Name = pcomObject.Name;
                //if (pcomObject.Parent != null && oldName != null)
                //    pcomObject.Parent.Rename(oldName, Name);

                Text = Name;

                if (pcomObject is WzProperty prop)
                {
                    ToolTipText = "Property";

                    string mapName = null,
                        streetName = null,
                        name = null,
                        id = null,
                        type = null;

                    SetString(prop["name"], out name);
                    SetString(prop["streetName"], out streetName);
                    SetString(prop["mapName"], out mapName);
                    SetString(prop["id"], out id);
                    SetString(prop["type"], out type);

                    if (mapName != null && name == null)
                    {
                        name = mapName;
                        if (streetName != null) name += " - " + streetName;
                    }

                    if (name != null)
                        Text += ": " + name;

                    if (id != null)
                        Text += " (id: " + id + ")";
                    if (type != null)
                        Text += " (type: " + type + ")";


                    if (pcomObject is WzImage image)
                    {
                        ToolTipText = "Image" + Environment.NewLine;
                        ToolTipText += "MagLevel: " + image.MagLevel + Environment.NewLine;
                        ToolTipText += "PixFormat: " + image.PixFormat + Environment.NewLine;
                        ToolTipText += "Resolution: " + image.Width + " x " + image.Height + Environment.NewLine;
                        ToolTipText += "Tile: " + image.TileWidth + " x " + image.TileHeight + Environment.NewLine;
                    }
                }
                else if (pcomObject is WzVector2D vector)
                {
                    ToolTipText = "Vector2D";
                }
                else if (pcomObject is WzList list)
                {
                    ToolTipText = pcomObject.ToString();
                }
                else if (pcomObject is WzUOL uol)
                {
                    object curObject = uol;
                    bool firstIter = true;

                    ToolTipText = "";
                    Text += " (UOL: ";
                    bool invalid = false;
                    while (curObject is WzUOL uolObj)
                    {
                        var actualObject = uolObj.ActualObject();
                        ToolTipText += "UOL: " + uolObj.Path + Environment.NewLine;
                        ToolTipText += "Actual Path: " + uolObj.ActualPath() + Environment.NewLine;
                        ToolTipText += "Actual Object: " + actualObject + Environment.NewLine;

                        if (actualObject == null)
                        {
                            ToolTipText += "!!! OBJECT DOESNT EXIST !!!" + Environment.NewLine;
                            invalid = true;
                        }

                        if (!firstIter) Text += " -> ";
                        firstIter = false;
                        Text += uol.Path;

                        curObject = actualObject;
                    }

                    Text += ")";

                    if (invalid) Text += " ERROR";
                }



                var infoLinkNode = Nodes["info"]?.Nodes["link"] as WZTreeNode;
                if (infoLinkNode != null)
                {
                    SetAdditionalInfo("link", infoLinkNode.WzObject.ToString(), true, false);
                }
                else
                {
                    RemoveAdditionalInfo("link", false);
                }
            }
            else if (WzObject != null)
            {
                ToolTipText = WzObject.ToString();
            }
            else if (IsNotLoaded())
            {
                SetNotLoadedTooltip();
            }
            else
            {
                ToolTipText = "null";
            }

            if (AdditionalInfo != null)
            {
                foreach (var o in AdditionalInfo)
                {
                    if (o.Value.ShowInText)
                    {
                        Text += " (" + o.Key + ": " + o.Value.Text + ")";
                    }

                    ToolTipText += Environment.NewLine;
                    ToolTipText += o.Key + ": " + o.Value.Text;
                }
            }
            ToolTipText = Text + Environment.NewLine + ToolTipText;
        }

        public void UpdateData()
        {

            if (WzObject is PcomObject pcomObject)
            {
                pcomObject.TreeNode = this;

                var oldName = Name;
                this.Name = pcomObject.Name;
                //if (pcomObject.Parent != null && oldName != null)
                //    pcomObject.Parent.Rename(oldName, Name);

                Text = Name;

                if (pcomObject is WzProperty prop)
                {
                    foreach (var kvp in prop)
                    {
                        AddNode(kvp.Value, -1, kvp.Key);
                    }
                }
                else if (pcomObject is WzVector2D vector)
                {
                    foreach (var name in new string[] { "X", "Y" })
                    {
                        AddNode(vector[name], -1, name);
                    }
                }
                else if (pcomObject is WzList list)
                {
                    for (var i = 0; i < list.ChildCount; i++)
                    {
                        var name = i.ToString();
                        var elem = list[name];

                        AddNode(elem, i, name);
                    }
                }
            }

            UpdateText();
        }
    }
}
