namespace FFmpegAssistant
{
    /// <summary>
    /// Log files under %APPDATA%\SweWolfSoftware\&lt;app&gt;\Logs\:
    ///   download_yyyyMMdd_HHmmss_&lt;name&gt;.txt  one per download: run details plus FFmpeg's full output
    ///   validate_yyyyMMdd_HHmmss_&lt;name&gt;.txt  one per Validate Video File check
    /// and, one level up in %APPDATA%\SweWolfSoftware\&lt;app&gt;\:
    ///   FFmpeg_calls.log                     one line per download attempt: "yyyy-MM-dd HH:mm:ss&lt;TAB&gt;command line"
    ///
    /// Shared with the sibling apps by copying this whole file. When copying, change only the
    /// namespace and AppFolderName. When changing this file, add a line to the history below,
    /// so the copies in the other apps can be compared with it.
    ///
    /// History:
    ///   2026-09-27  (MP3toMP4) Created: conversion log path moved here from Form1; added FFmpeg_calls.log.
    ///   2026-09-27  (MP3toMP4) FFmpeg_calls.log: over 0.5 MB, cut down to its newest 0.25 MB.
    ///   2026-09-27  (MP3toMP4) convert_*.log files older than 30 days are deleted (at start-up and
    ///               once a day), only files whose name exactly matches convert_yyyyMMdd_HHmmss.log.
    ///   2026-09-27  (MP3toMP4) Added FFmpegCallsLogFile (used by Help > Open FFmpeg Call Log);
    ///               LogFolder is also used by Help > Open Log Folder.
    ///   2026-10-13  (FFmpegAssistant) Copied from MP3toMP4. The per-run logs are named
    ///               download_/validate_yyyyMMdd_HHmmss_&lt;name&gt;.txt (NewDownloadLogFile,
    ///               NewValidationLogFile); the 30-day cleanup matches those names exactly.
    ///   2026-10-13  (FFmpegAssistant) FFmpeg_calls.log moved from Logs\ to the app folder (AppFolder),
    ///               so Logs\ holds only the per-run files. MP3toMP4 still has it in Logs\.
    /// </summary>
    internal static class LogFiles
    {
        private const string AppFolderName = "FFmpegAssist";

        internal static string LogFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SweWolfSoftware", AppFolderName, "Logs");

        // The app folder, one level above Logs: the call log is one long-lived file, like Queue.json
        private static string AppFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SweWolfSoftware", AppFolderName);

        internal static string FFmpegCallsLogFile => Path.Combine(AppFolder, "FFmpeg_calls.log");

        /// <summary>Creates the log folder if needed and returns a new, time-stamped download log path.</summary>
        internal static string NewDownloadLogFile(string fileName) => NewLogFile("download", fileName);

        /// <summary>Creates the log folder if needed and returns a new, time-stamped validation log path.</summary>
        internal static string NewValidationLogFile(string fileName) => NewLogFile("validate", fileName);

        private static string NewLogFile(string prefix, string fileName)
        {
            DeleteOldLogsOncePerDay();
            Directory.CreateDirectory(LogFolder);
            string name = Path.GetFileNameWithoutExtension(fileName);
            return Path.Combine(LogFolder, $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}_{name}.txt");
        }

        /// <summary>
        /// Appends one line to FFmpeg_calls.log, with the ffmpeg path quoted so the call can be copied
        /// and re-run in a terminal. Logging must never stop a download, so errors are ignored.
        /// </summary>
        internal static void AppendFFmpegCall(string ffmpeg, string ffmpegArgs)
        {
            try
            {
                Directory.CreateDirectory(AppFolder);
                string logFile = FFmpegCallsLogFile;
                string commandLine = $"\"{ffmpeg}\" {ffmpegArgs}".Replace("\r", "").Replace("\n", " ");
                File.AppendAllText(logFile,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\t{commandLine}\r\n", System.Text.Encoding.UTF8);
                TruncateToNewestHalf(logFile, FFmpegCallsMaxBytes);
            }
            catch { }
        }

        private const long FFmpegCallsMaxBytes = 512 * 1024;
        private const int RunLogMaxAgeDays = 30;

        // Exactly the names NewLogFile creates: download_yyyyMMdd_HHmmss_<name>.txt
        private static readonly System.Text.RegularExpressions.Regex RunLogNameRegex =
            new(@"^(download|validate)_[0-9]{8}_[0-9]{6}_.+\.txt$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        private static readonly object CleanupLock = new();
        private static DateTime _lastCleanupDate = DateTime.MinValue;

        /// <summary>
        /// Deletes download_*.txt and validate_*.txt files older than 30 days, on a background thread.
        /// Call it at start-up; NewLogFile also calls it, so an app that runs for weeks still cleans up,
        /// without a timer. Runs at most once per local calendar day. Errors are ignored.
        /// </summary>
        internal static void DeleteOldLogsOncePerDay()
        {
            lock (CleanupLock)
            {
                if (_lastCleanupDate == DateTime.Today) return;
                _lastCleanupDate = DateTime.Today;
            }

            Task.Run(() =>
            {
                try
                {
                    string folder = Path.GetFullPath(LogFolder);
                    if (!Directory.Exists(folder)) return;
                    DateTime cutoff = DateTime.Now.AddDays(-RunLogMaxAgeDays);
                    foreach (string file in Directory.EnumerateFiles(folder, "*.txt"))
                    {
                        try
                        {
                            // Double check before deleting: accept only the exact names NewLogFile
                            // creates, directly in the log folder.
                            string fullPath = Path.GetFullPath(file);
                            if (!string.Equals(Path.GetDirectoryName(fullPath), folder, StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (!RunLogNameRegex.IsMatch(Path.GetFileName(fullPath)))
                                continue;

                            if (File.GetLastWriteTime(fullPath) < cutoff) File.Delete(fullPath);
                        }
                        catch { } // e.g. open in an editor: try again next time
                    }
                }
                catch { }
            });
        }

        /// <summary>
        /// When the file is larger than maxBytes, keeps only the newest whole lines that fit in half of it,
        /// so the file isn't rewritten on every call. Writes a temporary file first and then replaces the
        /// log, so a failure never leaves a half-written log behind.
        /// </summary>
        private static void TruncateToNewestHalf(string file, long maxBytes)
        {
            if (new FileInfo(file).Length <= maxBytes) return;

            string[] lines = File.ReadAllLines(file, System.Text.Encoding.UTF8);
            long budget = maxBytes / 2, size = 0;
            int first = lines.Length;
            while (first > 0)
            {
                size += System.Text.Encoding.UTF8.GetByteCount(lines[first - 1]) + 2; // + CRLF
                if (size > budget) break;
                first--;
            }

            string temp = file + ".tmp";
            File.WriteAllLines(temp, lines[first..], System.Text.Encoding.UTF8);
            File.Move(temp, file, overwrite: true);
        }
    }
}
