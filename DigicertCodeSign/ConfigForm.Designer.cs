using System.Drawing;
using System.Windows.Forms;

namespace DigicertCodeSign
{
    partial class ConfigForm
    {
        private System.ComponentModel.IContainer components = null;

        private GroupBox grpCurrent;
        private Label lblPassword;
        private Label lblConfirm;
        private TextBox txtPassword;
        private TextBox txtConfirm;
        private CheckBox chkShow;
        private Button btnSave;
        private GroupBox grpHistory;
        private ListBox lstHistory;
        private Button btnCopyHistory;
        private Button btnRestore;
        private Button btnRemoveHistory;
        private Button btnClose;
        private Label lblHistoryInfo;
        private GroupBox grpSigning;
        private Label lblSignTool;
        private TextBox txtSignTool;
        private Button btnBrowseSignTool;
        private Label lblThumbprint;
        private TextBox txtThumbprint;
        private Button btnPickCert;
        private Label lblTimestamp;
        private TextBox txtTimestamp;
        private Label lblStartupFile;
        private TextBox txtStartupFile;
        private Button btnBrowseStartup;
        private Label lblSigningInfo;
        private Button btnSaveSigning;
        private ToolTip toolTip;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.grpCurrent = new GroupBox();
            this.chkShow = new CheckBox();
            this.btnSave = new Button();
            this.txtConfirm = new TextBox();
            this.txtPassword = new TextBox();
            this.lblConfirm = new Label();
            this.lblPassword = new Label();
            this.grpHistory = new GroupBox();
            this.btnRemoveHistory = new Button();
            this.btnRestore = new Button();
            this.btnCopyHistory = new Button();
            this.lstHistory = new ListBox();
            this.lblHistoryInfo = new Label();
            this.btnClose = new Button();
            this.components = new System.ComponentModel.Container();
            this.grpSigning = new GroupBox();
            this.lblSignTool = new Label();
            this.txtSignTool = new TextBox();
            this.btnBrowseSignTool = new Button();
            this.lblThumbprint = new Label();
            this.txtThumbprint = new TextBox();
            this.btnPickCert = new Button();
            this.lblTimestamp = new Label();
            this.txtTimestamp = new TextBox();
            this.lblStartupFile = new Label();
            this.txtStartupFile = new TextBox();
            this.btnBrowseStartup = new Button();
            this.lblSigningInfo = new Label();
            this.btnSaveSigning = new Button();
            this.toolTip = new ToolTip(this.components);
            this.grpCurrent.SuspendLayout();
            this.grpSigning.SuspendLayout();
            this.grpHistory.SuspendLayout();
            this.SuspendLayout();
            //
            // grpCurrent
            //
            this.grpCurrent.Controls.Add(this.chkShow);
            this.grpCurrent.Controls.Add(this.btnSave);
            this.grpCurrent.Controls.Add(this.txtConfirm);
            this.grpCurrent.Controls.Add(this.txtPassword);
            this.grpCurrent.Controls.Add(this.lblConfirm);
            this.grpCurrent.Controls.Add(this.lblPassword);
            this.grpCurrent.Font = new Font("Segoe UI", 9F);
            this.grpCurrent.Location = new Point(16, 12);
            this.grpCurrent.Name = "grpCurrent";
            this.grpCurrent.Size = new Size(528, 150);
            this.grpCurrent.TabIndex = 0;
            this.grpCurrent.TabStop = false;
            this.grpCurrent.Text = "Current DigiCert token password";
            //
            // lblPassword
            //
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new Point(16, 32);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new Size(60, 15);
            this.lblPassword.TabIndex = 0;
            this.lblPassword.Text = "Password:";
            //
            // txtPassword
            //
            this.txtPassword.Location = new Point(120, 28);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new Size(388, 23);
            this.txtPassword.TabIndex = 1;
            //
            // lblConfirm
            //
            this.lblConfirm.AutoSize = true;
            this.lblConfirm.Location = new Point(16, 68);
            this.lblConfirm.Name = "lblConfirm";
            this.lblConfirm.Size = new Size(84, 15);
            this.lblConfirm.TabIndex = 2;
            this.lblConfirm.Text = "Confirmation:";
            //
            // txtConfirm
            //
            this.txtConfirm.Location = new Point(120, 64);
            this.txtConfirm.Name = "txtConfirm";
            this.txtConfirm.PasswordChar = '*';
            this.txtConfirm.Size = new Size(388, 23);
            this.txtConfirm.TabIndex = 3;
            //
            // chkShow
            //
            this.chkShow.AutoSize = true;
            this.chkShow.Location = new Point(120, 96);
            this.chkShow.Name = "chkShow";
            this.chkShow.Size = new Size(108, 19);
            this.chkShow.TabIndex = 4;
            this.chkShow.Text = "Show password";
            this.chkShow.UseVisualStyleBackColor = true;
            this.chkShow.CheckedChanged += new System.EventHandler(this.chkShow_CheckedChanged);
            //
            // btnSave
            //
            this.btnSave.BackColor = Color.FromArgb(0, 120, 215);
            this.btnSave.FlatStyle = FlatStyle.Flat;
            this.btnSave.ForeColor = Color.White;
            this.btnSave.Location = new Point(388, 108);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(120, 30);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save password";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            //
            // grpSigning
            //
            this.grpSigning.Controls.Add(this.btnSaveSigning);
            this.grpSigning.Controls.Add(this.lblSigningInfo);
            this.grpSigning.Controls.Add(this.btnBrowseStartup);
            this.grpSigning.Controls.Add(this.txtStartupFile);
            this.grpSigning.Controls.Add(this.lblStartupFile);
            this.grpSigning.Controls.Add(this.txtTimestamp);
            this.grpSigning.Controls.Add(this.lblTimestamp);
            this.grpSigning.Controls.Add(this.btnPickCert);
            this.grpSigning.Controls.Add(this.txtThumbprint);
            this.grpSigning.Controls.Add(this.lblThumbprint);
            this.grpSigning.Controls.Add(this.btnBrowseSignTool);
            this.grpSigning.Controls.Add(this.txtSignTool);
            this.grpSigning.Controls.Add(this.lblSignTool);
            this.grpSigning.Font = new Font("Segoe UI", 9F);
            this.grpSigning.Location = new Point(16, 176);
            this.grpSigning.Name = "grpSigning";
            this.grpSigning.Size = new Size(528, 204);
            this.grpSigning.TabIndex = 1;
            this.grpSigning.TabStop = false;
            this.grpSigning.Text = "Signing settings";
            //
            // lblSignTool
            //
            this.lblSignTool.AutoSize = true;
            this.lblSignTool.Location = new Point(16, 30);
            this.lblSignTool.Name = "lblSignTool";
            this.lblSignTool.TabIndex = 0;
            this.lblSignTool.Text = "signtool.exe:";
            //
            // txtSignTool
            //
            this.txtSignTool.Location = new Point(120, 26);
            this.txtSignTool.Name = "txtSignTool";
            this.txtSignTool.Size = new Size(292, 23);
            this.txtSignTool.TabIndex = 1;
            this.toolTip.SetToolTip(this.txtSignTool, "Leave empty to use the newest signtool.exe from the Windows SDK.");
            //
            // btnBrowseSignTool
            //
            this.btnBrowseSignTool.Location = new Point(418, 25);
            this.btnBrowseSignTool.Name = "btnBrowseSignTool";
            this.btnBrowseSignTool.Size = new Size(90, 25);
            this.btnBrowseSignTool.TabIndex = 2;
            this.btnBrowseSignTool.Text = "Browse...";
            this.btnBrowseSignTool.UseVisualStyleBackColor = true;
            this.btnBrowseSignTool.Click += new System.EventHandler(this.btnBrowseSignTool_Click);
            //
            // lblThumbprint
            //
            this.lblThumbprint.AutoSize = true;
            this.lblThumbprint.Location = new Point(16, 62);
            this.lblThumbprint.Name = "lblThumbprint";
            this.lblThumbprint.TabIndex = 3;
            this.lblThumbprint.Text = "Certificate SHA1:";
            //
            // txtThumbprint
            //
            this.txtThumbprint.Location = new Point(120, 58);
            this.txtThumbprint.Name = "txtThumbprint";
            this.txtThumbprint.Size = new Size(292, 23);
            this.txtThumbprint.TabIndex = 4;
            this.toolTip.SetToolTip(this.txtThumbprint, "Thumbprint of the code signing certificate on the token (signtool /sha1).");
            //
            // btnPickCert
            //
            this.btnPickCert.Location = new Point(418, 57);
            this.btnPickCert.Name = "btnPickCert";
            this.btnPickCert.Size = new Size(90, 25);
            this.btnPickCert.TabIndex = 5;
            this.btnPickCert.Text = "Select...";
            this.btnPickCert.UseVisualStyleBackColor = true;
            this.btnPickCert.Click += new System.EventHandler(this.btnPickCert_Click);
            //
            // lblTimestamp
            //
            this.lblTimestamp.AutoSize = true;
            this.lblTimestamp.Location = new Point(16, 94);
            this.lblTimestamp.Name = "lblTimestamp";
            this.lblTimestamp.TabIndex = 6;
            this.lblTimestamp.Text = "Timestamp URL:";
            //
            // txtTimestamp
            //
            this.txtTimestamp.Location = new Point(120, 90);
            this.txtTimestamp.Name = "txtTimestamp";
            this.txtTimestamp.Size = new Size(388, 23);
            this.txtTimestamp.TabIndex = 7;
            //
            // lblStartupFile
            //
            this.lblStartupFile.AutoSize = true;
            this.lblStartupFile.Location = new Point(16, 126);
            this.lblStartupFile.Name = "lblStartupFile";
            this.lblStartupFile.TabIndex = 8;
            this.lblStartupFile.Text = "Sign on startup:";
            //
            // txtStartupFile
            //
            this.txtStartupFile.Location = new Point(120, 122);
            this.txtStartupFile.Name = "txtStartupFile";
            this.txtStartupFile.Size = new Size(292, 23);
            this.txtStartupFile.TabIndex = 9;
            this.toolTip.SetToolTip(this.txtStartupFile, "Optional. Signed automatically when the app starts and by \"Sign now\".");
            //
            // btnBrowseStartup
            //
            this.btnBrowseStartup.Location = new Point(418, 121);
            this.btnBrowseStartup.Name = "btnBrowseStartup";
            this.btnBrowseStartup.Size = new Size(90, 25);
            this.btnBrowseStartup.TabIndex = 10;
            this.btnBrowseStartup.Text = "Browse...";
            this.btnBrowseStartup.UseVisualStyleBackColor = true;
            this.btnBrowseStartup.Click += new System.EventHandler(this.btnBrowseStartup_Click);
            //
            // lblSigningInfo
            //
            this.lblSigningInfo.ForeColor = Color.DimGray;
            this.lblSigningInfo.Location = new Point(16, 156);
            this.lblSigningInfo.Name = "lblSigningInfo";
            this.lblSigningInfo.Size = new Size(360, 36);
            this.lblSigningInfo.TabIndex = 11;
            this.lblSigningInfo.Text = "Empty signtool path = auto-detect from the Windows SDK.\r\nEmpty startup file = no automatic signing.";
            //
            // btnSaveSigning
            //
            this.btnSaveSigning.BackColor = Color.FromArgb(0, 120, 215);
            this.btnSaveSigning.FlatStyle = FlatStyle.Flat;
            this.btnSaveSigning.ForeColor = Color.White;
            this.btnSaveSigning.Location = new Point(388, 158);
            this.btnSaveSigning.Name = "btnSaveSigning";
            this.btnSaveSigning.Size = new Size(120, 30);
            this.btnSaveSigning.TabIndex = 12;
            this.btnSaveSigning.Text = "Save settings";
            this.btnSaveSigning.UseVisualStyleBackColor = false;
            this.btnSaveSigning.Click += new System.EventHandler(this.btnSaveSigning_Click);
            //
            // grpHistory
            //
            this.grpHistory.Controls.Add(this.lblHistoryInfo);
            this.grpHistory.Controls.Add(this.btnRemoveHistory);
            this.grpHistory.Controls.Add(this.btnRestore);
            this.grpHistory.Controls.Add(this.btnCopyHistory);
            this.grpHistory.Controls.Add(this.lstHistory);
            this.grpHistory.Font = new Font("Segoe UI", 9F);
            this.grpHistory.Location = new Point(16, 394);
            this.grpHistory.Name = "grpHistory";
            this.grpHistory.Size = new Size(528, 200);
            this.grpHistory.TabIndex = 2;
            this.grpHistory.TabStop = false;
            this.grpHistory.Text = "Password history (last 5)";
            //
            // lblHistoryInfo
            //
            this.lblHistoryInfo.ForeColor = Color.DimGray;
            this.lblHistoryInfo.Location = new Point(16, 24);
            this.lblHistoryInfo.Name = "lblHistoryInfo";
            this.lblHistoryInfo.Size = new Size(492, 32);
            this.lblHistoryInfo.TabIndex = 0;
            this.lblHistoryInfo.Text = "When you save a new password, the previous one is kept here (max 5).";
            //
            // lstHistory
            //
            this.lstHistory.FormattingEnabled = true;
            this.lstHistory.ItemHeight = 15;
            this.lstHistory.Location = new Point(16, 60);
            this.lstHistory.Name = "lstHistory";
            this.lstHistory.Size = new Size(352, 124);
            this.lstHistory.TabIndex = 1;
            //
            // btnCopyHistory
            //
            this.btnCopyHistory.Location = new Point(388, 60);
            this.btnCopyHistory.Name = "btnCopyHistory";
            this.btnCopyHistory.Size = new Size(120, 30);
            this.btnCopyHistory.TabIndex = 2;
            this.btnCopyHistory.Text = "Copy";
            this.btnCopyHistory.UseVisualStyleBackColor = true;
            this.btnCopyHistory.Click += new System.EventHandler(this.btnCopyHistory_Click);
            //
            // btnRestore
            //
            this.btnRestore.Location = new Point(388, 100);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new Size(120, 30);
            this.btnRestore.TabIndex = 3;
            this.btnRestore.Text = "Restore as current";
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            //
            // btnRemoveHistory
            //
            this.btnRemoveHistory.Location = new Point(388, 140);
            this.btnRemoveHistory.Name = "btnRemoveHistory";
            this.btnRemoveHistory.Size = new Size(120, 30);
            this.btnRemoveHistory.TabIndex = 4;
            this.btnRemoveHistory.Text = "Remove";
            this.btnRemoveHistory.UseVisualStyleBackColor = true;
            this.btnRemoveHistory.Click += new System.EventHandler(this.btnRemoveHistory_Click);
            //
            // btnClose
            //
            this.btnClose.Location = new Point(444, 610);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(100, 32);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // ConfigForm
            //
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.White;
            this.CancelButton = this.btnClose;
            this.ClientSize = new Size(560, 658);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.grpHistory);
            this.Controls.Add(this.grpSigning);
            this.Controls.Add(this.grpCurrent);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigForm";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Configuration - Digicert CodeSign";
            this.grpCurrent.ResumeLayout(false);
            this.grpCurrent.PerformLayout();
            this.grpSigning.ResumeLayout(false);
            this.grpSigning.PerformLayout();
            this.grpHistory.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
