using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Schedule.Tasks.Proxy;

namespace Schedule.Tasks.Remoting
{
    public class RuntimeProxy : MarshalByRefObject, IRuntimeProxy
    {
        public void Start()
        {
            Schedule.Tasks.Runtime.Instance.Start();
        }

        public void Stop()
        {
            Schedule.Tasks.Runtime.Instance.Stop();
        }

        public void Pause()
        {
            Schedule.Tasks.Runtime.Instance.Pause();
        }

        public void Continue()
        {
            Schedule.Tasks.Runtime.Instance.Continue();
        }

        public void PauseTask(string key)
        {
            Schedule.Tasks.Runtime.Instance.PauseTask(key);
        }

        public void ContinueTask(string key)
        {
            Schedule.Tasks.Runtime.Instance.ContinueTask(key);
        }

        public void RunTask(string key)
        {
            Schedule.Tasks.Runtime.Instance.RunTask(key);
        }

        public List<string> Tasks()
        {
            Dictionary<string, TaskStatus> dict = Schedule.Tasks.Runtime.Instance.TaskStatus;
            List<string> tasks = new List<string>();
            foreach (string key in dict.Keys)
            {
                TaskStatus status = dict[key];
                Timer timer = Schedule.Tasks.Runtime.Instance.Timers[key];
                tasks.Add(string.Format("{0}|{1}|{2}|{3}({11})|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{12}", key, status.Enable, timer.Enabled, status.ScheduleName, status.LatestTime, status.NextTime, status.TaskType, timer.Interval, status.TaskSchedule.FromTime, status.TaskSchedule.ToTime, status.ExecutedTimes, status.TaskSchedule.ScheduleString, status.Executing));
            }
            return tasks;
        }

    }
}
