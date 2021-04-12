using log4net;
using Bravo.Common.Shared.Enums;
using Bravo.Common.Utilities;
using Bravo.Managers;
using Bravo.Managers.Runs;
using System;
using System.Data.Entity.SqlServer;

namespace Bravo.Clients.Agent
{
    class Program
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(Program));
        private static ManagerFactory factory = new ManagerFactory();
        private static IRunManager RunManager => factory.CreateManager<IRunManager>();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Logger.Debug("Agent started");

            Logger.Info($"Loading Native Assemblies from {AppDomain.CurrentDomain.BaseDirectory}");
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
            SqlProviderServices.SqlServerTypesAssemblyName = "Microsoft.SqlServer.Types, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

            if (args == null || args.Length < 1)
            {
                Logger.Error("No run id specified");
                System.Environment.Exit(-1);
            }
            else
            {
                Logger.Info($"Starting run id:{args[0]}");
            }

            var runId = int.Parse(args[0]);
            var processType = (AgentProcessType)int.Parse(args[1]);

            switch (processType)
            {
                case AgentProcessType.Input:
                    Logger.Info($"Generating input for run id:{args[0]}");
                    GenerateInput(runId);
                    break;
                case AgentProcessType.Analysis:
                    Logger.Info($"Running analysis for run id:{args[0]}");
                    RunAnalysis(runId);

                    Logger.Info($"Generating output for run id:{args[0]}");
                    GenerateOutput(runId);
                    break;
            }
        }

        private static void GenerateInput(int runId)
        {
            RunManager.GenerateInputFiles(runId);
        }

        private static void RunAnalysis(int runId)
        {
            RunManager.RunAnalysis(runId);
        }

        private static void GenerateOutput(int runId)
        {
            RunManager.GenerateOutputFiles(runId);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Logger.Error("Global Error Executing Run", ex);
            }
            else
            {
                Logger.Error("Global Error Executing Run");
            }
        }
    }
}
