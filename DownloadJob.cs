using System.Text.Json.Serialization;

namespace FFmpegAssistant
{
    /// <summary>What an M3U8 playlist contains, from the pre-fetch before a download.</summary>
    internal enum M3u8ContentType { Unknown, Subtitle, Video }

    /// <summary>
    /// One download, prepared when Download is clicked: the fields are checked, the questions
    /// (create folder, overwrite) are asked and the file name is final. Running it only uses this
    /// record, never the fields on Form1, which stay editable to prepare the next download.
    /// The stored properties are plain values so the job can be saved as JSON (for the download
    /// queue); the paths derived from them are not saved.
    /// </summary>
    internal sealed record DownloadJob
    {
        /// <summary>The FFmpeg command to run, with the output replaced by <see cref="DownloadPath"/>.</summary>
        public required string Command { get; init; }
        /// <summary>The command as it was in the Command box, before the output was replaced.</summary>
        public string OriginalCommand { get; init; } = "";
        public required string Folder { get; init; }
        public required string FileName { get; init; }
        /// <summary>"Enable Watching While Downloading": download to a .ts file, convert it at the end.</summary>
        public bool WatchMode { get; init; }
        public int M3u8SegmentCount { get; init; }
        public M3u8ContentType M3u8Type { get; init; }

        [JsonIgnore] public string OutputPath => Path.Combine(Folder, FileName);

        [JsonIgnore] public bool IsSrt => FileName.EndsWith(".srt", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Normal mode downloads to a "(part)" file, renamed to the final name only after a
        /// successful validation, to protect against power outages.
        /// </summary>
        [JsonIgnore]
        public string PartPath => Path.Combine(Folder,
            Path.GetFileNameWithoutExtension(FileName) + " (part)" + Path.GetExtension(FileName));

        /// <summary>The file FFmpeg writes to: the .ts file in watch mode, otherwise the "(part)" file.</summary>
        [JsonIgnore] public string DownloadPath => WatchMode ? Path.ChangeExtension(OutputPath, ".ts") : PartPath;
    }
}
