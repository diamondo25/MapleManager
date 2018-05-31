namespace MapleManager.Scripts.TextRenderer
{
    partial class InfoDialog
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
            this.txtExplanation = new System.Windows.Forms.TextBox();
            this.cbOptions = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.rtbExample = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtExplanation
            // 
            this.txtExplanation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExplanation.Location = new System.Drawing.Point(12, 39);
            this.txtExplanation.Multiline = true;
            this.txtExplanation.Name = "txtExplanation";
            this.txtExplanation.ReadOnly = true;
            this.txtExplanation.Size = new System.Drawing.Size(564, 303);
            this.txtExplanation.TabIndex = 1;
            // 
            // cbOptions
            // 
            this.cbOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbOptions.FormattingEnabled = true;
            this.cbOptions.Location = new System.Drawing.Point(12, 12);
            this.cbOptions.Name = "cbOptions";
            this.cbOptions.Size = new System.Drawing.Size(564, 21);
            this.cbOptions.TabIndex = 2;
            this.cbOptions.SelectedValueChanged += new System.EventHandler(this.cbOptions_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 345);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Example:";
            // 
            // rtbExample
            // 
            this.rtbExample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbExample.DetectUrls = false;
            this.rtbExample.Location = new System.Drawing.Point(12, 361);
            this.rtbExample.Name = "rtbExample";
            this.rtbExample.ReadOnly = true;
            this.rtbExample.Size = new System.Drawing.Size(564, 110);
            this.rtbExample.TabIndex = 4;
            this.rtbExample.Text = "";
            // 
            // InfoDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(588, 483);
            this.Controls.Add(this.rtbExample);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbOptions);
            this.Controls.Add(this.txtExplanation);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "InfoDialog";
            this.Text = "InfoDialog";
            this.Load += new System.EventHandler(this.InfoDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtExplanation;
        private System.Windows.Forms.ComboBox cbOptions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox rtbExample;
    }
}