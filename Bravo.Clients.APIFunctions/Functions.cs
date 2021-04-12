using Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Bravo.Common.DataContracts.APIFunctionModels;
using Bravo.Common.DataContracts.Models;
using Bravo.Common.DataContracts.Runs;
using Bravo.Common.Utilities;
using Bravo.Managers;
using Bravo.Managers.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using Bravo.Common.Shared.Enums;
using Bravo.Managers.Models;

namespace Bravo.Clients.APIFunctions
{
    public static class Functions
    {
        private const string WaterLevelChangeFileName = "Water Level Change";

        [FunctionName("RetrieveResult")]
        public static HttpResponseMessage RetrieveResult([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, ILogger logger)
        {
            Dependency.CreateContainer(logger);

            logger.LogInformation("C# HTTP trigger function processed a request: Retrieve Result.");

            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<RetrieveResultModel>(requestBody);

            if (data == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass run details in the request body");
            }

            if (!data.RunId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid run id in the request body");
            }

            if (string.IsNullOrEmpty(data.FileName))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a file name to be downloaded in the request body");
            }

            var subType = string.IsNullOrWhiteSpace(data.SubType) ? data.FileDate : data.SubType;

            var managerFactory = Dependency.Container.Resolve<ManagerFactory>();
            var runManager = managerFactory.CreateManager<IRunManager>();

            var runResult = runManager.GetRunResult(data.RunId.Value, data.FileName, subType, data.FileExtension);

            if (runResult == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, new
                {
                    RunId = data.RunId.Value,
                    Message = "There is no run associated with the run id provided, the run has not completed, or you do not have access to view the status of the run"
                });
            }

            return req.CreateResponse(HttpStatusCode.OK, runResult);
        }

