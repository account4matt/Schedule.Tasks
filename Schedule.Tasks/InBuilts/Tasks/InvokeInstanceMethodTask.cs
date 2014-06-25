using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Schedule.Tasks.InBuilts
{
    /// <summary>
    /// 调用指定入口的不带参数的实例方法
    /// </summary>
    ///<remarks>需要配置config的EntryPoint：例：<add key="EntryPoint" value="ClassLibrary1.Class1,ClassLibrary1:StartInstanceMethod[,StopMethod]"/></remarks>
    public class InvokeInstanceMethodTask : TaskBase
    {
        protected object _Instance = null;
        System.Reflection.MethodInfo _StopMethod = null;
        System.Reflection.MethodInfo _StartMethod = null;
        protected override void Execute()
        {
            string entryPoint = System.Configuration.ConfigurationManager.AppSettings["EntryPoint"];
            string[] entryPointArr = entryPoint.Split(':');
            Type type = Type.GetType(entryPointArr[0]);
            string[] methods = entryPointArr[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            _StartMethod = type.GetMethod(methods[0]);
            if (methods.Length > 1)
                _StopMethod = type.GetMethod(methods[1]);
            _Instance = System.Activator.CreateInstance(type);
            _StartMethod.Invoke(_Instance, null);
        }

        public override void Stop()
        {
            try
            {
                if (_StopMethod != null)
                    _StopMethod.Invoke(_Instance, null);
            }
            catch { }
            finally { }
        }
    }
}
