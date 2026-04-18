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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
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
            menuSetup = new ToolStripMenuItem();
            menuCreateShortcut = new ToolStripMenuItem();
            menuHelp = new ToolStripMenuItem();
            menuAbout = new ToolStripMenuItem();
            btnClear = new Button();
            label4 = new Label();
            txtStatus = new TextBox();
            groupBox1 = new GroupBox();
            lblEpisode = new Label();
            txtEpisode = new TextBox();
            lblSeason = new Label();
            txtSeason = new TextBox();
            label5 = new Label();
            btnTvShow = new Button();
            btnMovie = new Button();
            toolTip1 = new ToolTip(components);
            chkEnableWatchingWhileDownloading = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)dgvProgress).BeginInit();
            menuStrip.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // txtOriginalCommand
            // 
            txtOriginalCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOriginalCommand.Font = new Font("Segoe UI", 12F);
            txtOriginalCommand.Location = new Point(12, 57);
            txtOriginalCommand.Name = "txtOriginalCommand";
            txtOriginalCommand.Size = new Size(1039, 29);
            txtOriginalCommand.TabIndex = 0;
            toolTip1.SetToolTip(txtOriginalCommand, "Command that FFmpeg should run (the name of the download file will be adjusted)");
            // 
            // cboFolder
            // 
            cboFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboFolder.Font = new Font("Segoe UI", 12F);
            cboFolder.FormattingEnabled = true;
            cboFolder.Location = new Point(12, 264);
            cboFolder.Name = "cboFolder";
            cboFolder.Size = new Size(897, 29);
            cboFolder.TabIndex = 1;
            toolTip1.SetToolTip(cboFolder, "Download folder");
            // 
            // btnBrowseForFolder
            // 
            btnBrowseForFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseForFolder.Font = new Font("Segoe UI", 12F);
            btnBrowseForFolder.Location = new Point(926, 264);
            btnBrowseForFolder.Name = "btnBrowseForFolder";
            btnBrowseForFolder.Size = new Size(38, 29);
            btnBrowseForFolder.TabIndex = 2;
            btnBrowseForFolder.Text = "...";
            toolTip1.SetToolTip(btnBrowseForFolder, "Browse for folder");
            btnBrowseForFolder.UseVisualStyleBackColor = true;
            btnBrowseForFolder.Click += btnBrowseForFolder_Click;
            // 
            // txtFileName
            // 
            txtFileName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFileName.Font = new Font("Segoe UI", 12F);
            txtFileName.Location = new Point(12, 327);
            txtFileName.Name = "txtFileName";
            txtFileName.Size = new Size(897, 29);
            txtFileName.TabIndex = 3;
            toolTip1.SetToolTip(txtFileName, resources.GetString("txtFileName.ToolTip"));
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
            label2.Location = new Point(12, 240);
            label2.Name = "label2";
            label2.Size = new Size(54, 21);
            label2.TabIndex = 5;
            label2.Text = "Folder";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F);
            label3.Location = new Point(12, 303);
            label3.Name = "label3";
            label3.Size = new Size(80, 21);
            label3.TabIndex = 6;
            label3.Text = "File Name";
            // 
            // btnRun
            // 
            btnRun.Font = new Font("Segoe UI", 12F);
            btnRun.Location = new Point(14, 428);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(153, 36);
            btnRun.TabIndex = 5;
            btnRun.Text = "Download";
            toolTip1.SetToolTip(btnRun, "FFmpeg runs the command and downloads the video file");
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Segoe UI", 12F);
            btnCancel.Location = new Point(189, 428);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(153, 36);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Cancel";
            toolTip1.SetToolTip(btnCancel, "Cancel the download in progress");
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // dgvProgress
            // 
            dgvProgress.AllowUserToAddRows = false;
            dgvProgress.AllowUserToDeleteRows = false;
            dgvProgress.AllowUserToResizeRows = false;
            dgvProgress.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProgress.Location = new Point(14, 482);
            dgvProgress.Name = "dgvProgress";
            dgvProgress.ReadOnly = true;
            dgvProgress.RowHeadersVisible = false;
            dgvProgress.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProgress.Size = new Size(365, 139);
            dgvProgress.TabIndex = 8;
            dgvProgress.TabStop = false;
            toolTip1.SetToolTip(dgvProgress, "Feedback from FFmpeg");
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(14, 783);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(1050, 23);
            progressBar.TabIndex = 9;
            // 
            // btnOpenFile
            // 
            btnOpenFile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOpenFile.Font = new Font("Segoe UI", 12F);
            btnOpenFile.Location = new Point(14, 818);
            btnOpenFile.Name = "btnOpenFile";
            btnOpenFile.Size = new Size(153, 36);
            btnOpenFile.TabIndex = 10;
            btnOpenFile.Text = "Open File";
            toolTip1.SetToolTip(btnOpenFile, "Open the downloaded video/audio file in the associated application");
            btnOpenFile.UseVisualStyleBackColor = true;
            btnOpenFile.Click += btnOpenFile_Click_1;
            // 
            // btnOpenFolder
            // 
            btnOpenFolder.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOpenFolder.Font = new Font("Segoe UI", 12F);
            btnOpenFolder.Location = new Point(189, 818);
            btnOpenFolder.Name = "btnOpenFolder";
            btnOpenFolder.Size = new Size(153, 36);
            btnOpenFolder.TabIndex = 11;
            btnOpenFolder.Text = "Open Folder";
            toolTip1.SetToolTip(btnOpenFolder, "Open the folder in the Windows File Explorer");
            btnOpenFolder.UseVisualStyleBackColor = true;
            btnOpenFolder.Click += btnOpenFolder_Click_1;
            // 
            // btnOpenLogFile
            // 
            btnOpenLogFile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOpenLogFile.Font = new Font("Segoe UI", 12F);
            btnOpenLogFile.Location = new Point(365, 818);
            btnOpenLogFile.Name = "btnOpenLogFile";
            btnOpenLogFile.Size = new Size(153, 36);
            btnOpenLogFile.TabIndex = 12;
            btnOpenLogFile.Text = "Open Log File";
            toolTip1.SetToolTip(btnOpenLogFile, "Open FFmpeg's log file");
            btnOpenLogFile.UseVisualStyleBackColor = true;
            btnOpenLogFile.Click += btnOpenLogFile_Click_1;
            // 
            // lblEstimatedRemaining
            // 
            lblEstimatedRemaining.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblEstimatedRemaining.AutoSize = true;
            lblEstimatedRemaining.Font = new Font("Segoe UI", 10F);
            lblEstimatedRemaining.Location = new Point(14, 764);
            lblEstimatedRemaining.Name = "lblEstimatedRemaining";
            lblEstimatedRemaining.Size = new Size(186, 19);
            lblEstimatedRemaining.TabIndex = 14;
            lblEstimatedRemaining.Text = "Estimated remaining time: —";
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { menuSetup, menuHelp });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1074, 24);
            menuStrip.TabIndex = 15;
            // 
            // menuSetup
            // 
            menuSetup.DropDownItems.AddRange(new ToolStripItem[] { menuCreateShortcut });
            menuSetup.Name = "menuSetup";
            menuSetup.Size = new Size(49, 20);
            menuSetup.Text = "Setup";
            // 
            // menuCreateShortcut
            // 
            menuCreateShortcut.Name = "menuCreateShortcut";
            menuCreateShortcut.Size = new Size(165, 22);
            menuCreateShortcut.Text = "Create Shortcut...";
            menuCreateShortcut.Click += menuCreateShortcut_Click;
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
            btnClear.Location = new Point(365, 428);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(153, 36);
            btnClear.TabIndex = 7;
            btnClear.Text = "Clear";
            toolTip1.SetToolTip(btnClear, "Clear the boxes on the screen");
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 10F);
            label4.Location = new Point(14, 733);
            label4.Name = "label4";
            label4.Size = new Size(47, 19);
            label4.TabIndex = 14;
            label4.Text = "Status";
            // 
            // txtStatus
            // 
            txtStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtStatus.BorderStyle = BorderStyle.FixedSingle;
            txtStatus.Location = new Point(67, 733);
            txtStatus.Name = "txtStatus";
            txtStatus.ReadOnly = true;
            txtStatus.Size = new Size(997, 23);
            txtStatus.TabIndex = 9;
            txtStatus.TabStop = false;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lblEpisode);
            groupBox1.Controls.Add(txtEpisode);
            groupBox1.Controls.Add(lblSeason);
            groupBox1.Controls.Add(txtSeason);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(btnTvShow);
            groupBox1.Controls.Add(btnMovie);
            groupBox1.Location = new Point(14, 111);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(621, 114);
            groupBox1.TabIndex = 16;
            groupBox1.TabStop = false;
            groupBox1.Text = "Auto suggest folder and file name";
            // 
            // lblEpisode
            // 
            lblEpisode.AutoSize = true;
            lblEpisode.Location = new Point(482, 47);
            lblEpisode.Name = "lblEpisode";
            lblEpisode.Size = new Size(48, 15);
            lblEpisode.TabIndex = 6;
            lblEpisode.Text = "Episode";
            lblEpisode.Visible = false;
            // 
            // txtEpisode
            // 
            txtEpisode.Location = new Point(482, 65);
            txtEpisode.Name = "txtEpisode";
            txtEpisode.Size = new Size(63, 23);
            txtEpisode.TabIndex = 3;
            txtEpisode.Visible = false;
            // 
            // lblSeason
            // 
            lblSeason.AutoSize = true;
            lblSeason.Location = new Point(404, 47);
            lblSeason.Name = "lblSeason";
            lblSeason.Size = new Size(44, 15);
            lblSeason.TabIndex = 4;
            lblSeason.Text = "Season";
            lblSeason.Visible = false;
            // 
            // txtSeason
            // 
            txtSeason.Location = new Point(404, 65);
            txtSeason.Name = "txtSeason";
            txtSeason.Size = new Size(63, 23);
            txtSeason.TabIndex = 2;
            txtSeason.Visible = false;
            txtSeason.TextChanged += txtSeason_TextChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(15, 30);
            label5.Name = "label5";
            label5.Size = new Size(248, 15);
            label5.TabIndex = 2;
            label5.Text = "Select type of video to get an auto suggestion";
            // 
            // btnTvShow
            // 
            btnTvShow.Location = new Point(192, 65);
            btnTvShow.Name = "btnTvShow";
            btnTvShow.Size = new Size(153, 36);
            btnTvShow.TabIndex = 1;
            btnTvShow.Text = "TV Show";
            toolTip1.SetToolTip(btnTvShow, "Auto suggest folder and file name for a TV show");
            btnTvShow.UseVisualStyleBackColor = true;
            btnTvShow.Click += btnTvShow_Click;
            // 
            // btnMovie
            // 
            btnMovie.Location = new Point(15, 65);
            btnMovie.Name = "btnMovie";
            btnMovie.Size = new Size(153, 36);
            btnMovie.TabIndex = 0;
            btnMovie.Text = "Movie";
            toolTip1.SetToolTip(btnMovie, "Auto suggest folder and file name for a movie");
            btnMovie.UseVisualStyleBackColor = true;
            btnMovie.Click += btnMovie_Click;
            // 
            // chkEnableWatchingWhileDownloading
            // 
            chkEnableWatchingWhileDownloading.AutoSize = true;
            chkEnableWatchingWhileDownloading.Font = new Font("Segoe UI", 12F);
            chkEnableWatchingWhileDownloading.Location = new Point(14, 389);
            chkEnableWatchingWhileDownloading.Name = "chkEnableWatchingWhileDownloading";
            chkEnableWatchingWhileDownloading.Size = new Size(285, 25);
            chkEnableWatchingWhileDownloading.TabIndex = 4;
            chkEnableWatchingWhileDownloading.Text = "Enable Watching While Downloading";
            toolTip1.SetToolTip(chkEnableWatchingWhileDownloading, "Downloads as a .ts file first so you can watch while downloading, then converts to the final format automatically");
            chkEnableWatchingWhileDownloading.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AcceptButton = btnRun;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1074, 866);
            Controls.Add(chkEnableWatchingWhileDownloading);
            Controls.Add(groupBox1);
            Controls.Add(txtStatus);
            Controls.Add(btnClear);
            Controls.Add(btnOpenLogFile);
            Controls.Add(btnOpenFolder);
            Controls.Add(btnOpenFile);
            Controls.Add(label4);
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
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip;
            MinimumSize = new Size(618, 888);
            Name = "Form1";
            Text = "FFmpeg Assistant";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dgvProgress).EndInit();
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
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
        private ToolStripMenuItem menuSetup;
        private ToolStripMenuItem menuCreateShortcut;
        private ToolStripMenuItem menuHelp;
        private ToolStripMenuItem menuAbout;
        private Button btnOpenLogFile;
        private Label lblEstimatedRemaining;
        private Button btnClear;
        private Label label4;
        private TextBox txtStatus;
        private GroupBox groupBox1;
        private Button btnMovie;
        private Label label5;
        private Button btnTvShow;
        private ToolTip toolTip1;
        private CheckBox chkEnableWatchingWhileDownloading;
        private Label lblEpisode;
        private TextBox txtEpisode;
        private Label lblSeason;
        private TextBox txtSeason;
    }
}
