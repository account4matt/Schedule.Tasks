using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Runtime.Remoting;

namespace Schedule.Tasks.Host.WindowsService
{
    public partial class WSHost : ServiceBase
    {
        public WSHost()
        {
            InitializeComponent();
            this.CanPauseAndContinue = true;
        }

        protected override void OnStart(string[] args)
        {
            Schedule.Tasks.Runtime.Instance.Start();
            try
            {
                RemotingConfiguration.Configure(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schedule.Tasks.Host.WindowsService.exe.config"), false);
            }
            finally { }
        }

        protected override void OnStop()
        {
            Schedule.Tasks.Runtime.Instance.Stop();
            base.OnStop();
        }

        protected override void OnPause()
        {
            Schedule.Tasks.Runtime.Instance.Pause();
            base.OnPause();
        }

        protected override void OnContinue()
        {
            Schedule.Tasks.Runtime.Instance.Continue();
            base.OnContinue();
        }
    }
}
