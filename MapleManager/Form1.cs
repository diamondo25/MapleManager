using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapleManager.Controls;
using MapleManager.Scripts.Animator;
using MapleManager.Validators;
using MapleManager.WzTools;
using MapleManager.WzTools.FileSystem;
using MapleManager.WzTools.Objects;
using MapleManager.WzTools.Package;

namespace MapleManager
{
    public partial class Form1 : Form
    {
        private Dictionary<string, WZTreeNode> Root { get; set; } = new Dictionary<string, WZTreeNode>()
        {
            {"Base", null},
            {"Character", null},
            {"Effect", null},
            {"Etc", null},
            {"Item", null},
            {"Map", null},
            {"Mob", null},
            {"Npc", null},
            {"Reactor", null},
            {"Skill", null},
            {"Sound", null},
            {"String", null},
            {"UI", null},
        };

        private ScriptNode _mainScriptNode { get; }

        private static Encoder GifEncoder;
        private static ImageCodecInfo GifCodecInfo;

        public Form1()
        {
            InitializeComponent();
            ResetTree();

            LoadContentsOfFolder(@"C:\Users\Erwin\Desktop\WzFiles\Data.wz");
            
            _mainScriptNode = new ScriptNode(tvData, null);

        }

        private void ResetTree()
        {
            tvData.Nodes.Clear();
            foreach (var name in Root.Keys.ToList())
            {
                var node = new WZTreeNode();
                node.Name = name;
                node.Text = name;
                node.Tag = new NameSpaceDirectory();
                tvData.Nodes.Add(node);
                Root[name] = node;
            }
        }

        private void tvData_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;
            if (!(e.Node is WZTreeNode wtn)) return;
            var tag = wtn.WzObject;

            tsslPath.Text = e.Node.FullPath;

            TryLoadNode(tvData.SelectedNode as WZTreeNode);
            textBox1.Text = wtn.Text + Environment.NewLine;
            textBox1.Text += wtn.ToolTipText + Environment.NewLine;
            textBox1.Text += Environment.NewLine;
            textBox1.Text += Environment.NewLine;
            textBox1.Text += "Tag: " + tag?.ToString() + Environment.NewLine;
            textBox1.Text += "Tag Type: " + tag?.GetType()?.Name + Environment.NewLine;

            textBox2.Text = textBox1.Text
                .Replace("\\r", "\r")
                .Replace("\\n", "\n")
                .Replace("\\t", "\t");

            object workingObject = tag;
            if (workingObject is WzUOL uol)
            {
                workingObject = uol.ActualObject();
            }


            if (workingObject is WzImage img)
            {
                pbNodeImage.Image = img.Tile;
            }
            else
            {
                pbNodeImage.Image = null;
            }
            
            if (e.Node.Tag is NameSpaceDirectory || e.Node.Tag is NameSpaceFile)
            {
                tvData.ContextMenuStrip = cmsDirectory;
            }
            else if (e.Node.Tag is PcomObject)
            {
                tvData.ContextMenuStrip = cmsPropNode;
            }
            else
            {
                tvData.ContextMenuStrip = null;
            }
        }

        private void InsertDirectories(WZTreeNode parentNode, NameSpaceDirectory folder)
        {
            foreach (var dir in folder.SubDirectories)
            {
                var name = dir.Name;

                WZTreeNode node;

                if (parentNode.Nodes.ContainsKey(name))
                    node = parentNode.Nodes[name] as WZTreeNode;
                else
                {
                    node = new WZTreeNode();
                    node.Name = name;
                    node.Text = name;
                    node.Tag = folder;
                    parentNode.Nodes.Add(node);
                }


                InsertDirectories(node, dir);
            }

            InsertFiles(parentNode, folder);
        }

        private const string DummyNodeName = "---DUMMYNODE---";
        private void InsertFiles(TreeNode parentNode, NameSpaceDirectory folder)
        {
            var files = folder.Files.Where(x => x.Name.EndsWith(".img")).ToDictionary(x => x.Name, x => x);

            foreach (var kvp in files)
            {
                var name = kvp.Key;
                var node = new WZTreeNode();
                node.Name = name;
                node.Text = name;
                node.Tag = kvp.Value;
                parentNode.Nodes.Add(node);

                node.ToolTipText = "-- Not loaded --";
                node.Nodes.Add(DummyNodeName);
            }
        }

