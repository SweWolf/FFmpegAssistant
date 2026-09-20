namespace FFmpegAssistant
{
    internal partial class SettingsForm : Form
    {
        private const string CustomSoundSentinel = "Custom Sound File...";

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            txtFfmpegPath.Text = AppSettings.FfmpegExePath ?? string.Empty;
            txtNumberOfDownloadAttempts.Text = AppSettings.NumberOfDownloadAttempts.ToString();
            cboNewVersionCheck.SelectedItem = AppSettings.CheckForUpdatesOnStartup;

            cboFinishedDownlaodSound.Items.AddRange(SoundLibrary.GetAvailableSounds().ToArray());
            cboFinishedDownlaodSound.Items.Add(CustomSoundSentinel);

            cboActionWhenDownloadFinished.SelectedItem = AppSettings.ActionWhenDownloadFinished;

            string savedSound = AppSettings.FinishedDownloadSoundFile;
            if (string.IsNullOrEmpty(savedSound))
                savedSound = SoundLibrary.DefaultSoundFileName;

            var soundOptions = cboFinishedDownlaodSound.Items.OfType<SoundOption>().ToList();
            SoundOption? match = soundOptions.FirstOrDefault(o => o.FileName == savedSound);
            if (match != null)
            {
                cboFinishedDownlaodSound.SelectedItem = match;
            }
            else
            {
                // Not one of the bundled sounds — treat the saved value as a custom file path
                cboFinishedDownlaodSound.SelectedItem = CustomSoundSentinel;
                txtCustomActionSoundFile.Text = savedSound;
            }

            cboActionWhenDownloadFinished.SelectedIndexChanged += CboActionWhenDownloadFinished_SelectedIndexChanged;
            cboFinishedDownlaodSound.SelectedIndexChanged += CboFinishedDownlaodSound_SelectedIndexChanged;
            cmdPlayActionSound.Click += CmdPlayActionSound_Click;
            btnBrowseForCustomSoundFile.Click += BtnBrowseForCustomSoundFile_Click;
            UpdateSoundControlsVisibility();
        }

        private void CboActionWhenDownloadFinished_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateSoundControlsVisibility();
        }

        private void CboFinishedDownlaodSound_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateSoundControlsVisibility();
        }

        private void UpdateSoundControlsVisibility()
        {
            bool playSound = cboActionWhenDownloadFinished.SelectedItem?.ToString() == "Play a Sound";
            cboFinishedDownlaodSound.Visible = playSound;
            lblFinishedDownlaodSound.Visible = playSound;
            cmdPlayActionSound.Visible = playSound;

            bool customSound = playSound && IsCustomSoundSelected;
            label5.Visible = customSound;
            txtCustomActionSoundFile.Visible = customSound;
            btnBrowseForCustomSoundFile.Visible = customSound;
        }

        private bool IsCustomSoundSelected => (cboFinishedDownlaodSound.SelectedItem as string) == CustomSoundSentinel;

        /// <summary>
        /// Returns the sound identifier to persist/play for the current selection: the bundled
        /// file name, or the full path typed/browsed into the custom-sound text box.
        /// </summary>
        private string GetSelectedSoundIdentifier()
        {
            if (IsCustomSoundSelected)
                return txtCustomActionSoundFile.Text.Trim();

            return (cboFinishedDownlaodSound.SelectedItem as SoundOption)?.FileName ?? string.Empty;
        }

        private void CmdPlayActionSound_Click(object? sender, EventArgs e)
        {
            SoundLibrary.Play(GetSelectedSoundIdentifier());
        }

        private void BtnBrowseForCustomSoundFile_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select custom sound file",
                Filter = "WAV audio files (*.wav)|*.wav",
                DefaultExt = "wav"
            };

            string current = txtCustomActionSoundFile.Text.Trim();
            if (!string.IsNullOrEmpty(current))
            {
                string? dir = Path.GetDirectoryName(current);
                if (dir != null && Directory.Exists(dir))
                    ofd.InitialDirectory = dir;
            }

            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            if (!string.Equals(Path.GetExtension(ofd.FileName), ".wav", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Audio file must be of the type WAV.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtCustomActionSoundFile.Text = ofd.FileName;
        }

        private void btnBrowseFfmpeg_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Locate ffmpeg.exe",
                Filter = "ffmpeg.exe|ffmpeg.exe|Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                FileName = "ffmpeg.exe"
            };

            string current = txtFfmpegPath.Text.Trim();
            if (!string.IsNullOrEmpty(current))
            {
                string? dir = Path.GetDirectoryName(current);
                if (dir != null && Directory.Exists(dir))
                    ofd.InitialDirectory = dir;
            }

            if (ofd.ShowDialog(this) == DialogResult.OK)
                txtFfmpegPath.Text = ofd.FileName;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            bool playSound = cboActionWhenDownloadFinished.SelectedItem?.ToString() == "Play a Sound";
            if (playSound && IsCustomSoundSelected)
            {
                string customPath = txtCustomActionSoundFile.Text.Trim();
                if (string.IsNullOrEmpty(customPath) || !string.Equals(Path.GetExtension(customPath), ".wav", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Audio file must be of the type WAV.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            string path = txtFfmpegPath.Text.Trim();
            if (string.IsNullOrEmpty(path))
                AppSettings.ClearFfmpegExe();
            else
                AppSettings.SetFfmpegExe(path);

            string attemptsText = txtNumberOfDownloadAttempts.Text.Trim();
            AppSettings.NumberOfDownloadAttempts = int.TryParse(attemptsText, out int attempts) ? attempts : 1;

            AppSettings.CheckForUpdatesOnStartup = cboNewVersionCheck.SelectedItem?.ToString() ?? "Yes";

            AppSettings.ActionWhenDownloadFinished = cboActionWhenDownloadFinished.SelectedItem?.ToString() ?? "Play a Sound";
            AppSettings.FinishedDownloadSoundFile = GetSelectedSoundIdentifier();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
