namespace FFmpegAssistant
{
    /// <summary>
    /// Persists a mapping of TV show name → subfolder so the application can
    /// auto-suggest the correct folder the next time the same show is downloaded.
    ///
    /// File format (tab-separated, one entry per line):
    ///   ShowName TAB SubfolderName
    ///   e.g.  Robinson Gränslandet TAB Robinson Gränslandet
    ///
    /// Lookup is case-insensitive. Duplicate show names are updated in place.
    /// All I/O failures are silently swallowed so the host app is never affected.
    /// </summary>
    internal static class TvShowHistory
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SweWolfSoftware", "FFmpegAssist", "TvShowHistory.txt");

        /// <summary>
        /// Returns the subfolder name stored for <paramref name="showName"/>,
        /// or <c>null</c> if no entry exists.
        /// </summary>
        public static string? LookupFolder(string showName)
            => LookupFolderWithDiagnostics(showName).Subfolder;

        /// <summary>
        /// Same as <see cref="LookupFolder"/> but also returns a diagnostic string
        /// describing what happened — useful for debug logging.
        /// </summary>
        public static (string? Subfolder, string Diagnostics) LookupFolderWithDiagnostics(string showName)
        {
            try
            {
                if (!File.Exists(FilePath))
                    return (null, $"File not found: {FilePath}");

                var lines = ReadLines(FilePath);
                var comparisons = new System.Text.StringBuilder();

                foreach (string line in lines)
                {
                    if (!TryParseLine(line, out string key, out string value)) continue;

                    comparisons.Append($"['{key}' vs '{showName}'] ");

                    if (string.Equals(key, showName, StringComparison.OrdinalIgnoreCase))
                        return (value, $"Match found → subfolder='{value}'");
                }

                return (null, $"No match. Compared: {comparisons}");
            }
            catch (Exception ex)
            {
                return (null, $"Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses one line from the history file.
        /// Accepts both tab-separated (app-written) and multiple-spaces-separated
        /// (manually created in Notepad) formats.
        /// </summary>
        private static bool TryParseLine(string line, out string key, out string value)
        {
            key = value = string.Empty;
            if (string.IsNullOrWhiteSpace(line)) return false;

            // Prefer tab separator (unambiguous)
            int tab = line.IndexOf('\t');
            if (tab > 0)
            {
                key   = line[..tab].Trim();
                value = line[(tab + 1)..].Trim();
                return !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value);
            }

            // Fall back: split on two or more consecutive spaces
            // Matches "ShowName   SubfolderName" even when both sides contain single spaces
            var m = System.Text.RegularExpressions.Regex.Match(line, @"^(.+?)\s{2,}(.+)$");
            if (!m.Success) return false;

            key   = m.Groups[1].Value.Trim();
            value = m.Groups[2].Value.Trim();
            return !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Reads all lines from the history file using robust encoding detection:
        /// 1. UTF-8 BOM present  → UTF-8
        /// 2. No BOM, valid UTF-8 → UTF-8  (covers modern Notepad default)
        /// 3. No BOM, invalid UTF-8 → system ANSI  (covers legacy Notepad ANSI saves)
        /// </summary>
        private static IEnumerable<string> ReadLines(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);

            // Check for UTF-8 BOM (EF BB BF)
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                return System.Text.Encoding.UTF8
                    .GetString(bytes, 3, bytes.Length - 3)
                    .Split('\n')
                    .Select(l => l.TrimEnd('\r'));
            }

            // No BOM — try strict UTF-8 first (covers UTF-8 without BOM)
            try
            {
                var strictUtf8 = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
                return strictUtf8.GetString(bytes).Split('\n').Select(l => l.TrimEnd('\r'));
            }
            catch
            {
                // Not valid UTF-8 — fall back to system ANSI (Windows-1252 on Swedish Windows)
                return System.Text.Encoding.Default
                    .GetString(bytes)
                    .Split('\n')
                    .Select(l => l.TrimEnd('\r'));
            }
        }

        /// <summary>
        /// Saves or updates the mapping for <paramref name="showName"/>.
        /// If the show already exists in the file its subfolder is updated in place.
        /// Always writes UTF-8 with BOM so future reads are unambiguous.
        /// </summary>
        public static void SaveOrUpdate(string showName, string subfolder)
        {
            if (string.IsNullOrWhiteSpace(showName) || string.IsNullOrWhiteSpace(subfolder))
                return;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

                List<string> lines = File.Exists(FilePath)
                    ? [.. ReadLines(FilePath)]
                    : [];

                bool found = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (!TryParseLine(lines[i], out string key, out _)) continue;

                    if (string.Equals(key, showName, StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = $"{showName}\t{subfolder}";
                        found = true;
                        break;
                    }
                }

                if (!found)
                    lines.Add($"{showName}\t{subfolder}");

                // Write UTF-8 with BOM so encoding is unambiguous on all future reads
                File.WriteAllLines(FilePath, lines, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            }
            catch { /* never crash the host app */ }
        }
    }
}
