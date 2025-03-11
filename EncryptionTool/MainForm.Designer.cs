namespace EncryptionTool
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // Disposes resources
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            txtFilePath = new TextBox();
            btnBrowse = new Button();
            rdoEncrypt = new RadioButton();
            txtKey = new TextBox();
            btnStartCancel = new Button();
            label4 = new Label();
            progressBar1 = new CustomProgressBar();
            label1 = new Label();
            rdoDecrypt = new RadioButton();
            label2 = new Label();
            lblStatus = new Label();
            chkShowKey = new CheckBox();
            pnlContainer = new Panel();
            pnlContainer.SuspendLayout();
            SuspendLayout();
            // 
            // txtFilePath
            // 
            txtFilePath.Location = new Point(112, 42);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new Size(450, 23);
            txtFilePath.TabIndex = 0;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(568, 42);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(75, 23);
            btnBrowse.TabIndex = 1;
            btnBrowse.Text = "Browse";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // rdoEncrypt
            // 
            rdoEncrypt.AutoSize = true;
            rdoEncrypt.Location = new Point(112, 71);
            rdoEncrypt.Name = "rdoEncrypt";
            rdoEncrypt.Size = new Size(65, 19);
            rdoEncrypt.TabIndex = 2;
            rdoEncrypt.TabStop = true;
            rdoEncrypt.Text = "Encrypt";
            rdoEncrypt.UseVisualStyleBackColor = true;
            // 
            // txtKey
            // 
            txtKey.Location = new Point(112, 96);
            txtKey.Name = "txtKey";
            txtKey.Size = new Size(450, 23);
            txtKey.TabIndex = 3;
            txtKey.UseSystemPasswordChar = true;
            // 
            // btnStartCancel
            // 
            btnStartCancel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnStartCancel.Location = new Point(112, 125);
            btnStartCancel.Name = "btnStartCancel";
            btnStartCancel.Size = new Size(75, 23);
            btnStartCancel.TabIndex = 4;
            btnStartCancel.Text = "Start";
            btnStartCancel.UseVisualStyleBackColor = true;
            btnStartCancel.Click += btnStartCancel_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label4.Location = new Point(61, 180);
            label4.Name = "label4";
            label4.Size = new Size(45, 15);
            label4.TabIndex = 6;
            label4.Text = "Status:";
            // 
            // progressBar1
            // 
            progressBar1.IsIdle = true;
            progressBar1.Location = new Point(112, 154);
            progressBar1.Name = "progressBar1";
            progressBar1.ProgressText = "";
            progressBar1.Size = new Size(450, 23);
            progressBar1.TabIndex = 7;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label1.Location = new Point(47, 46);
            label1.Name = "label1";
            label1.Size = new Size(59, 15);
            label1.TabIndex = 8;
            label1.Text = "File-path:";
            // 
            // rdoDecrypt
            // 
            rdoDecrypt.AutoSize = true;
            rdoDecrypt.Location = new Point(183, 71);
            rdoDecrypt.Name = "rdoDecrypt";
            rdoDecrypt.Size = new Size(66, 19);
            rdoDecrypt.TabIndex = 9;
            rdoDecrypt.TabStop = true;
            rdoDecrypt.Text = "Decrypt";
            rdoDecrypt.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.Location = new Point(36, 104);
            label2.Name = "label2";
            label2.Size = new Size(70, 15);
            label2.TabIndex = 10;
            label2.Text = "Secret key:";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(112, 180);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(26, 15);
            lblStatus.TabIndex = 11;
            lblStatus.Text = "Idle";
            // 
            // chkShowKey
            // 
            chkShowKey.AutoSize = true;
            chkShowKey.Location = new Point(568, 103);
            chkShowKey.Name = "chkShowKey";
            chkShowKey.Size = new Size(91, 19);
            chkShowKey.TabIndex = 12;
            chkShowKey.Text = "Show / Hide";
            chkShowKey.UseVisualStyleBackColor = true;
            chkShowKey.CheckedChanged += chkShowKey_CheckedChanged;
            // 
            // pnlContainer
            // 
            pnlContainer.Controls.Add(label1);
            pnlContainer.Controls.Add(chkShowKey);
            pnlContainer.Controls.Add(txtFilePath);
            pnlContainer.Controls.Add(lblStatus);
            pnlContainer.Controls.Add(btnBrowse);
            pnlContainer.Controls.Add(label2);
            pnlContainer.Controls.Add(rdoEncrypt);
            pnlContainer.Controls.Add(rdoDecrypt);
            pnlContainer.Controls.Add(txtKey);
            pnlContainer.Controls.Add(btnStartCancel);
            pnlContainer.Controls.Add(progressBar1);
            pnlContainer.Controls.Add(label4);
            pnlContainer.Location = new Point(12, 12);
            pnlContainer.Name = "pnlContainer";
            pnlContainer.Size = new Size(688, 247);
            pnlContainer.TabIndex = 13;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(730, 286);
            Controls.Add(pnlContainer);
            Name = "MainForm";
            Text = "File Encryption / Decryption";
            pnlContainer.ResumeLayout(false);
            pnlContainer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.RadioButton rdoEncrypt;
        private System.Windows.Forms.TextBox txtKey;
        private System.Windows.Forms.Button btnStartCancel;
        private System.Windows.Forms.Label label4;
        private EncryptionTool.CustomProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton rdoDecrypt;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.CheckBox chkShowKey;
        private Panel pnlContainer;
    }
}
