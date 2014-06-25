using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schedule.Tasks
{
    [Serializable]
    public abstract class TaskBase : MarshalByRefObject, ITask
    {

        private object _MutexObjet = new object();

        /// <summary>
        /// Key of the Task.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Run the Task.
        /// </summary>
        public void Run()
        {
            lock (_MutexObjet)
            {
                Execute();
            }
        }

        /// <summary>
        /// Stop the Task
        /// </summary>
        public virtual void Stop()
        {
        }

        /// <summary>
        /// Overwrite the method to implement yours logic.
        /// </summary>
        protected abstract void Execute();


        public override object InitializeLifetimeService()
        {
            //never time out
            return null;
        }
    }
}
