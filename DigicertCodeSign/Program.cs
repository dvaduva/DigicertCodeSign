using System;
using System.Windows.Forms;

namespace DigicertCodeSign
{
    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            if (CommandLine.IsCommandLine(args))
                return CommandLine.Run(args);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return 0;
        }
    }
}
