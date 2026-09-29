using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFmpegAssistant
{
    internal enum QueueStatus { Waiting, Running, Done, Failed, Cancelled }

    /// <summary>One download in the queue, with how far it has come.</summary>
    internal sealed class QueueEntry
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required DownloadJob Job { get; init; }
        public QueueStatus Status { get; set; }
        /// <summary>Why the download failed (shown in the queue window and the report).</summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Loaded from the file as Running: the app ended during this download (e.g. a power outage).
        /// It is then Waiting again, to start from the beginning.
        /// </summary>
        [JsonIgnore] public bool WasInterrupted { get; set; }

        [JsonIgnore] public bool IsPending => Status is QueueStatus.Waiting or QueueStatus.Running;
    }

    /// <summary>
    /// The download queue. Waiting and running downloads are saved to
    /// %APPDATA%\SweWolfSoftware\FFmpegAssist\Queue.json after every change, so they survive a power
    /// outage or crash: a download still marked Running when the app starts was interrupted. Finished
    /// downloads (Done, Failed, Cancelled) are only kept in memory, for the queue window and the report.
    ///
    /// Only one app window may use the queue file: the one that owns a named mutex. Windows releases
    /// the mutex when that window closes, crashes or loses power, so it can never be left "stuck";
    /// the file alone can't tell a window using it from a file left over from a power outage.
    /// A window that doesn't own the queue still keeps its own download in <see cref="Entries"/>,
    /// but doesn't save it.
    /// </summary>
    internal sealed class DownloadQueue : IDisposable
    {
        private const string MutexName = @"Local\SweWolfSoftware.FFmpegAssist.DownloadQueue";

        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SweWolfSoftware", "FFmpegAssist", "Queue.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // å, ä, ö and quotes as they are (not an HTML page)
            Converters = { new JsonStringEnumConverter() } // "Waiting", not 0: readable in Notepad
        };

        private Mutex? _mutex;

        public List<QueueEntry> Entries { get; } = new();

        /// <summary>Raised after every change, for the queue window and the Job box.</summary>
        public event EventHandler? Changed;

        public bool IsOwner => _mutex != null;

        public int WaitingCount => Entries.Count(e => e.Status == QueueStatus.Waiting);

        public QueueEntry? NextWaiting() => Entries.FirstOrDefault(e => e.Status == QueueStatus.Waiting);

        /// <summary>True if a queue file with downloads exists (checked before taking the mutex at startup).</summary>
        public static bool HasSavedFile() => File.Exists(FilePath);

        /// <summary>
        /// Makes this window the owner of the queue, unless another window owns it. Loads the saved
        /// downloads (e.g. from before a power outage) after the ones this window already has.
        /// </summary>
        public bool TryTakeOwnership()
        {
            if (_mutex != null) return true;

            var mutex = new Mutex(false, MutexName);
            bool owned;
            try { owned = mutex.WaitOne(0); }
            catch (AbandonedMutexException) { owned = true; } // the owner ended without releasing it
            if (!owned)
            {
                mutex.Dispose();
                return false;
            }

            _mutex = mutex;
            Load();
            Save(); // this window's own download, if one is running
            Changed?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>Lets another window use the queue, when this one has nothing waiting or running.</summary>
        public void ReleaseOwnershipIfIdle()
        {
            if (_mutex == null || Entries.Any(e => e.IsPending)) return;
            _mutex.ReleaseMutex();
            _mutex.Dispose();
            _mutex = null;
        }

        public QueueEntry Add(DownloadJob job)
        {
            var entry = new QueueEntry { Job = job, Status = QueueStatus.Waiting };
            Entries.Add(entry);
            Update();
            return entry;
        }

        /// <summary>Removes a download that isn't running (a waiting one, or a finished one from the list).</summary>
        public void Remove(IEnumerable<QueueEntry> entries)
        {
            foreach (var entry in entries.ToList())
                if (entry.Status != QueueStatus.Running)
                    Entries.Remove(entry);
            Update();
        }

        public void RemoveWaiting() => Remove(Entries.Where(e => e.Status == QueueStatus.Waiting));

        /// <summary>Removes the finished downloads of the previous run, when a new run starts.</summary>
        public void ClearFinished()
        {
            Entries.RemoveAll(e => !e.IsPending);
            Update();
        }

        /// <summary>Saves the queue after a status change and tells the queue window.</summary>
        public void Update()
        {
            Save();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return;
                var saved = JsonSerializer.Deserialize<List<QueueEntry>>(File.ReadAllText(FilePath), JsonOptions);
                if (saved == null) return;
                foreach (var entry in saved)
                {
                    if (!entry.IsPending || Entries.Any(e => e.Id == entry.Id)) continue;
                    if (entry.Status == QueueStatus.Running)
                    {
                        entry.Status = QueueStatus.Waiting;
                        entry.WasInterrupted = true;
                    }
                    Entries.Add(entry);
                }
            }
            catch
            {
                // An unreadable file (e.g. cut off by a power outage) is ignored: it is replaced on the next save
            }
        }

        /// <summary>
        /// Saves the waiting and running downloads, or deletes the file when there are none. Writes a
        /// temporary file, flushed to disk, and then replaces the old file with it, so a power outage
        /// during the save leaves the old or the new file, never half a file.
        /// </summary>
        private void Save()
        {
            if (_mutex == null) return;
            try
            {
                var pending = Entries.Where(e => e.IsPending).ToList();
                if (pending.Count == 0)
                {
                    File.Delete(FilePath);
                    return;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                string tempPath = FilePath + ".tmp";
                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    JsonSerializer.Serialize(stream, pending, JsonOptions);
                    stream.Flush(flushToDisk: true);
                }
                File.Move(tempPath, FilePath, overwrite: true);
            }
            catch
            {
                // Saving is a safety net: a failure must never stop the download itself
            }
        }

        public void Dispose()
        {
            if (_mutex == null) return;
            _mutex.ReleaseMutex();
            _mutex.Dispose();
            _mutex = null;
        }
    }
}
