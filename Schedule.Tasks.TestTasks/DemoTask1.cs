using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Schedule.Tasks.TestTasks
{
    public class DemoTask1 : TaskBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected override void Execute()
        {
            log.Info(string.Format("Repository.Name:{0},Logger.Name:{1}", log.Logger.Repository.Name, log.Logger.Name));
            //throw new Exception("错了，还是错了");
        }
    }
}
