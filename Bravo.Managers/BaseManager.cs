using Bravo.Accessors;
using Bravo.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Bravo.Common.Utilities;

namespace Bravo.Managers
{
    public abstract class BaseManager
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(BaseManager));
        public EngineFactory EngineFactory { get; set; }

        public AccessorFactory AccessorFactory { get; set; }

        internal ManagerFactory ManagerFactory { get; set; }

        protected void SendManagerToManagerCall<T>(Func<T, Task> action) where T : class
        {
            var task = action(ManagerFactory.CreateManager<T>());
            if (task.Status == TaskStatus.Created)
            {
                task.Start();
            }
            task.ContinueWith(a => Logger.Error("Manager to Manager call failed.", a.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
