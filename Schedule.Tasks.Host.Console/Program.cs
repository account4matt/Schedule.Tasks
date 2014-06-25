using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;

namespace Schedule.Tasks.Host.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Schedule.Tasks.Runtime.Instance.Start();
            try
            {
                RemotingConfiguration.Configure(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schedule.Tasks.Host.Console.exe.config"), false);
            }
            finally { }
            System.Console.ReadKey();
            Schedule.Tasks.Runtime.Instance.Stop();
        }
    }
}
