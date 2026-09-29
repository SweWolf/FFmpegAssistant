namespace FFmpegAssistant
{
    partial class QueueForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            dgvQueue = new DataGridView();
            colStatus = new DataGridViewTextBoxColumn();
            colFileName = new DataGridViewTextBoxColumn();
            colFolder = new DataGridViewTextBoxColumn();
            colCommand = new DataGridViewTextBoxColumn();
            btnRemove = new Button();
            btnClose = new Button();
            toolTip1 = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)dgvQueue).BeginInit();
            SuspendLayout();
            //
            // dgvQueue
            //
            dgvQueue.AllowUserToAddRows = false;
            dgvQueue.AllowUserToDeleteRows = false;
            dgvQueue.AllowUserToResizeRows = false;
            dgvQueue.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvQueue.BackgroundColor = SystemColors.Window;
            dgvQueue.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvQueue.Columns.AddRange(new DataGridViewColumn[] { colStatus, colFileName, colFolder, colCommand });
            dgvQueue.Location = new Point(12, 12);
            dgvQueue.Name = "dgvQueue";
            dgvQueue.ReadOnly = true;
            dgvQueue.RowHeadersVisible = false;
            dgvQueue.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvQueue.ShowCellToolTips = true;
            dgvQueue.Size = new Size(876, 330);
            dgvQueue.TabIndex = 0;
            dgvQueue.SelectionChanged += dgvQueue_SelectionChanged;
            dgvQueue.KeyDown += dgvQueue_KeyDown;
            //
            // colStatus
            //
            colStatus.HeaderText = "Status";
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            colStatus.SortMode = DataGridViewColumnSortMode.NotSortable;
            colStatus.Width = 100;
            //
            // colFileName
            //
            colFileName.HeaderText = "File Name";
            colFileName.Name = "colFileName";
            colFileName.ReadOnly = true;
            colFileName.SortMode = DataGridViewColumnSortMode.NotSortable;
            colFileName.Width = 220;
            //
            // colFolder
            //
            colFolder.HeaderText = "Folder";
            colFolder.Name = "colFolder";
            colFolder.ReadOnly = true;
            colFolder.SortMode = DataGridViewColumnSortMode.NotSortable;
            colFolder.Width = 220;
            //
            // colCommand
            //
            colCommand.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colCommand.HeaderText = "Command";
            colCommand.Name = "colCommand";
            colCommand.ReadOnly = true;
            colCommand.SortMode = DataGridViewColumnSortMode.NotSortable;
            //
            // btnRemove
            //
            btnRemove.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnRemove.Location = new Point(702, 352);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(90, 28);
            btnRemove.TabIndex = 1;
            btnRemove.Text = "Remove";
            toolTip1.SetToolTip(btnRemove, "Remove the selected downloads from the queue (not the running one: use Cancel for that)");
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            //
            // btnClose
            //
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(798, 352);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(90, 28);
            btnClose.TabIndex = 2;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            //
            // QueueForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(900, 392);
            Controls.Add(btnClose);
            Controls.Add(btnRemove);
            Controls.Add(dgvQueue);
            MinimizeBox = false;
            Name = "QueueForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Download Queue";
            ((System.ComponentModel.ISupportInitialize)dgvQueue).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvQueue;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewTextBoxColumn colFileName;
        private DataGridViewTextBoxColumn colFolder;
        private DataGridViewTextBoxColumn colCommand;
        private Button btnRemove;
        private Button btnClose;
        private ToolTip toolTip1;
    }
}
