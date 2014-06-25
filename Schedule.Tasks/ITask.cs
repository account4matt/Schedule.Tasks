using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schedule.Tasks
{
    public interface ITask
    {
        /// <summary>
        /// Run the task
        /// </summary>
        void Run();
    }
}
