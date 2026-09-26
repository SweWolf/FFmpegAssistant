using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FFmpegAssistant
{
    public partial class Form1 : Form
    {
        internal const string AppTitle = "FFmpeg Assistant"; // caption for every message box

        private TimeSpan _totalDuration = TimeSpan.Zero;
        private string? _lastOutputPath;
        private string? _lastLogFile;
        private CancellationTokenSource? _cts;
        private bool _progressStarted;
        private bool _isValidating;
        private int _totalM3u8Segments;
        private int _m3u8SegmentsOpened;
        private M3u8ContentType _m3u8ContentType;
        private bool _updatingSeasonEpisode;
        private bool _settingCategoryFromAutoDetect;
        private bool _closeAfterCancel;
        private bool _commandSetByExtractFeature;
        private bool _settingExtractCommand;
        private string? _folderTextOnFocus;
        private bool _downloadRunning; // from the Download click until the run has ended, including the M3U8 pre-fetch

        // Folders the app itself suggested: created without asking if they don't exist yet
        private readonly HashSet<string> _suggestedFolders = new(StringComparer.OrdinalIgnoreCase);

        // -------------------------------------------------------------------------
        // Estimated remaining time — speed sampling
        // -------------------------------------------------------------------------

        private enum M3u8ContentType { Unknown, Subtitle, Video }

        /// <summary>
        /// Severity of a status message, used to color-code <see cref="txtStatus"/>
        /// when <see cref="AppSettings.ColorCodedStatusMessages"/> is enabled.
        /// </summary>
        private enum StatusLevel { Info, Success, Warning, Error, InProgress }

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

        /// <summary>
        /// The progress bar and estimated-remaining-time label cover the download and the
        /// post-download validation decode as one continuous 0-100% pass, not two separate
        /// ones — otherwise the countdown would hit zero and then restart once validation
        /// begins. Download fills 0-<see cref="DownloadPhaseWeightPercent"/>%; validation
        /// fills the rest. Before validation actually starts (and reports its own live
        /// progress), its remaining time is only a rough guess: total video duration divided
        /// by <see cref="ValidationSpeedEstimateDivisor"/>.
        /// </summary>
        private const int DownloadPhaseWeightPercent = 95;
        private const double ValidationSpeedEstimateDivisor = 30.0;

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

        private readonly string? _startupCommand;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        private const int EM_SETMARGINS = 0xD3;
        private const int EC_LEFTMARGIN = 0x1;

        public Form1(string? startupCommand = null)
        {
            _startupCommand = startupCommand;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Icon is set by the Designer via InitializeComponent (Form1.resx).
            // No override needed here — overriding can lose alpha channel transparency.

            // The form starts maximized. Make the size it gets when restored fit the screen's working area
            // (with display scaling the design size can be taller than the screen), centered.
            // While maximized, Size returns the maximized size, so start from RestoreBounds.
            var workArea = Screen.FromControl(this).WorkingArea;
            var restoreSize = WindowState == FormWindowState.Maximized ? RestoreBounds.Size : Size;
            int restoreWidth = Math.Min(restoreSize.Width, workArea.Width);
            int restoreHeight = Math.Min(restoreSize.Height, workArea.Height);
            Bounds = new Rectangle(workArea.X + (workArea.Width - restoreWidth) / 2,
                                   workArea.Y + (workArea.Height - restoreHeight) / 2,
                                   restoreWidth, restoreHeight);

            // Check for updates in the background — does not block startup
            if (AppSettings.CheckForUpdatesOnStartup == "Yes")
                _ = CheckForUpdatesAsync();

            SendMessage(txtAttempt.Handle, EM_SETMARGINS, EC_LEFTMARGIN, 5);

            InitializeProgressGrid();

#if DEBUG
            AddDebugMenu();
#endif

            // Clear the "download finished" taskbar flash as soon as the user is back in the app
            Activated += (s, _) => TaskbarFlash.Stop(this);

            string videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            AddSuggestedFolder(videos);
            AddSuggestedFolder(Path.Combine(videos, "Movies"));
            AddSuggestedFolder(Path.Combine(videos, "TV Shows"));
            cboFolder.SelectedIndex = 0;

            cboFolder.SelectedIndexChanged += (s, _) => SuggestNextEpisode(cboFolder.Text);
            cboFolder.Enter += (s, _) => _folderTextOnFocus = cboFolder.Text;
            cboFolder.Leave += (s, _) =>
            {
                if (!string.Equals(cboFolder.Text, _folderTextOnFocus, StringComparison.Ordinal))
                    SuggestNextEpisode(cboFolder.Text);
            };

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

            // Auto-apply TV show history as soon as a complete-looking FFmpeg command is pasted.
            // Also auto-wrap bare URLs and local M3U8 file paths into a full ffmpeg command.
            txtOriginalCommand.TextChanged += (s, _) =>
            {
                string cmd = txtOriginalCommand.Text.Trim();
                if (IsBarUrl(cmd) || IsLocalM3u8Path(cmd))
                {
                    string stripped = cmd.Trim('"');
                    txtOriginalCommand.Text = $"ffmpeg -i \"{stripped}\"";
                    txtFileName.Text = "";
                    return;
                }
                if (cmd.StartsWith("ffmpeg ", StringComparison.OrdinalIgnoreCase) && cmd.Length > 30)
                {
                    if (string.IsNullOrEmpty(txtFileName.Text.Trim()))
                    {
                        string? outFile = GetCommandOutputFilename(cmd);
                        if (outFile != null)
                            txtFileName.Text = outFile;
                    }
                    TryApplyTvShowHistory(cmd);
                }
            };
            // Also trigger when the box loses focus (catches manual edits)
            txtOriginalCommand.Leave += (s, _) => TryApplyTvShowHistory(txtOriginalCommand.Text.Trim());

            // Clear status when the user starts editing the input fields.
            // Also clear the extract-feature flag when the user replaces the command themselves.
            txtOriginalCommand.TextChanged += (s, _) =>
            {
                ClearStatus();
                if (!_settingExtractCommand)
                    _commandSetByExtractFeature = false;
            };
            txtFileName.TextChanged += (s, _) =>
            {
                ClearStatus();
            };

            // Command-line argument takes priority; fall back to clipboard when nothing was passed.
            string? startup = _startupCommand;
            if (startup == null && Clipboard.ContainsText())
            {
                string clip = Clipboard.GetText().Trim();
                if (clip.StartsWith("ffmpeg ", StringComparison.OrdinalIgnoreCase))
                    startup = clip;
            }

            if (startup != null)
            {
                if (IsBarUrl(startup) || IsLocalM3u8Path(startup))
                {
                    string stripped = startup.Trim('"');
                    txtOriginalCommand.Text = $"ffmpeg -i \"{stripped}\"";
                    txtFileName.Text = "";
                }
                else if (startup.StartsWith("ffmpeg ", StringComparison.OrdinalIgnoreCase))
                {
                    txtOriginalCommand.Text = startup;
                    if (_startupCommand == null) // came from clipboard — place cursor at start, don't select all
                        BeginInvoke(() => { txtOriginalCommand.SelectionStart = 0; txtOriginalCommand.SelectionLength = 0; });
                    // Defer until after the form is fully shown so the ComboBox updates correctly
                    BeginInvoke(() => TryApplyTvShowHistory(startup));
                }
            }
        }

        // -------------------------------------------------------------------------
        // Progress grid
        // -------------------------------------------------------------------------

        private static readonly string[] GridLabels =
            { "Duration", "Frame", "FPS", "Size", "Time", "Bitrate", "Speed", "Elapsed" };

        // The grid shows one Property/Value pair with 8 rows, or two pairs side by side with 4 rows
        // each when the window is too short for 8 rows (e.g. with 125 % display scaling).
        private readonly Dictionary<string, DataGridViewCell> _gridValueCells = new();
        private bool _gridSideBySide;
        private int _gridSingleWidth;   // grid width with one pair, after DPI/font scaling
        private int _gridSingleHeight;  // grid height with 8 rows
        private int _gridSideBySideHeight; // grid height with 4 rows
        private int _gridGap;           // spacing above and below the grid, after DPI/font scaling
        private bool _updatingLayout;

        private void InitializeProgressGrid()
        {
            dgvProgress.Font = new Font("Segoe UI", 10F);
            dgvProgress.AllowUserToAddRows = false;

            dgvProgress.DefaultCellStyle.SelectionBackColor = dgvProgress.DefaultCellStyle.BackColor;
            dgvProgress.DefaultCellStyle.SelectionForeColor = dgvProgress.DefaultCellStyle.ForeColor;

            dgvProgress.EnableHeadersVisualStyles = false;
            dgvProgress.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgvProgress.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(70, 70, 70);
            dgvProgress.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProgress.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvProgress.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 70, 70);
            dgvProgress.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

            // The default header divider blends into the dark header background — repaint it
            // brighter so the column split is actually visible, without touching the darker
            // grid lines used elsewhere in the (light-background) body of the grid.
            dgvProgress.CellPainting += (s, e) =>
            {
                if (e.RowIndex != -1 || e.ColumnIndex == dgvProgress.Columns.Count - 1 || e.Graphics == null) return;
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                using var pen = new Pen(Color.FromArgb(150, 150, 150));
                e.Graphics.DrawLine(pen, e.CellBounds.Right - 1, e.CellBounds.Top + 2, e.CellBounds.Right - 1, e.CellBounds.Bottom - 3);
                e.Handled = true;
            };

            _gridSingleWidth = dgvProgress.Width;
            _gridGap = dgvProgress.Top - btnRun.Bottom; // as placed in the Designer
            ApplyGridLayout(sideBySide: true);
            _gridSideBySideHeight = dgvProgress.Height;
            ApplyGridLayout(sideBySide: false);
            _gridSingleHeight = dgvProgress.Height;

            pnlContent.Resize += (s, _) => UpdateLayoutForHeight();
            UpdateLayoutForHeight();

            // Minimum height: everything visible with the grid side by side, but never taller than
            // the screen's working area (on a small screen the scrollbar takes over below that).
            // The positions are already DPI/font scaled, so this follows the display scaling.
            int compactHeight = MeasureFixedHeight() + _gridSideBySideHeight
                              + menuStrip.Height + (Height - ClientSize.Height);
            MinimumSize = new Size(MinimumSize.Width,
                                   Math.Min(compactHeight, Screen.FromControl(this).WorkingArea.Height));
        }

        /// <summary>
        /// Height the content needs without the grid: the top part (down to the Download button, anchored
        /// to the top), the spacing above and below the grid, and the bottom part (Status and below,
        /// anchored to the bottom). Measured without the scroll offset.
        /// </summary>
        private int MeasureFixedHeight()
        {
            int scrollY = pnlContent.DisplayRectangle.Y;
            int topPartHeight = btnRun.Bottom - scrollY;
            int bottomPartHeight = pnlContent.DisplayRectangle.Height - (txtStatus.Top - scrollY);
            return topPartHeight + 2 * _gridGap + bottomPartHeight;
        }

        /// <summary>
        /// Rebuilds the grid with one Property/Value pair (8 rows) or two pairs side by side (4 rows),
        /// keeping the values shown, and sizes the grid to exactly fit its rows.
        /// </summary>
        private void ApplyGridLayout(bool sideBySide)
        {
            var values = _gridValueCells.ToDictionary(kv => kv.Key, kv => kv.Value.Value?.ToString() ?? "");
            _gridSideBySide = sideBySide;

            dgvProgress.Rows.Clear();
            dgvProgress.Columns.Clear();
            int pairs = sideBySide ? 2 : 1;
            for (int pair = 0; pair < pairs; pair++)
            {
                dgvProgress.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "Property",
                    Width = 100,
                    ReadOnly = true,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
                dgvProgress.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "Value",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    ReadOnly = true,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    DividerWidth = pair < pairs - 1 ? 2 : 0 // separates the two pairs
                });
            }

            _gridValueCells.Clear();
            int rowCount = GridLabels.Length / pairs;
            for (int r = 0; r < rowCount; r++)
            {
                var row = dgvProgress.Rows[dgvProgress.Rows.Add()];
                for (int pair = 0; pair < pairs; pair++)
                {
                    string label = GridLabels[pair * rowCount + r];
                    row.Cells[pair * 2].Value = label;
                    row.Cells[pair * 2 + 1].Value = values.GetValueOrDefault(label, "");
                    _gridValueCells[label] = row.Cells[pair * 2 + 1];
                }
            }

            // Shrink the grid to exactly fit its rows — no grey empty space below
            dgvProgress.Width = _gridSingleWidth * pairs;
            dgvProgress.Height = dgvProgress.ColumnHeadersHeight
                               + dgvProgress.Rows.Cast<DataGridViewRow>().Sum(r => r.Height)
                               + 2; // border
        }

        /// <summary>
        /// Places the grid just above Status and keeps the top part (down to the Download button) and the
        /// bottom part (grid, Status and below) from overlapping when the window is short: first puts the
        /// grid's rows side by side, then, if that is not enough, makes the content scrollable.
        /// </summary>
        private void UpdateLayoutForHeight()
        {
            if (_updatingLayout || _gridSingleHeight == 0) return;
            _updatingLayout = true;
            try
            {
                int fixedHeight = MeasureFixedHeight();
                int available = pnlContent.ClientSize.Height;

                bool singleFits = fixedHeight + _gridSingleHeight <= available;
                bool roomForTwo = pnlContent.ClientSize.Width >= 2 * dgvProgress.Left + 2 * _gridSingleWidth;
                bool sideBySide = !singleFits && roomForTwo;
                if (sideBySide != _gridSideBySide)
                    ApplyGridLayout(sideBySide);

                int needed = fixedHeight + dgvProgress.Height;
                pnlContent.AutoScrollMinSize = needed > available ? new Size(0, needed) : Size.Empty;

                // Keep the grid just above Status: it shows FFmpeg's output, so it belongs with the
                // status part. Any extra height goes between the Download button and the grid.
                dgvProgress.Top = txtStatus.Top - _gridGap - dgvProgress.Height;
            }
            finally
            {
                // A resized ComboBox selects its text (e.g. when the scrollbar appears): undo that
                if (!cboFolder.Focused)
                    cboFolder.SelectionLength = 0;
                _updatingLayout = false;
            }
        }

        private void UpdateGridRow(string label, string value)
        {
            if (_gridValueCells.TryGetValue(label, out var cell))
                cell.Value = value;
        }

        private void ResetProgress()
        {
            _totalDuration = TimeSpan.Zero;
            _progressStarted = false;
            _isValidating = false;
            _m3u8SegmentsOpened = 0;
            _speedSamples.Clear();
            foreach (var cell in _gridValueCells.Values)
                cell.Value = "";
            progressBar.Value = 0;
            lblEstimatedRemaining.Text = "Estimated remaining time: —";
            ClearStatus();
            TaskbarProgress.Clear(this);
        }

        private void SetStatus(string message, StatusLevel level = StatusLevel.Info)
        {
            // Dark text on a light background color: colored text is hard to read for yellow and orange.
            // Normal messages keep the grey (non-editable) background of the idle Status box.
            Color back = AppSettings.ColorCodedStatusMessages
                ? level switch
                {
                    StatusLevel.InProgress => Color.FromArgb(255, 242, 168), // light yellow
                    StatusLevel.Warning => Color.FromArgb(255, 216, 168),    // light orange
                    StatusLevel.Error => Color.FromArgb(255, 199, 199),      // light red
                    StatusLevel.Success => Color.FromArgb(200, 240, 200),    // light green
                    _ => SystemColors.Control,
                }
                : SystemColors.Control;
            // Black on the fixed light colors (also in a dark/high-contrast theme); the theme's text color on grey
            Color fore = back == SystemColors.Control ? SystemColors.ControlText : Color.Black;

            void Apply()
            {
                txtStatus.BackColor = back;
                txtStatus.ForeColor = fore;
                txtStatus.Text = message;
            }
            if (InvokeRequired) Invoke(Apply); else Apply();
        }

        /// <summary>Empties the Status box and gives it back its idle (grey) look.</summary>
        private void ClearStatus() => SetStatus(string.Empty);

