namespace FFmpegAssistant
{
    partial class AboutForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            picIcon = new PictureBox();
            lblAppName = new Label();
            lblVersion = new Label();
            pnlContent = new Panel();
            grpFfmpeg = new GroupBox();
            lblFfmpegVer = new Label();
            lblFfmpegVerHeader = new Label();
            lnkFfmpeg = new LinkLabel();
            label1 = new Label();
            lblDescription = new Label();
            lblCopyright = new Label();
            lnkGitHub = new LinkLabel();
            lblBuiltWithHeader = new Label();
            lblBuiltWith = new Label();
            btnClose = new Button();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picIcon).BeginInit();
            pnlContent.SuspendLayout();
            grpFfmpeg.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(15, 52, 96);
            pnlHeader.Controls.Add(picIcon);
            pnlHeader.Controls.Add(lblAppName);
            pnlHeader.Controls.Add(lblVersion);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(440, 88);
            pnlHeader.TabIndex = 0;
            // 
            // picIcon
            // 
            picIcon.Location = new Point(20, 16);
            picIcon.Name = "picIcon";
            picIcon.Size = new Size(52, 52);
            picIcon.SizeMode = PictureBoxSizeMode.Zoom;
            picIcon.TabIndex = 0;
            picIcon.TabStop = false;
            // 
            // lblAppName
            // 
            lblAppName.AutoSize = true;
            lblAppName.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblAppName.ForeColor = Color.White;
            lblAppName.Location = new Point(84, 16);
            lblAppName.Name = "lblAppName";
            lblAppName.Size = new Size(177, 28);
            lblAppName.TabIndex = 1;
            lblAppName.Text = "FFmpeg Assistant";
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Font = new Font("Segoe UI", 9F);
            lblVersion.ForeColor = Color.FromArgb(160, 195, 225);
            lblVersion.Location = new Point(86, 54);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(72, 15);
            lblVersion.TabIndex = 2;
            lblVersion.Text = "Version 1.0.0";
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.White;
            pnlContent.Controls.Add(grpFfmpeg);
            pnlContent.Controls.Add(lblDescription);
            pnlContent.Controls.Add(lblCopyright);
            pnlContent.Controls.Add(lnkGitHub);
            pnlContent.Controls.Add(lblBuiltWithHeader);
            pnlContent.Controls.Add(lblBuiltWith);
            pnlContent.Controls.Add(btnClose);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 88);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(440, 332);
            pnlContent.TabIndex = 1;
            // 
            // grpFfmpeg
            // 
            grpFfmpeg.Controls.Add(lblFfmpegVer);
            grpFfmpeg.Controls.Add(lblFfmpegVerHeader);
            grpFfmpeg.Controls.Add(lnkFfmpeg);
            grpFfmpeg.Controls.Add(label1);
            grpFfmpeg.Location = new Point(15, 195);
            grpFfmpeg.Name = "grpFfmpeg";
            grpFfmpeg.Size = new Size(405, 78);
            grpFfmpeg.TabIndex = 6;
            grpFfmpeg.TabStop = false;
            grpFfmpeg.Text = "FFmpeg";
            // 
            // lblFfmpegVer
            // 
            lblFfmpegVer.AutoSize = true;
            lblFfmpegVer.Location = new Point(122, 50);
            lblFfmpegVer.Name = "lblFfmpegVer";
            lblFfmpegVer.Size = new Size(28, 15);
            lblFfmpegVer.TabIndex = 3;
            lblFfmpegVer.Text = "?.?.?";
            // 
            // lblFfmpegVerHeader
            // 
            lblFfmpegVerHeader.AutoSize = true;
            lblFfmpegVerHeader.Location = new Point(6, 50);
            lblFfmpegVerHeader.Name = "lblFfmpegVerHeader";
            lblFfmpegVerHeader.Size = new Size(98, 15);
            lblFfmpegVerHeader.TabIndex = 2;
            lblFfmpegVerHeader.Text = "Installed version: ";
            // 
            // lnkFfmpeg
            // 
            lnkFfmpeg.AutoSize = true;
            lnkFfmpeg.Location = new Point(122, 19);
            lnkFfmpeg.Name = "lnkFfmpeg";
            lnkFfmpeg.Size = new Size(107, 15);
            lnkFfmpeg.TabIndex = 1;
            lnkFfmpeg.TabStop = true;
            lnkFfmpeg.Text = "https://ffmpeg.org";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 19);
            label1.Name = "label1";
            label1.Size = new Size(66, 15);
            label1.TabIndex = 0;
            label1.Text = "Web Page: ";
            // 
            // lblDescription
            // 
            lblDescription.Font = new Font("Segoe UI", 9.5F);
            lblDescription.ForeColor = Color.FromArgb(50, 50, 50);
            lblDescription.Location = new Point(20, 18);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(400, 36);
            lblDescription.TabIndex = 0;
            lblDescription.Text = "Downloads and saves video streams using FFmpeg, with real-time progress tracking and automatic episode naming.";
            // 
            // lblCopyright
            // 
            lblCopyright.AutoSize = true;
            lblCopyright.Font = new Font("Segoe UI", 9F);
            lblCopyright.ForeColor = Color.FromArgb(120, 120, 120);
            lblCopyright.Location = new Point(20, 62);
            lblCopyright.Name = "lblCopyright";
            lblCopyright.Size = new Size(102, 15);
            lblCopyright.TabIndex = 1;
            lblCopyright.Text = "SweWolf Software";
            // 
            // lnkGitHub
            // 
            lnkGitHub.AutoSize = true;
            lnkGitHub.Font = new Font("Segoe UI", 9F);
            lnkGitHub.Location = new Point(20, 84);
            lnkGitHub.Name = "lnkGitHub";
            lnkGitHub.Size = new Size(255, 15);
            lnkGitHub.TabIndex = 2;
            lnkGitHub.TabStop = true;
            lnkGitHub.Text = "https://github.com/SweWolf/FFmpegAssistant";
            lnkGitHub.LinkClicked += lnkGitHub_LinkClicked;
            // 
            // lblBuiltWithHeader
            // 
            lblBuiltWithHeader.AutoSize = true;
            lblBuiltWithHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblBuiltWithHeader.ForeColor = Color.FromArgb(50, 50, 50);
            lblBuiltWithHeader.Location = new Point(20, 116);
            lblBuiltWithHeader.Name = "lblBuiltWithHeader";
            lblBuiltWithHeader.Size = new Size(64, 15);
            lblBuiltWithHeader.TabIndex = 3;
            lblBuiltWithHeader.Text = "Built with:";
            // 
            // lblBuiltWith
            // 
            lblBuiltWith.Font = new Font("Segoe UI", 9F);
            lblBuiltWith.ForeColor = Color.FromArgb(80, 80, 80);
            lblBuiltWith.Location = new Point(20, 136);
            lblBuiltWith.Name = "lblBuiltWith";
            lblBuiltWith.Size = new Size(400, 52);
            lblBuiltWith.TabIndex = 4;
            lblBuiltWith.Text = "• .NET 10 / Windows Forms";
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.BackColor = Color.FromArgb(15, 52, 96);
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(340, 290);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(80, 28);
            btnClose.TabIndex = 5;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // AboutForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(440, 420);
            Controls.Add(pnlContent);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "About FFmpeg Assistant";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picIcon).EndInit();
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
            grpFfmpeg.ResumeLayout(false);
            grpFfmpeg.PerformLayout();
            ResumeLayout(false);
        }

        private Panel pnlHeader;
        private PictureBox picIcon;
        private Label lblAppName;
        private Label lblVersion;
        private Panel pnlContent;
        private Label lblDescription;
        private Label lblCopyright;
        private Label lblBuiltWithHeader;
        private Label lblBuiltWith;
        private LinkLabel lnkGitHub;
        private Button btnClose;
        private GroupBox grpFfmpeg;
        private Label lblFfmpegVerHeader;
        private LinkLabel lnkFfmpeg;
        private Label label1;
        private Label lblFfmpegVer;
    }
}
