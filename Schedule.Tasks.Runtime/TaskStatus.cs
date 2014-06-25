using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schedule.Tasks
{
    public sealed class TaskStatus : MarshalByRefObject
    {
        public TaskStatus(TaskBase task, TaskScheduleBase taskSchedule)
        {
            this.Task = task;
            this.TaskSchedule = taskSchedule;
        }
        /// <summary>
        /// Task pertain to.
        /// </summary>
        public TaskBase Task { get; private set; }

        /// <summary>
        /// TaskSchedule pertain to.
        /// </summary>
        public TaskScheduleBase TaskSchedule { get; set; }

        /// <summary>
        /// Has executed times
        /// </summary>
        public long ExecutedTimes { get; set; }

        public DateTime LatestTime
        {
            get { return this.TaskSchedule.LatestTime; }
            set { this.TaskSchedule.LatestTime = value; }
        }

        /// <summary>
        /// Indicate whether the task was enable.
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Indicate whether has next time.
        /// </summary>
        public bool HasNextTime
        {
            get
            {
                return this.Enable 
                    && ExecutedTimes < this.TaskSchedule.MaxTimes 
                    && this.TaskSchedule.ToTime >= DateTime.Now;
            }
        }

        /// <summary>
        /// Indicat whether the task can be executed.
        /// </summary>
        public bool CanBeExecuted
        {
            get
            {
                return this.Enable 
                    && ExecutedTimes < this.TaskSchedule.MaxTimes 
                    && this.TaskSchedule.FromTime <= DateTime.Now 
                    && this.TaskSchedule.ToTime >= DateTime.Now;
            }
        }

        /// <summary>
        /// 暂存下次时间
        /// </summary>
        DateTime _NextTime = DateTime.MinValue;

        public DateTime NextTime
        {
            get
            {
                if (!HasNextTime)
                    return DateTime.MaxValue;
                if(_NextTime <= DateTime.Now)
                    _NextTime = this.TaskSchedule.GetNextTime();
                return _NextTime;
            }
        }

        public bool Executing { get; set; }

        public string TaskType { get; set; }

        public string ScheduleName { get { return this.TaskSchedule.Name; } }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
