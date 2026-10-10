using System;
using System.Threading;
using System.Windows.Automation;

namespace DigicertCodeSign
{
    /// <summary>
    /// Automates the SafeNet / DigiCert "Token Logon" dialog:
    /// enters the token password and clicks OK.
    /// </summary>
    public sealed class SafeNetAutoLogin : IDisposable
    {
        private readonly string _password;
        private readonly Action<string> _onStatus;
        private readonly object _sync = new object();
        private bool _handlerRegistered;
        private bool _disposed;
        private int _successCount;

        public SafeNetAutoLogin(string password, Action<string> onStatus)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password is required.", "password");

            _password = password;
            _onStatus = onStatus;
        }

        public int SuccessCount
        {
            get { return _successCount; }
        }

        /// <summary>
        /// Tries to fill any Token Logon dialog that is already open.
        /// </summary>
        public bool TryLoginExistingWindows()
        {
            bool any = false;

            AutomationElement root = AutomationElement.RootElement;
            AutomationElementCollection windows = root.FindAll(
                TreeScope.Children,
                Condition.TrueCondition);

            foreach (AutomationElement window in windows)
            {
                if (!IsTokenLogonWindow(window))
                    continue;

                if (FillAndSubmit(window))
                    any = true;
            }

            return any;
        }

        /// <summary>
        /// Watches for new Token Logon windows and auto-fills them.
        /// </summary>
        public void StartWatching()
        {
            lock (_sync)
            {
                if (_handlerRegistered || _disposed)
                    return;

                Automation.AddAutomationEventHandler(
                    WindowPattern.WindowOpenedEvent,
                    AutomationElement.RootElement,
                    TreeScope.Children,
                    OnWindowOpened);

                _handlerRegistered = true;
                RaiseStatus("Waiting for DigiCert / SafeNet Token Logon dialog...");
            }
        }

        public void StopWatching()
        {
            lock (_sync)
            {
                if (!_handlerRegistered)
                    return;

                try
                {
                    Automation.RemoveAutomationEventHandler(
                        WindowPattern.WindowOpenedEvent,
                        AutomationElement.RootElement,
                        OnWindowOpened);
                }
                catch
                {
                    // Ignore cleanup errors.
                }

                _handlerRegistered = false;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            StopWatching();
        }

        private void OnWindowOpened(object sender, AutomationEventArgs e)
        {
            try
            {
                AutomationElement window = sender as AutomationElement;
                if (window == null)
                    return;

                if (!IsTokenLogonWindow(window))
                    return;

                // Give the dialog time to fully create its controls.
                Thread.Sleep(300);
                WaitForInputIdle(window, 10000);
                FillAndSubmit(window);
            }
            catch (Exception ex)
            {
                RaiseStatus("Auto-login error: " + ex.Message);
            }
        }

        private static bool IsTokenLogonWindow(AutomationElement window)
        {
            if (window == null)
                return false;

            string name;
            try
            {
                name = window.Current.Name ?? string.Empty;
            }
            catch
            {
                return false;
            }

            name = name.Trim();
            if (name.Length == 0)
                return false;

            // English SafeNet title; also accept common variants.
            if (string.Equals(name, "Token Logon", StringComparison.OrdinalIgnoreCase))
                return true;

            if (name.IndexOf("Token Logon", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (name.IndexOf("Token Log-on", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            // Romanian / localized variants that still contain Token + Log
            if (name.IndexOf("Token", StringComparison.OrdinalIgnoreCase) >= 0 &&
                (name.IndexOf("Logon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 name.IndexOf("Log On", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 name.IndexOf("Autentific", StringComparison.OrdinalIgnoreCase) >= 0))
                return true;

            return false;
        }

        private bool FillAndSubmit(AutomationElement window)
        {
            WaitForInputIdle(window, 5000);

            AutomationElement edit = FindPasswordEdit(window);
            AutomationElement ok = FindOkButton(window);

            if (edit == null || ok == null)
            {
                RaiseStatus("Token Logon found, but password field or OK button was not detected.");
                return false;
            }

            ValuePattern value = edit.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
            if (value == null)
            {
                RaiseStatus("Password field does not support automated input.");
                return false;
            }

            value.SetValue(_password);

            InvokePattern invoke = ok.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            if (invoke == null)
            {
                RaiseStatus("OK button does not support automated click.");
                return false;
            }

            invoke.Invoke();
            Interlocked.Increment(ref _successCount);
            RaiseStatus("Password entered and OK clicked successfully.");
            return true;
        }

        private static AutomationElement FindPasswordEdit(AutomationElement window)
        {
            // Preferred: labeled "Token Password:"
            AutomationElement byName = window.FindFirst(
                TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit),
                    new PropertyCondition(AutomationElement.NameProperty, "Token Password:")));

            if (byName != null)
                return byName;

            // Any edit whose name contains "Password"
            AutomationElementCollection edits = window.FindAll(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

            foreach (AutomationElement edit in edits)
            {
                try
                {
                    if (edit.Current.IsPassword)
                        return edit;
                }
                catch
                {
                }
            }

            foreach (AutomationElement edit in edits)
            {
                try
                {
                    string name = edit.Current.Name ?? string.Empty;
                    if (name.IndexOf("Password", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("Parola", StringComparison.OrdinalIgnoreCase) >= 0)
                        return edit;
                }
                catch
                {
                }
            }

            // Last resort: first edit box in the dialog
            if (edits.Count > 0)
                return edits[0];

            return null;
        }

        private static AutomationElement FindOkButton(AutomationElement window)
        {
            string[] names = new[] { "OK", "&OK", "Ok", "Log On", "Logon", "Login", "Autentificare" };

            foreach (string name in names)
            {
                AutomationElement button = window.FindFirst(
                    TreeScope.Descendants,
                    new AndCondition(
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                        new PropertyCondition(AutomationElement.NameProperty, name)));

                if (button != null)
                    return button;
            }

            // Fallback: first non-Cancel button
            AutomationElementCollection buttons = window.FindAll(
                TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));

            foreach (AutomationElement button in buttons)
            {
                try
                {
                    string name = (button.Current.Name ?? string.Empty).Trim();
                    if (name.Length == 0)
                        continue;

                    if (name.Equals("Cancel", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("Anulare", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("&Cancel", StringComparison.OrdinalIgnoreCase))
                        continue;

                    return button;
                }
                catch
                {
                }
            }

            return null;
        }

        private static void WaitForInputIdle(AutomationElement window, int milliseconds)
        {
            try
            {
                object patternObj;
                if (window.TryGetCurrentPattern(WindowPattern.Pattern, out patternObj))
                {
                    WindowPattern pattern = patternObj as WindowPattern;
                    if (pattern != null)
                        pattern.WaitForInputIdle(milliseconds);
                }
            }
            catch
            {
            }
        }

        private void RaiseStatus(string message)
        {
            Action<string> handler = _onStatus;
            if (handler != null)
                handler(message);
        }
    }
}
