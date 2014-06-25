using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using log4net;
using System.Reflection;
using System.IO;

namespace Schedule.Tasks
{
    public sealed class Runtime : MarshalByRefObject
    {
        #region singleton

        private Runtime()
        {
            _MainTimer = new Timer(_MainTimerInterval);
            _MainTimer.Elapsed += new ElapsedEventHandler(_MainTimer_Elapsed);

            _Watcher = new FileSystemWatcher(AppDomain.CurrentDomain.BaseDirectory, "Schedule.Tasks.Runtime.dll.config");
            _Watcher.Changed += new FileSystemEventHandler(_Watcher_Changed);
        }

        private static readonly Runtime _instance = new Runtime();
        /// <summary>
        /// Singleton入口
        /// </summary>
        public static Runtime Instance { get { return _instance; } }

        #endregion

        #region fields

        /// <summary>
        /// 存放应用域，服务，服务状态，以及服务对应的计时器
        /// </summary>
        private Dictionary<string, AppDomain> _AppDomains = new Dictionary<string, AppDomain>();
        private Dictionary<string, TaskBase> _Tasks = new Dictionary<string, TaskBase>();
        private Dictionary<string, TaskStatus> _TaskStatus = new Dictionary<string, TaskStatus>();
        private Dictionary<string, Timer> _Timers = new Dictionary<string, Timer>();
        private Dictionary<Timer, TaskBase> _TimerTasks = new Dictionary<Timer, TaskBase>();

        static readonly ILog _Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 全局计时器的间隔时间
        /// </summary>
        private const double _MainTimerInterval = 6 * 60 * 60 * 1000;
        /// <summary>
        /// 全局计时器，间隔去激活处于在有效时间范围内未激活的服务
        /// </summary>
        private Timer _MainTimer = null;

        /// <summary>
        /// 全局锁资源
        /// </summary>
        private object _MutexObjet = new object();

        /// <summary>
        /// 服务配置文件最后被修改时间
        /// </summary>
        DateTime _LastModifyTime = DateTime.Now;
        /// <summary>
        /// 用于监视服务配置文件的变化
        /// </summary>
        private FileSystemWatcher _Watcher = null;

        private readonly string _ShadowCopyRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ShadowCopy");

        #endregion

        #region public

        /// <summary>
        /// 启动运行时
        /// </summary>
        public void Start()
        {
            _Logger.Info("Runtime Starting.");
            ClearShadowCopyFiles();
            LoadTasks();
            SetTimers();
            _Watcher.EnableRaisingEvents = true;
            _MainTimer.Start();
            _Logger.Info("Runtime Started.");
        }

        /// <summary>
        /// 停止运行时
        /// </summary>
        public void Stop()
        {
            _Logger.Info("Runtime Stopping.");
            _Watcher.EnableRaisingEvents = false;
            _MainTimer.Stop();
            CancelTasks();
            ClearShadowCopyFiles();
            _Logger.Info("Runtime Stopped.");
        }

        /// <summary>
        /// 暂停运行时
        /// </summary>
        public void Pause()
        {
            _Logger.Info("Runtime Pausing.");
            _Watcher.EnableRaisingEvents = false;
            _MainTimer.Stop();
            foreach (Timer timer in _Timers.Values)
            {
                timer.Stop();
            }
            _Logger.Info("Runtime Paused.");
        }

