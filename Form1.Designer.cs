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
            menuTools = new ToolStripMenuItem();
            menuCreateShortcut = new ToolStripMenuItem();
            menuSettings = new ToolStripMenuItem();
            subtitlesToolStripMenuItem = new ToolStripMenuItem();
            mnuExtractSubtitleFile = new ToolStripMenuItem();
            menuHelp = new ToolStripMenuItem();
            menuAbout = new ToolStripMenuItem();
            menuNewVersion = new ToolStripMenuItem();
            btnClear = new Button();
            label4 = new Label();
            txtStatus = new Label();
            groupBox1 = new GroupBox();
            pnlContent = new Panel();
            txtYear = new TextBox();
            lblYear = new Label();
            txtTitle = new TextBox();
            lblTitle = new Label();
            lblEpisode = new Label();
            txtEpisode = new TextBox();
            lblSeason = new Label();
            txtSeason = new TextBox();
            rdoTvShow = new RadioButton();
            rdoMovie = new RadioButton();
            toolTip1 = new ToolTip(components);
            chkEnableWatchingWhileDownloading = new CheckBox();
            label6 = new Label();
            txtAttempt = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dgvProgress).BeginInit();
            menuStrip.SuspendLayout();
            groupBox1.SuspendLayout();
            pnlContent.SuspendLayout();
            SuspendLayout();
            // 
            // txtOriginalCommand
            // 
            txtOriginalCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOriginalCommand.Font = new Font("Segoe UI", 12F);
            txtOriginalCommand.Location = new Point(12, 33);
            txtOriginalCommand.Name = "txtOriginalCommand";
            txtOriginalCommand.Size = new Size(1050, 29);
            txtOriginalCommand.TabIndex = 0;
            toolTip1.SetToolTip(txtOriginalCommand, "Command that FFmpeg should run (the name of the download file will be adjusted)");
            // 
            // cboFolder
            // 
            cboFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboFolder.Font = new Font("Segoe UI", 12F);
            cboFolder.FormattingEnabled = true;
            cboFolder.Location = new Point(10, 247);
            cboFolder.Name = "cboFolder";
            cboFolder.Size = new Size(897, 29);
            cboFolder.TabIndex = 1;
            toolTip1.SetToolTip(cboFolder, "Download folder");
            // 
            // btnBrowseForFolder
            // 
            btnBrowseForFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseForFolder.Font = new Font("Segoe UI", 12F);
            btnBrowseForFolder.Location = new Point(926, 247);
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
            txtFileName.Location = new Point(10, 310);
            txtFileName.Name = "txtFileName";
            txtFileName.Size = new Size(897, 29);
            txtFileName.TabIndex = 3;
            toolTip1.SetToolTip(txtFileName, resources.GetString("txtFileName.ToolTip"));
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(83, 21);
            label1.TabIndex = 4;
            label1.Text = "Command";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F);
            label2.Location = new Point(10, 223);
            label2.Name = "label2";
            label2.Size = new Size(54, 21);
            label2.TabIndex = 5;
            label2.Text = "Folder";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F);
            label3.Location = new Point(10, 286);
            label3.Name = "label3";
            label3.Size = new Size(80, 21);
            label3.TabIndex = 6;
            label3.Text = "File Name";
            // 
            // btnRun
            // 
            btnRun.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnRun.Location = new Point(12, 385);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(153, 44);
            btnRun.TabIndex = 5;
            btnRun.Text = "Download";
            toolTip1.SetToolTip(btnRun, "FFmpeg runs the command and downloads the video file");
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Segoe UI", 12F);
            btnCancel.Location = new Point(187, 385);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(153, 44);
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
            dgvProgress.Location = new Point(14, 449);
            dgvProgress.Name = "dgvProgress";
            dgvProgress.ReadOnly = true;
            dgvProgress.RowHeadersVisible = false;
            dgvProgress.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProgress.Size = new Size(365, 198);
            dgvProgress.TabIndex = 8;
            dgvProgress.TabStop = false;
            toolTip1.SetToolTip(dgvProgress, "Feedback from FFmpeg");
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(14, 742);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(1048, 23);
            progressBar.TabIndex = 9;
            // 
            // btnOpenFile
            // 
            btnOpenFile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOpenFile.Font = new Font("Segoe UI", 12F);
            btnOpenFile.Location = new Point(14, 777);
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
            btnOpenFolder.Location = new Point(189, 777);
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
            btnOpenLogFile.Location = new Point(365, 777);
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
            lblEstimatedRemaining.Location = new Point(14, 723);
            lblEstimatedRemaining.Name = "lblEstimatedRemaining";
            lblEstimatedRemaining.Size = new Size(186, 19);
            lblEstimatedRemaining.TabIndex = 14;
            lblEstimatedRemaining.Text = "Estimated remaining time: —";
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { menuTools, subtitlesToolStripMenuItem, menuHelp, menuNewVersion });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1074, 24);
            menuStrip.TabIndex = 15;
            // 
            // menuTools
            // 
            menuTools.DropDownItems.AddRange(new ToolStripItem[] { menuCreateShortcut, menuSettings });
            menuTools.Name = "menuTools";
            menuTools.Size = new Size(47, 20);
            menuTools.Text = "Tools";
            // 
            // menuCreateShortcut
            // 
            menuCreateShortcut.Name = "menuCreateShortcut";
            menuCreateShortcut.Size = new Size(165, 22);
            menuCreateShortcut.Text = "Create Shortcut...";
            menuCreateShortcut.Click += menuCreateShortcut_Click;
            // 
            // menuSettings
            // 
            menuSettings.Name = "menuSettings";
            menuSettings.Size = new Size(165, 22);
            menuSettings.Text = "Settings";
            menuSettings.Click += menuSettings_Click_1;
            // 
            // subtitlesToolStripMenuItem
            // 
            subtitlesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mnuExtractSubtitleFile });
            subtitlesToolStripMenuItem.Name = "subtitlesToolStripMenuItem";
            subtitlesToolStripMenuItem.Size = new Size(64, 20);
            subtitlesToolStripMenuItem.Text = "Subtitles";
            // 
            // mnuExtractSubtitleFile
            // 
            mnuExtractSubtitleFile.Name = "mnuExtractSubtitleFile";
            mnuExtractSubtitleFile.Size = new Size(182, 22);
            mnuExtractSubtitleFile.Text = "Extract Subtitle File...";
            mnuExtractSubtitleFile.Click += mnuExtractSubtitleFile_Click;
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
            // menuNewVersion
            // 
            menuNewVersion.Alignment = ToolStripItemAlignment.Right;
            menuNewVersion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            menuNewVersion.ForeColor = Color.Green;
            menuNewVersion.Name = "menuNewVersion";
            menuNewVersion.Size = new Size(142, 20);
            menuNewVersion.Text = "New Version Available";
            menuNewVersion.Visible = false;
            menuNewVersion.Click += menuNewVersion_Click;
            // 
            // btnClear
            // 
            btnClear.Font = new Font("Segoe UI", 12F);
            btnClear.Location = new Point(363, 385);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(153, 44);
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
            label4.Location = new Point(14, 692);
            label4.Name = "label4";
            label4.Size = new Size(47, 19);
            label4.TabIndex = 14;
            label4.Text = "Status";
            // 
            // txtStatus
            // 
            txtStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtStatus.BackColor = SystemColors.Control;
            txtStatus.BorderStyle = BorderStyle.FixedSingle;
            txtStatus.Location = new Point(67, 692);
            txtStatus.Name = "txtStatus";
            txtStatus.Padding = new Padding(5, 0, 0, 0);
            txtStatus.Size = new Size(824, 23);
            txtStatus.TabIndex = 9;
            txtStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(txtYear);
            groupBox1.Controls.Add(lblYear);
            groupBox1.Controls.Add(txtTitle);
            groupBox1.Controls.Add(lblTitle);
            groupBox1.Controls.Add(lblEpisode);
            groupBox1.Controls.Add(txtEpisode);
            groupBox1.Controls.Add(lblSeason);
            groupBox1.Controls.Add(txtSeason);
            groupBox1.Controls.Add(rdoTvShow);
            groupBox1.Controls.Add(rdoMovie);
            groupBox1.Location = new Point(14, 87);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(895, 122);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Auto-Suggest Folder and File Name";
            // 
            // txtYear
            // 
            txtYear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtYear.Location = new Point(591, 79);
            txtYear.MaxLength = 4;
            txtYear.Name = "txtYear";
            txtYear.Size = new Size(62, 23);
            txtYear.TabIndex = 3;
            toolTip1.SetToolTip(txtYear, "Optional — 2 or 4 digits, e.g. 26 or 2026");
            txtYear.TextChanged += txtYear_TextChanged;
            txtYear.Leave += txtYear_Leave;
            // 
            // lblYear
            // 
            lblYear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblYear.AutoSize = true;
            lblYear.Location = new Point(591, 61);
            lblYear.Name = "lblYear";
            lblYear.Size = new Size(29, 15);
            lblYear.TabIndex = 9;
            lblYear.Text = "Year";
            // 
            // txtTitle
            // 
            txtTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTitle.Location = new Point(15, 79);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(500, 23);
            txtTitle.TabIndex = 2;
            toolTip1.SetToolTip(txtTitle, "Title used to build the folder and file name");
            txtTitle.TextChanged += txtTitle_TextChanged;
            txtTitle.Leave += txtTitle_Leave;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(15, 61);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(30, 15);
            lblTitle.TabIndex = 7;
            lblTitle.Text = "Title";
            // 
            // lblEpisode
            // 
            lblEpisode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblEpisode.AutoSize = true;
            lblEpisode.Location = new Point(804, 61);
            lblEpisode.Name = "lblEpisode";
            lblEpisode.Size = new Size(48, 15);
            lblEpisode.TabIndex = 6;
            lblEpisode.Text = "Episode";
            lblEpisode.Visible = false;
            // 
            // txtEpisode
            // 
            txtEpisode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtEpisode.Location = new Point(804, 79);
            txtEpisode.Name = "txtEpisode";
            txtEpisode.Size = new Size(63, 23);
            txtEpisode.TabIndex = 5;
            toolTip1.SetToolTip(txtEpisode, "Optional");
            txtEpisode.Visible = false;
            // 
            // lblSeason
            // 
            lblSeason.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSeason.AutoSize = true;
            lblSeason.Location = new Point(726, 61);
            lblSeason.Name = "lblSeason";
            lblSeason.Size = new Size(44, 15);
            lblSeason.TabIndex = 4;
            lblSeason.Text = "Season";
            lblSeason.Visible = false;
            // 
            // txtSeason
            // 
            txtSeason.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSeason.Location = new Point(726, 79);
            txtSeason.Name = "txtSeason";
            txtSeason.Size = new Size(63, 23);
            txtSeason.TabIndex = 4;
            toolTip1.SetToolTip(txtSeason, "Optional");
            txtSeason.Visible = false;
            txtSeason.TextChanged += txtSeason_TextChanged;
            // 
            // rdoTvShow
            // 
            rdoTvShow.Location = new Point(140, 22);
            rdoTvShow.Name = "rdoTvShow";
            rdoTvShow.Size = new Size(153, 36);
            rdoTvShow.TabIndex = 1;
            rdoTvShow.Text = "TV Show";
            toolTip1.SetToolTip(rdoTvShow, "Auto-suggest folder and file name for a TV show");
            rdoTvShow.UseVisualStyleBackColor = true;
            rdoTvShow.CheckedChanged += rdoTvShow_CheckedChanged;
            // 
            // rdoMovie
            // 
            rdoMovie.Location = new Point(15, 22);
            rdoMovie.Name = "rdoMovie";
            rdoMovie.Size = new Size(153, 36);
            rdoMovie.TabIndex = 0;
            rdoMovie.Text = "Movie";
            toolTip1.SetToolTip(rdoMovie, "Auto-suggest folder and file name for a movie");
            rdoMovie.UseVisualStyleBackColor = true;
            rdoMovie.CheckedChanged += rdoMovie_CheckedChanged;
            // 
            // chkEnableWatchingWhileDownloading
            // 
            chkEnableWatchingWhileDownloading.AutoSize = true;
            chkEnableWatchingWhileDownloading.Font = new Font("Segoe UI", 12F);
            chkEnableWatchingWhileDownloading.Location = new Point(12, 346);
            chkEnableWatchingWhileDownloading.Name = "chkEnableWatchingWhileDownloading";
            chkEnableWatchingWhileDownloading.Size = new Size(285, 25);
            chkEnableWatchingWhileDownloading.TabIndex = 4;
            chkEnableWatchingWhileDownloading.Text = "Enable Watching While Downloading";
            toolTip1.SetToolTip(chkEnableWatchingWhileDownloading, "Downloads as a .ts file first so you can watch while downloading, then converts to the final format automatically.\nNo sound, message box or taskbar flash when the download is finished, unless you uncheck this box before then (errors are always shown).");
            chkEnableWatchingWhileDownloading.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 10F);
            label6.Location = new Point(937, 692);
            label6.Name = "label6";
            label6.Size = new Size(60, 19);
            label6.TabIndex = 17;
            label6.Text = "Attempt";
            // 
            // txtAttempt
            // 
            txtAttempt.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            txtAttempt.BorderStyle = BorderStyle.FixedSingle;
            txtAttempt.Location = new Point(1003, 692);
            txtAttempt.Name = "txtAttempt";
            txtAttempt.ReadOnly = true;
            txtAttempt.Size = new Size(59, 23);
            txtAttempt.TabIndex = 18;
            txtAttempt.TabStop = false;
            // 
            // pnlContent
            // 
            pnlContent.AutoScroll = true;
            pnlContent.Controls.Add(txtAttempt);
            pnlContent.Controls.Add(label6);
            pnlContent.Controls.Add(chkEnableWatchingWhileDownloading);
            pnlContent.Controls.Add(groupBox1);
            pnlContent.Controls.Add(txtStatus);
            pnlContent.Controls.Add(btnClear);
            pnlContent.Controls.Add(btnOpenLogFile);
            pnlContent.Controls.Add(btnOpenFolder);
            pnlContent.Controls.Add(btnOpenFile);
            pnlContent.Controls.Add(label4);
            pnlContent.Controls.Add(lblEstimatedRemaining);
            pnlContent.Controls.Add(progressBar);
            pnlContent.Controls.Add(dgvProgress);
            pnlContent.Controls.Add(label3);
            pnlContent.Controls.Add(label2);
            pnlContent.Controls.Add(label1);
            pnlContent.Controls.Add(txtFileName);
            pnlContent.Controls.Add(btnCancel);
            pnlContent.Controls.Add(btnRun);
            pnlContent.Controls.Add(btnBrowseForFolder);
            pnlContent.Controls.Add(cboFolder);
            pnlContent.Controls.Add(txtOriginalCommand);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 24);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(1074, 825);
            pnlContent.TabIndex = 0;
            // 
            // Form1
            // 
            AcceptButton = btnRun;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1074, 849);
            Controls.Add(pnlContent);
            Controls.Add(menuStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip;
            MinimumSize = new Size(618, 500);
            Name = "Form1";
            Text = "FFmpeg Assistant";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dgvProgress).EndInit();
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
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
        private ToolStripMenuItem menuTools;
        private ToolStripMenuItem menuCreateShortcut;
        private ToolStripMenuItem menuHelp;
        private ToolStripMenuItem menuAbout;
        private ToolStripMenuItem menuNewVersion;
        private ToolStripMenuItem menuSettings;
        private Button btnOpenLogFile;
        private Label lblEstimatedRemaining;
        private Button btnClear;
        private Label label4;
        private Label txtStatus;
        private GroupBox groupBox1;
        private Panel pnlContent;
        private RadioButton rdoMovie;
        private RadioButton rdoTvShow;
        private ToolTip toolTip1;
        private CheckBox chkEnableWatchingWhileDownloading;
        private Label lblEpisode;
        private TextBox txtEpisode;
        private Label lblSeason;
        private TextBox txtSeason;
        private Label label6;
        private TextBox txtAttempt;
        private ToolStripMenuItem subtitlesToolStripMenuItem;
        private ToolStripMenuItem mnuExtractSubtitleFile;
        private Label lblYear;
        private TextBox txtTitle;
        private Label lblTitle;
        private TextBox txtYear;
    }
}
