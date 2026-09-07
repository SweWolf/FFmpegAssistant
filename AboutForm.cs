using System.Reflection;

namespace FFmpegAssistant;

public partial class AboutForm : Form
{
    public AboutForm()
    {
        InitializeComponent();

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        lblVersion.Text = version != null
            ? $"Version {version.Major}.{version.Minor}.{version.Build}"
            : "Version 1.0.0";

        lblFfmpegVer.Text = GetFfmpegVersion();
        lnkFfmpeg.LinkClicked += lnkFfmpeg_LinkClicked;
        Shown += AboutForm_Shown;

        try
        {
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("FFmpegAssistant.Resources.FFmpegAssistant.png");
            if (stream != null)
                picIcon.Image = Image.FromStream(stream);
        }
        catch { }
    }

    private async void AboutForm_Shown(object sender, EventArgs e)
    {
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
        var result = await GitHubUpdateChecker.CheckAsync("SweWolf", "FFmpegAssistant", currentVersion);

        if (result == null || IsDisposed) return; // network error or form already closed

        if (result.IsUpdateAvailable)
        {
            lblUpdateStatus.Text = $"↑ Version {result.LatestVersion} available";
            lblUpdateStatus.ForeColor = Color.FromArgb(255, 210, 80); // warm yellow
        }
        else
        {
            lblUpdateStatus.Text = "✓ This is the latest version";
            lblUpdateStatus.ForeColor = Color.FromArgb(120, 210, 120); // light green
        }
    }

    private static string GetFfmpegVersion()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo(AppSettings.GetFfmpegExe(), "-version")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = System.Diagnostics.Process.Start(psi);
            if (process == null) return "Not found";
            string firstLine = process.StandardOutput.ReadLine() ?? "";
            process.WaitForExit();
            // First line: "ffmpeg version 7.1.1 Copyright (c) ..."
            var match = System.Text.RegularExpressions.Regex.Match(firstLine, @"ffmpeg version (\S+)");
            return match.Success ? match.Groups[1].Value : "Unknown";
        }
        catch { return "Not found"; }
    }

    private void btnClose_Click(object sender, EventArgs e) => Close();

    private void lnkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName        = "https://github.com/SweWolf/FFmpegAssistant",
            UseShellExecute = true,
        });
    }

    private void lnkFfmpeg_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName        = "https://ffmpeg.org",
            UseShellExecute = true,
        });
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape) { Close(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
