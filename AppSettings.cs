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

        private static readonly string ReplaceQasFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SweWolfSoftware", "FFmpegAssist", "audio-qas-replace.txt");

        private static string? _cachedReplaceQas;

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

        /// <summary>
        /// Returns "Yes", "No", or "Ask" (default when no setting has been saved).
        /// </summary>
        public static string ReplaceAudioQas
        {
            get
            {
                if (_cachedReplaceQas != null) return _cachedReplaceQas;
                try
                {
                    if (File.Exists(ReplaceQasFile))
                    {
                        string v = File.ReadAllText(ReplaceQasFile, System.Text.Encoding.UTF8).Trim();
                        if (v == "Yes" || v == "No" || v == "Ask")
                        {
                            _cachedReplaceQas = v;
                            return _cachedReplaceQas;
                        }
                    }
                }
                catch { }
                _cachedReplaceQas = "Ask";
                return _cachedReplaceQas;
            }
            set
            {
                _cachedReplaceQas = value;
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ReplaceQasFile)!);
                    File.WriteAllText(ReplaceQasFile, value, System.Text.Encoding.UTF8);
                }
                catch { /* never crash the host app */ }
            }
        }
    }
}