        [FunctionName("StartRun")]
        public static HttpResponseMessage StartRun(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger logger)
        {
            Dependency.CreateContainer(logger);

            logger.LogInformation("C# HTTP trigger function processed a request: Start Run.");

            NewRunModel data = null;
            var inputFiles = new List<InputFile>();

            var reqContentType = req.Content.Headers.ContentType.MediaType;

            if (reqContentType.Equals("multipart/form-data"))
            {
                var multipart = req.Content.ReadAsMultipartAsync().Result;

                foreach (var content in multipart.Contents)
                {
                    if (content.Headers.ContentDisposition.Name.Equals("\"files\"", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var file = new InputFile
                        {
                            FileContent = content.ReadAsByteArrayAsync().Result,
                            FileName = content.Headers.ContentDisposition.FileName.Replace("\"", "")
                        };

                        inputFiles.Add(file);
                    }
                    else if (content.Headers.ContentDisposition.Name.Equals("\"request\"", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var contentStr = content.ReadAsStringAsync().Result;
                        data = JsonConvert.DeserializeObject<NewRunModel>(contentStr);

                    }
                }
            }
            else
            {
                var contentStr = req.Content.ReadAsStringAsync().Result;
                data = JsonConvert.DeserializeObject<NewRunModel>(contentStr);
            }

            if (data == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass run details in the request body");
            }

            if (!data.ModelId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid model id in the request body");
            }

            if (!data.ScenarioId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid scenario id in the request body");
            }

            if (string.IsNullOrEmpty(data.Name))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid name in the request body");
            }

            var managerFactory = Dependency.Container.Resolve<ManagerFactory>();
            var modelManager = managerFactory.CreateManager<IModelManager>();
            var runManager = managerFactory.CreateManager<IRunManager>();

            // ensure model exists and  has the requested scenario
            var model = modelManager.FindModelWithScenario(data.ModelId.Value, data.ScenarioId.Value);

            if (model == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Model does not exist or Scenario not available for this model. Please update your inputs and try again.");
            }

            var containsScenarioFiles = model.Scenarios.First().Files.Length > 0;

            // ensure inputs are supplied
            if (containsScenarioFiles)
            {
                var requiredFiles = model.Scenarios.First().Files.Where(x => x.Required).ToList();

                foreach (var file in requiredFiles)
                {
                    if (!inputFiles.Any(x => x.FileName.Equals(file.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        return req.CreateErrorResponse(HttpStatusCode.BadRequest, $"Missing required input file \"{file.Name}\"");
                    }
                }
            }
            else
            {
                switch (model.Scenarios.First().InputControlType)
                {
                    case InputControlType.CanalTable:
                        if (data.RunCanalInputs == null || data.RunCanalInputs.Count == 0)
                        {
                            return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass canal inputs in the request body");
                        }
                        break;
                    case InputControlType.WellMap:
                        if (data.PivotedRunWellInputs == null || data.PivotedRunWellInputs.Count == 0)
                        {
                            return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass well inputs in the request body");
                        }
                        break;
                    case InputControlType.ZoneMap:
                        if (data.RunZoneInputs == null || data.RunZoneInputs.Count == 0)
                        {
                            return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass zone inputs in the request body");
                        }
                        break;
                    case InputControlType.ParticleMap:
                        if (data.RunWellParticleInputs == null || data.RunWellParticleInputs.Count == 0)
                        {
                            return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass particle inputs in the request body");
                        }
                        break;
                }
            }

            // create the run
            var saveResult = runManager.CreateOrUpdateRun(new Run
            {
                CreatedDate = DateTime.UtcNow,
                FileStorageLocator = Guid.NewGuid().ToString(),
                Name = data.Name,
                ModelId = data.ModelId.Value,
                ScenarioId = data.ScenarioId.Value,
                Status = RunStatus.Created,
                InputVolumeType = data.InputVolumeType ?? GetDefaultInputVolumeType(data.ScenarioId.Value),
                OutputVolumeType = data.OutputVolumeType ?? GetDefaultOutputVolumeType(data.ScenarioId.Value),
                IsDifferential = data.IsDifferential ?? true,
                Description = data.Description,
            });

            // save the inputs
            if (containsScenarioFiles)
            {
                saveResult.Scenario = model.Scenarios.First();

                foreach (var inputFile in inputFiles)
                {
                    runManager.UploadInputFile(saveResult, inputFile.FileName, inputFile.FileContent);
                }
            }
            else
            {
                switch (model.Scenarios.First().InputControlType)
                {
                    case InputControlType.CanalTable:
                        runManager.UpdateInputCanalData(saveResult, data.RunCanalInputs.ToArray());
                        break;
                    case InputControlType.WellMap:
                        runManager.UpdateInputWellData(data.PivotedRunWellInputs.ToArray(), saveResult.Id);
                        break;
                    case InputControlType.ZoneMap:
                        runManager.UpdateInputZoneData(data.RunZoneInputs.ToArray(), saveResult.Id);
                        break;
                    case InputControlType.ParticleMap:
                        runManager.UpdateInputWellParticleData(data.RunWellParticleInputs.ToArray(), saveResult.Id);
                        break;
                }
            }

            // start the run
            var queueRunSuccess = runManager.QueueRun(saveResult.Id, data.CreateMaps);

            var response = new RunResponseModel
            {
                RunId = saveResult.Id,
                Status = saveResult.Status.GetDisplayName(),
                Message = queueRunSuccess ? "Run is successfully queued" : "An error is encountered when trying to start a run"
            };

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        private static VolumeType GetDefaultInputVolumeType(int scenarioId)
        {
            switch (scenarioId)
            {
                case 1:
                case 2:
                case 6:
                case 11:
                    return VolumeType.Gallon;
                case 3:
                case 4:
                case 7:
                case 8:
                case 10:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                    return VolumeType.AcreFeet;
                default:
                    return VolumeType.CubicFeet;
            }
        }

        private static VolumeType GetDefaultOutputVolumeType(int scenarioId)
        {
            return VolumeType.AcreFeet;
        }

        [FunctionName("GetRunStatus")]
        public static HttpResponseMessage GetRunStatus(
          [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, ILogger logger)
        {
            Dependency.CreateContainer(logger);

            logger.LogInformation("C# HTTP trigger function processed a request: Get Run Status.");

            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<RunDetailModel>(requestBody);

            if (data == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass run details in the request body");
            }

            if (!data.RunId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid run id in the request body");
            }

            var managerFactory = Dependency.Container.Resolve<ManagerFactory>();
            var runManager = managerFactory.CreateManager<IRunManager>();

            var runStatus = runManager.GetRunStatus(data.RunId.Value);

            if (runStatus == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, new RunResponseModel
                {
                    RunId = data.RunId.Value,
                    Status = string.Empty,
                    Message = "There is no run associated with the run id provided or you do not have access to view the status of the run"
                });
            }

            var response = new RunResponseModel
            {
                RunId = data.RunId.Value,
                Status = runStatus.GetDisplayName(),
                Message = string.Empty
            };

            return req.CreateResponse(HttpStatusCode.OK, response);
        }

        [FunctionName("GetAvailableRunResults")]
        public static HttpResponseMessage GetAvailableRunResults([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, ILogger logger)
        {
            Dependency.CreateContainer(logger);

            logger.LogInformation("C# HTTP trigger function processed a request: Get Available Run Results.");

            string requestBody = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<RunDetailModel>(requestBody);

            if (data == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass run details in the request body");
            }

            if (!data.RunId.HasValue)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please pass a valid run id in the request body");
            }

            var managerFactory = Dependency.Container.Resolve<ManagerFactory>();
            var runManager = managerFactory.CreateManager<IRunManager>();

            var availableRunResults = runManager.FindAvailableRunResults(data.RunId.Value);

            if (availableRunResults == null)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, new {
                    RunId = data.RunId.Value,
                    Message = "There is no run associated with the run id provided, the run has not completed, or you do not have access to view the status of the run"
                });
            }

            return req.CreateResponse(HttpStatusCode.OK, availableRunResults);
        }

        private class AvailableRunResult
        {
            public string FileName { get; set; }
            public List<string> AvailableDates { get; set; }
            public string SomethingElse { get; set; }
        }

        private class AvailableRunResultHelper
        {
            public string FileName { get; set; }
            public List<string> AvailableDates { get; set; }
            public string SomethingElse { get; set; }
        }

        [FunctionName("GetRuns")]
        public static HttpResponseMessage GetRuns([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, ILogger logger)
        {
            Dependency.CreateContainer(logger);

            logger.LogInformation("C# HTTP trigger function processed a request: Get Runs.");

            var managerFactory = Dependency.Container.Resolve<ManagerFactory>();
            var runManager = managerFactory.CreateManager<IRunManager>();

            var runs = runManager.GetRuns();

            return req.CreateResponse(HttpStatusCode.OK, runs);
        }
    }
}
