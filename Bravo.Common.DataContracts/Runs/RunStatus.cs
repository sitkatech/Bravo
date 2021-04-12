using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Common.DataContracts.Runs
{
    public enum RunStatus
    {
        Created = 0,
        Queued = 1,
        Processing = 2,
        Complete = 3,
        [Display(Name = "System Error")]
        SystemError = 4,
        [Display(Name = "Invalid Output")]
        InvalidOutput = 5,
        [Display(Name = "Invalid Input")]
        InvalidInput = 6,
        [Display(Name = "Completed with Dry Cells")]
        HasDryCells = 7,
        [Display(Name = "Analysis Failed")]
        AnalysisFailed = 8,
        [Display(Name = "Analysis Succeeded")]
        AnalysisSuccess = 9,
        [Display(Name = "Processing Inputs")]
        ProcesingInputs = 10,
        [Display(Name = "Running Analysis")]
        RunningAnalysis = 11
    }
}
