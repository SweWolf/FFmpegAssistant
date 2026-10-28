using System.Diagnostics;

namespace FFmpegAssistant
{
    /// <summary>
    /// Finds the sibling apps (SweWolf Media Player, SweWolf File Explorer) and starts them with a path.
    /// The lookup follows D:\Dev\CLAUDE.md ("Finding a sibling app"): next to the running EXE, then
    /// ..\&lt;AppName&gt;\&lt;AppName&gt;.exe. Not found: null, and the caller uses the program Windows has.
    ///
    /// History:
    ///   2026-10-27  (FFmpegAssistant) New: Open File and Open Folder use the sibling apps when they are there.
    /// </summary>
    internal static class SiblingApps
    {
        internal const string MediaPlayerName = "SwewolfMediaPlayer";
        internal const string FileExplorerName = "SwewolfFileExplorer";

        internal static string? FindMediaPlayer() => Find(MediaPlayerName);

        internal static string? FindFileExplorer() => Find(FileExplorerName);

        private static string? Find(string appName)
        {
            string here = AppContext.BaseDirectory;
            string[] candidates =
            {
                Path.Combine(here, appName + ".exe"),
                Path.GetFullPath(Path.Combine(here, "..", appName, appName + ".exe")),
            };
            return candidates.FirstOrDefault(File.Exists);
        }

        /// <summary>Starts <paramref name="exe"/> with <paramref name="path"/> as its argument. False if it could not be started.</summary>
        internal static bool Start(string exe, string path)
        {
            try
            {
                var info = new ProcessStartInfo { FileName = exe, UseShellExecute = false };
                info.ArgumentList.Add(path);
                Process.Start(info);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
