namespace MapleManager.Scripts.TextRenderer
{
    partial class TextRenderForm
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
            this.txtOut = new System.Windows.Forms.RichTextBox();
            this.txtIn = new System.Windows.Forms.TextBox();
            this.lbParseResults = new System.Windows.Forms.ListBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.cbClickDetect = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtOut
            // 
            this.txtOut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtOut.DetectUrls = false;
            this.txtOut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOut.Location = new System.Drawing.Point(0, 0);
            this.txtOut.Name = "txtOut";
            this.txtOut.ReadOnly = true;
            this.txtOut.Size = new System.Drawing.Size(411, 209);
            this.txtOut.TabIndex = 0;
            this.txtOut.Text = "";
            // 
            // txtIn
            // 
            this.txtIn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtIn.Location = new System.Drawing.Point(0, 0);
            this.txtIn.Multiline = true;
            this.txtIn.Name = "txtIn";
            this.txtIn.Size = new System.Drawing.Size(411, 212);
            this.txtIn.TabIndex = 1;
            this.txtIn.TextChanged += new System.EventHandler(this.txtIn_TextChanged);
            // 
            // lbParseResults
            // 
            this.lbParseResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbParseResults.FormattingEnabled = true;
            this.lbParseResults.Location = new System.Drawing.Point(429, 38);
            this.lbParseResults.Name = "lbParseResults";
            this.lbParseResults.Size = new System.Drawing.Size(259, 420);
            this.lbParseResults.TabIndex = 2;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 38);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtIn);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtOut);
            this.splitContainer1.Size = new System.Drawing.Size(411, 425);
            this.splitContainer1.SplitterDistance = 212;
            this.splitContainer1.TabIndex = 3;
            // 
            // cbClickDetect
            // 
            this.cbClickDetect.AutoSize = true;
            this.cbClickDetect.Location = new System.Drawing.Point(12, 12);
            this.cbClickDetect.Name = "cbClickDetect";
            this.cbClickDetect.Size = new System.Drawing.Size(237, 17);
            this.cbClickDetect.TabIndex = 4;
            this.cbClickDetect.Text = "Auto detect selected node as NPC text node";
            this.cbClickDetect.UseVisualStyleBackColor = true;
            this.cbClickDetect.CheckedChanged += new System.EventHandler(this.cbClickDetect_CheckedChanged);
            // 
            // TextRenderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 478);
            this.Controls.Add(this.cbClickDetect);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.lbParseResults);
            this.Name = "TextRenderForm";
            this.Text = "TextRenderForm";
            this.Load += new System.EventHandler(this.TextRenderForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtOut;
        private System.Windows.Forms.TextBox txtIn;
        private System.Windows.Forms.ListBox lbParseResults;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.CheckBox cbClickDetect;
    }
}