namespace MapleManager.Scripts.Animator
{
    partial class AnimationForm
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
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnExportGif = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnExportGif
            // 
            this.btnExportGif.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportGif.Location = new System.Drawing.Point(444, 12);
            this.btnExportGif.Name = "btnExportGif";
            this.btnExportGif.Size = new System.Drawing.Size(75, 23);
            this.btnExportGif.TabIndex = 1;
            this.btnExportGif.Text = "Export GIF";
            this.btnExportGif.UseVisualStyleBackColor = true;
            this.btnExportGif.Click += new System.EventHandler(this.btnExportGif_Click);
            // 
            // AnimationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(531, 365);
            this.Controls.Add(this.btnExportGif);
            this.Name = "AnimationForm";
            this.Text = "AnimationForm";
            this.Load += new System.EventHandler(this.AnimationForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.AnimationForm_Paint);
            this.Resize += new System.EventHandler(this.AnimationForm_Resize);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnExportGif;
    }
}