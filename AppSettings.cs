namespace FFmpegAssistant
{
    /// <summary>
    /// Persists application settings to flat files under
    /// %APPDATA%\SweWolfSoftware\FFmpegAssist\.
    ///
    /// Currently stores one setting: the full path to ffmpeg.exe
    /// for systems where ffmpeg is not on the system PATH.
    ///
    /// All I/O failures are silently swallowed so the host app is never affected.
    /// </summary>
    internal static class AppSettings
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SweWolfSoftware", "FFmpegAssist", "ffmpeg-path.txt");

        // null = not yet loaded from disk; non-null after first call to GetFfmpegExe()
        private static string? _cachedFfmpegExe;

        /// <summary>
        /// Returns the stored ffmpeg.exe path, or <c>"ffmpeg"</c> as a fallback
        /// so the OS PATH is used for resolution. Value is cached after the first read.
        /// </summary>
        public static string GetFfmpegExe()
        {
            if (_cachedFfmpegExe != null) return _cachedFfmpegExe;
            _cachedFfmpegExe = LoadFfmpegPath() ?? "ffmpeg";
            return _cachedFfmpegExe;
        }

        /// <summary>
        /// Saves <paramref name="fullPath"/> as the ffmpeg.exe location and
        /// updates the in-memory cache immediately so the next call to
        /// <see cref="GetFfmpegExe"/> returns the new value without a disk read.
        /// </summary>
        public static void SetFfmpegExe(string fullPath)
        {
            _cachedFfmpegExe = fullPath;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, fullPath, System.Text.Encoding.UTF8);
            }
            catch { /* never crash the host app */ }
        }

        /// <summary>
        /// Returns the user-configured ffmpeg.exe path, or <c>null</c> if no
        /// custom path has been saved (i.e. the system PATH will be used).
        /// </summary>
        public static string? FfmpegExePath => LoadFfmpegPath();

        /// <summary>
        /// Removes the saved ffmpeg.exe path so the system PATH is used again.
        /// </summary>
        public static void ClearFfmpegExe()
        {
            _cachedFfmpegExe = null; // reset cache so next GetFfmpegExe() reloads from disk
            try { File.Delete(FilePath); } catch { /* never crash the host app */ }
        }

        private static string? LoadFfmpegPath()
        {
            try
            {
                if (!File.Exists(FilePath)) return null;
                string path = File.ReadAllText(FilePath, System.Text.Encoding.UTF8).Trim();
                return string.IsNullOrEmpty(path) ? null : path;
            }
            catch { return null; }
        }

        // -------------------------------------------------------------------------
        // audio_qas replacement behaviour: "Yes" | "No" | "Ask" (default)
        // -------------------------------------------------------------------------

        //private static readonly string ReplaceQasFile = Path.Combine(
        //    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        //    "SweWolfSoftware", "FFmpegAssist", "audio-qas-replace.txt");

        //private static string? _cachedReplaceQas;

        // -------------------------------------------------------------------------
        // Number of download attempts (1 = no retry, >1 = auto-retry on failure)
        // -------------------------------------------------------------------------

        private static readonly string AttemptsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SweWolfSoftware", "FFmpegAssist", "download-attempts.txt");

        private static int? _cachedAttempts;

        public static int NumberOfDownloadAttempts
        {
            get
            {
                if (_cachedAttempts.HasValue) return _cachedAttempts.Value;
                try
                {
                    if (File.Exists(AttemptsFile))
                    {
                        string text = File.ReadAllText(AttemptsFile, System.Text.Encoding.UTF8).Trim();
                        if (int.TryParse(text, out int v))
                        {
                            _cachedAttempts = v;
                            return v;
                        }
                    }
                }
                catch { }
                _cachedAttempts = 5;
                return 5;
            }
            set
            {
                _cachedAttempts = value;
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(AttemptsFile)!);
                    File.WriteAllText(AttemptsFile, value.ToString(), System.Text.Encoding.UTF8);
                }
                catch { /* never crash the host app */ }
            }
        }

        // -------------------------------------------------------------------------
        // Check for updates on startup: "Yes" (default) | "No"
        // -------------------------------------------------------------------------

        private static readonly string UpdateCheckFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SweWolfSoftware", "FFmpegAssist", "check-for-updates.txt");

        private static string? _cachedUpdateCheck;

        public static string CheckForUpdatesOnStartup
        {
            get
            {
                if (_cachedUpdateCheck != null) return _cachedUpdateCheck;
                try
                {
                    if (File.Exists(UpdateCheckFile))
                    {
                        string v = File.ReadAllText(UpdateCheckFile, System.Text.Encoding.UTF8).Trim();
                        if (v == "Yes" || v == "No")
                        {
                            _cachedUpdateCheck = v;
                            return v;
                        }
                    }
                }
                catch { }
                _cachedUpdateCheck = "Yes";
                return _cachedUpdateCheck;
            }
            set
            {
                _cachedUpdateCheck = value;
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(UpdateCheckFile)!);
                    File.WriteAllText(UpdateCheckFile, value, System.Text.Encoding.UTF8);
                }
                catch { /* never crash the host app */ }
            }
        }

        // -------------------------------------------------------------------------
        // Action to take when a download finishes: "Play a Sound" (default) | "Message Box" | "None"
        // -------------------------------------------------------------------------

        private static readonly string DownloadFinishedActionFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SweWolfSoftware", "FFmpegAssist", "download-finished-action.txt");

        private static string? _cachedDownloadFinishedAction;

        public static string ActionWhenDownloadFinished
        {
            get
            {
                if (_cachedDownloadFinishedAction != null) return _cachedDownloadFinishedAction;
                try
                {
                    if (File.Exists(DownloadFinishedActionFile))
                    {
                        string v = File.ReadAllText(DownloadFinishedActionFile, System.Text.Encoding.UTF8).Trim();
                        if (v == "Play a Sound" || v == "Message Box" || v == "None")
                        {
                            _cachedDownloadFinishedAction = v;
                            return v;
                        }
                    }
                }
                catch { }
                _cachedDownloadFinishedAction = "Play a Sound";
                return _cachedDownloadFinishedAction;
            }
            set
            {
                _cachedDownloadFinishedAction = value;
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(DownloadFinishedActionFile)!);
                    File.WriteAllText(DownloadFinishedActionFile, value, System.Text.Encoding.UTF8);
                }
                catch { /* never crash the host app */ }
            }
        }

        // -------------------------------------------------------------------------
        // Sound file (Resources\*.wav) used when ActionWhenDownloadFinished == "Play a Sound"
        // -------------------------------------------------------------------------

        private static readonly string DownloadFinishedSoundFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SweWolfSoftware", "FFmpegAssist", "download-finished-sound.txt");

        private static string? _cachedDownloadFinishedSound;

        public static string FinishedDownloadSoundFile
        {
            get
            {
                if (_cachedDownloadFinishedSound != null) return _cachedDownloadFinishedSound;
                try
                {
                    if (File.Exists(DownloadFinishedSoundFile))
                    {
                        _cachedDownloadFinishedSound = File.ReadAllText(DownloadFinishedSoundFile, System.Text.Encoding.UTF8).Trim();
                        return _cachedDownloadFinishedSound;
                    }
                }
                catch { }
                _cachedDownloadFinishedSound = string.Empty;
                return _cachedDownloadFinishedSound;
            }
            set
            {
                _cachedDownloadFinishedSound = value;
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(DownloadFinishedSoundFile)!);
                    File.WriteAllText(DownloadFinishedSoundFile, value, System.Text.Encoding.UTF8);
                }
                catch { /* never crash the host app */ }
            }
        }

        // -------------------------------------------------------------------------
        // Color-coded status messages: "Yes" (default) | "No"
        // Prepared for a future settings-form toggle; no UI control yet.
        // -------------------------------------------------------------------------

        private static readonly string ColorCodedStatusMessagesFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SweWolfSoftware", "FFmpegAssist", "color-coded-status-messages.txt");

        private static bool? _cachedColorCodedStatusMessages;

        public static bool ColorCodedStatusMessages
        {
            get
            {
                if (_cachedColorCodedStatusMessages.HasValue) return _cachedColorCodedStatusMessages.Value;
                try
                {
                    if (File.Exists(ColorCodedStatusMessagesFile))
                    {
                        string v = File.ReadAllText(ColorCodedStatusMessagesFile, System.Text.Encoding.UTF8).Trim();
                        if (v == "Yes" || v == "No")
                        {
                            _cachedColorCodedStatusMessages = v == "Yes";
                            return _cachedColorCodedStatusMessages.Value;
                        }
                    }
                }
                catch { }
                _cachedColorCodedStatusMessages = true;
                return true;
            }
            set
            {
                _cachedColorCodedStatusMessages = value;
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ColorCodedStatusMessagesFile)!);
                    File.WriteAllText(ColorCodedStatusMessagesFile, value ? "Yes" : "No", System.Text.Encoding.UTF8);
                }
                catch { /* never crash the host app */ }
            }
        }

        /// <summary>
        /// Returns "Yes", "No", or "Ask" (default when no setting has been saved).
        /// </summary>
        //public static string ReplaceAudioQas
        //{
        //    get
        //    {
        //        if (_cachedReplaceQas != null) return _cachedReplaceQas;
        //        try
        //        {
        //            if (File.Exists(ReplaceQasFile))
        //            {
        //                string v = File.ReadAllText(ReplaceQasFile, System.Text.Encoding.UTF8).Trim();
        //                if (v == "Yes" || v == "No" || v == "Ask")
        //                {
        //                    _cachedReplaceQas = v;
        //                    return _cachedReplaceQas;
        //                }
        //            }
        //        }
        //        catch { }
        //        _cachedReplaceQas = "Ask";
        //        return _cachedReplaceQas;
        //    }
        //    set
        //    {
        //        _cachedReplaceQas = value;
        //        try
        //        {
        //            Directory.CreateDirectory(Path.GetDirectoryName(ReplaceQasFile)!);
        //            File.WriteAllText(ReplaceQasFile, value, System.Text.Encoding.UTF8);
        //        }
        //        catch { /* never crash the host app */ }
        //    }
        //}
    }
}
