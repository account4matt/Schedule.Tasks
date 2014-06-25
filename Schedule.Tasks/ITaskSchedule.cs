using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schedule.Tasks
{
    public interface ITaskSchedule
    {
        /// <summary>
        /// get the next time from now
        /// </summary>
        /// <returns></returns>
        DateTime GetNextTime();
    }
}
