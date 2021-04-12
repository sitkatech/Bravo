﻿using Autofac;
using Microsoft.Extensions.Logging;
using Bravo.Managers;
using Bravo.Managers.Runs;

namespace Bravo.Clients.APIFunctions
{
    internal class Dependency
    {
        internal static IContainer Container { get; private set; }
        public static void CreateContainer(ILogger logger)
        {
            if (Container == null)
            {
                var builder = new ContainerBuilder();

                builder.RegisterType<ManagerFactory>().As<ManagerFactory>();
                builder.RegisterType<RunManager>().As<IRunManager>();

                builder.RegisterModule(new LoggingModule(logger));

                Container = builder.Build();
            }
        }
    }
}
