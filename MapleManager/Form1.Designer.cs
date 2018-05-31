namespace MapleManager
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("00002000.img");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Character", new System.Windows.Forms.TreeNode[] {
            treeNode1});
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Effect");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.scDataTreeAndContent = new System.Windows.Forms.SplitContainer();
            this.tvData = new System.Windows.Forms.TreeView();
            this.scTextBoxes = new System.Windows.Forms.SplitContainer();
            this.txtInfoBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtInfoBoxNormalized = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pbNodeImage = new System.Windows.Forms.PictureBox();
            this.btnGoToUOL = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiLoadDirectory = new System.Windows.Forms.ToolStripMenuItem();
            this.extractWZToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolStripSeperator = new System.Windows.Forms.ToolStripSeparator();
            this.modifyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.cutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.copyImageToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.validateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.itemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uOLsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslPath = new System.Windows.Forms.ToolStripStatusLabel();
            this.cmsPropNode = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmsDirectory = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullyLoadThisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scDataTreeAndContent)).BeginInit();
            this.scDataTreeAndContent.Panel1.SuspendLayout();
            this.scDataTreeAndContent.Panel2.SuspendLayout();
            this.scDataTreeAndContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scTextBoxes)).BeginInit();
            this.scTextBoxes.Panel1.SuspendLayout();
            this.scTextBoxes.Panel2.SuspendLayout();
            this.scTextBoxes.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbNodeImage)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.cmsDirectory.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 24);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(892, 541);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.scDataTreeAndContent);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(884, 515);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Data";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // scDataTreeAndContent
            // 
            this.scDataTreeAndContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scDataTreeAndContent.Location = new System.Drawing.Point(3, 3);
            this.scDataTreeAndContent.Name = "scDataTreeAndContent";
            // 
            // scDataTreeAndContent.Panel1
            // 
            this.scDataTreeAndContent.Panel1.Controls.Add(this.tvData);
            this.scDataTreeAndContent.Panel1MinSize = 100;
            // 
            // scDataTreeAndContent.Panel2
            // 
            this.scDataTreeAndContent.Panel2.Controls.Add(this.scTextBoxes);
            this.scDataTreeAndContent.Panel2.Controls.Add(this.panel1);
            this.scDataTreeAndContent.Panel2.Controls.Add(this.btnGoToUOL);
            this.scDataTreeAndContent.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
            this.scDataTreeAndContent.Size = new System.Drawing.Size(878, 509);
            this.scDataTreeAndContent.SplitterDistance = 158;
            this.scDataTreeAndContent.SplitterWidth = 10;
            this.scDataTreeAndContent.TabIndex = 0;
            this.scDataTreeAndContent.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.scDataTreeAndContent_SplitterMoved);
            // 
            // tvData
            // 
            this.tvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvData.FullRowSelect = true;
            this.tvData.HideSelection = false;
            this.tvData.LabelEdit = true;
            this.tvData.Location = new System.Drawing.Point(0, 0);
            this.tvData.Name = "tvData";
            treeNode1.Name = "Node2";
            treeNode1.Text = "00002000.img";
            treeNode2.Name = "Node0";
            treeNode2.Text = "Character";
            treeNode3.Name = "Node1";
            treeNode3.Text = "Effect";
            this.tvData.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode2,
            treeNode3});
            this.tvData.PathSeparator = "/";
            this.tvData.ShowNodeToolTips = true;
            this.tvData.Size = new System.Drawing.Size(158, 509);
            this.tvData.TabIndex = 0;
            this.tvData.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvData_BeforeExpand);
            this.tvData.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvData_BeforeSelect);
            this.tvData.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.tvData.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tvData_MouseDoubleClick);
            // 
            // scTextBoxes
            // 
            this.scTextBoxes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scTextBoxes.IsSplitterFixed = true;
            this.scTextBoxes.Location = new System.Drawing.Point(3, 3);
            this.scTextBoxes.Name = "scTextBoxes";
            // 
            // scTextBoxes.Panel1
            // 
            this.scTextBoxes.Panel1.Controls.Add(this.txtInfoBox);
            this.scTextBoxes.Panel1.Controls.Add(this.label1);
            // 
            // scTextBoxes.Panel2
            // 
            this.scTextBoxes.Panel2.Controls.Add(this.txtInfoBoxNormalized);
            this.scTextBoxes.Panel2.Controls.Add(this.label2);
            this.scTextBoxes.Size = new System.Drawing.Size(702, 214);
            this.scTextBoxes.SplitterDistance = 350;
            this.scTextBoxes.TabIndex = 6;
            // 
            // txtInfoBox
            // 
            this.txtInfoBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfoBox.Location = new System.Drawing.Point(3, 16);
            this.txtInfoBox.Multiline = true;
            this.txtInfoBox.Name = "txtInfoBox";
            this.txtInfoBox.ReadOnly = true;
            this.txtInfoBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtInfoBox.Size = new System.Drawing.Size(344, 195);
            this.txtInfoBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Raw:";
            // 
            // txtInfoBoxNormalized
            // 
            this.txtInfoBoxNormalized.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfoBoxNormalized.Location = new System.Drawing.Point(3, 16);
            this.txtInfoBoxNormalized.Multiline = true;
            this.txtInfoBoxNormalized.Name = "txtInfoBoxNormalized";
            this.txtInfoBoxNormalized.ReadOnly = true;
            this.txtInfoBoxNormalized.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtInfoBoxNormalized.Size = new System.Drawing.Size(342, 195);
            this.txtInfoBoxNormalized.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Cleaned:";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.BackgroundImage = global::MapleManager.Properties.Resources.image_bg;
            this.panel1.Controls.Add(this.pbNodeImage);
            this.panel1.Location = new System.Drawing.Point(3, 252);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(702, 254);
            this.panel1.TabIndex = 5;
            // 
            // pbNodeImage
            // 
            this.pbNodeImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbNodeImage.Location = new System.Drawing.Point(0, 0);
            this.pbNodeImage.Name = "pbNodeImage";
            this.pbNodeImage.Size = new System.Drawing.Size(702, 254);
            this.pbNodeImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbNodeImage.TabIndex = 0;
            this.pbNodeImage.TabStop = false;
            // 
            // btnGoToUOL
            // 
            this.btnGoToUOL.Location = new System.Drawing.Point(3, 223);
            this.btnGoToUOL.Name = "btnGoToUOL";
            this.btnGoToUOL.Size = new System.Drawing.Size(158, 23);
            this.btnGoToUOL.TabIndex = 4;
            this.btnGoToUOL.Text = "Go to reference of UOL";
            this.btnGoToUOL.UseVisualStyleBackColor = true;
            this.btnGoToUOL.Click += new System.EventHandler(this.btnGoToUOL_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.modifyToolStripMenuItem,
            this.toolsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(892, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.extractWZToolStripMenuItem,
            this.fileToolStripSeperator});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wZToolStripMenuItem,
            this.tsmiLoadDirectory});
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.loadToolStripMenuItem.Text = "Load";
            // 
            // wZToolStripMenuItem
            // 
            this.wZToolStripMenuItem.Name = "wZToolStripMenuItem";
            this.wZToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.wZToolStripMenuItem.Text = "WZ";
            this.wZToolStripMenuItem.Click += new System.EventHandler(this.wZToolStripMenuItem_Click);
            // 
            // tsmiLoadDirectory
            // 
            this.tsmiLoadDirectory.Name = "tsmiLoadDirectory";
            this.tsmiLoadDirectory.Size = new System.Drawing.Size(122, 22);
            this.tsmiLoadDirectory.Text = "Directory";
            this.tsmiLoadDirectory.Click += new System.EventHandler(this.tsmiLoadDirectory_Click);
            // 
            // extractWZToolStripMenuItem
            // 
            this.extractWZToolStripMenuItem.Name = "extractWZToolStripMenuItem";
            this.extractWZToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.extractWZToolStripMenuItem.Text = "Extract WZ";
            this.extractWZToolStripMenuItem.Click += new System.EventHandler(this.extractWZToolStripMenuItem_Click);
            // 
            // fileToolStripSeperator
            // 
            this.fileToolStripSeperator.Name = "fileToolStripSeperator";
            this.fileToolStripSeperator.Size = new System.Drawing.Size(127, 6);
            // 
            // modifyToolStripMenuItem
            // 
            this.modifyToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem1,
            this.cutToolStripMenuItem1,
            this.pasteToolStripMenuItem1,
            this.copyImageToClipboardToolStripMenuItem,
            this.saveImageToolStripMenuItem});
            this.modifyToolStripMenuItem.Name = "modifyToolStripMenuItem";
            this.modifyToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.modifyToolStripMenuItem.Text = "Edit";
            // 
            // copyToolStripMenuItem1
            // 
            this.copyToolStripMenuItem1.Enabled = false;
            this.copyToolStripMenuItem1.Name = "copyToolStripMenuItem1";
            this.copyToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem1.Size = new System.Drawing.Size(205, 22);
            this.copyToolStripMenuItem1.Text = "Copy";
            // 
            // cutToolStripMenuItem1
            // 
            this.cutToolStripMenuItem1.Enabled = false;
            this.cutToolStripMenuItem1.Name = "cutToolStripMenuItem1";
            this.cutToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.cutToolStripMenuItem1.Size = new System.Drawing.Size(205, 22);
            this.cutToolStripMenuItem1.Text = "Cut";
            // 
            // pasteToolStripMenuItem1
            // 
            this.pasteToolStripMenuItem1.Enabled = false;
            this.pasteToolStripMenuItem1.Name = "pasteToolStripMenuItem1";
            this.pasteToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.pasteToolStripMenuItem1.Size = new System.Drawing.Size(205, 22);
            this.pasteToolStripMenuItem1.Text = "Paste";
            // 
            // copyImageToClipboardToolStripMenuItem
            // 
            this.copyImageToClipboardToolStripMenuItem.Name = "copyImageToClipboardToolStripMenuItem";
            this.copyImageToClipboardToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.copyImageToClipboardToolStripMenuItem.Text = "Copy image to clipboard";
            this.copyImageToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyImageToClipboardToolStripMenuItem_Click);
            // 
            // saveImageToolStripMenuItem
            // 
            this.saveImageToolStripMenuItem.Name = "saveImageToolStripMenuItem";
            this.saveImageToolStripMenuItem.Size = new System.Drawing.Size(205, 22);
            this.saveImageToolStripMenuItem.Text = "Save image";
            this.saveImageToolStripMenuItem.Click += new System.EventHandler(this.saveImageToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.validateToolStripMenuItem,
            this.scriptsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // validateToolStripMenuItem
            // 
            this.validateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mapsToolStripMenuItem,
            this.itemsToolStripMenuItem,
            this.uOLsToolStripMenuItem});
            this.validateToolStripMenuItem.Name = "validateToolStripMenuItem";
            this.validateToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.validateToolStripMenuItem.Text = "Validate";
            // 
            // mapsToolStripMenuItem
            // 
            this.mapsToolStripMenuItem.Name = "mapsToolStripMenuItem";
            this.mapsToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.mapsToolStripMenuItem.Text = "Maps";
            // 
            // itemsToolStripMenuItem
            // 
            this.itemsToolStripMenuItem.Name = "itemsToolStripMenuItem";
            this.itemsToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.itemsToolStripMenuItem.Text = "Items";
            // 
            // uOLsToolStripMenuItem
            // 
            this.uOLsToolStripMenuItem.Name = "uOLsToolStripMenuItem";
            this.uOLsToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.uOLsToolStripMenuItem.Text = "UOLs";
            this.uOLsToolStripMenuItem.Click += new System.EventHandler(this.uOLsToolStripMenuItem_Click);
            // 
            // scriptsToolStripMenuItem
            // 
            this.scriptsToolStripMenuItem.Name = "scriptsToolStripMenuItem";
            this.scriptsToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.scriptsToolStripMenuItem.Text = "Scripts";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.tsslPath});
            this.statusStrip1.Location = new System.Drawing.Point(0, 565);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(892, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(34, 17);
            this.toolStripStatusLabel1.Text = "Path:";
            // 
            // tsslPath
            // 
            this.tsslPath.Name = "tsslPath";
            this.tsslPath.Size = new System.Drawing.Size(0, 17);
            // 
            // cmsPropNode
            // 
            this.cmsPropNode.Name = "cmsPropNode";
            this.cmsPropNode.Size = new System.Drawing.Size(61, 4);
            // 
            // cmsDirectory
            // 
            this.cmsDirectory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.cutToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.fullyLoadThisToolStripMenuItem});
            this.cmsDirectory.Name = "cmsDirectory";
            this.cmsDirectory.Size = new System.Drawing.Size(148, 114);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            // 
            // fullyLoadThisToolStripMenuItem
            // 
            this.fullyLoadThisToolStripMenuItem.Name = "fullyLoadThisToolStripMenuItem";
            this.fullyLoadThisToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
            this.fullyLoadThisToolStripMenuItem.Text = "Fully load this";
            this.fullyLoadThisToolStripMenuItem.Click += new System.EventHandler(this.fullyLoadThisToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(892, 587);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "MapleManager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.scDataTreeAndContent.Panel1.ResumeLayout(false);
            this.scDataTreeAndContent.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scDataTreeAndContent)).EndInit();
            this.scDataTreeAndContent.ResumeLayout(false);
            this.scTextBoxes.Panel1.ResumeLayout(false);
            this.scTextBoxes.Panel1.PerformLayout();
            this.scTextBoxes.Panel2.ResumeLayout(false);
            this.scTextBoxes.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scTextBoxes)).EndInit();
            this.scTextBoxes.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbNodeImage)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.cmsDirectory.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.SplitContainer scDataTreeAndContent;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem validateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mapsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uOLsToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wZToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsmiLoadDirectory;
        private System.Windows.Forms.ToolStripMenuItem extractWZToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmsPropNode;
        private System.Windows.Forms.ToolStripMenuItem modifyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem1;
        private System.Windows.Forms.ContextMenuStrip cmsDirectory;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.TextBox txtInfoBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtInfoBoxNormalized;
        private System.Windows.Forms.Button btnGoToUOL;
        private System.Windows.Forms.ToolStripMenuItem fullyLoadThisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scriptsToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel tsslPath;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pbNodeImage;
        private System.Windows.Forms.SplitContainer scTextBoxes;
        private System.Windows.Forms.ToolStripMenuItem copyImageToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveImageToolStripMenuItem;
        public System.Windows.Forms.TreeView tvData;
        private System.Windows.Forms.ToolStripSeparator fileToolStripSeperator;
    }
}

