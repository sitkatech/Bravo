using log4net;
using Bravo.Common.DataContracts.Runs;
using Bravo.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Accessors.Modflow
{
    class ModflowAccessor : IModflowAccessor
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(ModflowAccessor));

        private const string ModFlowSixExeName = "mf6.exe";
        private const string ModFlowSixZoneBudgetExeName = "zbud6.exe";

        public bool RunModflow(string exeName, string namFileName)
        {
            try
            {
                ProcessStartInfo processStartParams = new ProcessStartInfo()
                {
                    FileName = exeName,
                    WorkingDirectory = ConfigurationHelper.AppSettings.ModflowDataFolder,
                    Arguments = exeName == ModFlowSixExeName ? "" : namFileName
                };

                Logger.Debug("Starting Modflow");
                using (var process = Process.Start(processStartParams))
                {
                    process.WaitForExit();

                    Logger.Info($"Modflow exit code: {process.ExitCode}");

                    return (process.ExitCode == 0);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public AnalysisResult RunModpath(string exeName, string simFileName)
        {
            try
            {
                ProcessStartInfo processStartParams = new ProcessStartInfo()
                {
                    FileName = Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, exeName),
                    WorkingDirectory = ConfigurationHelper.AppSettings.ModflowDataFolder,
                    Arguments = simFileName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true

                };

                Logger.Debug("Starting Modpath");
                using (var process = Process.Start(processStartParams))
                {
                    var consoleOut = new StringBuilder();
                    while (!process.StandardOutput.EndOfStream)
                    {
                        consoleOut.AppendLine(process.StandardOutput.ReadLine());
                    }

                    process.WaitForExit();

                    Logger.Info($"Modpath exit code: {process.ExitCode}");

                    return new AnalysisResult()
                    {
                        Success = process.ExitCode == 0,
                        ConsoleOutput = consoleOut.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return new AnalysisResult() { Success = false };
            }
        }

        public bool RunZoneBudget(string exeName)
        {
            try
            {
                ProcessStartInfo processStartParams;

                if (string.Compare(exeName, ModFlowSixZoneBudgetExeName, true) == 0)
                {
                    processStartParams = new ProcessStartInfo
                    {
                        FileName = exeName,
                        WorkingDirectory = ConfigurationHelper.AppSettings.ModflowDataFolder
                    };

                    using (var process = Process.Start(processStartParams))
                    {
                        process.WaitForExit();
                        Logger.Info($"Zone Budget exit code: {process.ExitCode}");

                        return (process.ExitCode == 0);
                    }
                }

                var batchFileName = CreateBatchFile(exeName);
                processStartParams = new ProcessStartInfo
                {
                    FileName = batchFileName,
                    WorkingDirectory = ConfigurationHelper.AppSettings.ModflowDataFolder,
                };

                Logger.Debug("Starting Zone Budget");
                using (var process = Process.Start(processStartParams))
                {
                    process.WaitForExit();
                    Logger.Info($"Zone Budget exit code: {process.ExitCode}");

                    return (process.ExitCode == 0);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        private static string CreateBatchFile(string exeName)
        {
            // We create batch file because if we directly execute the exe and redirect the standard input, we end up with the UTF-8 BOM on the beginning of our files.
            // This doesn't happen when executing a batch file.
            var batchFileName = System.IO.Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, "zonebudget.generated.bat");
            var exeFileName = System.IO.Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, exeName);
            var inputDataFileName = System.IO.Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, "RunZoneBudget.bat");
            System.IO.File.WriteAllText(batchFileName, $"{exeFileName} < {inputDataFileName}");
            return batchFileName;
        }
    }
}
