using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace DigicertCodeSign
{
    /// <summary>
    /// Stores the DigiCert token password and the last 5 previous passwords.
    /// Values are protected with DPAPI (CurrentUser).
    /// </summary>
    public sealed class PasswordStore
    {
        public const int MaxHistory = 5;

        private readonly string _filePath;

        public string CurrentPassword { get; private set; }

        public IList<string> History { get; private set; }

        public PasswordStore()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DigicertCodeSign");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, "settings.xml");
            CurrentPassword = string.Empty;
            History = new List<string>();
            Load();
        }

        public bool HasPassword
        {
            get { return !string.IsNullOrEmpty(CurrentPassword); }
        }

        public void SetPassword(string newPassword)
        {
            if (newPassword == null)
                newPassword = string.Empty;

            newPassword = newPassword.Trim();

            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentException("Password cannot be empty.");

            if (!string.IsNullOrEmpty(CurrentPassword) &&
                !string.Equals(CurrentPassword, newPassword, StringComparison.Ordinal))
            {
                PushHistory(CurrentPassword);
            }

            CurrentPassword = newPassword;
            Save();
        }

        public void RestoreFromHistory(int index)
        {
            if (index < 0 || index >= History.Count)
                throw new ArgumentOutOfRangeException("index");

            string selected = History[index];
            History.RemoveAt(index);

            if (!string.IsNullOrEmpty(CurrentPassword) &&
                !string.Equals(CurrentPassword, selected, StringComparison.Ordinal))
            {
                PushHistory(CurrentPassword);
            }

            CurrentPassword = selected;
            Save();
        }

        public void RemoveHistoryAt(int index)
        {
            if (index < 0 || index >= History.Count)
                throw new ArgumentOutOfRangeException("index");

            History.RemoveAt(index);
            Save();
        }

        private void PushHistory(string password)
        {
            // Avoid duplicates at the front of history
            for (int i = History.Count - 1; i >= 0; i--)
            {
                if (string.Equals(History[i], password, StringComparison.Ordinal))
                    History.RemoveAt(i);
            }

            History.Insert(0, password);

            while (History.Count > MaxHistory)
                History.RemoveAt(History.Count - 1);
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
                return;

            try
            {
                XDocument doc = XDocument.Load(_filePath);
                XElement root = doc.Root;
                if (root == null)
                    return;

                XElement current = root.Element("Current");
                if (current != null && !string.IsNullOrEmpty(current.Value))
                    CurrentPassword = Unprotect(current.Value);

                History = new List<string>();
                XElement history = root.Element("History");
                if (history != null)
                {
                    foreach (XElement item in history.Elements("Password"))
                    {
                        if (string.IsNullOrEmpty(item.Value))
                            continue;

                        string plain = Unprotect(item.Value);
                        if (!string.IsNullOrEmpty(plain))
                            History.Add(plain);
                    }
                }

                while (History.Count > MaxHistory)
                    History.RemoveAt(History.Count - 1);
            }
            catch
            {
                CurrentPassword = string.Empty;
                History = new List<string>();
            }
        }

        private void Save()
        {
            XElement history = new XElement("History",
                History.Select(p => new XElement("Password", Protect(p))));

            XDocument doc = new XDocument(
                new XElement("Settings",
                    new XElement("Current", Protect(CurrentPassword ?? string.Empty)),
                    history));

            doc.Save(_filePath);
        }

        private static string Protect(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] protectedBytes = ProtectedData.Protect(
                data,
                null,
                DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedBytes);
        }

        private static string Unprotect(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            byte[] protectedBytes = Convert.FromBase64String(cipherText);
            byte[] data = ProtectedData.Unprotect(
                protectedBytes,
                null,
                DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(data);
        }

        public static string Mask(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "(not set)";

            if (password.Length <= 2)
                return new string('*', password.Length);

            return password[0] + new string('*', Math.Max(password.Length - 2, 1)) + password[password.Length - 1];
        }
    }
}
