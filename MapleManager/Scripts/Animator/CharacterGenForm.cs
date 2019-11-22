using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MapleManager.WzTools.Objects;

namespace MapleManager.Scripts.Animator
{
    public partial class CharacterGenForm : Form
    {
        private ScriptNode _mainNode;

        public CharacterGenForm(ScriptNode mainNode)
        {
            _mainNode = mainNode;
            InitializeComponent();
        }

        private void CharacterGenForm_Load(object sender, System.EventArgs e)
        {
            this.UseWaitCursor = true;
            new Thread(LoadData).Start();
        }


        private static string[] zmap;
        private static Dictionary<string, string> smap;

        public static int FindStringIndex(string[] arr, string x)
        {
            for (var i = 0; i < arr.Length; i++)
            {
                if (arr[i] == x) return i;
            }

            return -1;
        }

        private void UpdateToolstrip(string text)
        {
            toolStripStatusLabel1.Text = text;
        }

        private void LoadBlock(string name, Action action)
        {
            UpdateToolstrip("Loading " + name + ".img");
            action();
            UpdateToolstrip("Loaded " + name + ".img");
        }

        private readonly string[] ignoredCategories = new[]
        {
            "Bits",
            "MonsterBattle",
            "ArcaneForce",
            "Android",
            "Mechanic",
            "Taming" // Taming(Mob) is not yet supported
        };

        private void LoadScriptNodeToTreeView(ScriptNode node, TreeNodeCollection parent)
        {
            foreach (var subnode in node)
            {
                if (ignoredCategories.Contains(subnode.Name))
                {
                    continue;
                }

                var n = parent.Add(subnode.Name);
                // All nodes with the Name field will be written down with the name
                try
                {
                    var obj = subnode.Get();
                    if (!(obj is string) && subnode["name"] != null)
                    {
                        obj = subnode.GetString("name");
                    }

                    if (obj is string)
                    {
                        n.Text = obj.ToString();
                        n.Name = subnode.Name.PadLeft(8, '0') + ".img";
                        n.Tag = new ChosenItem
                        {
                            NodePath = "Character/" + n.GetFullPath(),
                            SelectedPath = n.FullPath,
                            ID = int.Parse(subnode.Name.TrimStart('0')),
                        };
                    }
                    else
                    {
                        LoadScriptNodeToTreeView(subnode, n.Nodes);
                    }
                }
                catch
                {
                }
            }
        }

        private void LoadData()
        {
            Invoke((MethodInvoker)delegate { itemSelectionTree.BeginUpdate(); });

            LoadBlock("zmap.img", delegate
            {
                var arr = new List<string>();
                foreach (var node in _mainNode.GetNode("Base/zmap.img"))
                {
                    arr.Add(node.Name);
                }

                arr.Reverse();

                zmap = arr.ToArray();
            });

            LoadBlock("smap.img", delegate
            {
                smap = new Dictionary<string, string>();
                foreach (var node in _mainNode.GetNode("Base/smap.img"))
                {
                    smap[node.Name] = node.GetString();
                }
            });

            LoadBlock("Skins", delegate
            {
                TreeNode skinNode = null;
                Invoke((MethodInvoker)delegate { skinNode = itemSelectionTree.Nodes.Add("Skin"); });
                foreach (var sn in _mainNode.GetNode("Character/"))
                {
                    if (!sn.Name.EndsWith(".img")) continue;
                    var nameWithoutImg = sn.Name.Remove(sn.Name.Length - 4);
                    var nameWithoutZeroes = nameWithoutImg.TrimStart('0');
                    if (!nameWithoutZeroes.StartsWith("2")) continue;
                    int id;
                    if (!int.TryParse(nameWithoutZeroes, out id)) continue;

                    Invoke((MethodInvoker)delegate
                   {
                       var node = skinNode.Nodes.Add("Skin color " + (id % 1000));
                       node.Tag = new ChosenItem
                       {
                           SelectedPath = node.FullPath,
                           NodePath = sn.GetFullPath(),
                           ID = (id % 1000),
                       };
                   });
                }
            });


            LoadBlock("Equip strings", delegate
            {
                var itemNode = _mainNode.GetNode("String/Item.img");
                ScriptNode scriptnodeToProcess;
                if (itemNode != null)
                {
                    scriptnodeToProcess = itemNode["Eqp"];
                }
                else
                {
                    // Load separate files
                    scriptnodeToProcess = _mainNode.GetNode("String/Eqp.img/Eqp");
                }

                if (scriptnodeToProcess != null)
                {
                    Invoke((MethodInvoker)delegate
                   {
                       LoadScriptNodeToTreeView(scriptnodeToProcess, itemSelectionTree.Nodes);
                   });
                }
            });

            Invoke((MethodInvoker)delegate
           {
               itemSelectionTree.EndUpdate();
               UseWaitCursor = false;
           });
        }


