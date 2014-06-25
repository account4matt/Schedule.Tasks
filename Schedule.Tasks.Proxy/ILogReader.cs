using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schedule.Tasks.Proxy
{
    public interface ILogReader
    {
        IList<string> Read(int index, int count);
        IList<string> Read(string type, int index, int count);
        void Clean();
        void Clean(string type);
        void Delete(string[] id);
    }
}
