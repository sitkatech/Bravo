using Bravo.Common.DataContracts.Runs;

namespace Bravo.Common.DataContracts.APIFunctionModels
{
    public class RunResponseModel
    {
        public int RunId { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }
    }
}
