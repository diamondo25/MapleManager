using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MapleManager.Controls;

namespace MapleManager.Scripts.TextRenderer
{
    public partial class TextRenderForm : Form
    {
        public TextRenderForm()
        {
            InitializeComponent();
        }

        public ScriptNode MainScriptNode { get; set; }

        private void TextRenderForm_Load(object sender, EventArgs e)
        {
            txtIn.Text = "#t4031161##b#c4031161##k/5 ";
            txtIn.Text = @"
Snail: #o100100#
Map 0: #m0#
Item 1000000: #t1000000#
Item 2000000: #t2000000#
Item 3010000: #t3010000#
Item 4000000: #t4000000#
Item 5010000: #t5010000#
Skill 

";
            ParseText();
        }
        
        private void ParseText()
        {
            var tp = new TextParser(txtIn.Text, MainScriptNode, null);
            var results = tp.RenderText(txtOut);
            lbParseResults.Items.Clear();
            foreach (var result in results)
            {
                lbParseResults.Items.Add(result);
            }
        }
        
        private void txtIn_TextChanged(object sender, EventArgs e)
        {
            ParseText();
        }



        private void cbClickDetect_CheckedChanged(object sender, EventArgs e)
        {
            Program.MainForm.tvData.AfterSelect -= TvDataOnAfterSelect;
            if (cbClickDetect.Checked)
            {
                Program.MainForm.tvData.AfterSelect += TvDataOnAfterSelect;
            }

        }

        private void TvDataOnAfterSelect(object sender, TreeViewEventArgs treeViewEventArgs)
        {
            var wtn = treeViewEventArgs.Node as WZTreeNode;
            if (wtn == null) return;

            var str = wtn.WzObject as string;
            if (str != null)
            {
                if (str.Contains('#'))
                {
                    txtIn.Lines = str.Replace("\\r", "").Replace("\\n", "\n").Split('\n');
                    ParseText();
                }
            }
        }

        private void btnOpenScriptFormatInfo_Click(object sender, EventArgs e)
        {
            var tmp = new InfoDialog();
            tmp.MainScriptNode = MainScriptNode;
            tmp.Show();
        }
    }
}
