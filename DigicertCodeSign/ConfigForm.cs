using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace DigicertCodeSign
{
    public partial class ConfigForm : Form
    {
        private const string CodeSigningOid = "1.3.6.1.5.5.7.3.3";

        private readonly PasswordStore _store;
        private readonly SigningSettings _signing;

        public ConfigForm(PasswordStore store, SigningSettings signing)
        {
            _store = store;
            _signing = signing;
            InitializeComponent();
            LoadCurrent();
            LoadSigning();
            RefreshHistory();
        }

        private void LoadSigning()
        {
            txtSignTool.Text = _signing.SignToolPath;
            txtThumbprint.Text = _signing.CertificateSha1;
            txtTimestamp.Text = _signing.TimestampUrl;
            txtStartupFile.Text = _signing.StartupFile;
        }

        private void LoadCurrent()
        {
            txtPassword.Text = _store.CurrentPassword ?? string.Empty;
            txtConfirm.Text = string.Empty;
        }

        private void RefreshHistory()
        {
            lstHistory.Items.Clear();

            if (_store.History.Count == 0)
            {
                lstHistory.Items.Add("(no previous passwords)");
                btnCopyHistory.Enabled = false;
                btnRestore.Enabled = false;
                btnRemoveHistory.Enabled = false;
                return;
            }

            for (int i = 0; i < _store.History.Count; i++)
            {
                string masked = PasswordStore.Mask(_store.History[i]);
                lstHistory.Items.Add((i + 1) + ". " + masked);
            }

            lstHistory.SelectedIndex = 0;
            btnCopyHistory.Enabled = true;
            btnRestore.Enabled = true;
            btnRemoveHistory.Enabled = true;
        }

        private int SelectedHistoryIndex()
        {
            if (_store.History.Count == 0)
                return -1;

            return lstHistory.SelectedIndex;
        }

        private void chkShow_CheckedChanged(object sender, EventArgs e)
        {
            char passwordChar = chkShow.Checked ? '\0' : '*';
            txtPassword.PasswordChar = passwordChar;
            txtConfirm.PasswordChar = passwordChar;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string password = (txtPassword.Text ?? string.Empty).Trim();
            string confirm = (txtConfirm.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show(this, "Enter a password.", "Configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (!string.Equals(password, confirm, StringComparison.Ordinal))
            {
                MessageBox.Show(this, "Password and confirmation do not match.", "Configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirm.Focus();
                return;
            }

            try
            {
                _store.SetPassword(password);
                txtConfirm.Text = string.Empty;
                RefreshHistory();
                MessageBox.Show(this, "Password saved. Previous password moved to history (max 5).",
                    "Configuration", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCopyHistory_Click(object sender, EventArgs e)
        {
            int index = SelectedHistoryIndex();
            if (index < 0)
                return;

            try
            {
                Clipboard.SetText(_store.History[index]);
                MessageBox.Show(this, "History password copied to clipboard.", "Configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            int index = SelectedHistoryIndex();
            if (index < 0)
                return;

            DialogResult result = MessageBox.Show(
                this,
                "Restore this password as the current DigiCert token password?\r\n" +
                "The current password will be moved into history.",
                "Configuration",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                _store.RestoreFromHistory(index);
                LoadCurrent();
                RefreshHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRemoveHistory_Click(object sender, EventArgs e)
        {
            int index = SelectedHistoryIndex();
            if (index < 0)
                return;

            DialogResult result = MessageBox.Show(
                this,
                "Remove this password from history?",
                "Configuration",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                _store.RemoveHistoryAt(index);
                RefreshHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBrowseSignTool_Click(object sender, EventArgs e)
        {
            string current = txtSignTool.Text.Trim();
            string path = BrowseFile(
                "Select signtool.exe",
                "signtool.exe|signtool.exe|Executable files (*.exe)|*.exe",
                current.Length > 0 ? current : SigningSettings.FindSignTool());

            if (path != null)
                txtSignTool.Text = path;
        }

        private void btnBrowseStartup_Click(object sender, EventArgs e)
        {
            string path = BrowseFile(
                "Select the file to sign on startup",
                "Signable files (*.exe;*.dll;*.msi;*.msix;*.appx;*.cab;*.sys;*.ocx)|*.exe;*.dll;*.msi;*.msix;*.appx;*.cab;*.sys;*.ocx|" +
                "All files (*.*)|*.*",
                txtStartupFile.Text.Trim());

            if (path != null)
                txtStartupFile.Text = path;
        }

        private string BrowseFile(string title, string filter, string current)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = title;
                dialog.Filter = filter;
                dialog.CheckFileExists = true;
                dialog.RestoreDirectory = true;

                if (!string.IsNullOrEmpty(current))
                {
                    string folder = Path.GetDirectoryName(current);
                    if (Directory.Exists(folder))
                        dialog.InitialDirectory = folder;
                }

                return dialog.ShowDialog(this) == DialogResult.OK ? dialog.FileName : null;
            }
        }

        private void btnPickCert_Click(object sender, EventArgs e)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection candidates = new X509Certificate2Collection(
                    store.Certificates
                        .Cast<X509Certificate2>()
                        .Where(c => c.HasPrivateKey && IsCodeSigning(c))
                        .ToArray());

                if (candidates.Count == 0)
                {
                    MessageBox.Show(this,
                        "No code signing certificate with a private key was found in the personal certificate store.\r\n" +
                        "Plug in the token and make sure SafeNet Authentication Client has registered its certificate.",
                        "Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                X509Certificate2Collection selected = X509Certificate2UI.SelectFromCollection(
                    candidates,
                    "Code signing certificate",
                    "Select the certificate used by signtool.",
                    X509SelectionFlag.SingleSelection,
                    Handle);

                if (selected.Count > 0)
                    txtThumbprint.Text = selected[0].Thumbprint;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                store.Close();
            }
        }

        private static bool IsCodeSigning(X509Certificate2 certificate)
        {
            foreach (X509Extension extension in certificate.Extensions)
            {
                X509EnhancedKeyUsageExtension eku = extension as X509EnhancedKeyUsageExtension;
                if (eku != null)
                    return eku.EnhancedKeyUsages.Cast<Oid>().Any(oid => oid.Value == CodeSigningOid);
            }

            return false;
        }

        private void btnSaveSigning_Click(object sender, EventArgs e)
        {
            string signTool = txtSignTool.Text.Trim();
            string thumbprint = SigningSettings.NormalizeThumbprint(txtThumbprint.Text);
            string timestamp = txtTimestamp.Text.Trim();
            string startupFile = txtStartupFile.Text.Trim();

            if (signTool.Length > 0 && !File.Exists(signTool))
            {
                MessageBox.Show(this, "signtool.exe was not found at:\r\n" + signTool, "Configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSignTool.Focus();
                return;
            }

            if (thumbprint.Length != 40)
            {
                MessageBox.Show(this, "The certificate SHA1 thumbprint must have 40 hex characters.", "Configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtThumbprint.Focus();
                return;
            }

            if (timestamp.Length > 0 && !Uri.IsWellFormedUriString(timestamp, UriKind.Absolute))
            {
                MessageBox.Show(this, "The timestamp URL is not valid.", "Configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTimestamp.Focus();
                return;
            }

            try
            {
                _signing.SignToolPath = signTool;
                _signing.CertificateSha1 = thumbprint;
                _signing.TimestampUrl = timestamp;
                _signing.StartupFile = startupFile;
                _signing.Save();
                LoadSigning();
                MessageBox.Show(this, "Signing settings saved.", "Configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Configuration",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
