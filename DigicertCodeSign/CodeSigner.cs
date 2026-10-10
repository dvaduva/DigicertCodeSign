using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace DigicertCodeSign
{
    public sealed class SignResult
    {
        public bool Success { get; set; }
        public int ExitCode { get; set; }
        public string Output { get; set; }
        public string ErrorMessage { get; set; }
        public string FilePath { get; set; }
    }

    /// <summary>
    /// Runs signtool with the configured DigiCert certificate.
    /// </summary>
    public static class CodeSigner
    {
        public static SignResult SignFile(string filePath, SigningSettings settings, int timeoutMilliseconds)
        {
            SignResult result = new SignResult
            {
                FilePath = filePath,
                ExitCode = -1,
                Output = string.Empty
            };

            string signTool = settings.ResolveSignToolPath();
            if (string.IsNullOrEmpty(signTool))
            {
                result.ErrorMessage = "signtool.exe was not found. Install the Windows SDK or set its path in Configuration.";
                return result;
            }

            if (!File.Exists(signTool))
            {
                result.ErrorMessage = "signtool.exe was not found at:\r\n" + signTool;
                return result;
            }

            if (!settings.HasCertificate)
            {
                result.ErrorMessage = "No certificate thumbprint configured. Set it in Configuration.";
                return result;
            }

            if (!File.Exists(filePath))
            {
                result.ErrorMessage = "File to sign was not found:\r\n" + filePath;
                return result;
            }

            StringBuilder arguments = new StringBuilder("sign");
            if (!string.IsNullOrEmpty(settings.TimestampUrl))
                arguments.AppendFormat(" /tr \"{0}\" /td sha256", settings.TimestampUrl);
            arguments.AppendFormat(" /fd sha256 /sha1 {0} \"{1}\"", settings.CertificateSha1, filePath);

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = signTool,
                    Arguments = arguments.ToString(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(filePath) ?? Environment.CurrentDirectory
                };

                StringBuilder output = new StringBuilder();

                using (Process process = new Process())
                {
                    process.StartInfo = psi;
                    process.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                            output.AppendLine(e.Data);
                    };
                    process.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                            output.AppendLine(e.Data);
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    bool finished = process.WaitForExit(timeoutMilliseconds);
                    if (!finished)
                    {
                        try { process.Kill(); }
                        catch { }

                        result.ErrorMessage = "Signing timed out after " + (timeoutMilliseconds / 1000) + " seconds.";
                        result.Output = output.ToString();
                        return result;
                    }

                    // Ensure async readers finish
                    process.WaitForExit();

                    result.ExitCode = process.ExitCode;
                    result.Output = output.ToString().Trim();
                    result.Success = process.ExitCode == 0;

                    if (!result.Success)
                    {
                        result.ErrorMessage = "signtool failed with exit code " + process.ExitCode + ".";
                        if (!string.IsNullOrEmpty(result.Output))
                            result.ErrorMessage += "\r\n\r\n" + result.Output;
                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = "Could not run signtool:\r\n" + ex.Message;
            }

            return result;
        }
    }
}
