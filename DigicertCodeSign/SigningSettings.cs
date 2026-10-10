using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DigicertCodeSign
{
    /// <summary>
    /// signtool options: certificate, timestamp server, signtool location and the file signed on startup.
    /// Stored in %APPDATA%\DigicertCodeSign\signing.xml.
    /// </summary>
    public sealed class SigningSettings
    {
        public const string DefaultTimestampUrl = "http://timestamp.digicert.com";

        private readonly string _filePath;

        /// <summary>Explicit signtool.exe path; empty means auto-detect from the Windows SDK.</summary>
        public string SignToolPath { get; set; }

        /// <summary>SHA1 thumbprint of the code signing certificate (passed to signtool /sha1).</summary>
        public string CertificateSha1 { get; set; }

        public string TimestampUrl { get; set; }

        /// <summary>File signed automatically when the app starts; empty disables startup signing.</summary>
        public string StartupFile { get; set; }

        public SigningSettings()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DigicertCodeSign");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, "signing.xml");
            SignToolPath = string.Empty;
            CertificateSha1 = string.Empty;
            TimestampUrl = DefaultTimestampUrl;
            StartupFile = string.Empty;
            Load();
        }

        public bool HasCertificate
        {
            get { return !string.IsNullOrEmpty(CertificateSha1); }
        }

        public bool HasStartupFile
        {
            get { return !string.IsNullOrEmpty(StartupFile); }
        }

        /// <summary>
        /// The configured signtool.exe, or the newest one found in the Windows 10/11 SDK.
        /// Returns null when none is found.
        /// </summary>
        public string ResolveSignToolPath()
        {
            if (!string.IsNullOrEmpty(SignToolPath))
                return SignToolPath;

            return FindSignTool();
        }

        public static string FindSignTool()
        {
            string programFiles = Environment.GetFolderPath(
                Environment.Is64BitOperatingSystem
                    ? Environment.SpecialFolder.ProgramFilesX86
                    : Environment.SpecialFolder.ProgramFiles);

            string kitBin = Path.Combine(programFiles, @"Windows Kits\10\bin");
            if (!Directory.Exists(kitBin))
                return null;

            string[] architectures = Environment.Is64BitOperatingSystem
                ? new[] { "x64", "x86" }
                : new[] { "x86" };

            // Versioned layout: bin\10.0.x.y\<arch>\signtool.exe (newest first), then the legacy bin\<arch>.
            List<string> folders = Directory.GetDirectories(kitBin)
                .Select(dir => new { Dir = dir, Version = ParseVersion(Path.GetFileName(dir)) })
                .Where(x => x.Version != null)
                .OrderByDescending(x => x.Version)
                .Select(x => x.Dir)
                .ToList();
            folders.Add(kitBin);

            foreach (string folder in folders)
            {
                foreach (string arch in architectures)
                {
                    string candidate = Path.Combine(Path.Combine(folder, arch), "signtool.exe");
                    if (File.Exists(candidate))
                        return candidate;
                }
            }

            return null;
        }

        private static Version ParseVersion(string text)
        {
            try
            {
                return new Version(text);
            }
            catch
            {
                return null;
            }
        }

        public static string NormalizeThumbprint(string thumbprint)
        {
            if (thumbprint == null)
                return string.Empty;

            // Thumbprints copied from certmgr contain spaces and sometimes an invisible LRM mark.
            return new string(thumbprint.Where(Uri.IsHexDigit).ToArray()).ToUpperInvariant();
        }

        public void Save()
        {
            XDocument doc = new XDocument(
                new XElement("Signing",
                    new XElement("SignToolPath", SignToolPath ?? string.Empty),
                    new XElement("CertificateSha1", CertificateSha1 ?? string.Empty),
                    new XElement("TimestampUrl", TimestampUrl ?? string.Empty),
                    new XElement("StartupFile", StartupFile ?? string.Empty)));

            doc.Save(_filePath);
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

                SignToolPath = ((string)root.Element("SignToolPath") ?? string.Empty).Trim();
                CertificateSha1 = NormalizeThumbprint((string)root.Element("CertificateSha1"));
                TimestampUrl = ((string)root.Element("TimestampUrl") ?? DefaultTimestampUrl).Trim();
                StartupFile = ((string)root.Element("StartupFile") ?? string.Empty).Trim();
            }
            catch
            {
                // Keep defaults on a corrupt file.
            }
        }
    }
}