        public class ChosenItem
        {
            public string NodePath;
            public string SelectedPath;
            public int ID;

            public override string ToString()
            {
                return SelectedPath + " (" + ID + ")";
            }

            public ScriptNode GetNode(ScriptNode mainNode)
            {
                return mainNode.GetNode(NodePath);
            }
        }

        public static ChosenItem TreeNodeToChosenItem(TreeNode tn)
        {
            return tn.Tag as ChosenItem;
        }

        public ScriptNode GetNode(TreeNode tn)
        {
            var ci = TreeNodeToChosenItem(tn);
            if (ci == null) return null;
            return ci.GetNode(_mainNode);
        }

        private void AddCurrentlySelectedTreeNode(object sender, EventArgs e)
        {
            AddItemSelectionNodeToSelectedNodes(itemSelectionTree.SelectedNode);
        }

        private void AddItemSelectionNodeToSelectedNodes(TreeNode node)
        {
            if (node == null || node.Tag == null) return;

            // Figure out if its already selected
            foreach (var lbi in selectedItems.Items)
            {
                if (lbi == node.Tag) return;
            }

            selectedItems.Items.Add(node.Tag);
            RenderCurrentlySelectedItems(null, null);
        }

        private void DeleteCurrentlySelectedListBoxItem(object sender, EventArgs e)
        {
            var curItem = selectedItems.SelectedItem;
            if (curItem == null) return;
            selectedItems.Items.Remove(curItem);
            RenderCurrentlySelectedItems(null, null);
        }

        public class SpriteSource
        {
            public WzCanvas Canvas { get; private set; }
            public Point Position { get; private set; }
            public string Category { get; private set; }

            public string ISlot { get; private set; }
            public string VSlot { get; private set; }
            public string BaseVSlot { get; private set; }
            public int InventorySlotNumber { get; private set; }

            public int Z { get; private set; }

            public SpriteSource(ScriptNode canvas, Point position, string category, string islot, string baseVslot,
                int job = 0)
            {
                if (islot == "" && baseVslot != "")
                    throw new Exception("Empty islot for non-empty vslot");

                Canvas = canvas.GetCanvas();
                Position = position;
                Category = category;
                ISlot = islot;
                BaseVSlot = baseVslot;
                VSlot = "";

                {
                    // Figure out which inventory slot number it is
                    // Basically just traverse the zmap, and the highest ID = inventory slot...?
                    var tmp = ISlot;
                    while (tmp.Length > 0)
                    {
                        var chunk = tmp.Substring(0, 2);
                        tmp = tmp.Substring(2);

                        var slot = FindStringIndex(zmap, chunk);

                        if (slot == -1) throw new Exception("Unknown islot specified");

                        if (slot > InventorySlotNumber)
                            InventorySlotNumber = slot;
                    }
                }

                var vslot = "";


                if (BaseVSlot == "Ae" &&
                    (job / 100 == 23 || job == 2002) &&
                    Canvas.GetString("z") != "backAccessoryEar"
                )
                {
                    Z = QueryZ(canvas, BaseVSlot, ref vslot, "accessoryEarOverHair");
                }
                else
                {
                    Z = QueryZ(canvas, BaseVSlot, ref vslot, null);
                }

                VSlot = vslot;
            }

