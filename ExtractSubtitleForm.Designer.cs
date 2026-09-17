namespace FFmpegAssistant
{
    partial class ExtractSubtitleForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblPrompt = new Label();
            lstStreams = new ListBox();
            btnOK = new Button();
            btnCancel = new Button();
            SuspendLayout();

            lblPrompt.AutoSize = true;
            lblPrompt.Location = new Point(12, 12);
            lblPrompt.Text = "Select a subtitle stream to extract:";

            lstStreams.FormattingEnabled = true;
            lstStreams.Location = new Point(12, 32);
            lstStreams.Size = new Size(460, 130);
            lstStreams.DoubleClick += lstStreams_DoubleClick;

            btnOK.Location = new Point(316, 174);
            btnOK.Size = new Size(75, 23);
            btnOK.Text = "OK";
            btnOK.Click += btnOK_Click;

            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(397, 174);
            btnCancel.Size = new Size(75, 23);
            btnCancel.Text = "Cancel";

            AcceptButton = btnOK;
            CancelButton = btnCancel;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(484, 209);
            Controls.Add(lblPrompt);
            Controls.Add(lstStreams);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Extract Subtitle File";
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblPrompt;
        private ListBox lstStreams;
        private Button btnOK;
        private Button btnCancel;
    }
}
