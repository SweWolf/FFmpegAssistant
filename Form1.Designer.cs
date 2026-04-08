namespace FFmpegAssistant
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtOriginalCommand = new TextBox();
            cboFolder = new ComboBox();
            btnBrowseForFolder = new Button();
            txtFileName = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            btnRun = new Button();
            SuspendLayout();
            // 
            // txtOriginalCommand
            // 
            txtOriginalCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtOriginalCommand.Font = new Font("Segoe UI", 12F);
            txtOriginalCommand.Location = new Point(12, 57);
            txtOriginalCommand.Name = "txtOriginalCommand";
            txtOriginalCommand.Size = new Size(765, 29);
            txtOriginalCommand.TabIndex = 0;
            txtOriginalCommand.Text = "txtOriginalCommand";
            // 
            // cboFolder
            // 
            cboFolder.Font = new Font("Segoe UI", 12F);
            cboFolder.FormattingEnabled = true;
            cboFolder.Location = new Point(12, 145);
            cboFolder.Name = "cboFolder";
            cboFolder.Size = new Size(623, 29);
            cboFolder.TabIndex = 1;
            // 
            // btnBrowseForFolder
            // 
            btnBrowseForFolder.Location = new Point(652, 141);
            btnBrowseForFolder.Name = "btnBrowseForFolder";
            btnBrowseForFolder.Size = new Size(43, 36);
            btnBrowseForFolder.TabIndex = 2;
            btnBrowseForFolder.Text = "...";
            btnBrowseForFolder.UseVisualStyleBackColor = true;
            // 
            // txtFileName
            // 
            txtFileName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFileName.Font = new Font("Segoe UI", 12F);
            txtFileName.Location = new Point(12, 240);
            txtFileName.Name = "txtFileName";
            txtFileName.Size = new Size(623, 29);
            txtFileName.TabIndex = 3;
            txtFileName.Text = "txtOriginalCommand";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F);
            label1.Location = new Point(12, 33);
            label1.Name = "label1";
            label1.Size = new Size(143, 21);
            label1.TabIndex = 4;
            label1.Text = "Original Command";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F);
            label2.Location = new Point(12, 121);
            label2.Name = "label2";
            label2.Size = new Size(54, 21);
            label2.TabIndex = 5;
            label2.Text = "Folder";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F);
            label3.Location = new Point(12, 216);
            label3.Name = "label3";
            label3.Size = new Size(80, 21);
            label3.TabIndex = 5;
            label3.Text = "File Name";
            // 
            // btnRun
            // 
            btnRun.Location = new Point(12, 310);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(153, 36);
            btnRun.TabIndex = 2;
            btnRun.Text = "Run";
            btnRun.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            AcceptButton = btnRun;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtFileName);
            Controls.Add(btnRun);
            Controls.Add(btnBrowseForFolder);
            Controls.Add(cboFolder);
            Controls.Add(txtOriginalCommand);
            Name = "Form1";
            Text = "FFmpeg Assistant";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtOriginalCommand;
        private ComboBox cboFolder;
        private Button btnBrowseForFolder;
        private TextBox txtFileName;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button btnRun;
    }
}