            private static int QueryZ(ScriptNode canvas, string baseSlot, ref string vSlot, string modifiedZ)
            {
                var zKeyName = modifiedZ != null ? modifiedZ : canvas.GetString("z");
                var zIndex = 0;

                if (!string.IsNullOrEmpty(baseSlot) && vSlot != null)
                {
                    if (!int.TryParse(zKeyName, out zIndex))
                    {
                        zIndex = FindStringIndex(zmap, zKeyName);
                        var tmp = "";
                        if (smap.TryGetValue(zKeyName, out tmp))
                        {
                            vSlot = CommonSlot(tmp, baseSlot);
                        }
                    }
                }


                return zIndex;
            }
        }

        public class SpriteInstance
        {
            public SpriteSource Source { get; set; }
            public Point Position { get; set; }
            public bool Visible { get; set; }

            public List<ActionFrame.MAPINFO> Group { get; set; }
        }

        public class ActionFrame
        {
            public class MAPINFO
            {
                public string Name { get; set; }
                public Point Position { get; set; }
            }

            public string ExclVSlot { get; private set; }
            public List<SpriteInstance> Sprites { get; private set; }
            public List<List<MAPINFO>> Groups { get; private set; }
            public bool Body { get; private set; }
            public bool MBRValid { get; private set; }

            public SpriteInstance BodySprite { get; private set; }

            public ActionFrame()
            {
                Sprites = new List<SpriteInstance>();
                Groups = new List<List<MAPINFO>>();
            }

            public void UpdateVisibility()
            {
                var tmp = new Dictionary<int, SpriteInstance>();

                if (!string.IsNullOrEmpty(ExclVSlot))
                {
                    for (var i = 0; i < ExclVSlot.Length; i += 2)
                    {
                        var key = ExclVSlot[i + 1] | (ExclVSlot[i] << 16);
                        tmp[key] = null;
                    }
                }

                // Find all sprites (in order) and pick the first one matching the given slot
                foreach (var si in Sprites)
                {
                    si.Visible = true;
                    var vslot = si.Source.VSlot;
                    for (var i = 0; i < vslot.Length; i += 2)
                    {
                        var key = vslot[i + 1] | (vslot[i] << 16);

                        if (!tmp.ContainsKey(key))
                        {
                            tmp[key] = si;
                            continue;
                        }

                        var elem = tmp[key];

                        if (elem == null)
                        {
                            si.Visible = false;
                            break;
                        }
                        else if (elem.Source.InventorySlotNumber != si.Source.InventorySlotNumber)
                        {
                            if (elem.Source.InventorySlotNumber > si.Source.InventorySlotNumber)
                            {
                                si.Visible = false;
                                break;
                            }

                            // Make other one invisible, and use this one instead
                            elem.Visible = false;
                            tmp[key] = si;
                        }

                    }
                }
            }

            public void Merge(string iSlot, string vSlot, ScriptNode rawSprite)
            {
                var si = new SpriteInstance();

                si.Source = new SpriteSource(rawSprite, new Point(), "", iSlot, vSlot);
                si.Position = new Point(
                    -si.Source.Canvas.CenterX,
                    -si.Source.Canvas.CenterY
                );

                if (Body)
                {
                    BodySprite = si;
                    Body = false;
                }

                si.Group = ExtractMap(rawSprite);

                Sprites.Add(si);
                // Sort sprites on Z index
                Sprites.Sort(delegate (SpriteInstance a, SpriteInstance b) { return a.Source.Z - b.Source.Z; });

                Groups.Add(si.Group);

                Console.WriteLine("Merging {0} {1} groups {2}", iSlot, vSlot, string.Join(", ", si.Group.Select(x => x.Name)));

                MBRValid = false;

                while (true)
                {
                    var a = si.Group;
                    var b = FindGroup(a);

                    if (b == null) break;

                    if (Groups.Count > 0 && Groups[0] == si.Group)
                    {
                        // Swap a and b
                        var c = a;
                        a = b;
                        b = c;
                    }

                    MergeGroup(a, b);
                }
            }

            private List<MAPINFO> FindGroup(List<MAPINFO> MIL)
            {
                foreach (var ours in Groups)
                {
                    if (ours == MIL) continue;

                    foreach (var our in ours)
                    {
                        foreach (var their in MIL)
                        {
                            if (their.Name == our.Name)
                            {
                                return ours;
                            }
                        }
                    }
                }

                return null;
            }

