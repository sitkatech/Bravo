using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Repository.Hierarchy;
using Bravo.Accessors.FileIO;
using Bravo.Accessors.Modflow;
using Bravo.Common.DataContracts.Runs;
using Bravo.Common.Utilities;

namespace Bravo.Engines
{
    internal class AnalysisEngine : BaseEngine, IAnalysisEngine
    {
        public AnalysisResult RunAnalysis(Run run)
        {
            var modflowAccessor = AccessorFactory.CreateAccessor<IModflowAccessor>();

            bool runResult;

            if (run.Model.IsModflowModel)
            {
                runResult = modflowAccessor.RunModflow(run.Model.ModflowExeName, run.Model.NamFileName);
                if (runResult && !string.IsNullOrWhiteSpace(run.Model.ZoneBudgetExeName))
                {
                    runResult = modflowAccessor.RunZoneBudget(run.Model.ZoneBudgetExeName);
                }
            }//modpath
            else
            {
                return modflowAccessor.RunModpath(run.Model.ModpathExeName, run.Model.SimulationFileName);
            }

            return new AnalysisResult() { Success = runResult };
        }
    }
}