        /// <summary>
        /// 继续运行时
        /// </summary>
        public void Continue()
        {
            _Logger.Info("Runtime Continueing.");
            SetTimers();
            _MainTimer.Start();
            _Watcher.EnableRaisingEvents = true;
            _Logger.Info("Runtime Continued.");
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="key"></param>
        public void PauseTask(string key)
        {
            if (_TaskStatus[key].Enable == true)
            {
                _Logger.InfoFormat("Task {0}({1}) Pausing.", key, _TaskStatus[key].TaskType);
                _TaskStatus[key].Enable = false;
                try
                {
                    _TaskStatus[key].Task.Stop();
                }
                catch(Exception ex) {
                    _Logger.Error("Task Stop Error", ex);
                }
                SetTaskTimer(_TaskStatus[key]);
                _Logger.InfoFormat("Task {0}({1}) Paused.", key, _TaskStatus[key].TaskType);
            }
        }

        /// <summary>
        /// 继续任务
        /// </summary>
        /// <param name="key"></param>
        public void ContinueTask(string key)
        {
            if (_TaskStatus[key].Enable == false)
            {
                _Logger.InfoFormat("Task {0}({1}) Continueing.", key, _TaskStatus[key].TaskType);
                _TaskStatus[key].Enable = true;
                SetTaskTimer(_TaskStatus[key]);
                _Logger.InfoFormat("Task {0}({1}) Continued.", key, _TaskStatus[key].TaskType);
            }
        }

        /// <summary>
        /// 手动运行任务
        /// </summary>
        /// <param name="key"></param>
        public void RunTask(string key)
        {
            TaskStatus status = _TaskStatus[key];
            _Logger.InfoFormat("Manual Run Task {0}({1}).", key, status.TaskType);
            if (status.CanBeExecuted)
                RunTask(status);
        }

        public Dictionary<string, TaskStatus> TaskStatus
        {
            get { return _TaskStatus; }
        }

        public Dictionary<string, Timer> Timers
        {
            get { return _Timers; }
        }

        #endregion

        #region register tasks

        /// <summary>
        /// 从配置文件读取任务服务
        /// </summary>
        void LoadTasks()
        {
            CancelTasks();
            RegisterTasks();
        }

        /// <summary>
        /// 注册单个任务服务
        /// </summary>
        /// <param name="taskElement"></param>
        void RegisterTask(TaskElement taskElement)
        {
            string taskName = null;
            try
            {
                taskName = taskElement.Name;
                if (string.IsNullOrEmpty(taskName))
                    return;

                //创建到独立应用域
                AppDomain appDomain = CreateAppDomain(taskName, taskElement.AssemblyPath, taskElement.CustomConfigFile);
                TaskBase task = CreateTask(appDomain, taskName, taskElement.Type);

                if (task != null)
                {
                   //创建任务状态
                    TaskStatus status = CreateTaskStatus(appDomain, task, taskElement);
                    //把应用域与任务注册到运行时
                    RegisterToRuntime(appDomain, status);

                    _Logger.Info(string.Format("Task:{2}({0}) was Loaded.First Run Time:{1}", taskElement.Type, _TaskStatus[taskName].NextTime, taskName));
                }
            }
            catch (Exception ex)
            {
                if (!(ex is System.AppDomainUnloadedException))
                    _Logger.Error(string.Format("Register Task Error:{2}({0}),{1}", taskElement.Type, ex.Message, taskName), ex);
            }
        }

        /// <summary>
        /// 创建应用域
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="assemblyPath"></param>
        /// <param name="configFile"></param>
        /// <returns></returns>
        AppDomain CreateAppDomain(string appName,string assemblyPath,string configFile)
        {
            if (!string.IsNullOrEmpty(assemblyPath))
                assemblyPath = string.Format("{0}\\", assemblyPath.TrimStart('\\', '/'));
            string appBase = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                if (System.IO.Path.GetPathRoot(assemblyPath) == "")
                    assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyPath);
                else
                    appBase = assemblyPath;
            }

            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationName = appName;
            setup.ApplicationBase = appBase;
            setup.PrivateBinPath = string.IsNullOrEmpty(assemblyPath) ? null : Path.GetDirectoryName(assemblyPath);
            setup.CachePath = setup.ApplicationBase;
            setup.ShadowCopyFiles = "true";
            setup.ShadowCopyDirectories = setup.PrivateBinPath;
            setup.CachePath = _ShadowCopyRootPath;
            if (!string.IsNullOrEmpty(configFile))
                setup.ConfigurationFile = configFile;

