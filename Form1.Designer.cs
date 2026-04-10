namespace FFmpegAssistant
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtOriginalCommand = new TextBox();
            cboFolder = new ComboBox();
            btnBrowseForFolder = new Button();
            txtFileName = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            btnRun = new Button();
            btnCancel = new Button();
            dgvProgress = new DataGridView();
            progressBar = new TextProgressBar();
            btnOpenFile = new Button();
            btnOpenFolder = new Button();
            btnOpenLogFile = new Button();
            lblEstimatedRemaining = new Label();
            menuStrip = new MenuStrip();
            menuHelp = new ToolStripMenuItem();
            menuAbout = new ToolStripMenuItem();
            btnClear = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvProgress).BeginInit();
            menuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // txtOriginalCommand
            // 
            txtOriginalCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOriginalCommand.Font = new Font("Segoe UI", 12F);
            txtOriginalCommand.Location = new Point(12, 57);
            txtOriginalCommand.Name = "txtOriginalCommand";
            txtOriginalCommand.Size = new Size(765, 29);
            txtOriginalCommand.TabIndex = 0;
            // 
            // cboFolder
            // 
            cboFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboFolder.Font = new Font("Segoe UI", 12F);
            cboFolder.FormattingEnabled = true;
            cboFolder.Location = new Point(12, 145);
            cboFolder.Name = "cboFolder";
            cboFolder.Size = new Size(623, 29);
            cboFolder.TabIndex = 1;
            // 
            // btnBrowseForFolder
            // 
            btnBrowseForFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseForFolder.Font = new Font("Segoe UI", 12F);
            btnBrowseForFolder.Location = new Point(652, 141);
            btnBrowseForFolder.Name = "btnBrowseForFolder";
            btnBrowseForFolder.Size = new Size(43, 36);
            btnBrowseForFolder.TabIndex = 2;
            btnBrowseForFolder.Text = "...";
            btnBrowseForFolder.UseVisualStyleBackColor = true;
            btnBrowseForFolder.Click += btnBrowseForFolder_Click;
            // 
            // txtFileName
            // 
            txtFileName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFileName.Font = new Font("Segoe UI", 12F);
            txtFileName.Location = new Point(12, 240);
            txtFileName.Name = "txtFileName";
            txtFileName.Size = new Size(623, 29);
            txtFileName.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F);
            label1.Location = new Point(12, 33);
            label1.Name = "label1";
            label1.Size = new Size(143, 21);
            label1.TabIndex = 4;
            label1.Text = "Original Command";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F);
            label2.Location = new Point(12, 121);
            label2.Name = "label2";
            label2.Size = new Size(54, 21);
            label2.TabIndex = 5;
            label2.Text = "Folder";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F);
            label3.Location = new Point(12, 216);
            label3.Name = "label3";
            label3.Size = new Size(80, 21);
            label3.TabIndex = 6;
            label3.Text = "File Name";
            // 
            // btnRun
            // 
            btnRun.Font = new Font("Segoe UI", 12F);
            btnRun.Location = new Point(12, 290);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(153, 36);
            btnRun.TabIndex = 4;
            btnRun.Text = "Run";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Segoe UI", 12F);
            btnCancel.Location = new Point(187, 290);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(153, 36);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // dgvProgress
            // 
            dgvProgress.AllowUserToAddRows = false;
            dgvProgress.AllowUserToDeleteRows = false;
            dgvProgress.AllowUserToResizeRows = false;
            dgvProgress.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvProgress.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProgress.Location = new Point(12, 342);
            dgvProgress.Name = "dgvProgress";
            dgvProgress.ReadOnly = true;
            dgvProgress.RowHeadersVisible = false;
            dgvProgress.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProgress.Size = new Size(776, 72);
            dgvProgress.TabIndex = 7;
            dgvProgress.TabStop = false;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(12, 452);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(776, 23);
            progressBar.TabIndex = 9;
            // 
            // btnOpenFile
            // 
            btnOpenFile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOpenFile.Font = new Font("Segoe UI", 12F);
            btnOpenFile.Location = new Point(12, 487);
            btnOpenFile.Name = "btnOpenFile";
            btnOpenFile.Size = new Size(153, 36);
            btnOpenFile.TabIndex = 8;
            btnOpenFile.Text = "Open File";
            btnOpenFile.UseVisualStyleBackColor = true;
            btnOpenFile.Click += btnOpenFile_Click_1;
            // 
            // btnOpenFolder
            // 
            btnOpenFolder.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOpenFolder.Font = new Font("Segoe UI", 12F);
            btnOpenFolder.Location = new Point(187, 487);
            btnOpenFolder.Name = "btnOpenFolder";
            btnOpenFolder.Size = new Size(153, 36);
            btnOpenFolder.TabIndex = 9;
            btnOpenFolder.Text = "Open Folder";
            btnOpenFolder.UseVisualStyleBackColor = true;
            btnOpenFolder.Click += btnOpenFolder_Click_1;
            // 
            // btnOpenLogFile
            // 
            btnOpenLogFile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOpenLogFile.Font = new Font("Segoe UI", 12F);
            btnOpenLogFile.Location = new Point(363, 487);
            btnOpenLogFile.Name = "btnOpenLogFile";
            btnOpenLogFile.Size = new Size(153, 36);
            btnOpenLogFile.TabIndex = 10;
            btnOpenLogFile.Text = "Open Log File";
            btnOpenLogFile.UseVisualStyleBackColor = true;
            btnOpenLogFile.Click += btnOpenLogFile_Click_1;
            // 
            // lblEstimatedRemaining
            // 
            lblEstimatedRemaining.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblEstimatedRemaining.AutoSize = true;
            lblEstimatedRemaining.Font = new Font("Segoe UI", 10F);
            lblEstimatedRemaining.Location = new Point(12, 433);
            lblEstimatedRemaining.Name = "lblEstimatedRemaining";
            lblEstimatedRemaining.Size = new Size(186, 19);
            lblEstimatedRemaining.TabIndex = 14;
            lblEstimatedRemaining.Text = "Estimated remaining time: —";
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { menuHelp });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(800, 24);
            menuStrip.TabIndex = 15;
            // 
            // menuHelp
            // 
            menuHelp.DropDownItems.AddRange(new ToolStripItem[] { menuAbout });
            menuHelp.Name = "menuHelp";
            menuHelp.Size = new Size(44, 20);
            menuHelp.Text = "Help";
            // 
            // menuAbout
            // 
            menuAbout.Name = "menuAbout";
            menuAbout.Size = new Size(107, 22);
            menuAbout.Text = "About";
            menuAbout.Click += menuAbout_Click;
            // 
            // btnClear
            // 
            btnClear.Font = new Font("Segoe UI", 12F);
            btnClear.Location = new Point(363, 290);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(153, 36);
            btnClear.TabIndex = 6;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // Form1
            // 
            AcceptButton = btnRun;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 535);
            Controls.Add(btnClear);
            Controls.Add(btnOpenLogFile);
            Controls.Add(btnOpenFolder);
            Controls.Add(btnOpenFile);
            Controls.Add(lblEstimatedRemaining);
            Controls.Add(progressBar);
            Controls.Add(dgvProgress);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtFileName);
            Controls.Add(btnCancel);
            Controls.Add(btnRun);
            Controls.Add(btnBrowseForFolder);
            Controls.Add(cboFolder);
            Controls.Add(txtOriginalCommand);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Name = "Form1";
            Text = "FFmpeg Assistant";
            WindowState = FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)dgvProgress).EndInit();
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtOriginalCommand;
        private ComboBox cboFolder;
        private Button btnBrowseForFolder;
        private TextBox txtFileName;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button btnRun;
        private Button btnCancel;
        private DataGridView dgvProgress;
        private TextProgressBar progressBar;
        private Button btnOpenFile;
        private Button btnOpenFolder;
        private MenuStrip menuStrip;
        private ToolStripMenuItem menuHelp;
        private ToolStripMenuItem menuAbout;
        private Button btnOpenLogFile;
        private Label lblEstimatedRemaining;
        private Button btnClear;
    }
}
