using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using log4net;
using System.IO;

namespace Schedule.Tasks
{
    public class ScheduleTaskSection : ConfigurationSection
    {
        public static ScheduleTaskSection Instance
        {
            get
            {
                try
                {
                    return System.Configuration.ConfigurationManager.OpenExeConfiguration(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schedule.Tasks.Runtime.dll")).GetSection("schedule.tasks") as ScheduleTaskSection;
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("Schedule.Tasks.Runtime").Error(string.Format("Read configuration Schedule.Tasks.Runtime.dll.config error:{0}", ex.Message), ex);
                    return null;
                }
            }
        }

        [ConfigurationProperty("tasks")]
        public TaskElementCollection Tasks
        {
            get { return this["tasks"] as TaskElementCollection; }
        }

        [ConfigurationProperty("schedules")]
        public ScheduleElementCollection Schedules
        {
            get { return this["schedules"] as ScheduleElementCollection; }
        }
    }

    public class TaskElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                return this["type"] as string;
            }
        }

        [ConfigurationProperty("assemblyPath", DefaultValue = null)]
        public string AssemblyPath
        {
            get
            {
                return this["assemblyPath"] as string;
            }
        }

        [ConfigurationProperty("customConfigFile", DefaultValue = null)]
        public string CustomConfigFile
        {
            get
            {
                return this["customConfigFile"] as string;
            }
        }

        [ConfigurationProperty("enable", DefaultValue = "true")]
        public string Enable
        {
            get
            {
                return this["enable"] as string;
            }
        }

        [ConfigurationProperty("schedule")]
        public string Schedule
        {
            get
            {
                return this["schedule"] as string;
            }
        }
    }

    public class TaskElementCollection : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new TaskElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TaskElement)element).Name;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "task"; }
        }

        public TaskElement this[int index]
        {
            get { return base.BaseGet(index) as TaskElement; }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }
    }

    public class ScheduleElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                return this["type"] as string;
            }
        }

        [ConfigurationProperty("scheduleString", IsRequired = true)]
        public string ScheduleString
        {
            get
            {
                return this["scheduleString"] as string;
            }
        }

        [ConfigurationProperty("maxTimes", DefaultValue = "9223372036854775807")]
        public string MaxTimes
        {
            get
            {
                return this["maxTimes"] as string;
            }
        }

        [ConfigurationProperty("fromTime", DefaultValue = "0001-1-1 0:00:00")]
        public string FromTime
        {
            get
            {
                return this["fromTime"] as string;
            }
        }

        [ConfigurationProperty("toTime", DefaultValue = "9999-12-31 23:59:59")]
        public string ToTime
        {
            get
            {
                return this["toTime"] as string;
            }
        }

        [ConfigurationProperty("times", DefaultValue = "9223372036854775807")]
        public string Times
        {
            get
            {
                return this["times"] as string;
            }
        }
    }

    public class ScheduleElementCollection : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new ScheduleElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ScheduleElement)element).Name;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "schedule"; }
        }

        public ScheduleElement this[int index]
        {
            get { return base.BaseGet(index) as ScheduleElement; }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }
    }
}