            private void MergeGroup(List<MAPINFO> listA, List<MAPINFO> listB)
            {
                var ret = new List<MAPINFO>();

                var sumA = new Point();
                var sumB = new Point();

                var sumCount = 0;

                if (listB.Count == 0)
                {
                    ret.AddRange(listA);
                }
                else
                {
                    foreach (var a in listA)
                    {
                        // Find a matching entry in B
                        var b = listB.Find(g => g.Name == a.Name);
                        // B doesnt have it, so add it
                        if (b == null)
                        {
                            ret.Add(a);
                            continue;
                        }

                        sumCount++;

                        sumA.Offset(a.Position);
                        sumB.Offset(b.Position);
                    }
                }

                int offsetX = 0;
                int offsetY = 0;

                if (sumCount > 0)
                {
                    // Figure out how far we must offset all the groups with this name
                    // Note: this is basically getting the average offset
                    offsetX = (sumB.X / sumCount) - (sumA.X / sumCount);
                    offsetY = (sumB.Y / sumCount) - (sumA.Y / sumCount);

                    Console.WriteLine("MergeGroup x {0} y {1}", offsetX, offsetY);

                    // Update all entries (make sure we don't modify the class MAPINFO)
                    ret = ret.Select(x =>
                    {
                        Console.WriteLine("Moving {0}", x.Name);
                        return new MAPINFO
                        {
                            Name = x.Name,
                            Position = new Point(
                                x.Position.X + offsetX,
                                x.Position.Y + offsetY
                            ),
                        };
                    }).ToList();
                }

                // Add entries to B
                listB.AddRange(ret);

                // Update sprites
                foreach (var sprite in Sprites)
                {
                    if (sprite.Group != listA) continue;
                    // Update group and position
                    sprite.Group = listB;

                    Console.WriteLine("Moving sprite {0} {1} {2}", sprite.Source.Canvas.GetFullPath(), offsetX, offsetY);
                    sprite.Position = new Point(
                        sprite.Position.X + offsetX,
                        sprite.Position.Y + offsetY
                    );
                }

                Groups.Remove(listA);
            }

            public static List<MAPINFO> ExtractMap(ScriptNode rawSprite)
            {
                var ret = new List<MAPINFO>();
                var map = rawSprite.GetNode("map");
                if (map != null)
                {
                    foreach (var mapNode in map)
                    {
                        var x = mapNode.GetInt32("x");
                        var y = mapNode.GetInt32("y");

                        var x2 = x; // Would otherwise be gotten from Vector2D, but are no-ops for Vector2D
                        var y2 = y;

                        ret.Add(new MAPINFO
                        {
                            Name = mapNode.Name,
                            Position = new Point(
                                (x2 + x) / 2,
                                (y2 + y) / 2
                            ),
                        });
                    }
                }

                return ret;
            }
        }

        // CommonSlot gets the two characters matching in A and B
        private static string CommonSlot(string a, string b)
        {
            string ret = "";
            for (var a_index = 0; a_index < a.Length; a_index += 2)
            {
                for (var b_index = 0; b_index < b.Length; b_index += 2)
                {
                    if (a[a_index] == b[b_index] && a[a_index + 1] == b[b_index + 1])
                    {
                        ret += a[a_index];
                        ret += a[a_index + 1];
                    }
                }
            }

            return ret;
        }

        public Dictionary<string, Point> _positionMap = new Dictionary<string, Point>();

        private Point ProcessPositionFromVector(ScriptNode vec, out bool isNew)
        {
            var x = vec.GetInt32("x");
            var y = vec.GetInt32("y");
            if (!_positionMap.ContainsKey(vec.Name))
            {
                isNew = true;
                return _positionMap[vec.Name] = new Point(x, y);
            }

            isNew = false;
            var curValue = _positionMap[vec.Name];

            return new Point(
                curValue.X - x,
                curValue.Y - y
            );
        }

        private bool blockRendering = false;

