using System.Drawing;
using System.Windows.Forms;

namespace DigicertCodeSign
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblTitle;
        private Label lblStatus;
        private Label lblHint;
        private Button btnLogin;
        private Button btnSign;
        private Button btnConfig;
        private Button btnSignFiles;
        private Button btnSafeNet;
        private Button btnExit;
        private Panel panelTop;
        private Label lblHistory;
        private ListView lvHistory;
        private ColumnHeader colFile;
        private ColumnHeader colLastSigned;
        private ColumnHeader colResult;
        private ColumnHeader colFolder;
        private Button btnResign;
        private Button btnOpenFolder;
        private Button btnRemoveHistory;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTop = new Panel();
            this.lblTitle = new Label();
            this.lblStatus = new Label();
            this.lblHint = new Label();
            this.btnLogin = new Button();
            this.btnSign = new Button();
            this.btnConfig = new Button();
            this.btnSignFiles = new Button();
            this.btnSafeNet = new Button();
            this.btnExit = new Button();
            this.lblHistory = new Label();
            this.lvHistory = new ListView();
            this.colFile = new ColumnHeader();
            this.colLastSigned = new ColumnHeader();
            this.colResult = new ColumnHeader();
            this.colFolder = new ColumnHeader();
            this.btnResign = new Button();
            this.btnOpenFolder = new Button();
            this.btnRemoveHistory = new Button();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            //
            // panelTop
            //
            this.panelTop.BackColor = Color.FromArgb(0, 82, 147);
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Dock = DockStyle.Top;
            this.panelTop.Location = new Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new Size(520, 64);
            this.panelTop.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.Location = new Point(16, 18);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(320, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "DigiCert Hardware Codesign Login";
            //
            // lblStatus
            //
            this.lblStatus.Font = new Font("Segoe UI", 9.75F);
            this.lblStatus.Location = new Point(24, 84);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(472, 24);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "Status";
            //
            // lblHint
            //
            this.lblHint.Font = new Font("Segoe UI", 9F);
            this.lblHint.ForeColor = Color.DimGray;
            this.lblHint.Location = new Point(24, 120);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new Size(472, 56);
            this.lblHint.TabIndex = 2;
            this.lblHint.Text = "Use \"Sign files...\" (or drag files here) to sign files.\r\nThe SafeNet Token Logon dialog is filled in automatically.";
            //
            // btnLogin
            //
            this.btnLogin.BackColor = Color.FromArgb(0, 120, 215);
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnLogin.ForeColor = Color.White;
            this.btnLogin.Location = new Point(24, 192);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new Size(150, 40);
            this.btnLogin.TabIndex = 3;
            this.btnLogin.Text = "Login (auto-fill)";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            //
            // btnSign
            //
            this.btnSign.BackColor = Color.FromArgb(16, 124, 16);
            this.btnSign.FlatStyle = FlatStyle.Flat;
            this.btnSign.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnSign.ForeColor = Color.White;
            this.btnSign.Location = new Point(186, 192);
            this.btnSign.Name = "btnSign";
            this.btnSign.Size = new Size(150, 40);
            this.btnSign.TabIndex = 4;
            this.btnSign.Text = "Sign now";
            this.btnSign.UseVisualStyleBackColor = false;
            this.btnSign.Click += new System.EventHandler(this.btnSign_Click);
            //
            // btnConfig
            //
            this.btnConfig.Font = new Font("Segoe UI", 10F);
            this.btnConfig.Location = new Point(348, 192);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new Size(148, 40);
            this.btnConfig.TabIndex = 5;
            this.btnConfig.Text = "Configuration";
            this.btnConfig.UseVisualStyleBackColor = true;
            this.btnConfig.Click += new System.EventHandler(this.btnConfig_Click);
            //
            // btnSignFiles
            //
            this.btnSignFiles.BackColor = Color.FromArgb(16, 124, 16);
            this.btnSignFiles.FlatStyle = FlatStyle.Flat;
            this.btnSignFiles.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnSignFiles.ForeColor = Color.White;
            this.btnSignFiles.Location = new Point(24, 244);
            this.btnSignFiles.Name = "btnSignFiles";
            this.btnSignFiles.Size = new Size(150, 40);
            this.btnSignFiles.TabIndex = 6;
            this.btnSignFiles.Text = "Sign files...";
            this.btnSignFiles.UseVisualStyleBackColor = false;
            this.btnSignFiles.Click += new System.EventHandler(this.btnSignFiles_Click);
            //
            // btnSafeNet
            //
            this.btnSafeNet.Font = new Font("Segoe UI", 10F);
            this.btnSafeNet.Location = new Point(186, 244);
            this.btnSafeNet.Name = "btnSafeNet";
            this.btnSafeNet.Size = new Size(150, 40);
            this.btnSafeNet.TabIndex = 7;
            this.btnSafeNet.Text = "SafeNet Tools";
            this.btnSafeNet.UseVisualStyleBackColor = true;
            this.btnSafeNet.Click += new System.EventHandler(this.btnSafeNet_Click);
            //
            // btnExit
            //
            this.btnExit.Font = new Font("Segoe UI", 10F);
            this.btnExit.Location = new Point(348, 244);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new Size(148, 40);
            this.btnExit.TabIndex = 8;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            //
            // lblHistory
            //
            this.lblHistory.AutoSize = true;
            this.lblHistory.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            this.lblHistory.Location = new Point(21, 302);
            this.lblHistory.Name = "lblHistory";
            this.lblHistory.TabIndex = 9;
            this.lblHistory.Text = "Signed files history  (double-click to re-sign)";
            //
            // lvHistory
            //
            this.lvHistory.CheckBoxes = true;
            this.lvHistory.Columns.AddRange(new ColumnHeader[] {
                this.colFile,
                this.colLastSigned,
                this.colResult,
                this.colFolder});
            this.lvHistory.Font = new Font("Segoe UI", 9F);
            this.lvHistory.FullRowSelect = true;
            this.lvHistory.HideSelection = false;
            this.lvHistory.Location = new Point(24, 326);
            this.lvHistory.Name = "lvHistory";
            this.lvHistory.ShowItemToolTips = true;
            this.lvHistory.Size = new Size(472, 210);
            this.lvHistory.TabIndex = 10;
            this.lvHistory.UseCompatibleStateImageBehavior = false;
            this.lvHistory.View = View.Details;
            this.lvHistory.ItemCheck += new ItemCheckEventHandler(this.lvHistory_ItemCheck);
            this.lvHistory.ItemChecked += new ItemCheckedEventHandler(this.lvHistory_ItemChecked);
            this.lvHistory.MouseDown += new MouseEventHandler(this.lvHistory_MouseDown);
            this.lvHistory.MouseUp += new MouseEventHandler(this.lvHistory_MouseUp);
            this.lvHistory.SelectedIndexChanged += new System.EventHandler(this.lvHistory_SelectedIndexChanged);
            this.lvHistory.DoubleClick += new System.EventHandler(this.lvHistory_DoubleClick);
            this.lvHistory.KeyDown += new KeyEventHandler(this.lvHistory_KeyDown);
            //
            // colFile
            //
            this.colFile.Text = "File";
            this.colFile.Width = 150;
            //
            // colLastSigned
            //
            this.colLastSigned.Text = "Last signed";
            this.colLastSigned.Width = 110;
            //
            // colResult
            //
            this.colResult.Text = "Result";
            this.colResult.Width = 60;
            //
            // colFolder
            //
            this.colFolder.Text = "Folder";
            this.colFolder.Width = 400;
            //
            // btnResign
            //
            this.btnResign.BackColor = Color.FromArgb(16, 124, 16);
            this.btnResign.FlatStyle = FlatStyle.Flat;
            this.btnResign.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnResign.ForeColor = Color.White;
            this.btnResign.Location = new Point(24, 548);
            this.btnResign.Name = "btnResign";
            this.btnResign.Size = new Size(150, 40);
            this.btnResign.TabIndex = 11;
            this.btnResign.Text = "Re-sign checked";
            this.btnResign.UseVisualStyleBackColor = false;
            this.btnResign.Click += new System.EventHandler(this.btnResign_Click);
            //
            // btnOpenFolder
            //
            this.btnOpenFolder.Font = new Font("Segoe UI", 10F);
            this.btnOpenFolder.Location = new Point(186, 548);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new Size(150, 40);
            this.btnOpenFolder.TabIndex = 12;
            this.btnOpenFolder.Text = "Open folder";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            //
            // btnRemoveHistory
            //
            this.btnRemoveHistory.Font = new Font("Segoe UI", 10F);
            this.btnRemoveHistory.Location = new Point(348, 548);
            this.btnRemoveHistory.Name = "btnRemoveHistory";
            this.btnRemoveHistory.Size = new Size(148, 40);
            this.btnRemoveHistory.TabIndex = 13;
            this.btnRemoveHistory.Text = "Remove from list";
            this.btnRemoveHistory.UseVisualStyleBackColor = true;
            this.btnRemoveHistory.Click += new System.EventHandler(this.btnRemoveHistory_Click);
            //
            // MainForm
            //
            this.AllowDrop = true;
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.White;
            this.ClientSize = new Size(520, 608);
            this.Controls.Add(this.btnRemoveHistory);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnResign);
            this.Controls.Add(this.lvHistory);
            this.Controls.Add(this.lblHistory);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnSafeNet);
            this.Controls.Add(this.btnSignFiles);
            this.Controls.Add(this.btnConfig);
            this.Controls.Add(this.btnSign);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.panelTop);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Digicert CodeSign";
            this.DragEnter += new DragEventHandler(this.MainForm_DragEnter);
            this.DragDrop += new DragEventHandler(this.MainForm_DragDrop);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
