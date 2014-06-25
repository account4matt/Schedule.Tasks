using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Schedule.Tasks.InBuilts
{
    /// <summary>
    /// 调用指定入口的不带参数的静态方法
    /// </summary>
    ///<remarks>需要配置config的EntryPoint：例：<add key="EntryPoint" value="ClassLibrary1.Class1,ClassLibrary1::StartStaticMethod[,StopMethod]"/></remarks>
    public class InvokeStaticMethodTask : TaskBase
    {
        protected Type _StaticMethodType = null;
        System.Reflection.MethodInfo _StopMethod = null;
        System.Reflection.MethodInfo _StartMethod = null;

        protected override void Execute()
        {
            string entryPoint = System.Configuration.ConfigurationManager.AppSettings["EntryPoint"];
            string[] entryPointArr = entryPoint.Split(':');
            _StaticMethodType = Type.GetType(entryPointArr[0]);
            string[] methods = entryPointArr[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            _StartMethod = _StaticMethodType.GetMethod(methods[0]);
            if (methods.Length > 1)
                _StopMethod = _StaticMethodType.GetMethod(methods[1]);
            _StartMethod.Invoke(null, null); 
        }

        public override void Stop()
        {
            try
            {
                if (_StopMethod != null)
                    _StopMethod.Invoke(null, null);
            }
            catch { }
            finally { }
        }
    }
}