        private void RenderCurrentlySelectedItems(object sender, EventArgs e)
        {
            if (blockRendering) return;

            _positionMap.Clear();

            var skinId = 0;
            var stance = "stand1"; // TODO: Fix for 2h
            var luminousLarkness = 3; // Different weapon
            var stanceFrame = 0;
            var stanceUol = stance + "/" + stanceFrame + "/";
            var itemNodes = new List<ScriptNode>();
            var renderedNodes = new Dictionary<string, List<SpriteSource>>();

            string earType = "normal";
            bool foundHidingCap = false;

            foreach (var lbi in selectedItems.Items)
            {
                var ci = (ChosenItem)lbi;
                if (ci.NodePath.Contains("Character/00"))
                {
                    skinId = ci.ID;
                }
                else
                {
                    var node = ci.GetNode(_mainNode);

                    if (node != null)
                    {
                        itemNodes.Add(node);
                    }
                    else
                    {
                        Console.WriteLine("Unable to find {0} for {1}", ci.NodePath, ci.ID);
                    }
                }
            }

            var actionFrame = new ActionFrame();

            var bodyNode = _mainNode.GetNode(string.Format("Character/{0:D8}.img", 2000 + skinId));
            bool isNewPosition;
            ProcessPositionFromVector(bodyNode.GetNode(stanceUol + "/body/map/navel"), out isNewPosition);

            itemNodes.Insert(0, bodyNode);
            itemNodes.Insert(1, _mainNode.GetNode(string.Format("Character/{0:D8}.img", 12000 + skinId)));

            // Load all items and their positions
            foreach (var item in itemNodes)
            {
                // TODO: For Cash Weapon covers items, get the weapon category id thingy
                // its basically the two digits after 1, eg 01[70]2557

                var nodeToRenderUOL = stanceUol;
                if (item.GetFullPath().Contains("Face/"))
                    nodeToRenderUOL = "default";

                ScriptNode nodeToRender = item.GetNode(nodeToRenderUOL);

                if (nodeToRender == null)
                {
                    Console.WriteLine("Unable to find {0} in {1}", nodeToRenderUOL, item.GetFullPath());
                    continue;
                }

                var infoNode = item.GetNode("info");
                if (infoNode == null) continue;

                var larkenessColors = nodeToRender.Get("weapon2") != null;

                foreach (var _imageNode in nodeToRender)
                {
                    var imageNode = _imageNode;
                    var category = imageNode.Name;

                    if (category == "ear" || category == "lefEar" || category == "highlefEar")
                    {
                        if (category != earType) continue;
                    }

                    if (larkenessColors && category != "weapon" + luminousLarkness)
                    {
                        // Skip Luminous coloars
                        continue;
                    }

                    if (category == "hairShade")
                    {
                        // Take subnode, based on the skin color. Go figure
                        // We'll fall back to 0 if not found
                        var skinAsString = skinId.ToString();
                        var tmp = imageNode.GetNode(skinAsString);
                        if (tmp == null) tmp = imageNode.GetNode("0");
                        if (tmp == null)
                        {
                            Console.WriteLine("Unable to find skin entry for hairShade node.");
                            continue;
                        }

                        imageNode = tmp;
                    }

                    var canvas = imageNode.GetCanvas();
                    if (canvas == null) continue;

                    var mapNodes = imageNode.GetNode("map");
                    if (mapNodes == null) continue;

                    var zLayer = imageNode.GetString("z");
                    var curX = 0;
                    var curY = 0;
                    foreach (var mapNode in mapNodes)
                    {
                        var x = mapNode.GetInt32("x");
                        var y = mapNode.GetInt32("y");
                        if (!_positionMap.ContainsKey(mapNode.Name))
                        {
                            _positionMap[mapNode.Name] = new Point(curX + x, curY + y);
                        }
                        else
                        {
                            var existingMap = _positionMap[mapNode.Name];
                            curX = existingMap.X - x;
                            curY = existingMap.Y - y;
                        }
                    }


                    if (!renderedNodes.ContainsKey(zLayer))
                        renderedNodes[zLayer] = new List<SpriteSource>();

                    var vslot = infoNode.GetString("vslot");
                    var islot = infoNode.GetString("islot");

                    // TODO: Set defaults
                    if (vslot == null || islot == null) continue;

                    actionFrame.Merge(islot, vslot, imageNode);


                    renderedNodes[zLayer].Add(new SpriteSource(
                        imageNode,
                        new Point(curX, curY),
                        category,
                        islot,
                        vslot
                    ));

                    if (islot.Contains("Cp") && vslot.Contains("H1"))
                    {
                        foundHidingCap = true;
                    }

                    Console.WriteLine("Adding item {0} category {1} with vslot {2} and islot {3} node path {4}",
                        item.Name, imageNode.Name, vslot, islot, item.GetFullPath());
                }
            }

            actionFrame.UpdateVisibility();


            var bm = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (var g = Graphics.FromImage(bm))
            {
                var centerX = bm.Width / 2;
                var centerY = (int)(bm.Height - (bm.Height * 0.20));

                centerX -= 50;

                foreach (var s in zmap)
                {
                    if (!renderedNodes.ContainsKey(s)) continue;

                    foreach (var data in renderedNodes[s])
                    {
                        var x = data.Position.X - data.Canvas.CenterX;
                        var y = data.Position.Y - data.Canvas.CenterY;

                        Console.WriteLine("Rendering {0} {1} {2}", data.Canvas.GetFullPath(), x, y);

                        x += centerX;
                        y += centerY;

                        if (data.Category == "hairOverHead" && foundHidingCap) continue;

                        g.DrawImage(data.Canvas.Tile, x, y);
                    }
                }

                centerX += 100;
                Console.WriteLine("-- new mode --");

                foreach (var si in actionFrame.Sprites.Where(x => x.Visible))
                {
                    var x = si.Position.X;
                    var y = si.Position.Y;
                    Console.WriteLine("Rendering {0} {1} {2}", si.Source.Canvas.GetFullPath(), x, y);

                    x += centerX;
                    y += centerY;

                    g.DrawImage(si.Source.Canvas.Tile, x, y);
                }

                pictureBox1.Image = bm;
            }
        }

