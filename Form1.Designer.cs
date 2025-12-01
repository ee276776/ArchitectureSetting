namespace ArchitectureSetting
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
            labelSolutionFolder = new Label();
            textBoxSolutionPath = new TextBox();
            buttonBrowseFolder = new Button();
            labelPlatform = new Label();
            comboBoxPlatform = new ComboBox();
            buttonBatchProcess = new Button();
            labelLog = new Label();
            textBoxLog = new TextBox();
            folderBrowserDialog = new FolderBrowserDialog();
            SuspendLayout();
            // 
            // labelSolutionFolder
            // 
            labelSolutionFolder.AutoSize = true;
            labelSolutionFolder.Location = new Point(12, 15);
            labelSolutionFolder.Name = "labelSolutionFolder";
            labelSolutionFolder.Size = new Size(94, 15);
            labelSolutionFolder.TabIndex = 0;
            labelSolutionFolder.Text = "方案資料夾路徑:";
            // 
            // textBoxSolutionPath
            // 
            textBoxSolutionPath.Location = new Point(12, 33);
            textBoxSolutionPath.Name = "textBoxSolutionPath";
            textBoxSolutionPath.Size = new Size(650, 23);
            textBoxSolutionPath.TabIndex = 1;
            // 
            // buttonBrowseFolder
            // 
            buttonBrowseFolder.Location = new Point(668, 33);
            buttonBrowseFolder.Name = "buttonBrowseFolder";
            buttonBrowseFolder.Size = new Size(75, 23);
            buttonBrowseFolder.TabIndex = 2;
            buttonBrowseFolder.Text = "瀏覽";
            buttonBrowseFolder.UseVisualStyleBackColor = true;
            buttonBrowseFolder.Click += buttonBrowseFolder_Click;
            // 
            // labelPlatform
            // 
            labelPlatform.AutoSize = true;
            labelPlatform.Location = new Point(12, 75);
            labelPlatform.Name = "labelPlatform";
            labelPlatform.Size = new Size(58, 15);
            labelPlatform.TabIndex = 3;
            labelPlatform.Text = "目標平台:";
            // 
            // comboBoxPlatform
            // 
            comboBoxPlatform.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxPlatform.FormattingEnabled = true;
            comboBoxPlatform.Items.AddRange(new object[] { "x64", "x86", "ARM64" });
            comboBoxPlatform.Location = new Point(12, 93);
            comboBoxPlatform.Name = "comboBoxPlatform";
            comboBoxPlatform.Size = new Size(150, 23);
            comboBoxPlatform.TabIndex = 4;
            // 
            // buttonBatchProcess
            // 
            buttonBatchProcess.Location = new Point(12, 135);
            buttonBatchProcess.Name = "buttonBatchProcess";
            buttonBatchProcess.Size = new Size(95, 30);
            buttonBatchProcess.TabIndex = 5;
            buttonBatchProcess.Text = "新增平台設定";
            buttonBatchProcess.UseVisualStyleBackColor = true;
            buttonBatchProcess.Click += buttonBatchProcess_Click;
            // 
            // labelLog
            // 
            labelLog.AutoSize = true;
            labelLog.Location = new Point(12, 180);
            labelLog.Name = "labelLog";
            labelLog.Size = new Size(58, 15);
            labelLog.TabIndex = 6;
            labelLog.Text = "處理日誌:";
            // 
            // textBoxLog
            // 
            textBoxLog.Location = new Point(12, 198);
            textBoxLog.Multiline = true;
            textBoxLog.Name = "textBoxLog";
            textBoxLog.ReadOnly = true;
            textBoxLog.ScrollBars = ScrollBars.Both;
            textBoxLog.Size = new Size(731, 240);
            textBoxLog.TabIndex = 7;
            // 
            // folderBrowserDialog
            // 
            folderBrowserDialog.Description = "請選擇方案資料夾 (包含 .sln 檔案的資料夾)";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(760, 450);
            Controls.Add(textBoxLog);
            Controls.Add(labelLog);
            Controls.Add(buttonBatchProcess);
            Controls.Add(comboBoxPlatform);
            Controls.Add(labelPlatform);
            Controls.Add(buttonBrowseFolder);
            Controls.Add(textBoxSolutionPath);
            Controls.Add(labelSolutionFolder);
            Name = "Form1";
            Text = "架構設定工具";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private Label labelSolutionFolder;
        private TextBox textBoxSolutionPath;
        private Button buttonBrowseFolder;
        private Label labelPlatform;
        private ComboBox comboBoxPlatform;
        private Button buttonBatchProcess;
        private Label labelLog;
        private TextBox textBoxLog;
        private FolderBrowserDialog folderBrowserDialog;
    }
}