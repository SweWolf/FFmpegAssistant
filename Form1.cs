using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FFmpegAssistant
{
    public partial class Form1 : Form
    {
        private TimeSpan                 _totalDuration = TimeSpan.Zero;
        private string?                  _lastOutputPath;
        private string?                  _lastLogFile;
        private CancellationTokenSource? _cts;

        private static readonly Regex DurationPattern =
            new(@"Duration:\s*(\d{2}):(\d{2}):(\d{2})\.(\d+)", RegexOptions.Compiled);

        private static readonly Regex ProgressPattern =
            new(@"frame=\s*(\d+)\s+fps=\s*(\S+)\s+q=\S+\s+size=\s*(\S+)\s+time=(\d{2}:\d{2}:\d{2}\.\d+)\s+bitrate=(\S+)\s+speed=(\S+)(?:\s+elapsed=(\S+))?",
                RegexOptions.Compiled);

        private static readonly Regex EpisodePattern =
            new(@"^(.+) - s(\d+)e(\d+)(\..+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Keywords that indicate an error line in FFmpeg output
        private static readonly string[] ErrorKeywords =
            { "error", "failed", "invalid data", "connection refused",
              "connection timed out", "no route to host", "no such file",
              "unable to open", "broken pipe", "i/o error", "network unreachable" };

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeProgressGrid();

            string videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            cboFolder.Items.Add(videos);
            cboFolder.Items.Add(Path.Combine(videos, "Movies"));
            cboFolder.Items.Add(Path.Combine(videos, "TV Shows"));
            cboFolder.SelectedIndex = 0;

            cboFolder.SelectedIndexChanged += (s, _) => SuggestNextEpisode(cboFolder.Text);

            btnOpenFile.Click    += btnOpenFile_Click;
            btnOpenFolder.Click  += btnOpenFolder_Click;
            btnOpenLogFile.Click += btnOpenLogFile_Click;

            btnOpenFile.Enabled    = false;
            btnOpenLogFile.Enabled = false;
            btnCancel.Enabled      = false;

            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText().Trim();
                if (text.StartsWith("ffmpeg ", StringComparison.OrdinalIgnoreCase))
                    txtOriginalCommand.Text = text;
            }
        }

        // -------------------------------------------------------------------------
        // Progress grid
        // -------------------------------------------------------------------------

        private static readonly string[] GridLabels =
            { "Duration", "Frame", "FPS", "Size", "Time", "Bitrate", "Speed", "Elapsed" };

        private void InitializeProgressGrid()
        {
            dgvProgress.Font = new Font("Segoe UI", 10F);

            dgvProgress.DefaultCellStyle.SelectionBackColor = dgvProgress.DefaultCellStyle.BackColor;
            dgvProgress.DefaultCellStyle.SelectionForeColor = dgvProgress.DefaultCellStyle.ForeColor;

            dgvProgress.EnableHeadersVisualStyles = false;
            dgvProgress.ColumnHeadersDefaultCellStyle.BackColor          = Color.FromArgb(70, 70, 70);
            dgvProgress.ColumnHeadersDefaultCellStyle.ForeColor          = Color.White;
            dgvProgress.ColumnHeadersDefaultCellStyle.Font               = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvProgress.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 70, 70);
            dgvProgress.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

            var colProperty = new DataGridViewTextBoxColumn
            {
                HeaderText = "Property",
                Name       = "colProperty",
                Width      = 100,
                ReadOnly   = true,
                SortMode   = DataGridViewColumnSortMode.NotSortable
            };
            var colValue = new DataGridViewTextBoxColumn
            {
                HeaderText   = "Value",
                Name         = "colValue",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly     = true,
                SortMode     = DataGridViewColumnSortMode.NotSortable
            };
            dgvProgress.Columns.Add(colProperty);
            dgvProgress.Columns.Add(colValue);

            foreach (string label in GridLabels)
                dgvProgress.Rows.Add(label, "");
        }

        private void UpdateGridRow(string label, string value)
        {
            foreach (DataGridViewRow row in dgvProgress.Rows)
            {
                if (row.Cells["colProperty"].Value?.ToString() == label)
                {
                    row.Cells["colValue"].Value = value;
                    return;
                }
            }
        }

        private void ResetProgress()
        {
            _totalDuration = TimeSpan.Zero;
            foreach (DataGridViewRow row in dgvProgress.Rows)
                row.Cells["colValue"].Value = "";
            progressBar.Value = 0;
        }

        // -------------------------------------------------------------------------
        // FFmpeg output parsing
        // -------------------------------------------------------------------------

        private void ProcessOutputLine(string line)
        {
            if (_totalDuration == TimeSpan.Zero)
            {
                var dm = DurationPattern.Match(line);
                if (dm.Success)
                {
                    _totalDuration = new TimeSpan(
                        0,
                        int.Parse(dm.Groups[1].Value),
                        int.Parse(dm.Groups[2].Value),
                        int.Parse(dm.Groups[3].Value),
                        int.Parse(dm.Groups[4].Value.PadRight(3, '0')[..3]));

                    Invoke(() => UpdateGridRow("Duration", _totalDuration.ToString(@"hh\:mm\:ss")));
                    return;
                }
            }

            var pm = ProgressPattern.Match(line);
            if (!pm.Success) return;

            string frame   = pm.Groups[1].Value;
            string fps     = pm.Groups[2].Value;
            string size    = pm.Groups[3].Value;
            string time    = pm.Groups[4].Value;
            string bitrate = pm.Groups[5].Value;
            string speed   = pm.Groups[6].Value;
            string elapsed = pm.Groups[7].Value;

            int percent = 0;
            if (_totalDuration > TimeSpan.Zero && TimeSpan.TryParse(time, out var current))
                percent = Math.Min((int)(current.TotalSeconds / _totalDuration.TotalSeconds * 100), 100);

            Invoke(() =>
            {
                UpdateGridRow("Frame",   frame);
                UpdateGridRow("FPS",     fps);
                UpdateGridRow("Size",    size);
                UpdateGridRow("Time",    time);
                UpdateGridRow("Bitrate", bitrate);
                UpdateGridRow("Speed",   speed);
                UpdateGridRow("Elapsed", elapsed);
                progressBar.Value = percent;
            });
        }

        private static bool IsErrorLine(string line)
        {
            // Skip progress lines — they won't contain real errors
            if (line.TrimStart().StartsWith("frame=", StringComparison.OrdinalIgnoreCase))
                return false;

            string lower = line.ToLowerInvariant();
            return ErrorKeywords.Any(kw => lower.Contains(kw));
        }

        // -------------------------------------------------------------------------
        // Button handlers
        // -------------------------------------------------------------------------

        private void btnBrowseForFolder_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.InitialDirectory    = cboFolder.Text;
            dialog.UseDescriptionForTitle = true;
            dialog.Description         = "Select output folder";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string selected = dialog.SelectedPath;
                if (!cboFolder.Items.Contains(selected))
                    cboFolder.Items.Add(selected);
                cboFolder.SelectedItem = selected;
                SuggestNextEpisode(selected);
            }
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            string originalCommand = txtOriginalCommand.Text.Trim();
            string folder          = cboFolder.Text.Trim();
            string fileName        = txtFileName.Text.Trim();

            if (string.IsNullOrEmpty(originalCommand) || string.IsNullOrEmpty(folder))
            {
                MessageBox.Show("Please fill in all fields.", "Missing input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                var lastArg = Regex.Match(originalCommand, @"(""[^""]*""|[^\s]+)\s*$");
                if (lastArg.Success)
                {
                    fileName         = Path.GetFileName(lastArg.Value.Trim().Trim('"'));
                    txtFileName.Text = fileName;
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    MessageBox.Show("Could not determine a file name from the original command.", "Missing input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            Directory.CreateDirectory(folder);

            string outputPath = Path.Combine(folder, fileName);
            string command    = ReplaceOutputFile(originalCommand, outputPath);

            if (File.Exists(outputPath))
            {
                var answer = MessageBox.Show(
                    $"File already exists:\n{outputPath}\n\nOverwrite?",
                    "File exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (answer != DialogResult.Yes)
                    return;

                command = Regex.Replace(command, @"^ffmpeg\s+", "ffmpeg -y ", RegexOptions.IgnoreCase);
            }

            string logsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SweWolfSoftware", "FFmpegAssist", "Logs");
            Directory.CreateDirectory(logsFolder);
            string logFile = Path.Combine(logsFolder, Path.GetFileNameWithoutExtension(fileName) + ".txt");

            string arguments = command[(command.IndexOf(' ') + 1)..];

            ResetProgress();
            _cts                   = new CancellationTokenSource();
            _lastLogFile           = logFile;
            btnRun.Enabled         = false;
            btnCancel.Enabled      = true;
            btnOpenFile.Enabled    = false;
            btnOpenLogFile.Enabled = false;

            try
            {
                btnOpenLogFile.Enabled = true;
                WriteAppLog($"START    : {fileName}");
                WriteAppLog($"COMMAND  : ffmpeg {arguments}");
                WriteAppLog($"OUTPUT   : {outputPath}");

                var (exitCode, errorLines) = await RunFfmpegAsync(arguments, logFile, _cts.Token);

                _lastOutputPath     = outputPath;
                btnOpenFile.Enabled = File.Exists(outputPath);

                if (exitCode != 0)
                {
                    string details = errorLines.Count > 0
                        ? string.Join("\n", errorLines.TakeLast(6))
                        : "No specific error details captured. See the log file.";

                    string message = $"FFmpeg exited with an error (code {exitCode}):\n\n{details}";
                    WriteAppLog($"RESULT   : FAILED (exit code {exitCode})");
                    WriteAppLog($"ERROR    : {details.ReplaceLineEndings(" | ")}");
                    MessageBox.Show(message, "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogError(fileName, message, logFile);
                }
                else
                {
                    WriteAppLog($"RESULT   : SUCCESS (exit code 0)");

                    bool valid = await ValidateVideoFileAsync(outputPath);
                    if (!valid)
                    {
                        string message = $"FFmpeg completed but the output file could not be validated as a working video.\n\n{outputPath}\n\nIt may be incomplete or corrupt. Check the log file for clues.";
                        WriteAppLog($"VALIDATE : FAILED — file may be corrupt or incomplete");
                        MessageBox.Show(message, "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        LogError(fileName, message, logFile);
                    }
                    else
                    {
                        WriteAppLog($"VALIDATE : OK");
                        progressBar.Value = 100;
                        MessageBox.Show("Done!", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                progressBar.Value = 0;
                WriteAppLog($"RESULT   : CANCELLED by user");
                MessageBox.Show("Download cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                string message = $"Unexpected error:\n{ex.Message}";
                WriteAppLog($"RESULT   : EXCEPTION — {ex.Message}");
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogError(fileName, message, logFile);
            }
            finally
            {
                _cts.Dispose();
                _cts              = null;
                btnRun.Enabled    = true;
                btnCancel.Enabled = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            btnCancel.Enabled = false;
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            if (_lastOutputPath == null || !File.Exists(_lastOutputPath))
            {
                MessageBox.Show("File not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Process.Start(new ProcessStartInfo(_lastOutputPath) { UseShellExecute = true });
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            if (_lastOutputPath != null && File.Exists(_lastOutputPath))
                Process.Start("explorer.exe", $"/select,\"{_lastOutputPath}\"");
            else
                Process.Start("explorer.exe", $"\"{cboFolder.Text}\"");
        }

        private void btnOpenLogFile_Click(object sender, EventArgs e)
        {
            if (_lastLogFile == null || !File.Exists(_lastLogFile))
            {
                MessageBox.Show("Log file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Process.Start(new ProcessStartInfo(_lastLogFile) { UseShellExecute = true });
        }

        // -------------------------------------------------------------------------
        // FFmpeg process
        // -------------------------------------------------------------------------

        private static string ReplaceOutputFile(string command, string newOutputPath)
        {
            var match = Regex.Match(command, @"(""[^""]*""|[^\s]+)\s*$");
            if (!match.Success)
                return command;

            string newArg = newOutputPath.Contains(' ') ? $"\"{newOutputPath}\"" : newOutputPath;
            return command[..match.Index] + newArg;
        }

        private async Task<(int ExitCode, List<string> ErrorLines)> RunFfmpegAsync(
            string arguments, string logFile, CancellationToken cancellationToken = default)
        {
            var psi = new ProcessStartInfo("ffmpeg", arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                RedirectStandardInput  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            using var process  = new Process { StartInfo = psi };
            await using var writer = new StreamWriter(logFile, append: false, System.Text.Encoding.UTF8) { AutoFlush = true };
            var writerLock  = new object();
            var errorLines  = new List<string>();

            void HandleLine(string? line)
            {
                if (line == null) return;
                lock (writerLock)
                {
                    writer.WriteLine(line);
                    if (IsErrorLine(line))
                        errorLines.Add(line.Trim());
                }
                ProcessOutputLine(line);
            }

            process.OutputDataReceived += (_, e) => HandleLine(e.Data);
            process.ErrorDataReceived  += (_, e) => HandleLine(e.Data);

            process.Start();
            process.StandardInput.Close();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                throw;
            }

            return (process.ExitCode, errorLines);
        }

        // -------------------------------------------------------------------------
        // File validation
        // -------------------------------------------------------------------------

        private static async Task<bool> ValidateVideoFileAsync(string filePath)
        {
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
                return false;

            var psi = new ProcessStartInfo(
                "ffprobe",
                $"-v error -show_entries format=duration -of csv=p=0 \"{filePath}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            try
            {
                using var probe = new Process { StartInfo = psi };
                probe.Start();
                string output = await probe.StandardOutput.ReadToEndAsync();
                await probe.WaitForExitAsync();

                return probe.ExitCode == 0
                    && double.TryParse(output.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double duration)
                    && duration > 0;
            }
            catch
            {
                // ffprobe not available — skip validation rather than falsely reporting an error
                return true;
            }
        }

        // -------------------------------------------------------------------------
        // Application log
        // -------------------------------------------------------------------------

        private static readonly string AppLogFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "SweWolfSoftware", "FFmpegAssist");

        private static readonly string AppLogFile =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "SweWolfSoftware", "FFmpegAssist", "FFmpegAssistant.log");

        private const long MaxLogBytes = 5 * 1024 * 1024; // 5 MB

        private static void WriteAppLog(string message)
        {
            try
            {
                Directory.CreateDirectory(AppLogFolder);

                // Trim to the most recent half when the file gets too large
                if (File.Exists(AppLogFile) && new FileInfo(AppLogFile).Length > MaxLogBytes)
                {
                    string[] lines  = File.ReadAllLines(AppLogFile, System.Text.Encoding.UTF8);
                    string[] kept   = lines[(lines.Length / 2)..];
                    File.WriteAllLines(AppLogFile, kept, System.Text.Encoding.UTF8);
                }

                string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                File.AppendAllText(AppLogFile, entry + Environment.NewLine, System.Text.Encoding.UTF8);
            }
            catch { /* never let logging crash the app */ }
        }

        // -------------------------------------------------------------------------
        // Error logging
        // -------------------------------------------------------------------------

        private static void LogError(string context, string details, string? relatedLogFile = null)
        {
            try
            {
                string errLogFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SweWolfSoftware", "FFmpegAssist", "Logs");
                Directory.CreateDirectory(errLogFolder);

                string errLogFile = Path.Combine(errLogFolder, "errors.log");
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]");
                sb.AppendLine($"File    : {context}");
                if (relatedLogFile != null)
                    sb.AppendLine($"Log     : {relatedLogFile}");
                sb.AppendLine($"Details : {details}");
                sb.AppendLine(new string('-', 60));

                File.AppendAllText(errLogFile, sb.ToString(), System.Text.Encoding.UTF8);
            }
            catch { /* never let error logging crash the app */ }
        }

        // -------------------------------------------------------------------------
        // Episode suggestion
        // -------------------------------------------------------------------------

        private void SuggestNextEpisode(string folder)
        {
            if (!Directory.Exists(folder))
                return;

            var matches = Directory.GetFiles(folder)
                .Select(f => EpisodePattern.Match(Path.GetFileName(f)))
                .Where(m => m.Success)
                .OrderBy(m => int.Parse(m.Groups[2].Value))
                .ThenBy(m => int.Parse(m.Groups[3].Value))
                .ToList();

            if (matches.Count == 0)
                return;

            var last      = matches.Last();
            string showName   = last.Groups[1].Value;
            int season        = int.Parse(last.Groups[2].Value);
            int episode       = int.Parse(last.Groups[3].Value) + 1;
            string ext        = last.Groups[4].Value;
            string seasonStr  = season.ToString().PadLeft(last.Groups[2].Length, '0');
            string episodeStr = episode.ToString().PadLeft(last.Groups[3].Length, '0');

            txtFileName.Text = $"{showName} - s{seasonStr}e{episodeStr}{ext}";
        }
    }
}
