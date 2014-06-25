using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schedule.Tasks
{
    public abstract class TaskScheduleBase : MarshalByRefObject,ITaskSchedule
    {
        /// <summary>
        /// 日程名称
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 日程设置字符串
        /// </summary>
        public virtual string ScheduleString { get; set; }

        /// <summary>
        /// 最近一次时间
        /// </summary>
        public virtual DateTime LatestTime { get; set; }

        /// <summary>
        /// 有效开始时间
        /// </summary>
        public virtual DateTime FromTime { get; set; }

        /// <summary>
        /// 有效结束时间
        /// </summary>
        public virtual DateTime ToTime { get; set; }

        /// <summary>
        /// 最多执行次数
        /// </summary>
        public virtual long MaxTimes { get; set; }

        /// <summary>
        /// 取下一次时间
        /// </summary>
        /// <returns></returns>
        public abstract DateTime GetNextTime();

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
