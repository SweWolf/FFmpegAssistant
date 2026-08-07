namespace FFmpegAssistant
{
    internal partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            txtFfmpegPath.Text = AppSettings.FfmpegExePath ?? string.Empty;
            cboReplaceQas.SelectedItem = AppSettings.ReplaceAudioQas;
            txtNumberOfDownloadAttempts.Text = AppSettings.NumberOfDownloadAttempts.ToString();
        }

        private void btnBrowseFfmpeg_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title    = "Locate ffmpeg.exe",
                Filter   = "ffmpeg.exe|ffmpeg.exe|Executable files (*.exe)|*.exe|All files (*.*)|*.*",
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
            string path = txtFfmpegPath.Text.Trim();
            if (string.IsNullOrEmpty(path))
                AppSettings.ClearFfmpegExe();
            else
                AppSettings.SetFfmpegExe(path);

            AppSettings.ReplaceAudioQas = cboReplaceQas.SelectedItem?.ToString() ?? "Ask";

            string attemptsText = txtNumberOfDownloadAttempts.Text.Trim();
            AppSettings.NumberOfDownloadAttempts = int.TryParse(attemptsText, out int attempts) ? attempts : 1;

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
