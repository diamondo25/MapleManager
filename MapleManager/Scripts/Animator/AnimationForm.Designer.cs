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
            this.tbFrame = new System.Windows.Forms.TrackBar();
            this.btnStartPause = new System.Windows.Forms.Button();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.useCheckerboard = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.tbFrame)).BeginInit();
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
            this.btnExportGif.Location = new System.Drawing.Point(541, 12);
            this.btnExportGif.Name = "btnExportGif";
            this.btnExportGif.Size = new System.Drawing.Size(75, 23);
            this.btnExportGif.TabIndex = 1;
            this.btnExportGif.Text = "Export GIF";
            this.btnExportGif.UseVisualStyleBackColor = true;
            this.btnExportGif.Click += new System.EventHandler(this.btnExportGif_Click);
            // 
            // tbFrame
            // 
            this.tbFrame.Location = new System.Drawing.Point(12, 12);
            this.tbFrame.Name = "tbFrame";
            this.tbFrame.Size = new System.Drawing.Size(394, 45);
            this.tbFrame.TabIndex = 2;
            this.tbFrame.ValueChanged += new System.EventHandler(this.tbFrame_ValueChanged);
            // 
            // btnStartPause
            // 
            this.btnStartPause.Location = new System.Drawing.Point(412, 12);
            this.btnStartPause.Name = "btnStartPause";
            this.btnStartPause.Size = new System.Drawing.Size(26, 23);
            this.btnStartPause.TabIndex = 3;
            this.btnStartPause.Text = "?";
            this.btnStartPause.UseVisualStyleBackColor = true;
            this.btnStartPause.Click += new System.EventHandler(this.btnStartPause_Click);
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar1.Location = new System.Drawing.Point(620, 0);
            this.vScrollBar1.Maximum = 500;
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(17, 464);
            this.vScrollBar1.TabIndex = 4;
            this.vScrollBar1.Value = 50;
            this.vScrollBar1.ValueChanged += new System.EventHandler(this.vScrollBar1_ValueChanged);
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScrollBar1.Location = new System.Drawing.Point(0, 447);
            this.hScrollBar1.Maximum = 500;
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(620, 17);
            this.hScrollBar1.TabIndex = 5;
            this.hScrollBar1.Value = 50;
            this.hScrollBar1.ValueChanged += new System.EventHandler(this.hScrollBar1_ValueChanged);
            // 
            // useCheckerboard
            // 
            this.useCheckerboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.useCheckerboard.AutoSize = true;
            this.useCheckerboard.Location = new System.Drawing.Point(523, 40);
            this.useCheckerboard.Name = "useCheckerboard";
            this.useCheckerboard.Size = new System.Drawing.Size(93, 17);
            this.useCheckerboard.TabIndex = 6;
            this.useCheckerboard.Text = "Checkerboard";
            this.useCheckerboard.UseVisualStyleBackColor = true;
            // 
            // AnimationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(637, 464);
            this.Controls.Add(this.useCheckerboard);
            this.Controls.Add(this.hScrollBar1);
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.btnStartPause);
            this.Controls.Add(this.tbFrame);
            this.Controls.Add(this.btnExportGif);
            this.Name = "AnimationForm";
            this.Text = "AnimationForm";
            this.Load += new System.EventHandler(this.AnimationForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.AnimationForm_Paint);
            this.Resize += new System.EventHandler(this.AnimationForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.tbFrame)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnExportGif;
        private System.Windows.Forms.TrackBar tbFrame;
        private System.Windows.Forms.Button btnStartPause;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.HScrollBar hScrollBar1;
        private System.Windows.Forms.CheckBox useCheckerboard;
    }
}