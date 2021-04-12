using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Bravo.Managers;
using Bravo.Managers.Runs;
using Microsoft.Extensions.Logging;
using Autofac;
using Bravo.Common.DataContracts.APIFunctionModels;
using Newtonsoft.Json;

namespace Bravo.Clients.APIFunctions
{
    public static class OrchestratorFunctions
    {
        [FunctionName("RunAnalysis")]
        public static HttpResponseMessage RunAnalysis([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, ILogger logger)
        {
            Dependency.CreateContainer(logger);
            logger.LogInformation("C# HTTP trigger function processed a request: Run Analysis.");

            // parse query parameter
            string runIdStr = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "RunId", true) == 0)
                .Value;

            int runId;

            if (!int.TryParse(runIdStr, out runId))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a valid run id on the query string or in the request body");
            }

            var managerFactory = Dependency.Container.Resolve<ManagerFactory>();
            var runManager = managerFactory.CreateManager<IRunManager>();

            runManager.QueueRunAnalysis(runId);

            var response = new RunResponseModel
            {
                RunId = runId,
                Message = "Run is queued for analysis"
            };

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        [FunctionName("GenerateOutputs")]
        public static HttpResponseMessage GenerateOutputs(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, 
            ILogger logger)
        {
            Dependency.CreateContainer(logger);

            logger.LogInformation("C# HTTP trigger function processed a request: Generate Outputs.");

            // parse query parameter
            string runIdStr = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "RunId", true) == 0)
                .Value;

            int runId;

            if (!int.TryParse(runIdStr, out runId))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a valid run id on the query string or in the request body");
            }

            var managerFactory = Dependency.Container.Resolve<ManagerFactory>();
            var runManager = managerFactory.CreateManager<IRunManager>();

            runManager.QueueGenerateOutput(runId);

            var response = new RunResponseModel
            {
                RunId = runId,
                Message = "Run is queued for output generation"
            };

            return req.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}