        private void LoadContentsOfFolder(string folder)
        {
            if (!Directory.Exists(folder)) return;
            var fs = new WzFileSystem();
            fs.Init(folder);
            LoadContentsSmart(fs);
        }

        private void LoadContentsSmart(WzNameSpace ns)
        {
            // Load in current structure
            foreach (var kvp in Root)
            {
                var existingDir = ns.SubDirectories.Find(x => x.Name == kvp.Key);
                if (existingDir != null)
                {
                    InsertDirectories(kvp.Value, existingDir);
                }
            }

            InsertFiles(Root["Base"], ns);
        }

        private void tsmiLoadDirectory_Click(object sender, EventArgs e)
        {
            var folderBrowserDialog = new FolderSelect.FolderSelectDialog();
            folderBrowserDialog.InitialDirectory = @"C:\Users\Erwin\Desktop\WzFiles\Data.wz";
            if (folderBrowserDialog.ShowDialog() == false) return;
            ResetTree();
            LoadContentsOfFolder(folderBrowserDialog.FileName);
        }

        private void wZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (InfoMessage("This will extract the WZ file? Are you sure?", MessageBoxButtons.OKCancel) ==
                DialogResult.Cancel) return;

            var ofd = new OpenFileDialog();
            ofd.FileName = @"C:\Program Files (x86)\MapleGlobalT_2 - kopie\Data.wz";
            ofd.Filter = "WZ Files|*.wz";

            if (ofd.ShowDialog() != DialogResult.OK) return;


            var folderBrowserDialog = new FolderSelect.FolderSelectDialog();
            if (folderBrowserDialog.ShowDialog() == false) return;
            ExtractWZFile(ofd.FileName, folderBrowserDialog.FileName);

