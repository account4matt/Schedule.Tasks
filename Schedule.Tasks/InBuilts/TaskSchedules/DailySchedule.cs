using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schedule.Tasks.InBuilts
{
    public class DailySchedule : TaskScheduleBase
    {
        public List<DateTime> TimeArray { get; set; }

        protected bool _Init = false;

        /// <summary>
        /// 初始化数据
        /// </summary>
        protected virtual void InitData()
        {
            List<DateTime> timeArray = new List<DateTime>();
            if (!string.IsNullOrEmpty(this.ScheduleString))
            {
                string[] timestrs = this.ScheduleString.Split(';');
                foreach (string timestr in timestrs)
                {
                    DateTime time = DateTime.Parse(timestr);
                    timeArray.Add(time);
                }
            }
            this._Init = true;
            this.TimeArray = timeArray;
        }

        public override DateTime GetNextTime()
        {
            if (!_Init)
                InitData();

            DateTime nextTime = DateTime.Now;
            if (FromTime <= DateTime.Now)
            {
                bool found = false;
                for (int i = 0; i < TimeArray.Count; i++)
                {
                    DateTime temp = DateTime.Parse(TimeArray[i].ToLongTimeString());
                    if (temp > DateTime.Now)
                    {
                        nextTime = temp;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    nextTime = DateTime.Parse(TimeArray[0].ToLongTimeString()).AddDays(1);
            }
            else
            {
                bool found = false;
                int dates = (FromTime.Date - DateTime.Now.Date).Days;
                for (int i = 0; i < TimeArray.Count; i++)
                {
                    DateTime temp = DateTime.Parse(TimeArray[i].ToLongTimeString()).AddDays(dates);
                    if (temp >= FromTime)
                    {
                        nextTime = temp;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    nextTime = DateTime.Parse(TimeArray[0].ToLongTimeString()).AddDays(dates + 1);
            }
            return nextTime;
        }
    }
}
