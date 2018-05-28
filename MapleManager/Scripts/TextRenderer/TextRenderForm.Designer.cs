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
            this.SuspendLayout();
            // 
            // txtOut
            // 
            this.txtOut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtOut.DetectUrls = false;
            this.txtOut.Location = new System.Drawing.Point(12, 223);
            this.txtOut.Name = "txtOut";
            this.txtOut.ReadOnly = true;
            this.txtOut.Size = new System.Drawing.Size(427, 199);
            this.txtOut.TabIndex = 0;
            this.txtOut.Text = "";
            // 
            // txtIn
            // 
            this.txtIn.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIn.Location = new System.Drawing.Point(12, 12);
            this.txtIn.Multiline = true;
            this.txtIn.Name = "txtIn";
            this.txtIn.Size = new System.Drawing.Size(427, 159);
            this.txtIn.TabIndex = 1;
            this.txtIn.TextChanged += new System.EventHandler(this.txtIn_TextChanged);
            // 
            // lbParseResults
            // 
            this.lbParseResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbParseResults.FormattingEnabled = true;
            this.lbParseResults.Location = new System.Drawing.Point(445, 12);
            this.lbParseResults.Name = "lbParseResults";
            this.lbParseResults.Size = new System.Drawing.Size(259, 407);
            this.lbParseResults.TabIndex = 2;
            // 
            // TextRenderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(716, 434);
            this.Controls.Add(this.lbParseResults);
            this.Controls.Add(this.txtIn);
            this.Controls.Add(this.txtOut);
            this.Name = "TextRenderForm";
            this.Text = "TextRenderForm";
            this.Load += new System.EventHandler(this.TextRenderForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtOut;
        private System.Windows.Forms.TextBox txtIn;
        private System.Windows.Forms.ListBox lbParseResults;
    }
}