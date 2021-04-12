using Bravo.Common.DataContracts.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Accessors.Modflow
{
    public interface IModflowAccessor
    {
        bool RunModflow(string exeName, string namFileName);

        AnalysisResult RunModpath(string exeName, string simFileName);

        bool RunZoneBudget(string exeName);
    }
}
