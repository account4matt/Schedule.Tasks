using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.Remoting;

namespace Schedule.Tasks.Host.Background
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Schedule.Tasks.Runtime.Instance.Start();
            try
            {
                RemotingConfiguration.Configure(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schedule.Tasks.Host.Background.exe.config"), false);
            }
            finally { }

            Application.Run();
        }
    }
}
