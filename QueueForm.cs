namespace FFmpegAssistant
{
    /// <summary>
    /// Tools > Download Queue (or a click in the Job box): the downloads of the queue with their status.
    /// Stays open next to the main window and follows the queue while it runs. Remove takes waiting
    /// downloads out of the queue, and finished ones off the list; the running one is cancelled with
    /// Cancel in the main window.
    /// </summary>
    internal partial class QueueForm : Form
    {
        private readonly DownloadQueue _queue;

        public QueueForm(DownloadQueue queue)
        {
            InitializeComponent();
            _queue = queue;
            _queue.Changed += Queue_Changed;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MinimumSize = Size;

            // CenterParent only works for ShowDialog: center on the main window here
            if (Owner != null)
            {
                var area = Screen.FromControl(Owner).WorkingArea;
                int x = Owner.Left + (Owner.Width - Width) / 2;
                int y = Owner.Top + (Owner.Height - Height) / 2;
                Location = new Point(Math.Clamp(x, area.Left, Math.Max(area.Left, area.Right - Width)),
                                     Math.Clamp(y, area.Top, Math.Max(area.Top, area.Bottom - Height)));
            }
            RefreshGrid();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _queue.Changed -= Queue_Changed;
            base.OnFormClosed(e);
        }

        private void Queue_Changed(object? sender, EventArgs e)
        {
            if (!IsDisposed) RefreshGrid();
        }

        private static string StatusText(QueueStatus status) => status switch
        {
            QueueStatus.Running => "Downloading",
            _ => status.ToString()
        };

        /// <summary>Fills the grid from the queue, keeping the selected rows selected.</summary>
        private void RefreshGrid()
        {
            var selected = dgvQueue.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => r.Tag).OfType<QueueEntry>().Select(q => q.Id).ToHashSet();
            int firstVisible = dgvQueue.FirstDisplayedScrollingRowIndex;

            dgvQueue.Rows.Clear();
            foreach (var entry in _queue.Entries)
            {
                int i = dgvQueue.Rows.Add(StatusText(entry.Status), entry.Job.FileName, entry.Job.Folder, entry.Job.Command);
                var row = dgvQueue.Rows[i];
                row.Tag = entry;
                if (entry.Reason != null)
                    row.Cells[colStatus.Index].ToolTipText = entry.Reason;
                row.Cells[colCommand.Index].ToolTipText = entry.Job.Command;
            }

            dgvQueue.ClearSelection();
            foreach (DataGridViewRow row in dgvQueue.Rows)
                if (row.Tag is QueueEntry q && selected.Contains(q.Id))
                    row.Selected = true;
            if (firstVisible >= 0 && firstVisible < dgvQueue.Rows.Count)
                dgvQueue.FirstDisplayedScrollingRowIndex = firstVisible;

            UpdateRemoveButton();
        }

        private List<QueueEntry> SelectedRemovable() => dgvQueue.SelectedRows.Cast<DataGridViewRow>()
            .Select(r => r.Tag).OfType<QueueEntry>()
            .Where(q => q.Status != QueueStatus.Running)
            .ToList();

        private void UpdateRemoveButton() => btnRemove.Enabled = SelectedRemovable().Count > 0;

        private void dgvQueue_SelectionChanged(object? sender, EventArgs e) => UpdateRemoveButton();

        private void btnRemove_Click(object? sender, EventArgs e)
        {
            var entries = SelectedRemovable();
            if (entries.Count == 0) return;
            _queue.Remove(entries);
        }

        private void dgvQueue_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                btnRemove_Click(sender, e);
                e.Handled = true;
            }
        }

        private void btnClose_Click(object? sender, EventArgs e) => Close();
    }
}
