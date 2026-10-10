using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DigicertCodeSign
{
    public partial class MainForm : Form
    {
        private const string SacToolsPath =
            @"C:\Program Files\SafeNet\Authentication\SAC\x64\SACTools.exe";

        private const int SignTimeoutMs = 180000; // 3 minutes (token prompt + timestamp)

        private const string SignFilesFilter =
            "Executable files (*.exe)|*.exe|" +
            "Signable files (*.exe;*.dll;*.msi;*.msix;*.appx;*.cab;*.sys;*.ocx)|*.exe;*.dll;*.msi;*.msix;*.appx;*.cab;*.sys;*.ocx|" +
            "All files (*.*)|*.*";

        private readonly PasswordStore _store;
        private readonly SigningSettings _signing;
        private readonly SignHistory _history;
        private SafeNetAutoLogin _autoLogin;
        private bool _signingInProgress;
        private bool _suppressItemCheck;

        public MainForm()
        {
            _store = new PasswordStore();
            _signing = new SigningSettings();
            _history = new SignHistory();
            InitializeComponent();
            foreach (Button button in new[] { btnLogin, btnSign, btnSignFiles, btnResign })
                GrayWhenDisabled(button);
            EnableFileDrop(this);
            RefreshHistoryList();
            RefreshStatus();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            BeginInvoke(new Action(StartStartupSign));
        }

        private void RefreshStatus()
        {
            if (!_store.HasPassword)
            {
                lblStatus.Text = "No password configured. Open Configuration first.";
                lblStatus.ForeColor = Color.DarkRed;
            }
            else if (!_signing.HasCertificate)
            {
                lblStatus.Text = "No certificate configured. Open Configuration first.";
                lblStatus.ForeColor = Color.DarkRed;
            }
            else
            {
                lblStatus.Text = "Password configured: " + PasswordStore.Mask(_store.CurrentPassword);
                lblStatus.ForeColor = Color.DarkGreen;
            }

            btnSign.Text = _signing.HasStartupFile
                ? "Sign " + Path.GetFileName(_signing.StartupFile)
                : "Sign now";

            UpdateButtons();
        }

        private bool IsConfigured
        {
            get { return _store.HasPassword && _signing.HasCertificate; }
        }

        private void UpdateButtons()
        {
            bool canSign = !_signingInProgress && IsConfigured;
            bool hasTargets = GetHistoryTargets().Count > 0;

            btnLogin.Enabled = !_signingInProgress && _store.HasPassword;
            btnSign.Enabled = canSign && _signing.HasStartupFile;
            btnSignFiles.Enabled = canSign;
            btnResign.Enabled = canSign && hasTargets;
            btnOpenFolder.Enabled = hasTargets;
            btnRemoveHistory.Enabled = !_signingInProgress && hasTargets;
            btnConfig.Enabled = !_signingInProgress;
            btnSafeNet.Enabled = !_signingInProgress;
        }

        // Flat colored buttons look the same when disabled; fade them out instead.
        private static void GrayWhenDisabled(Button button)
        {
            Color enabledColor = button.BackColor;
            button.EnabledChanged += (s, e) =>
                button.BackColor = button.Enabled ? enabledColor : Color.FromArgb(200, 200, 200);
        }

        private void SetHint(string text, Color color)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string, Color>(SetHint), text, color);
                return;
            }

            lblHint.Text = text;
            lblHint.ForeColor = color;
        }

        private void SetBusy(bool busy)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool>(SetBusy), busy);
                return;
            }

            _signingInProgress = busy;
            UpdateButtons();
            UseWaitCursor = busy;
        }

        private void StartStartupSign()
        {
            if (!IsConfigured)
            {
                SetHint(
                    "Set the DigiCert token password and the certificate in Configuration,\r\n" +
                    "then sign files with \"Sign files...\" or by dragging them here.",
                    Color.DarkRed);
                return;
            }

            if (!_signing.HasStartupFile)
            {
                SetHint(
                    "Use \"Sign files...\" (or drag files here) to sign.\r\n" +
                    "Set a startup file in Configuration to sign it automatically when the app starts.",
                    Color.DimGray);
                return;
            }

            RunSign(new[] { _signing.StartupFile }, showDialogs: true, keepWatcherAfter: true);
        }

        private void EnsureAutoLoginWatching()
        {
            if (_autoLogin == null)
            {
                _autoLogin = new SafeNetAutoLogin(_store.CurrentPassword, OnAutoLoginStatus);
                _autoLogin.TryLoginExistingWindows();
                _autoLogin.StartWatching();
            }
        }

        private void RunSign(IList<string> files, bool showDialogs, bool keepWatcherAfter)
        {
            if (_signingInProgress || files == null || files.Count == 0)
                return;

            if (!IsConfigured)
            {
                if (showDialogs)
                {
                    MessageBox.Show(
                        this,
                        "Set the DigiCert token password and the certificate in Configuration first.",
                        "Digicert CodeSign",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                return;
            }

            SetBusy(true);
            SetHint(
                "Signing " + DescribeFiles(files) + "...\r\nWaiting for SafeNet Token Logon (password will be entered automatically).",
                Color.DarkBlue);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    // Start watcher on UI thread so automation events work reliably.
                    Invoke(new Action(EnsureAutoLoginWatching));

                    // Small delay so the watcher is registered before signtool starts.
                    Thread.Sleep(400);

                    List<SignResult> results = new List<SignResult>();
                    for (int i = 0; i < files.Count; i++)
                    {
                        string file = files[i];

                        if (files.Count > 1)
                        {
                            SetHint(
                                string.Format("Signing {0}/{1}: {2}...\r\n", i + 1, files.Count, Path.GetFileName(file)) +
                                "Waiting for SafeNet Token Logon (password will be entered automatically).",
                                Color.DarkBlue);
                        }

                        SignResult result = CodeSigner.SignFile(file, _signing, SignTimeoutMs);
                        results.Add(result);
                        Invoke(new Action(() => RecordResult(result)));

                        // signtool missing, timeout or crash: the next files would fail the same way.
                        if (!result.Success && result.ExitCode == -1 && File.Exists(file))
                            break;
                    }

                    BeginInvoke(new Action(() => ShowSignResults(files, results, showDialogs, keepWatcherAfter)));
                }
                catch (Exception ex)
                {
                    BeginInvoke(new Action(() =>
                    {
                        SetBusy(false);
                        SetHint("Signing failed: " + ex.Message, Color.DarkRed);
                        if (showDialogs)
                        {
                            MessageBox.Show(
                                this,
                                "Signing failed:\r\n" + ex.Message,
                                "Digicert CodeSign",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }));
                }
            });
        }

        private void RecordResult(SignResult result)
        {
            // Files that do not exist are not added to history (existing entries show as "Missing").
            if (!File.Exists(result.FilePath))
            {
                RefreshHistoryList();
                return;
            }

            try
            {
                _history.Record(result.FilePath, result.Success);
            }
            catch (Exception ex)
            {
                SetHint("Could not save signing history: " + ex.Message, Color.DarkRed);
            }

            RefreshHistoryList();
        }

        private void ShowSignResults(IList<string> files, IList<SignResult> results, bool showDialogs, bool keepWatcherAfter)
        {
            SetBusy(false);

            List<SignResult> failed = results.Where(r => !r.Success).ToList();
            int succeeded = results.Count - failed.Count;
            int skipped = files.Count - results.Count;

            if (succeeded > 0)
            {
                if (!keepWatcherAfter)
                    StopAutoLogin();
                else
                    EnsureAutoLoginWatching();
            }

            if (failed.Count == 0 && skipped == 0)
            {
                SetHint(
                    (files.Count == 1 ? "Signing succeeded." : "Signed " + files.Count + " files.") + "\r\n" +
                    "Token session is active — you can sign other files with signtool scripts.\r\n" +
                    "Auto-login remains on for new Token Logon dialogs.",
                    Color.DarkGreen);

                if (showDialogs)
                {
                    string message;
                    if (results.Count == 1)
                    {
                        SignResult result = results[0];
                        message = "Signing succeeded.\r\n\r\n" +
                            result.FilePath +
                            (string.IsNullOrEmpty(result.Output) ? string.Empty : "\r\n\r\n" + result.Output);
                    }
                    else
                    {
                        message = "Signed " + results.Count + " files:\r\n\r\n" +
                            string.Join("\r\n", results.Select(r => r.FilePath).ToArray());
                    }

                    MessageBox.Show(
                        this,
                        message,
                        "Digicert CodeSign",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                return;
            }

            StringBuilder details = new StringBuilder();
            foreach (SignResult result in failed)
            {
                if (details.Length > 0)
                    details.Append("\r\n\r\n");

                if (files.Count > 1)
                    details.Append(Path.GetFileName(result.FilePath)).Append(": ");

                details.Append(string.IsNullOrEmpty(result.ErrorMessage) ? "Unknown signing error." : result.ErrorMessage);
            }

            if (skipped > 0)
                details.Append("\r\n\r\n").Append(skipped).Append(" file(s) were not attempted.");

            string summary = files.Count == 1
                ? "Signing failed."
                : string.Format("Signed {0} of {1} files.", succeeded, files.Count);

            SetHint(summary + "\r\n" + details, Color.DarkRed);

            if (showDialogs)
            {
                MessageBox.Show(
                    this,
                    summary + "\r\n\r\n" + details,
                    "Digicert CodeSign",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string DescribeFiles(IList<string> files)
        {
            return files.Count == 1 ? Path.GetFileName(files[0]) : files.Count + " files";
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!_store.HasPassword)
            {
                MessageBox.Show(
                    this,
                    "Set the DigiCert token password in Configuration first.",
                    "Digicert CodeSign",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                StopAutoLogin();

                _autoLogin = new SafeNetAutoLogin(_store.CurrentPassword, OnAutoLoginStatus);

                bool alreadyOpen = _autoLogin.TryLoginExistingWindows();
                if (alreadyOpen)
                {
                    SetHint(
                        "Token Logon dialog was open: password entered and OK clicked.",
                        Color.DarkGreen);
                }
                else
                {
                    _autoLogin.StartWatching();
                    SetHint(
                        "Auto-login is active.\r\n" +
                        "Start signing (signtool / build). When SafeNet shows Token Logon,\r\n" +
                        "the password will be entered and OK will be clicked automatically.",
                        Color.DarkBlue);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Could not start auto-login:\r\n" + ex.Message,
                    "Digicert CodeSign",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnSign_Click(object sender, EventArgs e)
        {
            RunSign(new[] { _signing.StartupFile }, showDialogs: true, keepWatcherAfter: true);
        }

        private void btnSignFiles_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select files to sign";
                dialog.Filter = SignFilesFilter;
                dialog.Multiselect = true;
                dialog.CheckFileExists = true;
                dialog.RestoreDirectory = true;

                SignHistoryEntry recent = _history.Entries.FirstOrDefault(entry => Directory.Exists(entry.Folder));
                if (recent != null)
                    dialog.InitialDirectory = recent.Folder;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                RunSign(dialog.FileNames, showDialogs: true, keepWatcherAfter: true);
            }
        }

        #region History

        private void RefreshHistoryList()
        {
            HashSet<string> checkedPaths = new HashSet<string>(
                lvHistory.CheckedItems.Cast<ListViewItem>().Select(item => (string)item.Tag),
                StringComparer.OrdinalIgnoreCase);

            lvHistory.BeginUpdate();
            try
            {
                lvHistory.Items.Clear();

                foreach (SignHistoryEntry entry in _history.Entries)
                {
                    bool exists = entry.Exists;

                    ListViewItem item = new ListViewItem(entry.FileName);
                    item.SubItems.Add(entry.LastSigned == DateTime.MinValue
                        ? string.Empty
                        : entry.LastSigned.ToString("yyyy-MM-dd HH:mm"));
                    item.SubItems.Add(!exists ? "Missing" : entry.Success ? "OK" : "Failed");
                    item.SubItems.Add(entry.Folder);
                    item.Tag = entry.FilePath;
                    item.ToolTipText = entry.FilePath + "\r\nSigned " + entry.SignCount + " time(s)";
                    item.Checked = checkedPaths.Contains(entry.FilePath);

                    if (!exists)
                        item.ForeColor = Color.Gray;
                    else if (!entry.Success)
                        item.ForeColor = Color.DarkRed;

                    lvHistory.Items.Add(item);
                }
            }
            finally
            {
                lvHistory.EndUpdate();
            }

            UpdateButtons();
        }

        /// <summary>
        /// Checked items; if nothing is checked, the selected items.
        /// </summary>
        private List<string> GetHistoryTargets()
        {
            IEnumerable<ListViewItem> items = lvHistory.CheckedItems.Count > 0
                ? lvHistory.CheckedItems.Cast<ListViewItem>()
                : lvHistory.SelectedItems.Cast<ListViewItem>();

            return items.Select(item => (string)item.Tag).ToList();
        }

        private void ResignFiles(IList<string> files)
        {
            if (_signingInProgress || files.Count == 0)
                return;

            List<string> missing = files.Where(f => !File.Exists(f)).ToList();
            List<string> existing = files.Where(File.Exists).ToList();

            if (missing.Count > 0)
            {
                string message = "These files no longer exist and will be skipped:\r\n\r\n" +
                    string.Join("\r\n", missing.ToArray());

                if (existing.Count == 0)
                {
                    MessageBox.Show(this, message, "Digicert CodeSign", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show(this, message, "Digicert CodeSign", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                    return;
            }

            RunSign(existing, showDialogs: true, keepWatcherAfter: true);
        }

        private void btnResign_Click(object sender, EventArgs e)
        {
            ResignFiles(GetHistoryTargets());
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            List<string> targets = GetHistoryTargets();
            if (targets.Count == 0)
                return;

            string path = targets[0];
            try
            {
                if (File.Exists(path))
                {
                    Process.Start("explorer.exe", "/select,\"" + path + "\"");
                }
                else if (Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Process.Start("explorer.exe", "\"" + Path.GetDirectoryName(path) + "\"");
                }
                else
                {
                    MessageBox.Show(
                        this,
                        "The folder no longer exists:\r\n" + Path.GetDirectoryName(path),
                        "Digicert CodeSign",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Could not open folder:\r\n" + ex.Message,
                    "Digicert CodeSign",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnRemoveHistory_Click(object sender, EventArgs e)
        {
            RemoveFromHistory(GetHistoryTargets());
        }

        private void RemoveFromHistory(IList<string> files)
        {
            if (_signingInProgress || files.Count == 0)
                return;

            try
            {
                _history.Remove(files);
            }
            catch (Exception ex)
            {
                SetHint("Could not save signing history: " + ex.Message, Color.DarkRed);
            }

            RefreshHistoryList();
        }

        private void lvHistory_DoubleClick(object sender, EventArgs e)
        {
            if (lvHistory.SelectedItems.Count == 1 && IsConfigured)
                ResignFiles(new[] { (string)lvHistory.SelectedItems[0].Tag });
        }

        private void lvHistory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveFromHistory(GetHistoryTargets());
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter && IsConfigured)
            {
                ResignFiles(GetHistoryTargets());
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.A && e.Control)
            {
                foreach (ListViewItem item in lvHistory.Items)
                    item.Selected = true;
                e.Handled = true;
            }
        }

        // A double-click on a ListView with CheckBoxes toggles the check; double-click here means "re-sign".
        private void lvHistory_MouseDown(object sender, MouseEventArgs e)
        {
            _suppressItemCheck = e.Clicks > 1;
        }

        private void lvHistory_MouseUp(object sender, MouseEventArgs e)
        {
            _suppressItemCheck = false;
        }

        private void lvHistory_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (_suppressItemCheck)
                e.NewValue = e.CurrentValue;
        }

        private void lvHistory_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            UpdateButtons();
        }

        private void lvHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        #endregion

        #region Drag & drop

        private void EnableFileDrop(Control control)
        {
            control.AllowDrop = true;
            if (control != this)
            {
                control.DragEnter += MainForm_DragEnter;
                control.DragDrop += MainForm_DragDrop;
            }

            foreach (Control child in control.Controls)
                EnableFileDrop(child);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = !_signingInProgress && IsConfigured && e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] dropped = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (dropped == null)
                return;

            List<string> files = dropped.Where(File.Exists).ToList();
            if (files.Count == 0)
                return;

            // Leave the drag source (Explorer) before signing starts.
            BeginInvoke(new Action(() => RunSign(files, showDialogs: true, keepWatcherAfter: true)));
        }

        #endregion

        private void OnAutoLoginStatus(string message)
        {
            SetHint(message, Color.DarkGreen);
        }

        private void StopAutoLogin()
        {
            if (_autoLogin != null)
            {
                _autoLogin.Dispose();
                _autoLogin = null;
            }
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            using (ConfigForm form = new ConfigForm(_store, _signing))
            {
                form.ShowDialog(this);
            }

            // Recreate watcher with the possibly updated password.
            StopAutoLogin();
            RefreshStatus();
        }

        private void btnSafeNet_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(SacToolsPath))
                {
                    MessageBox.Show(
                        this,
                        "SafeNet Authentication Client was not found at:\r\n" + SacToolsPath,
                        "Digicert CodeSign",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = SacToolsPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Could not open SafeNet Tools:\r\n" + ex.Message,
                    "Digicert CodeSign",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopAutoLogin();
            base.OnFormClosed(e);
        }
    }
}