#if DEBUG
        // -------------------------------------------------------------------------
        // Debug menu: only in Debug builds (Release builds don't contain this code)
        // -------------------------------------------------------------------------

        private CancellationTokenSource? _statusColorDemoCts;

        private void AddDebugMenu()
        {
            var menuDebug = new ToolStripMenuItem("Debug");
            menuDebug.DropDownItems.Add("Show Status Colors", null, (s, e) => _ = ShowStatusColorDemoAsync());
            menuStrip.Items.Insert(menuStrip.Items.IndexOf(menuHelp), menuDebug);
        }

        /// <summary>
        /// Shows each status level for 3 seconds through <see cref="SetStatus"/> (so exactly as a real
        /// download would), then clears the Status box. Starting it again restarts it. Not during a download.
        /// </summary>
        private async Task ShowStatusColorDemoAsync()
        {
            if (_downloadRunning) return;
            _statusColorDemoCts?.Cancel();
            using var cts = _statusColorDemoCts = new CancellationTokenSource();
            (string Text, StatusLevel Level)[] steps =
            {
                ("Connecting... (normal: grey)", StatusLevel.Info),
                ("Validating downloaded file... (in progress: light yellow)", StatusLevel.InProgress),
                ("Download failed — retrying (attempt 2 of 5)... (warning: light orange)", StatusLevel.Warning),
                ("Download failed — an error occurred. (error: light red)", StatusLevel.Error),
                ("Done (success: light green)", StatusLevel.Success),
            };
            try
            {
                foreach (var (text, level) in steps)
                {
                    SetStatus(text, level);
                    await Task.Delay(3000, cts.Token);
                }
            }
            catch (OperationCanceledException) { return; } // restarted, or a download took over the Status box
            finally
            {
                if (_statusColorDemoCts == cts) _statusColorDemoCts = null;
            }
            ClearStatus();
        }
