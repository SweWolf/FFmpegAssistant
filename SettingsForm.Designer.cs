namespace FFmpegAssistant
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            grpFfmpeg = new GroupBox();
            lblFfmpegPath = new Label();
            txtFfmpegPath = new TextBox();
            btnBrowseFfmpeg = new Button();
            lblFfmpegHint = new Label();
            btnOK = new Button();
            btnCancel = new Button();
            groupBox1 = new GroupBox();
            cboReplaceQas = new ComboBox();
            label1 = new Label();
            groupBox2 = new GroupBox();
            label3 = new Label();
            txtNumberOfDownloadAttempts = new TextBox();
            label2 = new Label();
            grpFfmpeg.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // grpFfmpeg
            // 
            grpFfmpeg.Controls.Add(lblFfmpegPath);
            grpFfmpeg.Controls.Add(txtFfmpegPath);
            grpFfmpeg.Controls.Add(btnBrowseFfmpeg);
            grpFfmpeg.Controls.Add(lblFfmpegHint);
            grpFfmpeg.Location = new Point(12, 12);
            grpFfmpeg.Name = "grpFfmpeg";
            grpFfmpeg.Size = new Size(470, 105);
            grpFfmpeg.TabIndex = 0;
            grpFfmpeg.TabStop = false;
            grpFfmpeg.Text = "FFmpeg";
            // 
            // lblFfmpegPath
            // 
            lblFfmpegPath.AutoSize = true;
            lblFfmpegPath.Location = new Point(10, 22);
            lblFfmpegPath.Name = "lblFfmpegPath";
            lblFfmpegPath.Size = new Size(110, 15);
            lblFfmpegPath.TabIndex = 0;
            lblFfmpegPath.Text = "Path to ffmpeg.exe:";
            // 
            // txtFfmpegPath
            // 
            txtFfmpegPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFfmpegPath.Location = new Point(10, 42);
            txtFfmpegPath.Name = "txtFfmpegPath";
            txtFfmpegPath.Size = new Size(370, 23);
            txtFfmpegPath.TabIndex = 1;
            // 
            // btnBrowseFfmpeg
            // 
            btnBrowseFfmpeg.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseFfmpeg.Location = new Point(388, 40);
            btnBrowseFfmpeg.Name = "btnBrowseFfmpeg";
            btnBrowseFfmpeg.Size = new Size(72, 27);
            btnBrowseFfmpeg.TabIndex = 2;
            btnBrowseFfmpeg.Text = "Browse...";
            btnBrowseFfmpeg.UseVisualStyleBackColor = true;
            btnBrowseFfmpeg.Click += btnBrowseFfmpeg_Click;
            // 
            // lblFfmpegHint
            // 
            lblFfmpegHint.AutoSize = true;
            lblFfmpegHint.ForeColor = SystemColors.GrayText;
            lblFfmpegHint.Location = new Point(10, 74);
            lblFfmpegHint.Name = "lblFfmpegHint";
            lblFfmpegHint.Size = new Size(201, 15);
            lblFfmpegHint.TabIndex = 3;
            lblFfmpegHint.Text = "Leave empty to use the system PATH";
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.Location = new Point(326, 398);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 27);
            btnOK.TabIndex = 7;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(407, 398);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 27);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(cboReplaceQas);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(12, 283);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(470, 96);
            groupBox1.TabIndex = 5;
            groupBox1.TabStop = false;
            groupBox1.Text = "Adjust Command";
            // 
            // cboReplaceQas
            // 
            cboReplaceQas.DropDownStyle = ComboBoxStyle.DropDownList;
            cboReplaceQas.FormattingEnabled = true;
            cboReplaceQas.Items.AddRange(new object[] { "Yes", "No", "Ask" });
            cboReplaceQas.Location = new Point(9, 47);
            cboReplaceQas.Name = "cboReplaceQas";
            cboReplaceQas.Size = new Size(281, 23);
            cboReplaceQas.TabIndex = 6;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 29);
            label1.Name = "label1";
            label1.Size = new Size(208, 15);
            label1.TabIndex = 0;
            label1.Text = "Replace \"audio_qas\" with \"audio_eng\"";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(txtNumberOfDownloadAttempts);
            groupBox2.Controls.Add(label2);
            groupBox2.Location = new Point(10, 138);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(472, 115);
            groupBox2.TabIndex = 3;
            groupBox2.TabStop = false;
            groupBox2.Text = "Auto Retry On Download Failure";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ForeColor = SystemColors.GrayText;
            label3.Location = new Point(15, 79);
            label3.Name = "label3";
            label3.Size = new Size(136, 15);
            label3.TabIndex = 2;
            label3.Text = "Leave empty to not retry";
            // 
            // yxyNumberOfDownloadAttempts
            // 
            txtNumberOfDownloadAttempts.Location = new Point(15, 43);
            txtNumberOfDownloadAttempts.Name = "txtNumberOfDownloadAttempts";
            txtNumberOfDownloadAttempts.Size = new Size(199, 23);
            txtNumberOfDownloadAttempts.TabIndex = 4;
            txtNumberOfDownloadAttempts.Text = "5";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 21);
            label2.Name = "label2";
            label2.Size = new Size(148, 15);
            label2.TabIndex = 0;
            label2.Text = "Maximum Number of Attempts";
            // 
            // SettingsForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(494, 437);
            ControlBox = false;
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(grpFfmpeg);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            Load += SettingsForm_Load;
            grpFfmpeg.ResumeLayout(false);
            grpFfmpeg.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpFfmpeg;
        private Label    lblFfmpegPath;
        private TextBox  txtFfmpegPath;
        private Button   btnBrowseFfmpeg;
        private Label    lblFfmpegHint;
        private Button   btnOK;
        private Button   btnCancel;
        private GroupBox groupBox1;
        private Label label1;
        private ComboBox cboReplaceQas;
        private GroupBox groupBox2;
        private Label label3;
        private TextBox txtNumberOfDownloadAttempts;
        private Label label2;
    }
}
