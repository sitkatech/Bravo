using System;

namespace Bravo.Common.DataContracts.APIFunctionModels
{
    public class RunSummaryResponseModel
    {
        public int RunId { get; set; }

        public string RunName { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Status { get; set; }
    }
}