            return AppDomain.CreateDomain(appName, null, setup);
        }

        /// <summary>
        /// 创建任务体
        /// </summary>
        /// <param name="appDomain"></param>
        /// <param name="taskName"></param>
        /// <param name="typeString"></param>
        /// <returns></returns>
        TaskBase CreateTask(AppDomain appDomain,string taskName, string typeString)
        {
            string[] type = typeString.Split(',');
            TaskBase task = appDomain.CreateInstanceAndUnwrap(type[1], type[0]) as TaskBase;
            task.Name = taskName;
            return task;
        }

        /// <summary>
        /// 创建任务状态
        /// </summary>
        /// <param name="appDomain"></param>
        /// <param name="task"></param>
        /// <param name="taskElement"></param>
        /// <returns></returns>
        TaskStatus CreateTaskStatus(AppDomain appDomain,TaskBase task, TaskElement taskElement)
        {
            bool enable;
            bool.TryParse(taskElement.Enable, out enable);
            TaskStatus status = null;
            TaskScheduleBase schedule = CreateTaskSchedule(appDomain, taskElement.Schedule);
            if (schedule != null)
            {
                status = new TaskStatus(task, schedule) { Enable = enable, TaskType = taskElement.Type };
            }
            return status;
        }

        /// <summary>
        /// 创建任务计划
        /// </summary>
        /// <param name="appDomain"></param>
        /// <param name="scheduleName"></param>
        /// <returns></returns>
        TaskScheduleBase CreateTaskSchedule(AppDomain appDomain, string scheduleName)
        {
            ScheduleElement scheduleElement = FindScheduleElement(scheduleName);
            TaskScheduleBase schedule = null;
            if (scheduleElement != null)
                schedule = CreateTaskSchedule(appDomain, scheduleElement);
            if (schedule == null)
                throw new Exception(string.Format("the schedule:{0} cann't be registered.", scheduleName));

            return schedule;
        }

        ScheduleElement FindScheduleElement(string scheduleName)
        {
            ScheduleTaskSection taskSection = ScheduleTaskSection.Instance;
            ScheduleElementCollection sec = taskSection.Schedules;
            ScheduleElement scheduleElement = null;
            for (int i = 0; i < sec.Count; i++)
            {
                if (string.Compare(scheduleName, sec[i].Name, true) == 0)
                {
                    scheduleElement = sec[i];
                    break;
                }
            }
            return scheduleElement;
        }

        TaskScheduleBase CreateTaskSchedule(AppDomain appDomain, ScheduleElement scheduleElement)
        {
            TaskScheduleBase schedule = null;
            if (scheduleElement != null)
            {
                string[] type = scheduleElement.Type.Split(',');
                schedule = appDomain.CreateInstanceAndUnwrap(type[1], type[0]) as TaskScheduleBase;
            }
            if (schedule != null)
                SetTaskSchedule(schedule, scheduleElement);
            return schedule;
        }

        void SetTaskSchedule(TaskScheduleBase schedule, ScheduleElement scheduleElement)
        {
            schedule.Name = scheduleElement.Name;
            long maxTimes;
            if (!long.TryParse(scheduleElement.MaxTimes, out maxTimes))
                maxTimes = long.MaxValue;
            DateTime fromtime;
            if (!DateTime.TryParse(scheduleElement.FromTime, out fromtime))
                fromtime = DateTime.MinValue;
            DateTime totime;
            if (!DateTime.TryParse(scheduleElement.ToTime, out totime))
                totime = DateTime.MaxValue;
            schedule.ScheduleString = scheduleElement.ScheduleString;
            schedule.MaxTimes = maxTimes;
            schedule.FromTime = fromtime;
            schedule.ToTime = totime;
        }

        void RegisterToRuntime(AppDomain appDomain,TaskStatus status)
        {
            string taskName = status.Task.Name;

            Timer timer = new Timer() { AutoReset = false };
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);

