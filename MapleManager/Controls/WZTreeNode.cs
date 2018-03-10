using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapleManager.WzTools.Objects;

namespace MapleManager.Controls
{
    public class WZTreeNode : TreeNode
    {
        public object WzObject { get; set; }

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

        public void UpdateData()
        {
            void SetString(object x, out string y)
            {
                if (x is string g) y = g;
                else y = null;
            }

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
                    foreach (var kvp in prop)
                    {
                        AddNode(kvp.Value, -1, kvp.Key);
                    }

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
                    foreach (var name in new string[] {"X", "Y"})
                    {
                        AddNode(vector[name], -1, name);
                    }
                }
                else if (pcomObject is WzList list)
                {
                    ToolTipText = pcomObject.ToString();
                    for (var i = 0; i < list.ChildCount; i++)
                    {
                        var name = i.ToString();
                        var elem = list[name];

                        AddNode(elem, i, name);
                    }
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



                var infoLinkNode = Nodes["info"]?.Nodes["link"];
                if (infoLinkNode != null)
                    Text += " (link: " + infoLinkNode.Tag + ")";
            }
            else
            {
                ToolTipText = WzObject?.ToString();
            }
        }
    }
}
