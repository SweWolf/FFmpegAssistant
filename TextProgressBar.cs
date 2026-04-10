namespace FFmpegAssistant
{
    /// <summary>
    /// A ProgressBar that renders a percentage label centred inside the bar.
    /// </summary>
    public class TextProgressBar : ProgressBar
    {
        public TextProgressBar()
        {
            // Required so our OnPaint is called instead of the native renderer
            SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect    = ClientRectangle;
            Graphics  g       = e.Graphics;
            float     percent = Maximum > 0 ? (float)Value / Maximum : 0f;

            // Background
            using (var back = new SolidBrush(Color.FromArgb(225, 225, 225)))
                g.FillRectangle(back, rect);

            // Filled portion
            if (percent > 0)
            {
                var fill = new Rectangle(rect.X, rect.Y, (int)(rect.Width * percent), rect.Height);
                using var brush = new SolidBrush(Color.FromArgb(6, 176, 37));
                g.FillRectangle(brush, fill);
            }

            // Border
            ControlPaint.DrawBorder(g, rect, Color.FromArgb(180, 180, 180), ButtonBorderStyle.Solid);

            // Percentage text — drawn twice for a readable outline effect
            string text     = $"{percent * 100:0}%";
            using var font  = new Font("Segoe UI", 9F, FontStyle.Bold);
            SizeF textSize  = g.MeasureString(text, font);
            float tx        = (rect.Width  - textSize.Width)  / 2f;
            float ty        = (rect.Height - textSize.Height) / 2f;

            // White outline (draw at offsets)
            using (var outline = new SolidBrush(Color.FromArgb(200, Color.White)))
                foreach (var offset in new[] { (-1,-1),(1,-1),(-1,1),(1,1) })
                    g.DrawString(text, font, outline, tx + offset.Item1, ty + offset.Item2);

            // Dark text on top
            g.DrawString(text, font, Brushes.Black, tx, ty);
        }
    }
}