        [Flags]
        public enum Job
        {
            Warrior = 1 << 0,
            Magician = 1 << 1,
            Archer = 1 << 2,
            Thief = 1 << 3,
            Pirate = 1 << 4,
        }

        private bool itemIsJobCompatible(TreeNode tn, Job job)
        {
            var x = GetNode(tn);
            if (x == null) return false;

            Job reqJob = (Job)x.GetInt32("info/reqJob", 0);
            if (reqJob == 0) return true;

            return reqJob.HasFlag(job);
        }


        private bool isCashItem(TreeNode tn)
        {
            var x = GetNode(tn);
            if (x == null) return false;

            return x.GetInt32("info/cash", 0) != 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            selectedItems.Items.Clear();
            // Coat & Pants or Longcoat
            var rnd = new Random();

            blockRendering = true;

            bool longcoat = rnd.Next() % 100 > 50;
            Job job = (Job)(1 << rnd.Next(0, 4));

            bool cashEquips = rnd.Next() % 100 > 50;

            UpdateToolstrip(string.Format("Generating {0} with {1} longcoat with {2} cash equips", job,
                longcoat ? "a" : "no", cashEquips ? "" : "no"));

            foreach (TreeNode tns in itemSelectionTree.Nodes)
            {
                var category = tns.Text;
                if (category == "Longcoat" && !longcoat) continue;
                if ((category == "Coat" || category == "Pants") && longcoat) continue;

                bool noCheckCashEquips = category == "Face" || category == "Skin";

                // Dragons are only for magicians. go figure
                if (job != Job.Magician && category == "Dragon") continue;

                var skipped = 0;
                while (skipped < tns.Nodes.Count)
                {
                    var node = tns.Nodes[rnd.Next() % tns.Nodes.Count];
                    if (itemIsJobCompatible(node, job) &&
                        (noCheckCashEquips || isCashItem(node) == cashEquips))
                    {
                        AddItemSelectionNodeToSelectedNodes(node);
                        break;
                    }
                    else
                    {
                        skipped++;
                    }
                }
            }

            itemSelectionTree.SelectedNode = null;

            blockRendering = false;
            RenderCurrentlySelectedItems(null, null);
        }
    }
}