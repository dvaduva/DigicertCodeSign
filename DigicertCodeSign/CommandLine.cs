using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace DigicertCodeSign
{
    /// <summary>
    /// Headless mode: DigicertCodeSign.exe --sign &lt;file&gt; [&lt;file&gt; ...]
    /// Signs the files with the saved configuration, answers the Token Logon dialog
    /// and exits with 0 on success, 1 when signing failed, 2 on bad arguments or configuration.
    /// </summary>
    public static class CommandLine
    {
        public const int ExitSuccess = 0;
        public const int ExitSignFailed = 1;
        public const int ExitUsage = 2;

        private const int SignTimeoutMs = 180000; // 3 minutes (token prompt + timestamp)

        private const int AttachParentProcess = -1;
        private const int StdOutputHandle = -11;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int processId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int handle);

        /// <summary>True when the arguments ask for command line mode instead of the window.</summary>
        public static bool IsCommandLine(string[] args)
        {
            return args != null && args.Length > 0;
        }

        public static int Run(string[] args)
        {
            AttachParentConsole();

            int exitCode = ExitUsage;

            // UI Automation clients should run in the MTA; the event handler then works without a message loop.
            Thread worker = new Thread(() => exitCode = Execute(args));
            worker.SetApartmentState(ApartmentState.MTA);
            worker.Start();
            worker.Join();

            return exitCode;
        }

        private static int Execute(string[] args)
        {
            string first = args[0].ToLowerInvariant();

            if (first == "--help" || first == "-h" || first == "/?" || first == "-?")
            {
                PrintUsage(Console.Out);
                return ExitSuccess;
            }

            if (first != "--sign" && first != "-s" && first != "/sign")
            {
                Console.Error.WriteLine("Unknown option: " + args[0]);
                PrintUsage(Console.Error);
                return ExitUsage;
            }

            List<string> files = new List<string>();
            for (int i = 1; i < args.Length; i++)
            {
                if (!string.IsNullOrEmpty(args[i].Trim()))
                    files.Add(Path.GetFullPath(args[i].Trim()));
            }

            if (files.Count == 0)
            {
                Console.Error.WriteLine("--sign needs at least one file.");
                PrintUsage(Console.Error);
                return ExitUsage;
            }

            PasswordStore store = new PasswordStore();
            SigningSettings signing = new SigningSettings();

            if (!store.HasPassword || !signing.HasCertificate)
            {
                Console.Error.WriteLine(
                    "Set the DigiCert token password and the certificate in Configuration first " +
                    "(start DigicertCodeSign.exe without arguments).");
                return ExitUsage;
            }

            SignHistory history = new SignHistory();
            int failed = 0;

            using (SafeNetAutoLogin autoLogin = new SafeNetAutoLogin(store.CurrentPassword, Console.WriteLine))
            {
                autoLogin.TryLoginExistingWindows();
                autoLogin.StartWatching();

                // Small delay so the watcher is registered before signtool starts.
                Thread.Sleep(400);

                for (int i = 0; i < files.Count; i++)
                {
                    string file = files[i];
                    Console.WriteLine(string.Format("Signing {0}/{1}: {2}", i + 1, files.Count, file));

                    SignResult result = CodeSigner.SignFile(file, signing, SignTimeoutMs);
                    Record(history, result);

                    if (result.Success)
                    {
                        if (!string.IsNullOrEmpty(result.Output))
                            Console.WriteLine(result.Output);
                        Console.WriteLine("OK");
                        continue;
                    }

                    failed++;
                    Console.Error.WriteLine(string.IsNullOrEmpty(result.ErrorMessage) ? "Unknown signing error." : result.ErrorMessage);

                    // signtool missing, timeout or crash: the next files would fail the same way.
                    if (result.ExitCode == -1 && File.Exists(file))
                    {
                        int skipped = files.Count - i - 1;
                        if (skipped > 0)
                            Console.Error.WriteLine(skipped + " file(s) were not attempted.");
                        failed += skipped;
                        break;
                    }
                }
            }

            Console.WriteLine(failed == 0
                ? (files.Count == 1 ? "Signing succeeded." : "Signed " + files.Count + " files.")
                : string.Format("Signed {0} of {1} files.", files.Count - failed, files.Count));

            return failed == 0 ? ExitSuccess : ExitSignFailed;
        }

        private static void Record(SignHistory history, SignResult result)
        {
            // Same rule as the window: files that do not exist are not added to history.
            if (!File.Exists(result.FilePath))
                return;

            try
            {
                history.Record(result.FilePath, result.Success);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Could not save signing history: " + ex.Message);
            }
        }

        private static void PrintUsage(TextWriter writer)
        {
            writer.WriteLine("Usage:");
            writer.WriteLine("  DigicertCodeSign.exe                            open the window");
            writer.WriteLine("  DigicertCodeSign.exe --sign <file> [<file>...]  sign files and exit");
            writer.WriteLine("  DigicertCodeSign.exe --help                     show this help");
            writer.WriteLine();
            writer.WriteLine("Exit codes: 0 = signed, 1 = signing failed, 2 = bad arguments or configuration.");
        }

        // The app is a WinExe: write to the console it was started from, unless output is already redirected.
        private static void AttachParentConsole()
        {
            IntPtr stdout = GetStdHandle(StdOutputHandle);
            if (stdout == IntPtr.Zero || stdout == new IntPtr(-1))
                AttachConsole(AttachParentProcess);
        }
    }
}
