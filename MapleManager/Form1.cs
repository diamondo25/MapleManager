using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
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
            {"Morph", null},
            {"Npc", null},
            {"Quest", null},
            {"Reactor", null},
            {"Skill", null},
            {"Sound", null},
            {"String", null},
            {"TamingMob", null},
            {"UI", null},
        };

        private ScriptNode _mainScriptNode { get; }

        public Form1()
        {
            InitializeComponent();
            ResetTree();

            LoadContentsOfFolder(@"C:\Users\Erwin\Desktop\WzFiles\Data.wz");

            _mainScriptNode = new ScriptNode(tvData, null);

        }

        private void Form1_Load(object sender, EventArgs e)
        {

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

            var anim = new Animator();
            anim.Start(_mainScriptNode);
        }


        private void ResetTree()
        {
            tvData.Nodes.Clear();
            foreach (var name in Root.Keys.ToList())
            {
                var node = new WZTreeNode();
                node.Name = name;
                node.Text = name;
                node.Tag = new NameSpaceDirectory
                {
                    TreeNode = node
                };
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
                .Replace("\\r", "")
                .Replace("\\n", "\r\n")
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
                kvp.Value.TreeNode = node;
                node.SetNotLoaded();

                parentNode.Nodes.Add(node);
            }
        }

        private void LoadContentsOfFolder(string folder)
        {
            if (!Directory.Exists(folder)) return;
            var fs = new WzFileSystem();
            fs.Init(folder);
            BeginTreeUpdate();
            LoadContentsSmart(fs);
            EndTreeUpdate();
        }

        private void LoadContentsSmart(WzNameSpace ns)
        {
            // Load in current structure
            foreach (var kvp in Root)
            {
                foreach (var existingDir in ns.SubDirectories.FindAll(x => x.Name.StartsWith(kvp.Key)))
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
            ofd.Multiselect = true;

            if (ofd.ShowDialog() != DialogResult.OK) return;


            var folderBrowserDialog = new FolderSelect.FolderSelectDialog();
            if (folderBrowserDialog.ShowDialog() == false) return;
            ExtractWZFile(folderBrowserDialog.FileName, ofd.FileNames);

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
            ofd.Multiselect = true;

            if (ofd.ShowDialog() != DialogResult.OK) return;

            var folderBrowserDialog = new FolderSelect.FolderSelectDialog();
            if (folderBrowserDialog.ShowDialog() == false) return;

            ExtractWZFile(folderBrowserDialog.FileName, ofd.FileNames);
        }

        private void ExtractWZFile(string extractPath, params string[] wzFiles)
        {
            var key = Prompt("WZ Key?");

            foreach (var wzFile in wzFiles)
            {
                var fsp = new WzPackage(wzFile, key);

                try
                {
                    fsp.Process();
                }
                catch (Exception ex)
                {
                    ErrorMessage($"Exception occurred while loading file: {ex}");
                    return;
                }


                fsp.Extract(extractPath);

            }
            InfoMessage("Done extracting!");
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
#if DEBUG
                script.Start(_mainScriptNode);
#else
                try
                {
                    script.Start(_mainScriptNode);
                }
                catch (Exception ex)
                {
                    ErrorMessage($"Unable to run script.\r\n\r\n{ex}");
                }
#endif
                EndTreeUpdate();
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

        private void tvData_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TryLoadNode(tvData.SelectedNode as WZTreeNode);
        }

        public void TryLoadNode(WZTreeNode node)
        {
            if (node == null) return;
            try
            {
                // tvData.BeginUpdate();
                node.TryLoad(false);
                // tvData.EndUpdate();
            }
            catch (NotImplementedException ex)
            {
                ErrorMessage($"Unable to load {node.Name}... {ex}");
            }
        }

        public void BeginTreeUpdate()
        {
            tvData.BeginUpdate();
        }

        public void EndTreeUpdate()
        {
            tvData.EndUpdate();
        }

        private void tvData_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TryLoadNode(e.Node as WZTreeNode);
        }

        private void uOLsToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                        else if (subNode.Tag is NameSpaceFile && wtn.IsNotLoaded())
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
