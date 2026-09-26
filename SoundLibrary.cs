using System.Globalization;
using System.Media;
using System.Reflection;

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
    /// Discovers and plays the .wav files embedded in the executable (Resources\*.wav in the project,
    /// embedded as "Sounds/&lt;file name&gt;"). They are embedded rather than copied next to the exe,
    /// because the single-file release ships only the exe. A bundled sound is identified by its file
    /// name; a custom sound by its full path.
    /// </summary>
    internal static class SoundLibrary
    {
        /// <summary>
        /// Sound used when the user hasn't made an active choice yet.
        /// </summary>
        public const string DefaultSoundFileName = "universfield-simple-notification-152054.wav";

        private const string ResourcePrefix = "Sounds/"; // matches the LogicalName in FFmpegAssistant.csproj

        private static readonly Assembly ThisAssembly = typeof(SoundLibrary).Assembly;

        // SoundPlayer plays asynchronously from memory: keep the player referenced until the next sound,
        // so it isn't garbage collected (and cut off) while playing.
        private static SoundPlayer? _currentPlayer;

        /// <summary>
        /// Lists every embedded .wav file, with a human-friendly display name derived
        /// from the file name (dashes → spaces, proper case, trailing id number dropped).
        /// </summary>
        public static List<SoundOption> GetAvailableSounds()
        {
            var options = new List<SoundOption>();
            try
            {
                foreach (string resourceName in ThisAssembly.GetManifestResourceNames())
                {
                    if (!resourceName.StartsWith(ResourcePrefix, StringComparison.Ordinal) ||
                        !resourceName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                        continue;
                    string fileName = resourceName[ResourcePrefix.Length..];
                    options.Add(new SoundOption(FormatDisplayName(fileName), fileName));
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
        /// Plays a sound given either a bundled sound's file name or a full path to a custom
        /// .wav file elsewhere. So that something is always heard, it falls back to
        /// <see cref="DefaultSoundFileName"/>, then to any other bundled sound, and last to the
        /// Windows system sound, when a sound is missing or can't be played (e.g. a custom file that
        /// was deleted, or isn't a valid .wav).
        /// </summary>
        public static void Play(string fileNameOrPath)
        {
            if (TryPlay(fileNameOrPath) || TryPlay(DefaultSoundFileName)) return;
            foreach (var sound in GetAvailableSounds())
                if (TryPlay(sound.FileName)) return;
            try { SystemSounds.Asterisk.Play(); } catch { /* never crash the host app */ }
        }

        private static bool TryPlay(string fileNameOrPath)
        {
            SoundPlayer? player = null;
            try
            {
                player = CreatePlayer(fileNameOrPath);
                if (player == null) return false;
                player.Load(); // reads the sound now, so a missing or unreadable file fails here
                player.Play(); // asynchronous; throws if the file isn't a valid .wav
                _currentPlayer?.Dispose();
                _currentPlayer = player;
                return true;
            }
            catch
            {
                player?.Dispose();
                return false;
            }
        }

        private static SoundPlayer? CreatePlayer(string fileNameOrPath)
        {
            if (string.IsNullOrEmpty(fileNameOrPath)) return null;

            if (Path.IsPathRooted(fileNameOrPath))
                return File.Exists(fileNameOrPath) ? new SoundPlayer(fileNameOrPath) : null;

            Stream? stream = ThisAssembly.GetManifestResourceStream(ResourcePrefix + fileNameOrPath);
            return stream == null ? null : new SoundPlayer(stream);
        }
    }
}
