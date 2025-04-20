namespace GoodsDefImport
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
            btnSelectFile = new Button();
            lbDataFile = new Label();
            btnImport = new Button();
            progressBar1 = new ProgressBar();
            lbProgress = new Label();
            lbStatus = new Label();
            SuspendLayout();
            // 
            // btnSelectFile
            // 
            btnSelectFile.Location = new Point(12, 12);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.Size = new Size(94, 29);
            btnSelectFile.TabIndex = 0;
            btnSelectFile.Text = "Data file";
            btnSelectFile.UseVisualStyleBackColor = true;
            btnSelectFile.Click += btnSelectFile_Click;
            // 
            // lbDataFile
            // 
            lbDataFile.AutoSize = true;
            lbDataFile.Location = new Point(112, 16);
            lbDataFile.Name = "lbDataFile";
            lbDataFile.Size = new Size(50, 20);
            lbDataFile.TabIndex = 1;
            lbDataFile.Text = "label1";
            // 
            // btnImport
            // 
            btnImport.Location = new Point(295, 82);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(94, 29);
            btnImport.TabIndex = 2;
            btnImport.Text = "Import";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(12, 47);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(692, 29);
            progressBar1.TabIndex = 3;
            // 
            // lbProgress
            // 
            lbProgress.AutoSize = true;
            lbProgress.Location = new Point(395, 86);
            lbProgress.Name = "lbProgress";
            lbProgress.Size = new Size(50, 20);
            lbProgress.TabIndex = 4;
            lbProgress.Text = "label1";
            // 
            // lbStatus
            // 
            lbStatus.AutoSize = true;
            lbStatus.Location = new Point(12, 86);
            lbStatus.Name = "lbStatus";
            lbStatus.Size = new Size(50, 20);
            lbStatus.TabIndex = 5;
            lbStatus.Text = "label1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(716, 119);
            Controls.Add(lbStatus);
            Controls.Add(lbProgress);
            Controls.Add(progressBar1);
            Controls.Add(btnImport);
            Controls.Add(lbDataFile);
            Controls.Add(btnSelectFile);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSelectFile;
        private Label lbDataFile;
        private Button btnImport;
        private ProgressBar progressBar1;
        private Label lbProgress;
        private Label lbStatus;
    }
}
