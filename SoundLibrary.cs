using System.Globalization;
using System.Media;

namespace FFmpegAssistant
{
    /// <summary>
    /// Represents one selectable notification sound in the "Play a Sound" dropdown.
    /// </summary>
    internal sealed class SoundOption
    {
        public string Name { get; }
        public string FileName { get; }

        public SoundOption(string name, string fileName)
        {
            Name = name;
            FileName = fileName;
        }

        public override string ToString() => Name;
    }

    /// <summary>
    /// Discovers and plays the .wav files shipped in the Resources folder next to the executable.
    /// </summary>
    internal static class SoundLibrary
    {
        /// <summary>
        /// Sound used when the user hasn't made an active choice yet.
        /// </summary>
        public const string DefaultSoundFileName = "universfield-simple-notification-152054.wav";

        private static string ResourcesDirectory => Path.Combine(AppContext.BaseDirectory, "Resources");

        /// <summary>
        /// Lists every .wav file in Resources, with a human-friendly display name derived
        /// from the file name (dashes → spaces, proper case, trailing id number dropped).
        /// </summary>
        public static List<SoundOption> GetAvailableSounds()
        {
            var options = new List<SoundOption>();
            try
            {
                if (Directory.Exists(ResourcesDirectory))
                {
                    foreach (string file in Directory.GetFiles(ResourcesDirectory, "*.wav"))
                    {
                        string fileName = Path.GetFileName(file);
                        options.Add(new SoundOption(FormatDisplayName(fileName), fileName));
                    }
                }
            }
            catch { /* never crash the host app */ }

            options.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            return options;
        }

        public static string FormatDisplayName(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            string[] words = name.Split('-', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1)
                words = words[..^1]; // drop the trailing resource-id number

            var textInfo = CultureInfo.InvariantCulture.TextInfo;
            return string.Join(" ", words.Select(w => textInfo.ToTitleCase(w.ToLowerInvariant())));
        }

        /// <summary>
        /// Plays a sound given either a bundled Resources file name or a full path to a
        /// custom .wav file elsewhere. Falls back to <see cref="DefaultSoundFileName"/> (or the
        /// first available bundled sound, if that one is missing) when unresolvable.
        /// </summary>
        public static void Play(string fileNameOrPath)
        {
            try
            {
                string? path = ResolvePath(fileNameOrPath) ?? ResolvePath(DefaultSoundFileName);
                if (path == null)
                {
                    var sounds = GetAvailableSounds();
                    if (sounds.Count == 0) return;
                    path = Path.Combine(ResourcesDirectory, sounds[0].FileName);
                }

                var player = new SoundPlayer(path);
                player.Play(); // asynchronous — do not dispose immediately or playback gets cut off
            }
            catch { /* never crash the host app */ }
        }

        private static string? ResolvePath(string fileNameOrPath)
        {
            if (string.IsNullOrEmpty(fileNameOrPath)) return null;
            string path = Path.IsPathRooted(fileNameOrPath)
                ? fileNameOrPath
                : Path.Combine(ResourcesDirectory, fileNameOrPath);
            return File.Exists(path) ? path : null;
        }
    }
}
