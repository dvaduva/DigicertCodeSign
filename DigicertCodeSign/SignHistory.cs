using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DigicertCodeSign
{
    public sealed class SignHistoryEntry
    {
        public string FilePath { get; set; }
        public DateTime LastSigned { get; set; }
        public bool Success { get; set; }
        public int SignCount { get; set; }

        public string FileName
        {
            get { return Path.GetFileName(FilePath); }
        }

        public string Folder
        {
            get { return Path.GetDirectoryName(FilePath) ?? string.Empty; }
        }

        public bool Exists
        {
            get { return File.Exists(FilePath); }
        }
    }

    /// <summary>
    /// Keeps the list of files signed with the app (most recent first).
    /// </summary>
    public sealed class SignHistory
    {
        public const int MaxEntries = 100;

        private readonly string _filePath;

        public IList<SignHistoryEntry> Entries { get; private set; }

        public SignHistory()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DigicertCodeSign");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, "history.xml");
            Entries = new List<SignHistoryEntry>();
            Load();
        }

        public void Record(string filePath, bool success)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            string fullPath = Path.GetFullPath(filePath);

            SignHistoryEntry entry = Find(fullPath);
            if (entry != null)
                Entries.Remove(entry);
            else
                entry = new SignHistoryEntry { FilePath = fullPath };

            entry.LastSigned = DateTime.Now;
            entry.Success = success;
            entry.SignCount++;

            Entries.Insert(0, entry);

            while (Entries.Count > MaxEntries)
                Entries.RemoveAt(Entries.Count - 1);

            Save();
        }

        public void Remove(IEnumerable<string> filePaths)
        {
            foreach (string path in filePaths.ToList())
            {
                SignHistoryEntry entry = Find(path);
                if (entry != null)
                    Entries.Remove(entry);
            }

            Save();
        }

        private SignHistoryEntry Find(string filePath)
        {
            return Entries.FirstOrDefault(e =>
                string.Equals(e.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
                return;

            try
            {
                XDocument doc = XDocument.Load(_filePath);
                if (doc.Root == null)
                    return;

                foreach (XElement item in doc.Root.Elements("File"))
                {
                    string path = (string)item.Attribute("path");
                    if (string.IsNullOrEmpty(path) || Find(path) != null)
                        continue;

                    DateTime lastSigned;
                    DateTime.TryParse(
                        (string)item.Attribute("lastSigned"),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out lastSigned);

                    int count;
                    int.TryParse((string)item.Attribute("count"), out count);

                    Entries.Add(new SignHistoryEntry
                    {
                        FilePath = path,
                        LastSigned = lastSigned,
                        Success = (string)item.Attribute("success") == "true",
                        SignCount = count
                    });
                }

                while (Entries.Count > MaxEntries)
                    Entries.RemoveAt(Entries.Count - 1);
            }
            catch
            {
                Entries = new List<SignHistoryEntry>();
            }
        }

        private void Save()
        {
            XDocument doc = new XDocument(
                new XElement("SignHistory",
                    Entries.Select(e => new XElement("File",
                        new XAttribute("path", e.FilePath),
                        new XAttribute("lastSigned", e.LastSigned.ToString("o", CultureInfo.InvariantCulture)),
                        new XAttribute("success", e.Success ? "true" : "false"),
                        new XAttribute("count", e.SignCount.ToString(CultureInfo.InvariantCulture))))));

            doc.Save(_filePath);
        }
    }
}