            LoadContentsOfFolder(folderBrowserDialog.FileName);
        }

        private string Prompt(string question)
        {
            return Microsoft.VisualBasic.Interaction.InputBox(question, "MapleManager");
        }

        private DialogResult InfoMessage(string msg, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            return MessageBox.Show(msg, "MapleManager", buttons);
        }

        private DialogResult ErrorMessage(string msg)
        {
            return MessageBox.Show(msg, "MapleManager", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void extractWZToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.FileName = @"C:\Program Files (x86)\MapleGlobalT_2 - kopie\Data.wz";
            ofd.Filter = "WZ Files|*.wz";

            if (ofd.ShowDialog() != DialogResult.OK) return;

            var folderBrowserDialog = new FolderSelect.FolderSelectDialog();
            if (folderBrowserDialog.ShowDialog() == false) return;

            ExtractWZFile(ofd.FileName, folderBrowserDialog.FileName);
        }

        private WzPackage ExtractWZFile(string wzFile, string extractPath)
        {
            var key = Prompt("WZ Key?");

            var fsp = new WzPackage(wzFile, key);

            try
            {
                fsp.Process();
            }
            catch (Exception ex)
            {
                ErrorMessage($"Exception occurred while loading file: {ex}");
                return null;
            }


            fsp.Extract(extractPath);

            InfoMessage("Done extracting!");
            return fsp;
        }

        private bool LoadScript(out IScript scriptInterface, params string[] filenames)
        {
            try
            {
                var script = Scripting.Scripts.CompileScript(filenames);

                if (script.Errors.Count > 0)
                {
                    var errorlines = Environment.NewLine;
                    foreach (CompilerError error in script.Errors)
                    {
                        errorlines += "[" + (error.IsWarning ? "W" : "E") + "]";
                        errorlines += $"{error.FileName}:{error.Line}.{error.Column} : {error.ErrorText} ({error.ErrorNumber})" + Environment.NewLine;
                    }
                    throw new Exception(errorlines);
                }

                scriptInterface = Scripting.Scripts.FindInterface<IScript>(script.CompiledAssembly);
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage($"Unable to compile scripts...\r\n\r\n{ex}");
            }
            scriptInterface = null;
            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //var anim = new Animator();
            //anim.Start(_mainScriptNode);

            var tsmi = scriptsToolStripMenuItem;

            void addScript(string scriptName, string path)
            {
                var mi = new ToolStripMenuItem
                {
                    Text = scriptName,
                    Name = path
                };


                var recompileItem = new ToolStripMenuItem
                {
                    Text = "Recompile",
                    Tag = mi,
                };
                recompileItem.Click += RecompileScriptItem;
                mi.DropDownItems.Add(recompileItem);

                var startItem = new ToolStripMenuItem
                {
                    Text = "Start",
                    Tag = mi,
                };
                startItem.Click += RunScriptItem;
                mi.DropDownItems.Add(startItem);


                var stopItem = new ToolStripMenuItem
                {
                    Text = "Stop",
                    Tag = mi,
                };
                stopItem.Click += StopScriptItem;
                mi.DropDownItems.Add(stopItem);

                RecompileScriptItem(recompileItem, null);

                tsmi.DropDownItems.Add(mi);
            }

            foreach (var scriptFile in new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Scripts")).GetFiles("*.cs"))
            {
                addScript(scriptFile.Name, scriptFile.FullName);
            }

            foreach (var scriptDir in new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Scripts")).GetDirectories())
            {
                addScript(scriptDir.Name, scriptDir.FullName);
            }
        }

        private void RecompileScriptItem(object sender, EventArgs eventArgs)
        {
            var mi = sender as ToolStripMenuItem;
            var pmi = mi.Tag as ToolStripMenuItem;

            string[] files;
            if (Directory.Exists(pmi.Name))
                files = new DirectoryInfo(pmi.Name).GetFiles("*.cs", SearchOption.AllDirectories).Select(x => x.FullName).ToArray();
            else
                files = new string[] { pmi.Name };


            if (LoadScript(out var script, files))
            {
                pmi.Tag = script;
            }

            Trace.WriteLine("hurr");
        }


        private void RunScriptItem(object sender, EventArgs eventArgs)
        {
            var mi = sender as ToolStripMenuItem;
            var pmi = mi.Tag as ToolStripMenuItem;

            if (pmi.Tag is IScript script)
            {
                try
                {
                    script.Start(_mainScriptNode);
                }
                catch (Exception ex)
                {
                    ErrorMessage($"Unable to run script.\r\n\r\n{ex}");
                }
            }
            Trace.WriteLine("hurr");
        }

        private void StopScriptItem(object sender, EventArgs eventArgs)
        {
            var mi = sender as ToolStripMenuItem;
            var pmi = mi.Tag as ToolStripMenuItem;

            if (pmi.Tag is IScript script)
            {
                script.Stop();
            }
            Trace.WriteLine("durr");
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }


        private bool HasDummyNode(TreeNode parentNode)
        {
            if (parentNode.Nodes.Count == 0) return false;
            if (parentNode.Nodes[0].Text == DummyNodeName) return true;
            return false;
        }

        private void AddDummyNode(TreeNode parentNode)
        {
            parentNode.Nodes.Add(DummyNodeName);
        }

        private void NameNonPcomObject(TreeNode parentNode, object obj)
        {
            parentNode.ToolTipText = obj?.ToString();
        }

        private void FinalTryBeforeDummyNode(TreeNode parentNode, object obj)
        {
            if (obj is PcomObject)
                AddDummyNode(parentNode);
            else
                NameNonPcomObject(parentNode, obj);
        }

        private int deep = 0;
        private void BuildTreeFromData(TreeNode parentNode, object obj, bool recursive = false)
        {
            deep++;
            if (!recursive || deep >= 10)
                recursive = false;

            object nameNode = null;
            if (obj is WzProperty prop)
            {
                parentNode.ToolTipText = "Property";
                foreach (var kvp in prop)
                {
                    var subNode = parentNode.Nodes.Add(kvp.Key, kvp.Key);
                    subNode.Tag = kvp.Value;
                    if (recursive)
                        BuildTreeFromData(subNode, kvp.Value, recursive);
                    else
                        FinalTryBeforeDummyNode(subNode, kvp.Value);
                }

                string mapName = null, streetName = null, name = null,
                     id = null, type = null;
                TreeNode tn;
                if ((tn = parentNode.Nodes["name"]) != null) name = tn.Tag as string;
                if ((tn = parentNode.Nodes["streetName"]) != null) streetName = tn.Tag as string;
                if ((tn = parentNode.Nodes["mapName"]) != null) mapName = tn.Tag as string;
                if ((tn = parentNode.Nodes["id"]) != null) id = tn.Tag as string;
                if ((tn = parentNode.Nodes["type"]) != null) type = tn.Tag as string;

                if (mapName != null && name == null)
                {
                    name = mapName;
                    if (streetName != null) name += " - " + streetName;
                }

                if (name != null)
                    parentNode.Text += ": " + name;

                if (id != null)
                    parentNode.Text += " (id: " + id + ")";
                if (type != null)
                    parentNode.Text += " (type: " + type + ")";

            }
            else if (obj is WzList list)
            {
                parentNode.ToolTipText = obj.ToString();
                for (var i = 0; i < list.ChildCount; i++)
                {
                    var name = i.ToString();
                    var elem = list[name];
                    var subNode = parentNode.Nodes.Add(name, name);
                    subNode.Tag = elem;

                    if (recursive)
                        BuildTreeFromData(subNode, elem, recursive);
                    else
                        FinalTryBeforeDummyNode(subNode, elem);

                }
            }
            else if (obj is WzVector2D vector)
            {
                parentNode.ToolTipText = "Vector2D";
                foreach (var name in new string[] { "X", "Y" })
                {
                    var subNode = parentNode.Nodes.Add(name, name);
                    subNode.Tag = vector[name];
                    subNode.ToolTipText = subNode.Tag.ToString();
                }
            }
            else if (obj is WzUOL uol)
            {
                object curObject = uol;
                bool firstIter = true;

                parentNode.ToolTipText = "";
                parentNode.Text += " (UOL: ";
                bool invalid = false;
                while (curObject is WzUOL uolObj)
                {
                    var actualObject = uolObj.ActualObject();
                    parentNode.ToolTipText += "UOL: " + uolObj.Path + Environment.NewLine;
                    parentNode.ToolTipText += "Actual Path: " + uolObj.ActualPath() + Environment.NewLine;
                    parentNode.ToolTipText += "Actual Object: " + actualObject + Environment.NewLine;

                    if (actualObject == null)
                    {
                        parentNode.ToolTipText += "!!! OBJECT DOESNT EXIST !!!" + Environment.NewLine;
                        invalid = true;
                    }

                    if (!firstIter) parentNode.Text += " -> ";
                    firstIter = false;
                    parentNode.Text += uol.Path;

                    curObject = actualObject;
                }

                parentNode.Text += ")";

                if (invalid) parentNode.Text += " ERROR";
            }
            else
            {
                NameNonPcomObject(parentNode, obj);
            }

            var infoLinkNode = parentNode.Nodes["info"]?.Nodes["link"];
            if (infoLinkNode != null)
                parentNode.Text += " (link: " + infoLinkNode.Tag + ")";
            deep--;
        }


        private void tvData_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TryLoadNode(tvData.SelectedNode as WZTreeNode);
        }

        private void TryLoadNode(WZTreeNode node)
        {

            if (node == null) return;
            if (node.Tag is NameSpaceFile nsf)
            {
                if (!HasDummyNode(node)) return;
                Trace.WriteLine("Loading: " + node.FullPath);
                node.Nodes.Clear();

                try
                {
                    var obj = nsf.Object;

                    tvData.BeginUpdate();
                    node.WzObject = obj;
                    node.UpdateData();
                    tvData.EndUpdate();
                }
                catch (NotImplementedException ex)
                {
                    ErrorMessage($"Unable to load {node.Name}... {ex}");
                }

            }
        }

        private void tvData_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TryLoadNode(e.Node as WZTreeNode);
        }


        private IEnumerable<TreeNode> GetAllUnloadedNodes(TreeNode parent)
        {
            if (parent.Tag is NameSpaceFile)
            {
                if (parent.Nodes.Count == 0 ||
                    parent.Nodes[0].Text != DummyNodeName)
                    yield break;
                yield return parent;
                yield break;
            }

            //Trace.WriteLine(parent.FullPath);
            foreach (TreeNode subNode in parent.Nodes)
            {
                foreach (var allUnloadedNode in GetAllUnloadedNodes(subNode))
                {
                    yield return allUnloadedNode;
                }
            }

        }

        private void uOLsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvData.BeginUpdate();
            var context = TaskScheduler.FromCurrentSynchronizationContext();
            var tasks = new List<Task>();

            var nodeReinserts = new TreeNode[tvData.Nodes.Count];


            foreach (TreeNode tvDataNode in tvData.Nodes)
            {
                var idx = tvDataNode.Index;
                nodeReinserts[idx] = tvDataNode;
            }
            tvData.Nodes.Clear();

            foreach (var kvp in nodeReinserts)
            {
                var tvDataNode = kvp;

                foreach (var node in GetAllUnloadedNodes(tvDataNode))
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        var tn = new TreeNode();
                        tn.Tag = node.Tag;
                        tn.Text = node.Text;
                        tn.Name = node.Name;
                        BuildTreeFromData(tn, (node.Tag as NameSpaceFile).Object);

                        int index = node.Index;
                        var parent = node.Parent;
                        parent.Nodes[index] = tn;

                    }));
                }
            }

            Task.WaitAll(tasks.ToArray());
            Trace.WriteLine($"All nodes loaded {tasks.Count}. Inserting");

            tvData.Nodes.AddRange(nodeReinserts);

            Trace.WriteLine($"Done!");
            tvData.EndUpdate();
        }
        
        private void btnGoToUOL_Click(object sender, EventArgs e)
        {
            var node = tvData.SelectedNode;
            if (node?.Tag is WzUOL uol)
            {
                node = node.Parent;
                foreach (var element in uol.Path.Split('/'))
                {
                    if (node == null)
                    {
                        ErrorMessage("Unable to resolve UOL!!!");
                        return;
                    }
                    if (element == "..") node = node.Parent;
                    else if (element == ".") continue;
                    else node = node.Nodes[element];
                }
                if (node == null)
                {
                    ErrorMessage("Unable to resolve UOL!!!");
                    return;
                }

                node.EnsureVisible();
                tvData.SelectedNode = node;
            }
        }

        private void fullyLoadThisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvData.SelectedNode?.Tag is NameSpaceDirectory)
            {
                var nodesToLoad = new List<WZTreeNode>();

                void loadNodes(WZTreeNode tn)
                {
                    foreach (TreeNode subNode in tn.Nodes)
                    {
                        if (!(subNode is WZTreeNode wtn)) continue;
                        
                        if (subNode.Tag is NameSpaceDirectory)
                            loadNodes(wtn);
                        else if (subNode.Tag is NameSpaceFile && HasDummyNode(subNode))
                            nodesToLoad.Add(wtn);
                    }
                }
                loadNodes(tvData.SelectedNode as WZTreeNode);



                tvData.BeginUpdate();

                foreach (var node in nodesToLoad)
                {
                    var nsf = node.Tag as FSFile;
                    var obj = nsf.Object;
                    node.WzObject = obj;
                    node.Nodes.Clear();
                    
                    node.UpdateData();
                }
                tvData.EndUpdate();
            }
        }

        private void copyImageToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pbNodeImage.Image == null) return;

            var image = pbNodeImage.Image;
            image.CopyMultiFormatBitmapToClipboard();
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pbNodeImage.Image == null) return;
            var sfd = new SaveFileDialog();
            sfd.Filter = "PNG|*.png";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                pbNodeImage.Image.Save(sfd.FileName, ImageFormat.Png);
            }
        }
    }
}