#endif

        /// <summary>
        /// Reacts to a finished download per the "Action When Download Finished" setting:
        /// play a sound, show a "download is complete" message box, or do nothing (status already shows "Done").
        /// Skipped entirely if the user already asked to close the application, or in watch mode while
        /// "Enable Watching While Downloading" is still checked: the user is watching the video, so only
        /// the status shows "Done" (errors are still reported). Unchecking it during the download brings
        /// the usual notification back; checking it during a normal download doesn't silence that one.
        /// </summary>
        private void NotifyDownloadFinished(bool watchMode)
        {
            if (_closeAfterCancel || (watchMode && chkEnableWatchingWhileDownloading.Checked)) return;

            TaskbarFlash.FlashIfInactive(this);

            switch (AppSettings.ActionWhenDownloadFinished)
            {
                case "Message Box":
                    MessageBox.Show("The download is complete.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case "None":
                    break;
                default: // "Play a Sound"
                    SoundLibrary.Play(AppSettings.FinishedDownloadSoundFile);
                    break;
            }
        }

        // -------------------------------------------------------------------------
        // FFmpeg output parsing
        // -------------------------------------------------------------------------

        private void ProcessOutputLine(string line)
        {
            // M3U8 segment-based progress: count real segment openings vs total from pre-fetched playlist.
            // Segment type is determined by M3U8 content type detected at pre-fetch time. Only used
            // while the total duration is still unknown — once FFmpeg reports a Duration, the normal
            // time-based progress below is reliable and takes over exclusively; otherwise the two would
            // keep overwriting each other's Time/progress-bar updates as their lines interleave.
            if (_totalM3u8Segments > 0 &&
                _totalDuration == TimeSpan.Zero &&
                line.Contains("Opening '", StringComparison.OrdinalIgnoreCase) &&
                IsRealM3u8Segment(line))
            {
                _m3u8SegmentsOpened++;
                int pct = Math.Min(_m3u8SegmentsOpened * 100 / _totalM3u8Segments, 99);
                if (!_progressStarted)
                {
                    _progressStarted = true;
                    SetStatus(_m3u8ContentType == M3u8ContentType.Subtitle
                        ? "Downloading subtitles..."
                        : "Downloading segments...");
                }
                Invoke(() =>
                {
                    progressBar.Value = pct;
                    UpdateGridRow("Time", $"{_m3u8SegmentsOpened}/{_totalM3u8Segments}");
                    TaskbarProgress.SetNormal(this, pct, 100);
                });
                return;
            }

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
                int phasePercent = Math.Min((int)(current.TotalSeconds / _totalDuration.TotalSeconds * 100), 100);

                double effectiveSpeed = SpeedMode == EstimationMode.Stable
                    ? (_speedSamples.Count > 0 ? _speedSamples.Min() : currentSpeed)
                    : currentSpeed;

                double? phaseRemainingSecs = effectiveSpeed > 0 && current.TotalSeconds > 0
                    ? (_totalDuration.TotalSeconds - current.TotalSeconds) / effectiveSpeed
                    : null;

                if (_isValidating)
                {
                    // Last slice of the bar; validation's own live progress drives the estimate directly.
                    percent = DownloadPhaseWeightPercent + phasePercent * (100 - DownloadPhaseWeightPercent) / 100;
                    if (phaseRemainingSecs.HasValue)
                        estimatedRemaining = TimeSpan.FromSeconds(phaseRemainingSecs.Value).ToString(@"h\:mm\:ss");
                }
                else
                {
                    // First slice of the bar; pad the estimate with a rough guess for the
                    // validation pass still to come (see DownloadPhaseWeightPercent).
                    percent = phasePercent * DownloadPhaseWeightPercent / 100;
                    if (phaseRemainingSecs.HasValue)
                    {
                        double estimatedValidationSecs = _totalDuration.TotalSeconds / ValidationSpeedEstimateDivisor;
                        estimatedRemaining = TimeSpan.FromSeconds(phaseRemainingSecs.Value + estimatedValidationSecs).ToString(@"h\:mm\:ss");
                    }
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
                TaskbarProgress.SetNormal(this, percent, 100);

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
            // Disable Download and Clear right away: the M3U8 pre-fetch at the start of the run can take
            // several seconds, and a second click meanwhile would start a second, parallel run on the
            // same files (e.g. "the process cannot access the file" for the FFmpeg log).
            if (_downloadRunning) return;
            _downloadRunning = true;
#if DEBUG
            _statusColorDemoCts?.Cancel();
#endif
            btnRun.Enabled = false;
            btnClear.Enabled = false;
            try
            {
                await RunDownloadAsync();
            }
            finally
            {
                _downloadRunning = false;
                if (!IsDisposed) // the window may have been closed at the end of a cancelled run
                {
                    btnRun.Enabled = true;
                    btnClear.Enabled = true;
                }
            }
        }

        private async Task RunDownloadAsync()
        {
            ClearStatus();

            string originalCommand = txtOriginalCommand.Text.Trim();
            string folder = cboFolder.Text.Trim();
            string fileName = txtFileName.Text.Trim();

            if (string.IsNullOrEmpty(originalCommand) || string.IsNullOrEmpty(folder))
            {
                MessageBox.Show("Please enter the command and the folder.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Pre-fetch the M3U8 playlist (if applicable) to detect content type and segment count.
            // This runs before filename determination so we can suggest the right default extension.
            var m3u8Info = await TryGetM3u8InfoAsync(originalCommand);
            _m3u8ContentType = m3u8Info.Type;
            _totalM3u8Segments = m3u8Info.SegmentCount;
            _m3u8SegmentsOpened = 0;

            // The file name may already be non-empty here — e.g. auto-suggested from the Title/TV
            // Show workflow before we had any way to know this was actually a subtitle stream. Now
            // that the playlist confirms it, force the extension to .srt regardless of what was
            // guessed before (a folder/episode-continuation guess has no idea this is a subtitle).
            if (_m3u8ContentType == M3u8ContentType.Subtitle && !string.IsNullOrEmpty(fileName) &&
                !fileName.EndsWith(".srt", StringComparison.OrdinalIgnoreCase))
            {
                fileName = Path.ChangeExtension(fileName, ".srt");
                txtFileName.Text = fileName;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                var lastArg = Regex.Match(originalCommand, @"(""[^""]*""|[^\s]+)\s*$");
                if (lastArg.Success)
                {
                    string rawArg = lastArg.Value.Trim().Trim('"');
                    if (rawArg.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        rawArg.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                        IsLocalM3u8Path(rawArg))
                    {
                        // No explicit output filename — use M3U8 content type to pick a sensible default
                        fileName = _m3u8ContentType == M3u8ContentType.Subtitle ? "subtitles.srt" : "output.mp4";
                    }
                    else
                        fileName = Path.GetFileName(rawArg);
                    txtFileName.Text = fileName;
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    MessageBox.Show("Could not determine a file name from the original command.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // If the filename doesn't already end with the correct extension, append it.
            // We append rather than replace so that "Episode.TheCoolName" becomes
            // "Episode.TheCoolName.mp4" rather than losing the user's intended text.
            string correctExt = GetCommandOutputExtension(originalCommand);
            if (!string.IsNullOrEmpty(correctExt) &&
                !fileName.EndsWith(correctExt, StringComparison.OrdinalIgnoreCase) &&
                !fileName.EndsWith(".srt", StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName + correctExt;
                txtFileName.Text = fileName;
            }

            // Illegal characters would make CreateDirectory throw, or FFmpeg fail with a cryptic exit code
            if (GetInvalidPathCharError(folder) is string folderError)
            {
                MessageBox.Show(folderError, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboFolder.Focus();
                return;
            }
            if (GetInvalidPathCharError(fileName) is string nameError)
            {
                MessageBox.Show(nameError, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFileName.Focus();
                return;
            }

            // FFmpeg can't create folders: create a missing one here (asking first, unless the app suggested it)
            if (!EnsureFolderExists(folder, askFirst: !_suggestedFolders.Contains(Path.TrimEndingDirectorySeparator(folder))))
            {
                cboFolder.Focus();
                return;
            }

            // Save TV show history so the folder is auto-suggested next time
            const string tvShowsMarker = @"\TV Shows\";
            int tvIdx = folder.IndexOf(tvShowsMarker, StringComparison.OrdinalIgnoreCase);
            if (tvIdx >= 0)
            {
                string afterMarker = folder[(tvIdx + tvShowsMarker.Length)..];
                string subfolder = afterMarker.Split('\\', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
                string? showName = ExtractShowName(originalCommand);
                if (!string.IsNullOrEmpty(subfolder) && showName != null)
                    TvShowHistory.SaveOrUpdate(showName, subfolder);
            }

            string outputPath = Path.Combine(folder, fileName);

            bool isSrt = fileName.EndsWith(".srt", StringComparison.OrdinalIgnoreCase);

            // In watch-while-downloading mode, download to a .ts file first.
            // In normal mode, download to a "(part)" file to protect against power outages —
            // the file is renamed to the final name only after successful validation.
            // SRT files use the same (part) protection but skip watch mode and video validation.
            bool watchMode = chkEnableWatchingWhileDownloading.Checked && !isSrt;
            string partPath = Path.Combine(folder,
                Path.GetFileNameWithoutExtension(fileName) + " (part)" + Path.GetExtension(fileName));
            string downloadPath = watchMode
                ? Path.ChangeExtension(outputPath, ".ts")
                : partPath;

            string command = ReplaceOutputFile(originalCommand, downloadPath);

            // When the input is a local M3U8 file, FFmpeg restricts allowed protocols to
            // file,crypto,data — blocking https:// segment URLs inside the playlist.
            // Inject the protocol whitelist before the -i flag so all segments can be fetched.
            var localM3u8InputMatch = Regex.Match(command,
                @"-i\s+(?:""[^""]*\.m3u[8]?""|[^\s]*\.m3u[8]?(?=\s|$))",
                RegexOptions.IgnoreCase);
            if (localM3u8InputMatch.Success)
            {
                string whitelist = "-protocol_whitelist file,crypto,data,http,https,tcp,tls -allowed_extensions ALL ";
                command = command[..localM3u8InputMatch.Index] + whitelist + command[localM3u8InputMatch.Index..];
            }

            // Overwrite protection — always check the final output file
            if (File.Exists(outputPath))
            {
                var answer = MessageBox.Show(
                    $"The output file already exists:\n{outputPath}\n\nDo you want to overwrite it?",
                    AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

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

            int maxAttempts = AppSettings.NumberOfDownloadAttempts;
            int attempt = 0;
            txtAttempt.Text = "";

            // Keep the PC from going to sleep until all attempts, the validation and the
            // watch-mode conversion are done (released when this method exits, by any path)
            using var sleepBlocker = SleepBlocker.Begin();

            bool keepTrying = true;
            while (keepTrying)
            {
                keepTrying = false;
                attempt++;
                txtAttempt.Text = attempt.ToString();

                // On retry, force -y so FFmpeg overwrites any leftover partial file from the previous attempt
                string currentArguments = attempt > 1 && !arguments.StartsWith("-y ", StringComparison.OrdinalIgnoreCase)
                    ? "-y " + arguments
                    : arguments;

                ResetProgress();
                _cts = new CancellationTokenSource();
                _lastLogFile = logFile;
                btnRun.Enabled = false;
                btnCancel.Enabled = true;
                btnClear.Enabled = false;
                btnOpenFile.Enabled = false;
                btnOpenLogFile.Enabled = false;

                try
                {
                    btnOpenLogFile.Enabled = true;
                    SetStatus("Connecting...");
                    WriteAppLog($"START    : {fileName}");
                    WriteAppLog($"COMMAND  : ffmpeg {currentArguments}");
                    WriteAppLog($"OUTPUT   : {outputPath}");

                    // Set download path early so Open File can activate as soon as the file appears
                    _lastOutputPath = downloadPath;

                    var (exitCode, errorLines) = await RunFfmpegAsync(currentArguments, logFile, _cts.Token);

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
                        TaskbarProgress.SetError(this, 100, 100);

                        if (maxAttempts > 1 && attempt < maxAttempts && !_closeAfterCancel)
                        {
                            try { if (File.Exists(downloadPath)) File.Delete(downloadPath); } catch { }
                            WriteAppLog($"RETRY    : Auto-retry {attempt + 1} of {maxAttempts} after exit code {exitCode}");
                            SetStatus($"Download failed — retrying (attempt {attempt + 1} of {maxAttempts})...", StatusLevel.Warning);
                            keepTrying = true;
                        }
                        else
                        {
                            SetStatus("Download failed — an error occurred.", StatusLevel.Error);
                            TaskbarFlash.FlashIfInactive(this);
                            MessageBox.Show(message, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            LogError(fileName, message, logFile);
                        }
                    }
                    else
                    {
                        WriteAppLog($"RESULT   : SUCCESS (exit code 0)");

                        // If watch mode: convert .ts → final format before validating
                        if (watchMode)
                        {
                            string finalExt = Path.GetExtension(outputPath).TrimStart('.');
                            SetStatus($"Converting to {finalExt}...");
                            TaskbarProgress.SetIndeterminate(this);
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
                                SetStatus("Conversion failed — .ts file kept.", StatusLevel.Error);
                                TaskbarFlash.FlashIfInactive(this);
                                return;
                            }
                        }

                        // SRT subtitle files: rename (part) file to final name, skip video validation
                        if (isSrt)
                        {
                            if (new FileInfo(partPath).Length == 0)
                            {
                                WriteAppLog($"RESULT   : FAILED — output file is empty (0 bytes)");
                                progressBar.Value = 0;
                                lblEstimatedRemaining.Text = "Estimated remaining time: —";
                                LogError(fileName, "Output file is empty — download may have failed", logFile);
                                TaskbarFlash.FlashIfInactive(this);
                                MessageBox.Show(
                                    $"The output file is empty (0 bytes):\n\n{partPath}\n\n" +
                                    "The download likely failed — e.g. blocked segments or an invalid source.\n" +
                                    "Check the log file for details.",
                                    AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                continue;
                            }

                            SetStatus("Finalizing...");
                            if (File.Exists(outputPath)) File.Delete(outputPath);
                            File.Move(partPath, outputPath);
                            _lastOutputPath = outputPath;
                            btnOpenFile.Enabled = true;
                            WriteAppLog($"FINALIZE : Renamed (part) file to final name");
                            WriteAppLog($"RESULT   : SUCCESS — SRT subtitle file");
                            progressBar.Value = 100;
                            lblEstimatedRemaining.Text = "Estimated remaining time: 0:00:00";
                            TaskbarProgress.Clear(this);
                            SetStatus("Done", StatusLevel.Success);
                            NotifyDownloadFinished(watchMode);
                            continue;
                        }

                        SetStatus("Validating downloaded file...", StatusLevel.InProgress);

                        // Validation continues the same progress bar/estimate rather than starting
                        // a second 0-100% pass (see DownloadPhaseWeightPercent). Its decode speed
                        // differs a lot from the download/copy that just finished, so the speed
                        // samples are cleared to avoid skewing the validation-phase estimate.
                        _isValidating = true;
                        _speedSamples.Clear();
                        progressBar.Value = DownloadPhaseWeightPercent;
                        TaskbarProgress.SetNormal(this, DownloadPhaseWeightPercent, 100);

                        // In normal mode validate the part file; in watch mode validate the final file
                        string validatePath = watchMode ? outputPath : partPath;
                        bool valid = await ValidateVideoFileAsync(validatePath, logFile, _cts.Token);
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
                            TaskbarProgress.Clear(this);
                            SetStatus("Done", StatusLevel.Success);
                            NotifyDownloadFinished(watchMode);
                        }
                        else
                        {
                            WriteAppLog($"VALIDATE : FAILED — file is corrupted");
                            progressBar.Value = 0;
                            lblEstimatedRemaining.Text = "Estimated remaining time: —";

                            if (maxAttempts > 1 && attempt < maxAttempts && !_closeAfterCancel)
                            {
                                try { if (File.Exists(validatePath)) File.Delete(validatePath); } catch { }
                                WriteAppLog($"RETRY    : Auto-retry {attempt + 1} of {maxAttempts} — corrupted file");
                                SetStatus($"File corrupted — retrying (attempt {attempt + 1} of {maxAttempts})...", StatusLevel.Warning);
                                keepTrying = true;
                            }
                            else
                            {
                                LogError(fileName, "File validation failed — corrupted download", logFile);

                                TaskbarFlash.FlashIfInactive(this);
                                var deleteAnswer = MessageBox.Show(
                                    $"The downloaded file appears to be corrupted:\n\n{validatePath}\n\nDo you want to delete the file?",
                                    AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

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
                                        AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                    if (retryAnswer == DialogResult.Yes)
                                    {
                                        WriteAppLog($"RETRY    : User requested retry");
                                        keepTrying = true;
                                    }
                                    else
                                    {
                                        SetStatus("Downloaded file corrupted — file deleted.", StatusLevel.Error);
                                    }
                                }
                                else
                                {
                                    SetStatus("Downloaded file corrupted.", StatusLevel.Error);
                                }
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    progressBar.Value = 0;
                    lblEstimatedRemaining.Text = "Estimated remaining time: —";
                    TaskbarProgress.Clear(this);
                    WriteAppLog($"RESULT   : CANCELLED by user");

                    if (_closeAfterCancel)
                    {
                        // Application is closing — silently delete the partial file, no prompts
                        if (File.Exists(downloadPath))
                        {
                            try
                            {
                                File.Delete(downloadPath);
                                WriteAppLog($"CLEANUP  : Partial file deleted on application close");
                            }
                            catch (Exception ex)
                            {
                                WriteAppLog($"CLEANUP  : Failed to delete partial file on close — {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(downloadPath))
                        {
                            var answer = MessageBox.Show(
                                $"Download was cancelled.\n\nA partial file was saved:\n{downloadPath}\n\nDelete it?",
                                AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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
                }
                catch (System.ComponentModel.Win32Exception win32ex)
                    when (win32ex.NativeErrorCode == 2 || win32ex.NativeErrorCode == 3)
                {
                    // FFmpeg executable not found (NativeErrorCode 2 = file not found, 3 = path not found)
                    progressBar.Value = 0;
                    lblEstimatedRemaining.Text = "Estimated remaining time: —";
                    TaskbarProgress.SetError(this, 100, 100);
                    WriteAppLog($"RESULT   : FFMPEG NOT FOUND — {win32ex.Message}");

                    TaskbarFlash.FlashIfInactive(this);
                    var answer = MessageBox.Show(
                        "FFmpeg was not found on this system.\n\nWould you like to locate ffmpeg.exe?",
                        AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (answer == DialogResult.Yes)
                    {
                        using var ofd = new OpenFileDialog
                        {
                            Title = "Locate ffmpeg.exe",
                            Filter = "ffmpeg.exe|ffmpeg.exe|Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                            FileName = "ffmpeg.exe"
                        };
                        if (ofd.ShowDialog(this) == DialogResult.OK)
                        {
                            AppSettings.SetFfmpegExe(ofd.FileName);
                            WriteAppLog($"CONFIG   : ffmpeg path set to {ofd.FileName}");
                            keepTrying = true;
                            SetStatus("Retrying with located FFmpeg...", StatusLevel.Warning);
                        }
                        else
                        {
                            SetStatus("FFmpeg not found — download cancelled.", StatusLevel.Error);
                            LogError(fileName, "FFmpeg executable not found", logFile);
                        }
                    }
                    else
                    {
                        SetStatus("FFmpeg not found — download cancelled.", StatusLevel.Error);
                        LogError(fileName, "FFmpeg executable not found", logFile);
                    }
                }
                catch (Exception ex)
                {
                    string message = $"Unexpected error:\n{ex.Message}";
                    progressBar.Value = 0;
                    lblEstimatedRemaining.Text = "Estimated remaining time: —";
                    TaskbarProgress.SetError(this, 100, 100);
                    SetStatus($"Error: {ex.Message}", StatusLevel.Error);
                    WriteAppLog($"RESULT   : EXCEPTION — {ex.Message}");
                    TaskbarFlash.FlashIfInactive(this);
                    MessageBox.Show(message, AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        btnClear.Enabled = true;
                    }

                    // If the user closed the window during a download, finish closing now
                    if (_closeAfterCancel && !keepTrying)
                    {
                        _closeAfterCancel = false;
                        Close();
                    }
                }
            }
        }

        /// <summary>
        /// Adds <paramref name="folder"/> to the folder list and remembers it as suggested by the app,
        /// so <see cref="EnsureFolderExists"/> creates it without asking.
        /// </summary>
        private void AddSuggestedFolder(string folder)
        {
            if (!cboFolder.Items.Contains(folder))
                cboFolder.Items.Add(folder);
            _suggestedFolders.Add(Path.TrimEndingDirectorySeparator(folder));
        }

        /// <summary>
        /// Creates <paramref name="folder"/> (including any missing parent folders) if it doesn't exist yet,
        /// asking first when <paramref name="askFirst"/> is true. Returns false if the user declines
        /// or the folder could not be created.
        /// </summary>
        private bool EnsureFolderExists(string folder, bool askFirst)
        {
            if (Directory.Exists(folder)) return true;

            if (askFirst)
            {
                var answer = MessageBox.Show(
                    $"The folder \"{folder}\" does not exist.\n\nDo you want to create it now?",
                    AppTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (answer != DialogResult.OK) return false;
            }

            try
            {
                Directory.CreateDirectory(folder);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not create the folder:\n{folder}\n\n{ex.Message}",
                    AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            btnCancel.Enabled = false;
            btnClear.Enabled = true;
        }

        private void btnOpenFile_Click_1(object sender, EventArgs e)
        {
            if (_lastOutputPath == null || !File.Exists(_lastOutputPath))
            {
                MessageBox.Show("The downloaded file was not found.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Process.Start(new ProcessStartInfo(_lastOutputPath) { UseShellExecute = true });
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtOriginalCommand.Clear();
            cboFolder.SelectedIndex = 0;
            txtFileName.Clear();
            txtTitle.Clear();
            txtYear.Clear();

            rdoMovie.Checked = false;
            rdoTvShow.Checked = false;

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

            txtOriginalCommand.Focus();
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

        private string? _updateReleasePageUrl;

        private async Task CheckForUpdatesAsync()
        {
            var currentVersion = System.Reflection.Assembly
                .GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);

            var result = await GitHubUpdateChecker.CheckAsync(
                "SweWolf", "FFmpegAssistant", currentVersion);

            if (result is { IsUpdateAvailable: true })
            {
                _updateReleasePageUrl = result.ReleasePageUrl;
                menuNewVersion.Visible = true;
            }
        }

        private void menuNewVersion_Click(object sender, EventArgs e)
        {
            if (_updateReleasePageUrl != null)
                Process.Start(new ProcessStartInfo(_updateReleasePageUrl) { UseShellExecute = true });
        }

        private void menuCreateShortcut_Click(object sender, EventArgs e)
        {
            using var form = new CreateShortcutForm(Application.ExecutablePath);
            form.ShowDialog(this);
        }

        private void menuSettings_Click_1(object sender, EventArgs e)
        {
            using var form = new SettingsForm();
            form.ShowDialog(this);
        }

        private async void mnuExtractSubtitleFile_Click(object sender, EventArgs e)
        {
            // Default to the last downloaded video file; fall back to the Windows Videos folder
            string[] videoExts = { ".mkv", ".mp4", ".avi", ".mov", ".ts", ".m2ts", ".wmv" };
            string videosFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            bool lastIsVideo = _lastOutputPath != null &&
                File.Exists(_lastOutputPath) &&
                videoExts.Contains(Path.GetExtension(_lastOutputPath), StringComparer.OrdinalIgnoreCase);
            using var ofd = new OpenFileDialog
            {
                Title = "Select a video file to extract subtitles from",
                Filter = "Video files|*.mkv;*.mp4;*.avi;*.mov;*.ts;*.m2ts;*.wmv|All files (*.*)|*.*",
                InitialDirectory = lastIsVideo ? Path.GetDirectoryName(_lastOutputPath)! : videosFolder,
                FileName = lastIsVideo ? Path.GetFileName(_lastOutputPath) : string.Empty
            };
            if (ofd.ShowDialog(this) != DialogResult.OK) return;
            string videoPath = ofd.FileName;

            // Probe the file for subtitle streams using ffprobe
            string ffprobePath = GetFfprobeExe();
            string probeArgs = $"-v quiet -print_format json -show_streams \"{videoPath}\"";

            var psi = new ProcessStartInfo(ffprobePath, probeArgs)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            List<SubtitleStream> streams;
            try
            {
                using var proc = Process.Start(psi)!;
                string json = await proc.StandardOutput.ReadToEndAsync();
                await proc.WaitForExitAsync();
                streams = ParseSubtitleStreams(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not probe the file:\n\n{ex.Message}\n\nMake sure ffprobe.exe is installed alongside ffmpeg.exe.",
                    AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (streams.Count == 0)
            {
                MessageBox.Show(
                    "No subtitle streams were found in the selected file.",
                    AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Let the user pick which subtitle stream to extract
            using var dlg = new ExtractSubtitleForm(streams);
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            SubtitleStream selected = dlg.SelectedStream!;

            // Build the FFmpeg extraction command and populate the main window
            string suggestedName = Path.GetFileNameWithoutExtension(videoPath) + selected.SuggestedExtension;
            string extractCommand = $"ffmpeg -i \"{videoPath}\" -map 0:{selected.StreamIndex} \"{suggestedName}\"";
            _settingExtractCommand = true;
            txtOriginalCommand.Text = extractCommand;
            _settingExtractCommand = false;
            _commandSetByExtractFeature = true;
            txtFileName.Text = suggestedName;
        }

        private static string GetFfprobeExe()
        {
            string ffmpeg = AppSettings.GetFfmpegExe();
            if (ffmpeg == "ffmpeg") return "ffprobe";
            string dir = Path.GetDirectoryName(ffmpeg) ?? string.Empty;
            string probe = Path.Combine(dir, "ffprobe.exe");
            return File.Exists(probe) ? probe : "ffprobe";
        }

        private static List<SubtitleStream> ParseSubtitleStreams(string json)
        {
            var result = new List<SubtitleStream>();
            if (string.IsNullOrWhiteSpace(json)) return result;

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("streams", out var streamsEl)) return result;

            foreach (var el in streamsEl.EnumerateArray())
            {
                if (!el.TryGetProperty("codec_type", out var typeEl) ||
                    typeEl.GetString() != "subtitle") continue;

                int index = el.TryGetProperty("index", out var idxEl) ? idxEl.GetInt32() : 0;
                string codec = el.TryGetProperty("codec_name", out var codecEl) ? codecEl.GetString() ?? "" : "";
                string lang = "";
                string title = "";
                if (el.TryGetProperty("tags", out var tags))
                {
                    if (tags.TryGetProperty("language", out var langEl)) lang = langEl.GetString() ?? "";
                    if (tags.TryGetProperty("title", out var titleEl)) title = titleEl.GetString() ?? "";
                }
                result.Add(new SubtitleStream(index, codec, lang, title));
            }
            return result;
        }

        private void btnOpenLogFile_Click_1(object sender, EventArgs e)
        {
            if (_lastLogFile == null || !File.Exists(_lastLogFile))
            {
                MessageBox.Show("The log file was not found.", AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Process.Start(new ProcessStartInfo(_lastLogFile) { UseShellExecute = true });
        }

        // -------------------------------------------------------------------------
        // FFmpeg process
        // -------------------------------------------------------------------------

        private static async Task<(int SegmentCount, M3u8ContentType Type)> TryGetM3u8InfoAsync(string command)
        {
            try
            {
                var m = Regex.Match(command, @"-i\s+""([^""]+)""", RegexOptions.IgnoreCase);
                if (!m.Success) return (0, M3u8ContentType.Unknown);

                string input = m.Groups[1].Value;

                // Only attempt for .m3u8/.m3u URLs or local M3U8 files — never read arbitrary files
                bool isUrl = input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                             input.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
                bool isM3u8Ext = input.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase) ||
                                 input.EndsWith(".m3u", StringComparison.OrdinalIgnoreCase);
                bool isFile = isM3u8Ext && File.Exists(input);
                if (!isUrl && !isFile) return (0, M3u8ContentType.Unknown);

                string content;
                if (isFile)
                    content = await File.ReadAllTextAsync(input);
                else
                {
                    using var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                    content = await client.GetStringAsync(input);
                }

                // Master playlist — has variant stream entries; cannot count leaf segments here
                if (content.Contains("#EXT-X-STREAM-INF", StringComparison.OrdinalIgnoreCase))
                    return (0, M3u8ContentType.Video);

                // Count real (non-dummy) #EXTINF segment URLs and classify by extension
                int count = 0;
                bool isSubtitle = false;
                bool isVideo = false;
                bool nextIsUrl = false;
                foreach (string line in content.Split('\n'))
                {
                    string t = line.Trim();
                    if (t.StartsWith("#EXTINF:", StringComparison.OrdinalIgnoreCase))
                    {
                        nextIsUrl = true;
                    }
                    else if (nextIsUrl)
                    {
                        nextIsUrl = false;
                        if (!t.Contains("dummy", StringComparison.OrdinalIgnoreCase))
                        {
                            count++;
                            string tl = t.ToLowerInvariant();
                            if (tl.Contains(".webvtt") || tl.Contains(".vtt"))
                                isSubtitle = true;
                            else if (tl.Contains(".ts") || tl.Contains(".m4s"))
                                isVideo = true;
                        }
                    }
                }

                M3u8ContentType type = isSubtitle ? M3u8ContentType.Subtitle
                                     : isVideo    ? M3u8ContentType.Video
                                                  : M3u8ContentType.Unknown;
                return (count, type);
            }
            catch { return (0, M3u8ContentType.Unknown); }
        }

        private bool IsRealM3u8Segment(string ffmpegLine)
        {
            if (ffmpegLine.Contains("dummy", StringComparison.OrdinalIgnoreCase))
                return false;

            string lower = ffmpegLine.ToLowerInvariant();
            return _m3u8ContentType == M3u8ContentType.Subtitle
                ? lower.Contains(".webvtt'") || lower.Contains(".vtt'")
                : lower.Contains(".ts'") || lower.Contains(".m4s'");
        }

        private static bool IsBarUrl(string s) =>
            (s.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
             s.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) &&
            !s.Contains(' ');

        // Returns true for a bare local file path to an M3U8 playlist (quoted or unquoted)
        private static bool IsLocalM3u8Path(string s)
        {
            string stripped = s.Trim('"').Trim();
            return stripped.Length >= 3
                && char.IsLetter(stripped[0]) && stripped[1] == ':' && stripped[2] == '\\'
                && (stripped.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase) ||
                    stripped.EndsWith(".m3u", StringComparison.OrdinalIgnoreCase));
        }

        private static string GetCommandOutputExtension(string command)
        {
            var match = Regex.Match(command, @"(""[^""]*""|[^\s]+)\s*$");
            if (!match.Success) return string.Empty;
            // If the last arg is the input (no explicit output in the command), return empty
            if (LastArgIsInput(command, match)) return string.Empty;
            return Path.GetExtension(match.Value.Trim().Trim('"')); // e.g. ".mp4"
        }

        /// <summary>
        /// Returns an error message if <paramref name="path"/> contains a character that isn't
        /// allowed in a file or folder name on this OS (on Windows " &lt; &gt; | : * ? and control
        /// characters), otherwise null. FFmpeg only reports a cryptic exit code (-22) for those.
        /// (Same routine in all the SweWolf FFmpeg apps.)
        /// </summary>
        private static string? GetInvalidPathCharError(string path)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            string root = Path.GetPathRoot(path) ?? "";
            foreach (string name in path[root.Length..].Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            {
                int i = name.IndexOfAny(invalid);
                if (i < 0) continue;

                string found = char.IsControl(name[i]) ? "an invisible control character" : $"the character \"{name[i]}\"";
                string shown = string.Join(" ", invalid.Where(c => !char.IsControl(c)
                    && c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar));
                return $"\"{name}\" contains {found}, which is not allowed in file and folder names."
                    + (shown.Length > 0 ? $"\n\nThese characters are not allowed: {shown}" : "");
            }
            return null;
        }

        private static string ReplaceOutputFile(string command, string newOutputPath)
        {
            var match = Regex.Match(command, @"(""[^""]*""|[^\s]+)\s*$");
            if (!match.Success)
                return command;

            string newArg = newOutputPath.Contains(' ') ? $"\"{newOutputPath}\"" : newOutputPath;

            // If the last arg is the input (no explicit output), append rather than replace
            if (LastArgIsInput(command, match))
                return command.TrimEnd() + " " + newArg;

            return command[..match.Index] + newArg;
        }

        // Returns true when the last argument of the command is an input (not an output):
        // either an http/https URL, or the argument directly following the -i flag.
        private static bool LastArgIsInput(string command, Match lastArgMatch)
        {
            string lastArg = lastArgMatch.Value.Trim().Trim('"');
            if (lastArg.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                lastArg.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return true;
            // Check whether the text before this arg ends with "-i"
            return command[..lastArgMatch.Index].TrimEnd()
                .EndsWith("-i", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<(int ExitCode, List<string> ErrorLines)> RunFfmpegAsync(
            string arguments, string logFile, CancellationToken cancellationToken = default)
        {
            var psi = new ProcessStartInfo(AppSettings.GetFfmpegExe(), arguments)
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
                // Wait for the process to fully exit so it releases its file handle
                // before the caller tries to delete the partial file.
                try { await process.WaitForExitAsync(); } catch { }
                throw;
            }

            return (process.ExitCode, errorLines);
        }

        // -------------------------------------------------------------------------
        // File validation
        // -------------------------------------------------------------------------

        /// <summary>
        /// Validates a video file by fully decoding it with FFmpeg and checking for decode errors.
        /// Runs through <see cref="RunFfmpegAsync"/> (default verbosity, not "-v error") so the
        /// existing progress parsing drives the progress bar and estimated-remaining-time label
        /// during the decode, instead of the check running silently in the background.
        /// Returns true if the file is OK, false if it is corrupted or unreadable.
        /// </summary>
        private async Task<bool> ValidateVideoFileAsync(string filePath, string logFile, CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
                return false;

            try
            {
                var (exitCode, errorLines) = await RunFfmpegAsync(
                    $"-i \"{filePath}\" -f null -", logFile, cancellationToken);
                return exitCode == 0 && errorLines.Count == 0;
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

        private void SyncShowNameToFolder(string folder)
        {
            string folderShowName = Path.GetFileName(folder);
            if (string.IsNullOrEmpty(folderShowName)) return;
            var cur = EpisodePattern.Match(txtFileName.Text.Trim());
            if (!cur.Success) return;
            if (!cur.Groups[1].Value.Equals(folderShowName, StringComparison.OrdinalIgnoreCase))
                txtFileName.Text = $"{folderShowName} - s{cur.Groups[2].Value}e{cur.Groups[3].Value}{cur.Groups[4].Value}";
        }

        /// <summary>
        /// Clears the File Name box when it doesn't already belong to showDisplayName — e.g. a
        /// leftover raw guess from the command's output argument, or an episode name for a
        /// different show — so that a subsequent <see cref="SuggestNextEpisode"/> call is free
        /// to fill in the real next-episode name instead of refusing to touch what looks like
        /// someone else's deliberate filename. Leaves the extract-subtitle feature's filename
        /// alone, matching SuggestNextEpisode's own guard.
        /// </summary>
        private void ClearFileNameIfNotForShow(string showDisplayName)
        {
            if (_commandSetByExtractFeature) return;

            var m = EpisodePattern.Match(txtFileName.Text.Trim());
            bool matchesShow = m.Success && m.Groups[1].Value.Equals(showDisplayName, StringComparison.OrdinalIgnoreCase);
            if (!matchesShow)
                txtFileName.Clear();
        }

        private void SuggestNextEpisode(string folder)
        {
            if (_commandSetByExtractFeature) return;
            if (!Directory.Exists(folder))
            {
                SyncShowNameToFolder(folder);
                return;
            }

            // Only scan files whose extension matches the command's output extension so that
            // e.g. extracting s01e01.srt from a folder of .mp4 files finds no .srt episodes
            // and exits without touching the filename the extract feature already set.
            string commandExt = GetCommandOutputExtension(txtOriginalCommand.Text.Trim());

            var matches = Directory.GetFiles(folder)
                .Where(f => string.IsNullOrEmpty(commandExt) ||
                            Path.GetExtension(f).Equals(commandExt, StringComparison.OrdinalIgnoreCase))
                .Select(f => EpisodePattern.Match(Path.GetFileName(f)))
                .Where(m => m.Success)
                .OrderBy(m => int.Parse(m.Groups[2].Value))
                .ThenBy(m => int.Parse(m.Groups[3].Value))
                .ToList();

            if (matches.Count == 0)
            {
                SyncShowNameToFolder(folder);
                return;
            }

            var last = matches.Last();
            string showName = last.Groups[1].Value;
            int season = int.Parse(last.Groups[2].Value);
            int episode = int.Parse(last.Groups[3].Value) + 1;
            string ext = !string.IsNullOrEmpty(commandExt) ? commandExt : last.Groups[4].Value;
            string seasonStr = season.ToString().PadLeft(last.Groups[2].Length, '0');
            string episodeStr = episode.ToString().PadLeft(last.Groups[3].Length, '0');

            // Only overwrite the filename when it is empty, or when the current name is the
            // immediately preceding episode for this show (sequential download flow).
            // Any other value — e.g. a name set by the extract-subtitle feature — is left alone.
            string current = txtFileName.Text.Trim();
            if (!string.IsNullOrEmpty(current))
            {
                var cur = EpisodePattern.Match(current);
                bool isImmediatelyPreceding = cur.Success &&
                    cur.Groups[1].Value.Equals(showName, StringComparison.OrdinalIgnoreCase) &&
                    int.Parse(cur.Groups[2].Value) == season &&
                    int.Parse(cur.Groups[3].Value) + 1 == episode;
                if (!isImmediatelyPreceding) return;
            }

            txtFileName.Text = $"{showName} - s{seasonStr}e{episodeStr}{ext}";

            // Pre-fill the Season/Episode boxes to match the suggestion so the
            // user can see — and immediately override — the suggested values.
            _updatingSeasonEpisode = true;
            txtSeason.Text = season.ToString();
            txtEpisode.Text = episode.ToString();
            _updatingSeasonEpisode = false;
        }

        private void rdoMovie_CheckedChanged(object sender, EventArgs e)
        {
            if (!rdoMovie.Checked) return;

            cboFolder.SelectedIndex = 1;

            if (!_settingCategoryFromAutoDetect)
            {
                string? showName = ExtractShowName(txtOriginalCommand.Text.Trim());
                if (!string.IsNullOrEmpty(showName))
                    txtTitle.Text = showName;

                TryAutoDetectCategoryAndYearFromTitle();
            }

            lblSeason.Visible = false;
            txtSeason.Visible = false;
            lblEpisode.Visible = false;
            txtEpisode.Visible = false;
            txtSeason.Text = "";
            txtEpisode.Text = "";

            UpdateFolderAndFileNameFromTitle();

            if (!_settingCategoryFromAutoDetect)
                btnRun.Focus();
        }

        private void txtSeason_TextChanged(object sender, EventArgs e)
        {
            if (_updatingSeasonEpisode) return;
            StripNonDigits(txtSeason);
            if (txtSeason.Text.Length > 0 && txtEpisode.Text.Length == 0)
                txtEpisode.Text = "1";
            UpdateFileNameFromSeasonEpisode();
        }

        private void txtEpisode_TextChanged(object sender, EventArgs e)
        {
            if (_updatingSeasonEpisode) return;
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
        /// overriding whatever the folder scan suggested. If the current filename doesn't
        /// already match the TV-show naming pattern — e.g. this is the very first episode
        /// of a show, so there was no prior file for the folder scan to seed it from — the
        /// show name and extension are derived from the current filename instead, with the
        /// season/episode numbers defaulting to two digits. Does nothing if the filename is
        /// empty, or either box is empty / contains a non-positive number.
        /// </summary>
        private void UpdateFileNameFromSeasonEpisode()
        {
            if (!int.TryParse(txtSeason.Text, out int season) || season < 1) return;
            if (!int.TryParse(txtEpisode.Text, out int episode) || episode < 1) return;

            string showName;
            string ext;
            int seasonDigits = 2;
            int episodeDigits = 2;

            var m = EpisodePattern.Match(txtFileName.Text);
            if (m.Success)
            {
                showName = m.Groups[1].Value;
                ext = m.Groups[4].Value;
                seasonDigits = m.Groups[2].Length;
                episodeDigits = m.Groups[3].Length;
            }
            else
            {
                string current = txtFileName.Text.Trim();
                if (string.IsNullOrEmpty(current)) return;
                ext = Path.GetExtension(current);
                if (string.IsNullOrEmpty(ext)) return;
                // Prefer the folder's show-folder name (includes year if user added it)
                string folderName = Path.GetFileName(cboFolder.Text.Trim());
                if (!string.IsNullOrEmpty(folderName))
                    showName = folderName;
                else
                {
                    string? extracted = ExtractShowName(txtOriginalCommand.Text.Trim());
                    if (extracted == null) return;
                    showName = extracted;
                }
            }

            string seasonStr = season.ToString().PadLeft(seasonDigits, '0');
            string episodeStr = episode.ToString().PadLeft(episodeDigits, '0');

            txtFileName.Text = $"{showName} - s{seasonStr}e{episodeStr}{ext}";
        }

        private void rdoTvShow_CheckedChanged(object sender, EventArgs e)
        {
            if (!rdoTvShow.Checked) return;

            lblSeason.Visible = true;
            txtSeason.Visible = true;
            lblEpisode.Visible = true;
            txtEpisode.Visible = true;

            cboFolder.SelectedIndex = 2;

            if (!_settingCategoryFromAutoDetect)
            {
                string? showName = ExtractShowName(txtOriginalCommand.Text.Trim());
                if (!string.IsNullOrEmpty(showName))
                    txtTitle.Text = showName;

                TryAutoDetectCategoryAndYearFromTitle();
            }

            UpdateFolderAndFileNameFromTitle();

            if (!_settingCategoryFromAutoDetect)
                btnRun.Focus();
        }

        /// <summary>
        /// Runs after the Title box is set — either auto-filled from the command when a radio
        /// button is checked, or manually typed and then left. Scans the Movies and/or TV Shows
        /// base folders for an existing subfolder named exactly "Title" or "Title (Year)". Only
        /// the category matching an already-checked radio button is scanned; if neither is
        /// checked, both are. A single match checks the corresponding radio button (without
        /// re-parsing Title from the command) and fills in the year if the folder name carried
        /// one. Zero or multiple matches make no changes, since there is nothing — or too much —
        /// to guess from.
        /// </summary>
        private void TryAutoDetectCategoryAndYearFromTitle()
        {
            string title = txtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title)) return;

            var matches = new List<(bool isTvShow, string? year)>();

            if (!rdoTvShow.Checked)
                matches.AddRange(FindTitleFolderMatches(cboFolder.Items[1]?.ToString(), title, isTvShow: false));
            if (!rdoMovie.Checked)
                matches.AddRange(FindTitleFolderMatches(cboFolder.Items[2]?.ToString(), title, isTvShow: true));

            if (matches.Count != 1) return;

            var (isTvShow, year) = matches[0];

            if (!string.IsNullOrEmpty(year))
                txtYear.Text = year;

            _settingCategoryFromAutoDetect = true;
            if (isTvShow)
                rdoTvShow.Checked = true;
            else
                rdoMovie.Checked = true;
            _settingCategoryFromAutoDetect = false;
        }

        /// <summary>
        /// Yields (isTvShow, year) for each immediate subfolder of baseFolder whose name is
        /// exactly title, or title followed by " (YYYY)". year is null for an exact-name match.
        /// </summary>
        private static IEnumerable<(bool isTvShow, string? year)> FindTitleFolderMatches(string? baseFolder, string title, bool isTvShow)
        {
            if (string.IsNullOrEmpty(baseFolder) || !Directory.Exists(baseFolder))
                yield break;

            foreach (string dir in Directory.GetDirectories(baseFolder))
            {
                string name = Path.GetFileName(dir);
                if (name.Equals(title, StringComparison.OrdinalIgnoreCase))
                {
                    yield return (isTvShow, null);
                    continue;
                }

                var m = Regex.Match(name, @"^(.*) \((\d{4})\)$");
                if (m.Success && m.Groups[1].Value.Equals(title, StringComparison.OrdinalIgnoreCase))
                    yield return (isTvShow, m.Groups[2].Value);
            }
        }

        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
            UpdateFolderAndFileNameFromTitle();
        }

        private void txtTitle_Leave(object sender, EventArgs e)
        {
            TryAutoDetectCategoryAndYearFromTitle();
        }

        private void txtYear_TextChanged(object sender, EventArgs e)
        {
            StripNonDigits(txtYear);
            UpdateFolderAndFileNameFromTitle();
        }

        /// <summary>
        /// Expands a 2-digit year to 4 digits when the user leaves the field, using a rolling
        /// pivot of "current year + 2": values at or below the pivot become 20XX (near-future
        /// releases), values above it become 19XX. E.g. in 2026 (pivot 28): 26-28 -> 2026-2028,
        /// 29-99 -> 1929-1999.
        /// </summary>
        private void txtYear_Leave(object sender, EventArgs e)
        {
            string? expanded = ComputeDisplayYear(txtYear.Text);
            if (expanded != null && expanded != txtYear.Text)
                txtYear.Text = expanded;
        }

        private static string? ComputeDisplayYear(string yearDigits)
        {
            if (yearDigits.Length == 4) return yearDigits;
            if (yearDigits.Length != 2 || !int.TryParse(yearDigits, out int twoDigitYear)) return null;

            int pivot = (DateTime.Now.Year + 2) % 100;
            int century = twoDigitYear <= pivot ? 2000 : 1900;
            return (century + twoDigitYear).ToString();
        }

        /// <summary>
        /// Rebuilds the folder and file name from the Title/Year boxes. For a movie, the file
        /// name becomes "Title (Year).ext". For a TV show, the subfolder becomes "Title (Year)"
        /// and the episode suggestion logic takes over the file name from there. Does nothing
        /// while the Title box is empty, or while neither radio button is checked.
        /// </summary>
        private void UpdateFolderAndFileNameFromTitle()
        {
            string title = txtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title)) return;

            string? year = ComputeDisplayYear(txtYear.Text.Trim());
            string displayName = year != null ? $"{title} ({year})" : title;

            if (rdoMovie.Checked)
            {
                string ext = GetOutputFileExtensionForNaming();
                txtFileName.Text = displayName + ext;
            }
            else if (rdoTvShow.Checked)
            {
                string baseTvFolder = cboFolder.Items[2]?.ToString() ?? string.Empty;
                string showFolder = Path.Combine(baseTvFolder, displayName);
                AddSuggestedFolder(showFolder);
                cboFolder.SelectedItem = showFolder;

                ClearFileNameIfNotForShow(displayName);
                SuggestNextEpisode(showFolder);

                if (string.IsNullOrEmpty(txtFileName.Text.Trim()))
                {
                    string? outFile = GetCommandOutputFilename(txtOriginalCommand.Text.Trim());
                    if (outFile != null)
                        txtFileName.Text = outFile;
                }
            }
        }

        /// <summary>
        /// Extension for the output file: prefers the command's actual output argument,
        /// falling back to the last argument's extension when the command has no
        /// clearly-identifiable output (e.g. an input-only command typed so far).
        /// </summary>
        private string GetOutputFileExtensionForNaming()
        {
            string command = txtOriginalCommand.Text.Trim();
            string ext = GetCommandOutputExtension(command);
            if (!string.IsNullOrEmpty(ext)) return ext;

            var lastArg = Regex.Match(command, @"(""[^""]*""|[^\s]+)\s*$");
            return lastArg.Success ? Path.GetExtension(lastArg.Value.Trim().Trim('"')) : string.Empty;
        }

        /// <summary>
        /// Extracts a clean show or movie name from the output filename in an FFmpeg command.
        /// Strips everything from the first '-' or '[' delimiter onwards.
        /// Returns null if no name could be extracted.
        /// </summary>
        private static readonly string[] OutputFileExtensions =
            { ".mp4", ".mkv", ".avi", ".mov", ".ts", ".m2ts", ".wmv", ".srt", ".ass", ".vtt", ".mp3", ".m4a", ".aac" };

        private static string? GetCommandOutputFilename(string command)
        {
            var quoted = Regex.Match(command, @"""([^""]+)""\s*$");
            if (quoted.Success)
            {
                string name = Path.GetFileName(quoted.Groups[1].Value.Trim());
                if (OutputFileExtensions.Contains(Path.GetExtension(name), StringComparer.OrdinalIgnoreCase))
                    return name;
            }
            var unquoted = Regex.Match(command, @"(\S+)\s*$");
            if (unquoted.Success)
            {
                string name = Path.GetFileName(unquoted.Groups[1].Value);
                if (OutputFileExtensions.Contains(Path.GetExtension(name), StringComparer.OrdinalIgnoreCase))
                    return name;
            }
            return null;
        }

        private static string? ExtractShowName(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return null;

            var lastArg = Regex.Match(command, @"(""[^""]*""|[^\s]+)\s*$");
            if (!lastArg.Success) return null;

            string nameOnly = Path.GetFileNameWithoutExtension(lastArg.Value.Trim().Trim('"'));
            if (string.IsNullOrWhiteSpace(nameOnly)) return null;

            int dashIndex = nameOnly.IndexOf('-');
            int bracketIndex = nameOnly.IndexOf('[');

            int delimIndex = (dashIndex, bracketIndex) switch
            {
                ( >= 0, >= 0) => Math.Min(dashIndex, bracketIndex),
                ( >= 0, _) => dashIndex,
                (_, >= 0) => bracketIndex,
                _ => -1
            };

            string name = delimIndex > 0 ? nameOnly[..delimIndex].Trim() : nameOnly.Trim();
            return string.IsNullOrEmpty(name) ? null : name;
        }

        /// <summary>
        /// Checks the TV show history for the command's show name.
        /// If a match is found, automatically sets the folder and triggers TV show mode.
        /// </summary>
        // -------------------------------------------------------------------------
        // Close protection
        // -------------------------------------------------------------------------

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_cts != null)
            {
                var result = MessageBox.Show(
                    "A download is in progress.\n\n" +
                    "If you close the application now, the partial file will be deleted.\n\n" +
                    "Close anyway?",
                    AppTitle,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2); // Cancel is the default

                e.Cancel = true; // Always prevent immediate close — we handle it ourselves

                if (result != DialogResult.OK)
                    return; // User cancelled — nothing to do

                // User confirmed — cancel the download; the finally block will close the form
                _closeAfterCancel = true;
                _cts.Cancel();
                return;
            }

            base.OnFormClosing(e);
        }

        // -------------------------------------------------------------------------
        // Keyboard shortcuts
        // -------------------------------------------------------------------------

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Button? button = keyData switch
            {
                Keys.Control | Keys.O => btnOpenFile,
                Keys.Control | Keys.Shift | Keys.O => btnOpenFolder,
                Keys.Control | Keys.E => btnRun,
                Keys.Alt | Keys.B => btnBrowseForFolder,
                _ => null
            };

            if (button != null)
            {
                if (button.Enabled) button.PerformClick();
                return true;
            }

            if (keyData == Keys.F6)
            {
                txtOriginalCommand.Focus();
                txtOriginalCommand.SelectAll();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void TryApplyTvShowHistory(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;
            if (!rdoTvShow.Checked) return;

            string? showName = ExtractShowName(command);
            //WriteAppLog($"HISTORY  : Extracted show name = '{showName}'");
            if (showName == null) return;

            var (subfolder, _) = TvShowHistory.LookupFolderWithDiagnostics(showName);
            //WriteAppLog($"HISTORY  : {diagnostics}");
            if (subfolder == null) return;

            string baseTvFolder = cboFolder.Items[2]?.ToString() ?? string.Empty;
            string showFolder = Path.Combine(baseTvFolder, subfolder);

            AddSuggestedFolder(showFolder);
            cboFolder.Text = showFolder;

            ClearFileNameIfNotForShow(subfolder);
            SuggestNextEpisode(showFolder);
        }
    }
}
