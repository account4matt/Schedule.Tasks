using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schedule.Tasks.TestTasks
{
    public class DemoExceptionTask1 : TaskBase
    {
        protected override void Execute()
        {
            throw new Exception(string.Format("我错了，{0}",DateTime.Now));
        }
    }
}
