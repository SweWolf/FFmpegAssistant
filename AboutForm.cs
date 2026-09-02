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

        try
        {
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("FFmpegAssistant.Resources.FFmpegAssistant.png");
            if (stream != null)
                picIcon.Image = Image.FromStream(stream);
        }
        catch { }
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
