using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schedule.Tasks.InBuilts
{
    public class IntervalSchedule : TaskScheduleBase
    {
        protected long _Interval = 0;

        const long _DefaultInterval = 600000;

        protected bool _Init = false;

        /// <summary>
        /// 初始化数据
        /// </summary>
        protected virtual void InitData()
        {
            long interval;
            if (!long.TryParse(this.ScheduleString, out interval))
                interval = _DefaultInterval;
            _Interval = interval;
            _Init = true;
        }

        public override DateTime GetNextTime()
        {
            if (!_Init)
                InitData();

            DateTime nextTime;
            if (FromTime <= DateTime.Now)
            {
                nextTime = this.LatestTime.AddMilliseconds(_Interval);
                if (nextTime <= DateTime.Now) 
                    nextTime = DateTime.Now.AddMilliseconds(_Interval);
            }
            else
            {
                nextTime = FromTime;
            }
            return nextTime;
        }
    }
}
