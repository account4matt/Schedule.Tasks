using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace Schedule.Tasks.InBuilts
{
    /// <summary>
    /// 调用可执行文件
    /// </summary>
    ///<remarks>需要配置config的InvokeExeFileTask_ExeFilePath：例：<add key="InvokeExeFileTask_ExeFilePath" value="e:\exefile.exe"/></remarks>
    ///<remarks>可选择配置config的InvokeExeFileTask_StartParameter：例：<add key="InvokeExeFileTask_StartParameter" value=""/></remarks>
    public class InvokeExeFileTask : TaskBase
    {
        protected Process _Process = new Process();
        protected override void Execute()
        {
            string filePath = System.Configuration.ConfigurationManager.AppSettings["InvokeExeFileTask_ExeFilePath"];
            bool singleton = false;
            bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["InvokeExeFileTask_RunSingleton"], out singleton);
            if (singleton)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                Process[] ps = Process.GetProcessesByName(fileName);
                foreach (Process p in ps)
                {
                    try
                    {
                        p.Kill();
                    }
                    catch{}
                }
            }
            _Process.StartInfo.FileName = filePath;
            _Process.StartInfo.WorkingDirectory = Path.GetDirectoryName(filePath);
            _Process.StartInfo.CreateNoWindow = true;
            _Process.StartInfo.Arguments = string.Format("{0}", System.Configuration.ConfigurationManager.AppSettings["InvokeExeFileTask_StartParameter"]);
            _Process.Start();
            _Process.WaitForExit();
        }

        public override void Stop()
        {
            try
            {
                _Process.Kill();
            }
            catch { }
            finally { }
        }
    }
}
