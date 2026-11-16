using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace WorstPC
{
    internal static class Program
    {
        private const string MUTEX_NAME = "WORST_PC_SINGLETON";
        private const string PIPE_NAME = "WORST_PC_PIPE";

        private static Mutex _single;
        private static MainForm _form;

        [STAThread]
        private static void Main(string[] args)
        {
            string protoArg = null;
            foreach (string a in args)
                if (a != null && a.StartsWith("worstdash:", StringComparison.OrdinalIgnoreCase)) protoArg = a;

            bool created;
            _single = new Mutex(true, MUTEX_NAME, out created);

            if (!created)
            {
                // Another copy is already running. If we were launched by the login redirect,
                // hand the URL to it; otherwise just exit.
                if (protoArg != null) SendToRunning(protoArg);
                return;
            }

            TryRegisterScheme();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _form = new MainForm();
            StartPipeServer();

            if (protoArg != null) _form.QueueProtocol(protoArg);

            Application.Run(_form);
        }

        // ---- single-instance IPC: deliver a worstdash:// url to the running copy ----

        private static void SendToRunning(string url)
        {
            try
            {
                using (NamedPipeClientStream c = new NamedPipeClientStream(".", PIPE_NAME, PipeDirection.Out))
                {
                    c.Connect(3000);
                    using (StreamWriter w = new StreamWriter(c)) { w.WriteLine(url); w.Flush(); }
                }
            }
            catch { }
        }

        private static void StartPipeServer()
        {
            Thread t = new Thread(PipeLoop);
            t.IsBackground = true;
            t.Start();
        }

        private static void PipeLoop()
        {
            while (true)
            {
                try
                {
                    using (NamedPipeServerStream s = new NamedPipeServerStream(PIPE_NAME, PipeDirection.In))
                    {
                        s.WaitForConnection();
                        using (StreamReader r = new StreamReader(s))
                        {
                            string url = r.ReadLine();
                            if (!string.IsNullOrEmpty(url) && _form != null)
                            {
                                try { _form.BeginInvoke((Action)(() => _form.QueueProtocol(url))); } catch { }
                            }
                        }
                    }
                }
                catch { Thread.Sleep(500); }
            }
        }

        // ---- register worstdash:// to this exe (per-user, no admin) ----

        private static void TryRegisterScheme()
        {
            try
            {
                string exe = Application.ExecutablePath;
                using (RegistryKey root = Registry.CurrentUser.CreateSubKey(@"Software\Classes\worstdash"))
                {
                    root.SetValue("", "URL:WORST login");
                    root.SetValue("URL Protocol", "");
                    using (RegistryKey cmd = root.CreateSubKey(@"shell\open\command"))
                        cmd.SetValue("", "\"" + exe + "\" \"%1\"");
                }
            }
            catch { }
        }
    }
}
