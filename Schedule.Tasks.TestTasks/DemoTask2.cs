using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schedule.Tasks.TestTasks
{
    public class DemoTask2 : TaskBase
    {
        protected override void Execute()
        {
            Console.WriteLine("The DemoTask2:{0}",DateTime.Now.ToString());
        }

        public void PublicMethod()
        {
            Console.WriteLine("The DemoTask2 PublicMethod:{0}", DateTime.Now.ToString());
        }
    }
}