            _AppDomains[taskName] = appDomain;
            _Tasks[taskName] = status.Task;
            _TaskStatus[taskName] = status;
            if (_Timers.ContainsKey(taskName))
                _TimerTasks.Remove(_Timers[taskName]);
            _Timers[taskName] = timer;
            _TimerTasks[timer] = status.Task;
        }

        /// <summary>
        /// 注册新任务服务，注销取消的任务服务
        /// </summary>
        void RegisterTasks()
        {
            lock (_MutexObjet)
            {
                try
                {
                    ScheduleTaskSection taskSection = ScheduleTaskSection.Instance;
                    if (taskSection == null)
                    {
                        _Logger.Error("Read configuration schedule.tasks section error.");
                        return;
                    }
                    TaskElementCollection taskElementCollection = taskSection.Tasks;
                    if (taskElementCollection == null)
                    {
                        _Logger.Error("Can't find tasks in schedule.tasks section.");
                        return;
                    }
                    Dictionary<string, TaskElement> temp = new Dictionary<string, TaskElement>();
                    foreach (TaskElement taskElement in taskElementCollection)
                    {
                        temp[taskElement.Name] = taskElement;
                    }
                    TrimDeletedTasks(temp);
                    RegisterNewTasks(temp);
                }
                catch (Exception ex)
                {
                    if (!(ex is System.AppDomainUnloadedException))
                        _Logger.Error(string.Format("Register Tasks Error:{0}", ex.Message), ex);
                }
            }
        }

        void TrimDeletedTasks(Dictionary<string, TaskElement> newTaskDict)
        {
            Dictionary<string, bool> deleted = new Dictionary<string, bool>();
            //获取已取消的服务
            foreach (string key in _Tasks.Keys)
            {
                if (!newTaskDict.ContainsKey(key))
                    deleted[key] = true;
            }
            //注销已取消的服务
            foreach (string key in deleted.Keys)
            {
                CancelTask(key);
            }
        }

        void RegisterNewTasks(Dictionary<string, TaskElement> newTaskDict)
        {
            foreach (string key in newTaskDict.Keys)
            {
                //注册新服务
                if (!_Tasks.ContainsKey(key))
                    RegisterTask(newTaskDict[key]);
            }
        }

        /// <summary>
        /// 注销单个任务服务
        /// </summary>
        /// <param name="key"></param>
        void CancelTask(string key)
        {
            string typename = null;
            try
            {
                AppDomain appDomain = _AppDomains[key];
                string shadowPath = Path.Combine(appDomain.SetupInformation.CachePath, key);
                typename = _TaskStatus[key].TaskType;
                _Timers[key].Stop();
                _AppDomains.Remove(key);
                try
                {
                    _Tasks[key].Stop();
                }
                catch { }
                _Tasks.Remove(key);
                _TaskStatus.Remove(key);
                _TimerTasks.Remove(_Timers[key]);
                _Timers[key].Close();
                _Timers.Remove(key);
                AppDomain.Unload(appDomain);
                DeleteDir(shadowPath);
                _Logger.Info(string.Format("Task:{1}({0}) was Cancelled.", typename, key));
            }
            catch (Exception ex)
            {
                _Logger.Error(string.Format("Cancel Task {2}({1}) Error:{0}", ex.Message, typename, key), ex);
            }
        }

        /// <summary>
        /// 注销全部服务
        /// </summary>
        void CancelTasks()
        {
            lock (_MutexObjet)
            {
                string[] keys = _Tasks.Keys.ToArray();
                foreach (string key in keys)
                {
                    CancelTask(key);
                }
            }
        }

        /// <summary>
        /// 消除全部影子文件
        /// </summary>
        void ClearShadowCopyFiles()
        {
            DeleteDir(_ShadowCopyRootPath);
        }

        void DeleteDir(string dir)
        {
            try
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
            catch { }
            finally { }
        }

        #endregion

        #region run task

        /// <summary>
        /// 执行服务
        /// </summary>
        /// <param name="status">执行的服务状态</param>
        void RunTask(TaskStatus status)
        {
            lock (status)
            {
                string taskName = status.Task.Name;
                try
                {
                    status.Executing = true;
                    status.LatestTime = DateTime.Now;
                    status.ExecutedTimes++;
                    _Logger.InfoFormat("Run Task Start:{1}({0})", status.TaskType, taskName);
                    status.Task.Run();
                    _Logger.InfoFormat("Run Task Finish:{2}({0}),Next Time:{1}", status.TaskType, _TaskStatus.ContainsKey(taskName) ? status.NextTime : DateTime.MaxValue, taskName);
                }
                catch (Exception ex)
                {
                    if (!(ex is System.AppDomainUnloadedException))
                        _Logger.Error(string.Format("Run Task Error:{2}({0}),{1}", status.TaskType, ex.Message, taskName), ex);
                }
                finally
                {
                    status.Executing = false;
                }
            }
        }

        #endregion

        #region watch config

        /// <summary>
        /// 服务Config文件被修改后检查新增与删除的服务，并作对应的注册与注销操作。修改某个服务配置，使用先删除该服务后新增的方式。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            DateTime md = File.GetLastWriteTime(e.FullPath);
            if (md != _LastModifyTime)
            {
                _LastModifyTime = md;
                RegisterTasks();
                SetTimers();
            }
        }

        #endregion

        #region timers

        /// <summary>
        /// 激活现在开始之后一段时间_MainTimerInterval之内需要运行但未被激活的服务。
        /// </summary>
        void SetTimers()
        {
            DateTime nextHour = DateTime.Now.AddMilliseconds(_MainTimerInterval);
            DateTime to = nextHour.AddMinutes(5); //增加一小段时间，避免临界线可能存在的一些问题。
            foreach (TaskStatus status in _TaskStatus.Values)
            {
                SetTaskTimer(status, to);
            }
        }

        /// <summary>
        /// 激活单个任务
        /// </summary>
        /// <param name="status"></param>
        void SetTaskTimer(TaskStatus status)
        {
            SetTaskTimer(status, DateTime.Now.AddMilliseconds(_MainTimerInterval));
        }

        /// <summary>
        /// 激活单个任务
        /// </summary>
        /// <param name="status"></param>
        /// <param name="validTime"></param>
        void SetTaskTimer(TaskStatus status, DateTime validTime)
        {            
            Timer timer = Timers[status.Task.Name];
            timer.Stop();
            if (status.Enable)
            {
                DateTime nextTime = status.NextTime;
                //如果下次执行时间小于全局计时器的间隔，则直接激活下次运行时间；
                //否则不作处理，交给全局计时器来处理。
                if (nextTime < validTime)
                {
                    StartTimer(timer, nextTime);
                }
            }
        }

        /// <summary>
        /// 设置单个计划器
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="dateto"></param>
        void StartTimer(Timer timer, DateTime dateto)
        {
            timer.Interval = ((dateto - DateTime.Now).TotalMilliseconds);
            timer.Start();
        }

        /// <summary>
        /// 全局计时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SetTimers();
        }

        /// <summary>
        /// 服务计时时间到，执行服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Timer timer = sender as Timer;
                timer.Stop();
                TaskBase task = _TimerTasks[timer];
                string taskName = task.Name;
                TaskStatus status = _TaskStatus[taskName];

                //服务置于可用状态时，执行服务的操作
                if (status.CanBeExecuted)
                    RunTask(status);
                if (_TaskStatus.ContainsKey(taskName))
                    SetTaskTimer(status);
            }
            catch (Exception ex)
            {
                if (!(ex is System.AppDomainUnloadedException))
                    _Logger.Error(string.Format("timer_Elapsed Error:{0}", ex.Message), ex);
            }
        }       

        #endregion

        #region marshalByrefObject

        public override object InitializeLifetimeService()
        {
            //不过期
            return null;
        }

        #endregion
    }
}
