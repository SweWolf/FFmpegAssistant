using System.Diagnostics;

namespace FFmpegAssistant
{
    /// <summary>
    /// Opens the app's help: Help\help.html next to the EXE, in the default browser.
    /// help.html is built from docs\help.md with docs\build-help.cmd and copied to the output
    /// folder by the .csproj (Content item with Link="Help\help.html").
    ///
    /// Planned: open the online help on GitHub Pages first, and keep a copy of it that is
    /// downloaded in the background, for when there is no internet.
    ///
    /// Shared with the sibling apps by copying this whole file. When copying, change only the
    /// namespace. When changing this file, add a line to the history below, so the copies in
    /// the other apps can be compared with it.
    ///
    /// History:
    ///   2026-09-27  Created: opens Help\help.html next to the EXE (Help > [App] Help, F1).
    /// </summary>
    internal static class HelpFiles
    {
        internal static string LocalHelpFile =>
            Path.Combine(AppContext.BaseDirectory, "Help", "help.html");

        /// <summary>Opens the help in the default browser. <paramref name="caption"/> is the message box caption.</summary>
        internal static void OpenHelp(string caption)
        {
            string file = LocalHelpFile;
            if (!File.Exists(file))
            {
                MessageBox.Show($"The help file was not found:\n{file}",
                    caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(file) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open the help file:\n{file}\n\n{ex.Message}",
                    caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
