using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schedule.Tasks.Proxy
{
    public interface IRuntimeProxy
    {
        void Start();

        void Stop();

        void Pause();

        void Continue();

        void PauseTask(string key);

        void ContinueTask(string key);

        void RunTask(string key);

        List<string> Tasks();

    }
}
