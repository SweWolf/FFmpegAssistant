using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FFmpegAssistant
{
    public partial class Form1 : Form
    {
        private TimeSpan _totalDuration = TimeSpan.Zero;
        private string? _lastOutputPath;
        private string? _lastLogFile;
        private CancellationTokenSource? _cts;
        private bool _progressStarted;

        // -------------------------------------------------------------------------
        // Estimated remaining time — speed sampling
        // -------------------------------------------------------------------------

        private enum EstimationMode { Stable, CurrentSpeed }

        /// <summary>
        /// Controls how the estimated remaining time is calculated.
        /// Stable  = lowest of the last <see cref="SpeedSampleCount"/> non-zero speed samples (default).
        /// CurrentSpeed = live FFmpeg speed value only.
        /// </summary>
        private const EstimationMode SpeedMode = EstimationMode.Stable;
        private const int SpeedSampleCount = 5;

        /// <summary>
        /// When true, grid values are formatted for readability (normalised elapsed time,
        /// size converted to MB). When false, raw FFmpeg output is shown as-is.
        /// Future: expose this in a Settings dialog.
        /// </summary>
        private const bool AdjustedFeedback = true;
        private readonly Queue<double> _speedSamples = new();

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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Icon is set by the Designer via InitializeComponent (Form1.resx).
            // No override needed here — overriding can lose alpha channel transparency.

            // Check for updates in the background — does not block startup
            _ = CheckForUpdatesAsync();

            InitializeProgressGrid();

            string videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            cboFolder.Items.Add(videos);
            cboFolder.Items.Add(Path.Combine(videos, "Movies"));
            cboFolder.Items.Add(Path.Combine(videos, "TV Shows"));
            cboFolder.SelectedIndex = 0;

            cboFolder.SelectedIndexChanged += (s, _) => SuggestNextEpisode(cboFolder.Text);
            cboFolder.Leave += (s, _) => SuggestNextEpisode(cboFolder.Text);

            btnOpenFile.Enabled = false;
            btnOpenLogFile.Enabled = false;
            btnCancel.Enabled = false;

            // Select all text when a text box receives focus — makes it easy to replace the value
            void SelectAllOnFocus(object? s, EventArgs _) { if (s is TextBox tb) tb.SelectAll(); }
            txtOriginalCommand.Enter += SelectAllOnFocus;
            txtFileName.Enter += SelectAllOnFocus;
            txtSeason.Enter += SelectAllOnFocus;
            txtEpisode.Enter += SelectAllOnFocus;

            // Season/Episode: accept digits only (keyboard path — blocks the character immediately)
            void NumericOnly(object? s, KeyPressEventArgs e)
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            }
            txtSeason.KeyPress += NumericOnly;
            txtEpisode.KeyPress += NumericOnly;

            // Season/Episode: wire up the TextChanged handlers (paste path is handled there too)
            txtEpisode.TextChanged += txtEpisode_TextChanged;

            // Clear status when the user starts editing the input fields
            txtOriginalCommand.TextChanged += (s, _) => txtStatus.Clear();
            txtFileName.TextChanged += (s, _) => txtStatus.Clear();

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
            dgvProgress.AllowUserToAddRows = false;

            dgvProgress.DefaultCellStyle.SelectionBackColor = dgvProgress.DefaultCellStyle.BackColor;
            dgvProgress.DefaultCellStyle.SelectionForeColor = dgvProgress.DefaultCellStyle.ForeColor;

            dgvProgress.EnableHeadersVisualStyles = false;
            dgvProgress.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(70, 70, 70);
            dgvProgress.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProgress.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvProgress.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 70, 70);
            dgvProgress.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

            var colProperty = new DataGridViewTextBoxColumn
            {
                HeaderText = "Property",
                Name = "colProperty",
                Width = 100,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            var colValue = new DataGridViewTextBoxColumn
            {
                HeaderText = "Value",
                Name = "colValue",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            dgvProgress.Columns.Add(colProperty);
            dgvProgress.Columns.Add(colValue);

            foreach (string label in GridLabels)
                dgvProgress.Rows.Add(label, "");

            // Shrink the grid to exactly fit its rows — no grey empty space below
            int exactHeight = dgvProgress.ColumnHeadersHeight
                            + dgvProgress.Rows.Cast<DataGridViewRow>().Sum(r => r.Height)
                            + 2; // border
            dgvProgress.Height = exactHeight;
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
            _progressStarted = false;
            _speedSamples.Clear();
            foreach (DataGridViewRow row in dgvProgress.Rows)
                row.Cells["colValue"].Value = "";
            progressBar.Value = 0;
            lblEstimatedRemaining.Text = "Estimated remaining time: —";
            txtStatus.Clear();
        }

        private void SetStatus(string message)
        {
            if (InvokeRequired)
                Invoke(() => txtStatus.Text = message);
            else
                txtStatus.Text = message;
        }

        // -------------------------------------------------------------------------
        // FFmpeg output parsing
        // -------------------------------------------------------------------------

        private void ProcessOutputLine(string line)
        {
            // Status updates during the pre-download phase
            if (!_progressStarted)
            {
                if (line.Contains("Opening '", StringComparison.OrdinalIgnoreCase))
                    SetStatus("Fetching stream information...");
                else if (line.StartsWith("Input #", StringComparison.OrdinalIgnoreCase))
                    SetStatus("Analysing input streams...");
            }

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
                    SetStatus("Starting download...");
                    return;
                }
            }

            var pm = ProgressPattern.Match(line);
            if (!pm.Success) return;

            string frame = pm.Groups[1].Value;
            string fps = pm.Groups[2].Value;
            string size = pm.Groups[3].Value;
            string time = pm.Groups[4].Value;
            string bitrate = pm.Groups[5].Value;
            string speed = pm.Groups[6].Value;
            string elapsed = pm.Groups[7].Value;

            int percent = 0;
            string estimatedRemaining = "—";

            // Parse FFmpeg's speed value (e.g. "1.82x" → 1.82 video-seconds per real-second)
            double currentSpeed = 0;
            if (double.TryParse(speed.TrimEnd('x'), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedSpeed)
                && parsedSpeed > 0)
            {
                currentSpeed = parsedSpeed;
                _speedSamples.Enqueue(currentSpeed);
                if (_speedSamples.Count > SpeedSampleCount)
                    _speedSamples.Dequeue();
            }

            if (_totalDuration > TimeSpan.Zero && TimeSpan.TryParse(time, out var current))
            {
                percent = Math.Min((int)(current.TotalSeconds / _totalDuration.TotalSeconds * 100), 100);

                double effectiveSpeed = SpeedMode == EstimationMode.Stable
                    ? (_speedSamples.Count > 0 ? _speedSamples.Min() : currentSpeed)
                    : currentSpeed;

                if (effectiveSpeed > 0 && current.TotalSeconds > 0)
                {
                    double remainingSecs = (_totalDuration.TotalSeconds - current.TotalSeconds) / effectiveSpeed;
                    estimatedRemaining = TimeSpan.FromSeconds(remainingSecs).ToString(@"h\:mm\:ss");
                }
            }

            if (!_progressStarted)
            {
                _progressStarted = true;
                SetStatus("Downloading...");
            }

            string displaySize = AdjustedFeedback ? FormatSize(size) : size;
            string displayElapsed = AdjustedFeedback && !string.IsNullOrEmpty(elapsed)
                                    ? FormatElapsed(elapsed) : elapsed;

            Invoke(() =>
            {
                UpdateGridRow("Frame", frame);
                UpdateGridRow("FPS", fps);
                UpdateGridRow("Size", displaySize);
                UpdateGridRow("Time", time);
                UpdateGridRow("Bitrate", bitrate);
                UpdateGridRow("Speed", speed);
                UpdateGridRow("Elapsed", displayElapsed);
                progressBar.Value = percent;
                lblEstimatedRemaining.Text = $"Estimated remaining time: {estimatedRemaining}";

                // Enable Open File as soon as the download file appears on disk (watch mode)
                if (!btnOpenFile.Enabled && _lastOutputPath != null && File.Exists(_lastOutputPath))
                    btnOpenFile.Enabled = true;
            });
        }

        /// <summary>Normalises FFmpeg elapsed string (e.g. "0:00:36.63") to HH:mm:ss.</summary>
        private static string FormatElapsed(string raw)
        {
            if (TimeSpan.TryParse(raw, CultureInfo.InvariantCulture, out var ts))
                return ts.ToString(@"hh\:mm\:ss");
            return raw;
        }

        /// <summary>Converts FFmpeg size string (e.g. "73984KiB") to MB with one decimal.</summary>
        private static string FormatSize(string raw)
        {
            var m = Regex.Match(raw, @"^([\d.]+)\s*(KiB|kB|MiB|MB|GiB|GB)$", RegexOptions.IgnoreCase);
            if (!m.Success) return raw;
            if (!double.TryParse(m.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                return raw;

            double mb = m.Groups[2].Value.ToUpperInvariant() switch
            {
                "KIB" or "KB" => val / 1024.0,
                "MIB" or "MB" => val,
                "GIB" or "GB" => val * 1024.0,
                _ => val / 1024.0
            };
            return $"{mb:F1} MB";
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
            dialog.InitialDirectory = cboFolder.Text;
            dialog.UseDescriptionForTitle = true;
            dialog.Description = "Select output folder";

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
            txtStatus.Text = "";

            string originalCommand = txtOriginalCommand.Text.Trim();
            string folder = cboFolder.Text.Trim();
            string fileName = txtFileName.Text.Trim();

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
                    fileName = Path.GetFileName(lastArg.Value.Trim().Trim('"'));
                    txtFileName.Text = fileName;
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    MessageBox.Show("Could not determine a file name from the original command.", "Missing input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // If the filename doesn't already end with the correct extension, append it.
            // We append rather than replace so that "Episode.TheCoolName" becomes
            // "Episode.TheCoolName.mp4" rather than losing the user's intended text.
            string correctExt = GetCommandOutputExtension(originalCommand);
            if (!string.IsNullOrEmpty(correctExt) &&
                !fileName.EndsWith(correctExt, StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName + correctExt;
                txtFileName.Text = fileName;
            }

            Directory.CreateDirectory(folder);

            string outputPath = Path.Combine(folder, fileName);

            // In watch-while-downloading mode, download to a .ts file first.
            // In normal mode, download to a "(part)" file to protect against power outages —
            // the file is renamed to the final name only after successful validation.
            bool watchMode = chkEnableWatchingWhileDownloading.Checked;
            string partPath = Path.Combine(folder,
                Path.GetFileNameWithoutExtension(fileName) + " (part)" + Path.GetExtension(fileName));
            string downloadPath = watchMode
                ? Path.ChangeExtension(outputPath, ".ts")
                : partPath;

            string command = ReplaceOutputFile(originalCommand, downloadPath);

            // Overwrite protection — always check the final output file
            if (File.Exists(outputPath))
            {
                var answer = MessageBox.Show(
                    $"File already exists:\n{outputPath}\n\nOverwrite?",
                    "File exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (answer != DialogResult.Yes)
                {
                    SetStatus("Download cancelled — file already exists.");
                    return;
                }
                // Normal mode downloads to partPath so no -y needed for the final file.
                // Watch mode: -y is added to the conversion step instead.
            }

            // If a leftover download file exists from a previous interrupted run, overwrite it
            if (File.Exists(downloadPath))
                command = Regex.Replace(command, @"^ffmpeg\s+", "ffmpeg -y ", RegexOptions.IgnoreCase);

            string logsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SweWolfSoftware", "FFmpegAssist", "Logs");
            Directory.CreateDirectory(logsFolder);
            string logFile = Path.Combine(logsFolder, Path.GetFileNameWithoutExtension(fileName) + ".txt");

            string arguments = command[(command.IndexOf(' ') + 1)..];

            bool keepTrying = true;
            while (keepTrying)
            {
                keepTrying = false;

                ResetProgress();
                _cts = new CancellationTokenSource();
                _lastLogFile = logFile;
                btnRun.Enabled = false;
                btnCancel.Enabled = true;
                btnOpenFile.Enabled = false;
                btnOpenLogFile.Enabled = false;

                try
                {
                    btnOpenLogFile.Enabled = true;
                    SetStatus("Connecting...");
                    WriteAppLog($"START    : {fileName}");
                    WriteAppLog($"COMMAND  : ffmpeg {arguments}");
                    WriteAppLog($"OUTPUT   : {outputPath}");

                    // Set download path early so Open File can activate as soon as the file appears
                    _lastOutputPath = downloadPath;

                    var (exitCode, errorLines) = await RunFfmpegAsync(arguments, logFile, _cts.Token);

                    btnOpenFile.Enabled = File.Exists(downloadPath);

                    if (exitCode != 0)
                    {
                        string details = errorLines.Count > 0
                            ? string.Join("\n", errorLines.TakeLast(6))
                            : "No specific error details captured. See the log file.";

                        string message = $"FFmpeg exited with an error (code {exitCode}):\n\n{details}";
                        WriteAppLog($"RESULT   : FAILED (exit code {exitCode})");
                        WriteAppLog($"ERROR    : {details.ReplaceLineEndings(" | ")}");
                        progressBar.Value = 0;
                        lblEstimatedRemaining.Text = "Estimated remaining time: —";
                        SetStatus("Download failed — an error occurred.");
                        MessageBox.Show(message, "Download Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        LogError(fileName, message, logFile);
                    }
                    else
                    {
                        WriteAppLog($"RESULT   : SUCCESS (exit code 0)");

                        // If watch mode: convert .ts → final format before validating
                        if (watchMode)
                        {
                            string finalExt = Path.GetExtension(outputPath).TrimStart('.');
                            SetStatus($"Converting to {finalExt}...");
                            WriteAppLog($"CONVERT  : {downloadPath} → {outputPath}");

                            string convArgs = $"-y -i \"{downloadPath}\" -c copy \"{outputPath}\"";
                            var (convCode, _) = await RunFfmpegAsync(convArgs, logFile, _cts.Token);

                            if (convCode == 0)
                            {
                                try { File.Delete(downloadPath); } catch { }
                                WriteAppLog($"CONVERT  : SUCCESS — .ts file deleted");
                                _lastOutputPath = outputPath;
                                btnOpenFile.Enabled = File.Exists(outputPath);
                            }
                            else
                            {
                                WriteAppLog($"CONVERT  : FAILED (exit code {convCode})");
                                SetStatus("Conversion failed — .ts file kept.");
                                return;
                            }
                        }

                        SetStatus("Validating downloaded file...");

                        // In normal mode validate the part file; in watch mode validate the final file
                        string validatePath = watchMode ? outputPath : partPath;
                        bool valid = await ValidateVideoFileAsync(validatePath);
                        if (valid)
                        {
                            // In normal mode: rename the (part) file to the final name now that it's verified
                            if (!watchMode)
                            {
                                SetStatus("Finalizing...");
                                if (File.Exists(outputPath)) File.Delete(outputPath);
                                File.Move(partPath, outputPath);
                                _lastOutputPath = outputPath;
                                btnOpenFile.Enabled = true;
                                WriteAppLog($"FINALIZE : Renamed (part) file to final name");
                            }

                            WriteAppLog($"VALIDATE : OK");
                            progressBar.Value = 100;
                            lblEstimatedRemaining.Text = "Estimated remaining time: 0:00:00";
                            SetStatus("Done");
                            MessageBox.Show("Done!", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            WriteAppLog($"VALIDATE : FAILED — file is corrupted");
                            progressBar.Value = 0;
                            lblEstimatedRemaining.Text = "Estimated remaining time: —";
                            LogError(fileName, "File validation failed — corrupted download", logFile);

                            var deleteAnswer = MessageBox.Show(
                                $"The downloaded file appears to be corrupted:\n\n{validatePath}\n\nDo you want to delete the file?",
                                "File Corrupted", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                            if (deleteAnswer == DialogResult.Yes)
                            {
                                try
                                {
                                    File.Delete(validatePath);
                                    WriteAppLog($"CLEANUP  : Corrupted file deleted by user");
                                }
                                catch (Exception ex)
                                {
                                    WriteAppLog($"CLEANUP  : Failed to delete corrupted file — {ex.Message}");
                                }

                                var retryAnswer = MessageBox.Show(
                                    "Do you want to try to download again?",
                                    "Retry Download", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                if (retryAnswer == DialogResult.Yes)
                                {
                                    WriteAppLog($"RETRY    : User requested retry");
                                    keepTrying = true;
                                }
                                else
                                {
                                    SetStatus("Downloaded file corrupted — file deleted.");
                                }
                            }
                            else
                            {
                                SetStatus("Downloaded file corrupted.");
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    progressBar.Value = 0;
                    lblEstimatedRemaining.Text = "Estimated remaining time: —";
                    WriteAppLog($"RESULT   : CANCELLED by user");

                    if (File.Exists(downloadPath))
                    {
                        var answer = MessageBox.Show(
                            $"Download was cancelled.\n\nA partial file was saved:\n{downloadPath}\n\nDelete it?",
                            "Cancelled", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (answer == DialogResult.Yes)
                        {
                            try
                            {
                                File.Delete(downloadPath);
                                WriteAppLog($"CLEANUP  : Partial file deleted by user");
                                SetStatus("Cancelled — partial file deleted.");
                            }
                            catch (Exception ex)
                            {
                                WriteAppLog($"CLEANUP  : Failed to delete partial file — {ex.Message}");
                                SetStatus("Cancelled — could not delete partial file.");
                            }
                        }
                        else
                        {
                            SetStatus("Cancelled — partial file kept.");
                        }
                    }
                    else
                    {
                        SetStatus("Cancelled.");
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Unexpected error:\n{ex.Message}";
                    progressBar.Value = 0;
                    lblEstimatedRemaining.Text = "Estimated remaining time: —";
                    SetStatus($"Error: {ex.Message}");
                    WriteAppLog($"RESULT   : EXCEPTION — {ex.Message}");
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogError(fileName, message, logFile);
                }
                finally
                {
                    _cts.Dispose();
                    _cts = null;
                    if (!keepTrying)
                    {
                        btnRun.Enabled = true;
                        btnCancel.Enabled = false;
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            btnCancel.Enabled = false;
        }

        private void btnOpenFile_Click_1(object sender, EventArgs e)
        {
            if (_lastOutputPath == null || !File.Exists(_lastOutputPath))
            {
                MessageBox.Show("File not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Process.Start(new ProcessStartInfo(_lastOutputPath) { UseShellExecute = true });
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtOriginalCommand.Clear();
            cboFolder.SelectedIndex = 0;
            txtFileName.Clear();

            ResetProgress();

            _lastOutputPath = null;
            _lastLogFile = null;
            btnOpenFile.Enabled = false;
            btnOpenLogFile.Enabled = false;

            lblSeason.Visible = false;
            txtSeason.Visible = false;
            lblEpisode.Visible = false;
            txtEpisode.Visible = false;
            txtSeason.Text = "";
            txtEpisode.Text = "";
        }

        private void btnOpenFolder_Click_1(object sender, EventArgs e)
        {
            string folder = cboFolder.Text.Trim();
            string fileName = txtFileName.Text.Trim();

            // If we can build a path from the current UI fields and the file already exists,
            // open Explorer with the file pre-selected — works during and after a download
            if (!string.IsNullOrEmpty(folder) && !string.IsNullOrEmpty(fileName))
            {
                string candidate = Path.Combine(folder, fileName);
                if (File.Exists(candidate))
                {
                    Process.Start("explorer.exe", $"/select,\"{candidate}\"");
                    return;
                }
            }

            // File doesn't exist yet — just open the folder
            Process.Start("explorer.exe", $"\"{folder}\"");
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            using var about = new AboutForm();
            about.ShowDialog(this);
        }

        private async Task CheckForUpdatesAsync()
        {
            var currentVersion = System.Reflection.Assembly
                .GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);

            var result = await GitHubUpdateChecker.CheckAsync(
                "SweWolf", "FFmpegAssistant", currentVersion);

            if (result is { IsUpdateAvailable: true })
            {
                var answer = MessageBox.Show(
                    $"A new version is available: {result.LatestVersion}\n\n" +
                    $"You are running version {currentVersion.Major}.{currentVersion.Minor}.{currentVersion.Build}.\n\n" +
                    $"Do you want to go to the download page?",
                    "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (answer == DialogResult.Yes)
                    Process.Start(new ProcessStartInfo(result.ReleasePageUrl)
                    { UseShellExecute = true });
            }
        }

        private void menuCreateShortcut_Click(object sender, EventArgs e)
        {
            using var form = new CreateShortcutForm(Application.ExecutablePath);
            form.ShowDialog(this);
        }

        private void btnOpenLogFile_Click_1(object sender, EventArgs e)
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

        private static string GetCommandOutputExtension(string command)
        {
            var match = Regex.Match(command, @"(""[^""]*""|[^\s]+)\s*$");
            if (!match.Success) return string.Empty;
            return Path.GetExtension(match.Value.Trim().Trim('"')); // e.g. ".mp4"
        }

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
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            await using var writer = new StreamWriter(logFile, append: false, System.Text.Encoding.UTF8) { AutoFlush = true };
            var writerLock = new object();
            var errorLines = new List<string>();

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
            process.ErrorDataReceived += (_, e) => HandleLine(e.Data);

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

        /// <summary>
        /// Validates a video file by running FFmpeg over it and checking for decode errors.
        /// Returns true if the file is OK, false if it is corrupted or unreadable.
        /// </summary>
        private static async Task<bool> ValidateVideoFileAsync(string filePath)
        {
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
                return false;

            var psi = new ProcessStartInfo(
                "ffmpeg",
                $"-v error -i \"{filePath}\" -f null -")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var process = new Process { StartInfo = psi };
                process.Start();
                string stderr = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                // Any output on stderr means FFmpeg found decode errors
                return string.IsNullOrWhiteSpace(stderr);
            }
            catch
            {
                // FFmpeg not available — skip validation rather than falsely reporting an error
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
                    string[] lines = File.ReadAllLines(AppLogFile, System.Text.Encoding.UTF8);
                    string[] kept = lines[(lines.Length / 2)..];
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

            var last = matches.Last();
            string showName = last.Groups[1].Value;
            int season = int.Parse(last.Groups[2].Value);
            int episode = int.Parse(last.Groups[3].Value) + 1;
            string ext = last.Groups[4].Value;
            string seasonStr = season.ToString().PadLeft(last.Groups[2].Length, '0');
            string episodeStr = episode.ToString().PadLeft(last.Groups[3].Length, '0');

            txtFileName.Text = $"{showName} - s{seasonStr}e{episodeStr}{ext}";
        }

        private void btnMovie_Click(object sender, EventArgs e)
        {
            cboFolder.SelectedIndex = 1;

            // Try to extract a clean movie name from the output filename in the original command
            string originalCommand = txtOriginalCommand.Text.Trim();
            if (!string.IsNullOrEmpty(originalCommand))
            {
                var lastArg = Regex.Match(originalCommand, @"(""[^""]*""|[^\s]+)\s*$");
                if (lastArg.Success)
                {
                    string raw = lastArg.Value.Trim().Trim('"');
                    string ext = Path.GetExtension(raw);
                    string nameOnly = Path.GetFileNameWithoutExtension(raw);

                    // Find the earliest delimiter: '-' or '['
                    int dashIndex = nameOnly.IndexOf('-');
                    int bracketIndex = nameOnly.IndexOf('[');

                    int delimIndex = (dashIndex, bracketIndex) switch
                    {
                        ( >= 0, >= 0) => Math.Min(dashIndex, bracketIndex),
                        ( >= 0, _) => dashIndex,
                        (_, >= 0) => bracketIndex,
                        _ => -1   // no delimiter — use full name
                    };

                    string cleanName = delimIndex > 0
                        ? nameOnly[..delimIndex].Trim()
                        : nameOnly.Trim();

                    if (!string.IsNullOrEmpty(cleanName))
                        txtFileName.Text = cleanName + ext;
                }
            }

            lblSeason.Visible = false;
            txtSeason.Visible = false;
            lblEpisode.Visible = false;
            txtEpisode.Visible = false;
            txtSeason.Text = "";
            txtEpisode.Text = "";

            btnRun.Focus();
        }

        private void txtSeason_TextChanged(object sender, EventArgs e)
        {
            StripNonDigits(txtSeason);
            if (txtSeason.Text.Length > 0 && txtEpisode.Text.Length == 0)
                txtEpisode.Text = "1";
            UpdateFileNameFromSeasonEpisode();
        }

        private void txtEpisode_TextChanged(object sender, EventArgs e)
        {
            StripNonDigits(txtEpisode);
            if (txtEpisode.Text.Length > 0 && txtSeason.Text.Length == 0)
                txtSeason.Text = "1";
            UpdateFileNameFromSeasonEpisode();
        }

        /// <summary>
        /// Removes any non-digit characters from a TextBox, preserving the caret position.
        /// Handles text pasted from the clipboard that may contain non-numeric characters.
        /// </summary>
        private static void StripNonDigits(TextBox tb)
        {
            string digits = new string(tb.Text.Where(char.IsDigit).ToArray());
            if (digits == tb.Text) return;                       // nothing to strip
            int caret = Math.Max(0, tb.SelectionStart - (tb.Text.Length - digits.Length));
            tb.Text = digits;                                    // triggers TextChanged again,
            tb.SelectionStart = Math.Min(caret, digits.Length); // but digits==tb.Text so it exits immediately
        }

        /// <summary>
        /// Rebuilds the filename using the current Season and Episode box values,
        /// overriding whatever the folder scan suggested. Does nothing if the
        /// current filename does not match the TV-show naming pattern, or if
        /// either box is empty / contains a non-positive number.
        /// </summary>
        private void UpdateFileNameFromSeasonEpisode()
        {
            var m = EpisodePattern.Match(txtFileName.Text);
            if (!m.Success) return;

            if (!int.TryParse(txtSeason.Text, out int season) || season < 1) return;
            if (!int.TryParse(txtEpisode.Text, out int episode) || episode < 1) return;

            string showName = m.Groups[1].Value;
            string ext = m.Groups[4].Value;
            string seasonStr = season.ToString().PadLeft(m.Groups[2].Length, '0');
            string episodeStr = episode.ToString().PadLeft(m.Groups[3].Length, '0');

            txtFileName.Text = $"{showName} - s{seasonStr}e{episodeStr}{ext}";
        }

        private void btnTvShow_Click(object sender, EventArgs e)
        {
            cboFolder.SelectedIndex = 2;

            string baseTvFolder = cboFolder.Items[2]?.ToString() ?? string.Empty;

            // Try to extract the show name from the output filename in the original command
            string originalCommand = txtOriginalCommand.Text.Trim();
            if (!string.IsNullOrEmpty(originalCommand))
            {
                var lastArg = Regex.Match(originalCommand, @"(""[^""]*""|[^\s]+)\s*$");
                if (lastArg.Success)
                {
                    string outputFile = Path.GetFileNameWithoutExtension(lastArg.Value.Trim().Trim('"'));
                    // Find the earliest delimiter: '-' or '['
                    int dashIndex = outputFile.IndexOf('-');
                    int bracketIndex = outputFile.IndexOf('[');

                    int delimIndex = (dashIndex, bracketIndex) switch
                    {
                        ( >= 0, >= 0) => Math.Min(dashIndex, bracketIndex),
                        ( >= 0, _) => dashIndex,
                        (_, >= 0) => bracketIndex,
                        _ => -1   // no delimiter — use full name
                    };

                    string showName = delimIndex > 0
                        ? outputFile[..delimIndex].Trim()
                        : outputFile.Trim();

                    if (!string.IsNullOrEmpty(showName))
                    {
                        string showFolder = Path.Combine(baseTvFolder, showName);
                        if (!cboFolder.Items.Contains(showFolder))
                            cboFolder.Items.Add(showFolder);
                        cboFolder.SelectedItem = showFolder;
                        SuggestNextEpisode(showFolder);
                    }
                }
            }

            lblSeason.Visible = true;
            txtSeason.Visible = true;
            lblEpisode.Visible = true;
            txtEpisode.Visible = true;

            btnRun.Focus();
        }
    }
}
