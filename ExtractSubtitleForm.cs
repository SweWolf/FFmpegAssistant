namespace FFmpegAssistant
{
    public partial class ExtractSubtitleForm : Form
    {
        public SubtitleStream? SelectedStream { get; private set; }

        public ExtractSubtitleForm(List<SubtitleStream> streams)
        {
            InitializeComponent();
            foreach (var s in streams)
                lstStreams.Items.Add(s);
            if (lstStreams.Items.Count > 0)
                lstStreams.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (lstStreams.SelectedItem is SubtitleStream s)
            {
                SelectedStream = s;
                DialogResult = DialogResult.OK;
            }
        }

        private void lstStreams_DoubleClick(object sender, EventArgs e) => btnOK_Click(sender, e);
    }

    public record SubtitleStream(int StreamIndex, string Codec, string Language, string Title,
                                 bool Forced = false, bool HearingImpaired = false)
    {
        public override string ToString()
        {
            var parts = new List<string> { $"Stream #{StreamIndex}" };
            if (!string.IsNullOrEmpty(Language)) parts.Add(Language.ToUpperInvariant());
            if (!string.IsNullOrEmpty(Title)) parts.Add(Title);
            if (Forced) parts.Add("Forced");
            if (HearingImpaired) parts.Add("SDH");
            parts.Add($"[{Codec}]");
            return string.Join(" — ", parts);
        }

        public string SuggestedExtension => Codec.ToLowerInvariant() switch
        {
            "subrip" or "mov_text" => ".srt",
            "ass" or "ssa" => ".ass",
            "webvtt" => ".vtt",
            "hdmv_pgs_subtitle" => ".sup",
            "dvd_subtitle" => ".sub",
            _ => ".srt"
        };

        /// <summary>
        /// True for text subtitles, which FFmpeg can convert to SRT. False for image subtitles
        /// (PGS from Blu-ray, VobSub from DVD, DVB), which would need OCR.
        /// </summary>
        public bool IsText => Codec.ToLowerInvariant() is not
            ("hdmv_pgs_subtitle" or "dvd_subtitle" or "dvb_subtitle" or "xsub" or "dvb_teletext");
    }
}
