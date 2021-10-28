using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Bravo.Common.DataContracts.Runs;
using log4net;
using Newtonsoft.Json;
using Bravo.Common.Utilities;
using Bravo.Accessors.Runs;
using Bravo.Accessors.FileIO;
using Bravo.Common.Shared;
using Bravo.Engines.RunDataParse;
using Bravo.Accessors.Models;
using Bravo.Accessors.Containers;
using Bravo.Common.Shared.Extensions;
using Bravo.Engines.ModelInputOutputEngines;
using Bravo.Common.DataContracts.Models;
using Bravo.Engines;
using Bravo.Common.Shared.Enums;
using Bravo.Accessors.Queue;
using Bravo.Accessors.APIFunctions;
using System.IO;
using Bravo.Common.DataContracts.APIFunctionModels;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Bravo.Managers.Runs
{
    public class RunManager : BaseManager, IRunManager
    {
        private string AGENT_EXECUTABLE_NAME = $"{ConfigurationHelper.AppSettings.DockerAgentContainerPath}\\Bravo.Clients.Agent.exe";
        private static readonly ILog Logger = Logging.GetLogger(typeof(RunManager));
        private static readonly Regex FileNameParseRegEx = new Regex(@"(?<hidden>!?)(?<id>\d+)\-(?<name>[^\\]+)((?=(?<extension>\.json))|(?=(?<extension>\.kml)))", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private const string WaterLevelChangeFileName = "Water Level Change";
        private const string WaterLevelFileName = "Water Level";
        private const string DrawdownFileName = "Drawdown";
        private const string WaterBudgetByZoneFileName = "Water Budget By Zone";
        private const string WaterBudgetByBudgetItemFileName = "Water Budget By Budget Item";
        private static readonly RunStatus[] RunStatusToDelete = { RunStatus.Complete, RunStatus.InvalidInput, RunStatus.InvalidOutput, RunStatus.SystemError };

        public Run CreateOrUpdateRun(Run run)
        {
            Logger.Info("Creating or Updating run");

            if (!AccessorFactory.CreateAccessor<IModelAccessor>().FindAllModels()
                .Select(m => m.Id).Contains(run.ModelId))
            {
                throw new Exception($"Can't create run for model {run.ModelId}. Model could not be found.");
            }

            return AccessorFactory.CreateAccessor<IRunAccessor>().CreateOrUpdateRun(run);
        }

        public Run FindRun(int runId, bool includeHiddenFiles = false)
        {
            Logger.Info($"Finding run {runId}");

            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId);

            if (_finishedStatuses.Contains(run.Status))
            {
                var files = blobFileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(run), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
                run.AvailableRunResults = files.Select(a => FileNameParseRegEx.Match(a))
                                               .Where(a => a.Success)
                                               .Where(a => includeHiddenFiles ? true : string.IsNullOrWhiteSpace(a.Groups["hidden"].Value))
                                               .Select(a => new RunResultListItem
                                               {
                                                   RunResultId = int.Parse(a.Groups["id"].Value),
                                                   RunResultName = a.Groups["name"].Value,
                                                   RunResultFileExtension = a.Groups["extension"].Value
                                               }).ToList();
            }

            if (run.Scenario.Files != null && run.Scenario.Files.Length > 0)
            {
                var files = blobFileAccessor.GetFilesInDirectory(StorageLocations.InputDirectoryPathForRun(run.FileStorageLocator),
                    ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);

                foreach (var scenarioFile in run.Scenario.Files)
                {
                    scenarioFile.Uploaded = files.Any(x => x.Equals(scenarioFile.Name, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            var runInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedCanalInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            if (runInputData != null)
            {
                run.CanalRunInputs =
                    JsonConvert.DeserializeObject<List<RunCanalInput>>(Encoding.UTF8.GetString(runInputData));
            }
            else if (run.Scenario.InputControlType == InputControlType.CanalTable && !IsCustomInput(run))
            {
                //we can build a template of inputs from the model specs
                run.CanalRunInputs = BuildCanalInputsForRun(run);

                //save them
                UpdateInputCanalData(run, run.CanalRunInputs.ToArray());
            }

            var wellMapInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            if (wellMapInputData != null)
            {
                run.WellMapInputs = (JsonConvert.DeserializeObject<RunWellInput[]>(Encoding.UTF8.GetString(wellMapInputData))).ToList();

                run.PivotedWellMapInputs = BuildWellPivotedInputData(run.WellMapInputs.ToArray(), run).ToList();
            }

            var runZoneInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedZoneInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            if (runZoneInputData != null)
            {
                run.RunZoneInputs = (JsonConvert.DeserializeObject<RunZoneInput[]>(Encoding.UTF8.GetString(runZoneInputData))).ToList();
            }

            var wellParticleMapInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            if (wellParticleMapInputData != null)
            {
                run.RunWellParticleInputs = (JsonConvert.DeserializeObject<RunWellParticleInput[]>(Encoding.UTF8.GetString(wellParticleMapInputData))).ToList();
            }

            return run;
        }


        public List<AvailableRunResult> FindAvailableRunResults(int runId)
        {
            Logger.Info($"Finding run {runId}");

            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId);

            if (run == null || !_finishedStatuses.Contains(run.Status))
            {
                return null;
            }

            var result = new List<AvailableRunResult>();

            var files = blobFileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(run), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder)
                .Select(a => new { Match = FileNameParseRegEx.Match(a), FullName = a })
                .Where(a => a.Match.Success)
                .Select(a => new
                {
                    IsHidden = !string.IsNullOrWhiteSpace(a.Match.Groups["hidden"].Value),
                    FileName = a.Match.Groups["name"].Value,
                    Extension = a.Match.Groups["extension"].Value,
                    FullName = a.FullName
                })
                .GroupBy(a => a.FileName)
                .ToList();
            foreach (var file in files)
            {
                if (!file.First().IsHidden)
                {
                    var availableRunResult = new AvailableRunResult
                    {
                        FileName = file.Key,
                        AvailableFileTypes = file.Select(a => a.Extension).Distinct().ToList()
                    };
                    if (IsWaterBudgetZoneFile(file.Key) || IsWaterBudgetItemFile(file.Key))
                    {
                        var fileData = blobFileAccessor.GetFile(OutputFilePathForRun(run.FileStorageLocator, file.First().FullName), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
                        var fileDetails = JsonConvert.DeserializeObject<WaterBudgetResultHelper>(System.Text.Encoding.UTF8.GetString(fileData));

                        availableRunResult.AvailableSubTypes = fileDetails.RelatedResultOptions.Select(a => a.Label).ToList();

                        availableRunResult.AvailableFileTypes = files
                            .Where(a => availableRunResult.AvailableSubTypes.Any(b => string.Equals(a.Key, b, StringComparison.OrdinalIgnoreCase)))
                            .SelectMany(a => a)
                            .Select(a => a.Extension)
                            .Distinct()
                            .ToList();
                    }
                    else if (IsWaterLevelChangeFile(file.Key) || IsWaterLevelFile(file.Key) || IsDrawdownFile(file.Key))
                    {
                        var fileData = blobFileAccessor.GetFile(OutputFilePathForRun(run.FileStorageLocator, file.First().FullName), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
                        var fileDetails = JsonConvert.DeserializeObject<WaterChangeResultHelper>(System.Text.Encoding.UTF8.GetString(fileData));

                        availableRunResult.AvailableSubTypes = fileDetails.ResultSets.First().MapData.AvailableStressPeriods.Select(a => a.Label).ToList();

                        availableRunResult.AvailableFileTypes = files
                            .Where(a => availableRunResult.AvailableSubTypes.Any(b => string.Equals(a.Key, b, StringComparison.OrdinalIgnoreCase)))
                            .SelectMany(a => a)
                            .Select(a => a.Extension)
                            .Distinct()
                            .ToList();
                    }
                    result.Add(availableRunResult);
                }
            }

            return result;
        }

        private class FileDetailsHelper
        {
            public bool IsHidden { get; set; }
            public string FileName { get; set; }
            public string Extension { get; set; }
            public string FullName { get; set; }
        }

        private static bool IsWaterBudgetZoneFile(string fileName)
        {
            return string.Equals(fileName, WaterBudgetByZoneFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWaterBudgetItemFile(string fileName)
        {
            return string.Equals(fileName, WaterBudgetByBudgetItemFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWaterLevelChangeFile(string fileName)
        {
            return string.Equals(fileName, WaterLevelChangeFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWaterLevelFile(string fileName)
        {
            return string.Equals(fileName, WaterLevelFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDrawdownFile(string fileName)
        {
            return string.Equals(fileName, DrawdownFileName, StringComparison.OrdinalIgnoreCase);
        }

        public RunResultResponseModel GetRunResult(int runId, string fileName, string subType, string fileType)
        {
            Logger.Info($"Finding run results {fileName}/{subType}{fileType} for run {runId}");

            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId);

            if (run == null || !_finishedStatuses.Contains(run.Status))
            {
                return null;
            }

            var allFileData = GetFileDetails(blobFileAccessor, run);
            var fileData = allFileData.SingleOrDefault(a => string.Equals(a.Key, fileName));

            if (fileData == null)
            {
                return null;
            }

            fileType = string.IsNullOrWhiteSpace(fileType) ? ".json" : fileType.ToLower();

            if (IsWaterLevelChangeFile(fileName) || IsWaterLevelFile(fileName) || IsDrawdownFile(fileName))
            {
                return FindHeatMapData(runId, subType, fileType, allFileData, blobFileAccessor, run, fileData);
            }

            if ((IsWaterBudgetItemFile(fileName) || IsWaterBudgetZoneFile(fileName)) && !string.IsNullOrWhiteSpace(subType))
            {
                fileData = allFileData.SingleOrDefault(a => string.Equals(a.Key, subType));

                if (fileData == null)
                {
                    return null;
                }
            }

            var matchingType = fileData.FirstOrDefault(a => a.Extension == fileType);
            if (matchingType == null)
            {
                return null;
            }

            return new RunResultResponseModel
            {
                RunId = runId,
                FileDetails = System.Text.Encoding.UTF8.GetString(blobFileAccessor.GetFile(
                    OutputFilePathForRun(run.FileStorageLocator, matchingType.FullName),
                    ConfigurationHelper.AppSettings.BlobStorageModelDataFolder))
            };
        }

        private RunResultResponseModel FindHeatMapData(int runId, string subType, string fileType, List<IGrouping<string, FileDetailsHelper>> allFileData, IBlobFileAccessor blobFileAccessor, Run run, IGrouping<string, FileDetailsHelper> fileData)
        {
            if (string.IsNullOrWhiteSpace(subType))
            {
                //the subType was not specified so use the first period
                return FindWaterChangeDataForFirstPeriod(runId, subType, fileType, blobFileAccessor, run, fileData);
            }

            var subFile = allFileData.FirstOrDefault(a => string.Equals(a.Key, subType, StringComparison.OrdinalIgnoreCase));
            if (subFile != null)
            {
                var matchingType = subFile.FirstOrDefault(a => a.Extension == fileType);
                if (matchingType == null)
                {
                    return null;
                }

                return new RunResultResponseModel
                {
                    RunId = runId,
                    FileDetails = System.Text.Encoding.UTF8.GetString(blobFileAccessor.GetFile(
                        OutputFilePathForRun(run.FileStorageLocator, matchingType.FullName),
                        ConfigurationHelper.AppSettings.BlobStorageModelDataFolder))
                };
            }

            //it may be the first period
            return FindWaterChangeDataForFirstPeriod(runId, subType, fileType, blobFileAccessor, run, fileData);
        }

        private RunResultResponseModel FindWaterChangeDataForFirstPeriod(int runId, string subType, string fileType, IBlobFileAccessor blobFileAccessor, Run run, IGrouping<string, FileDetailsHelper> fileData)
        {
            if (fileType != ".json" && fileType != ".kml")
            {
                return null;
            }

            var mainFileData = blobFileAccessor.GetFile(OutputFilePathForRun(run.FileStorageLocator, fileData.First().FullName), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            var mainFileString = System.Text.Encoding.UTF8.GetString(mainFileData);
            var fileDetails = JsonConvert.DeserializeObject<WaterChangeResultHelper>(mainFileString);

            if (!string.IsNullOrWhiteSpace(subType) && !string.Equals(fileDetails.RunResultName, subType, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (fileType == ".json")
            {
                return new RunResultResponseModel
                {
                    RunId = runId,
                    FileDetails = mainFileString
                };
            }

            //must be fileType == ".kml"
            if (string.IsNullOrWhiteSpace(fileDetails.ResultSets.First().MapData.KmlString))
            {
                return null;
            }
            return new RunResultResponseModel
            {
                RunId = runId,
                FileDetails = fileDetails.ResultSets.First().MapData.KmlString
            };
        }

        private List<IGrouping<string, FileDetailsHelper>> GetFileDetails(IBlobFileAccessor blobFileAccessor, Run run)
        {
            return blobFileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(run), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder)
                .Select(a => new { Match = FileNameParseRegEx.Match(a), FullName = a })
                .Where(a => a.Match.Success)
                .Select(a => new FileDetailsHelper
                {
                    IsHidden = !string.IsNullOrWhiteSpace(a.Match.Groups["hidden"].Value),
                    FileName = a.Match.Groups["name"].Value,
                    Extension = a.Match.Groups["extension"].Value,
                    FullName = a.FullName
                })
                .GroupBy(a => a.FileName)
                .ToList();
        }

        private class WaterBudgetResultHelper
        {
            public List<RelatedResultOption> RelatedResultOptions { get; set; }
        }

        private class WaterChangeResultHelper
        {
            public List<WaterChangeResultSetHelper> ResultSets { get; set; }
            public string RunResultName { get; set; }
        }

        private class WaterChangeResultSetHelper
        {
            public WaterChangeMapDataHelper MapData { get; set; }
        }

        private class WaterChangeMapDataHelper
        {
            public string KmlString { get; set; }
            public List<WaterChangeAvailableStressPeriodHelper> AvailableStressPeriods { get; set; }
        }

        private class WaterChangeAvailableStressPeriodHelper
        {
            public string Label { get; set; }
        }

        private class RelatedResultOption
        {
            public string Label { get; set; }
        }

        public List<Run> FindRuns(RunFilter filter, int pageNum = 0)
        {
            Logger.Info($"Finding runs for page #{pageNum}");

            var recordCount = ConfigurationHelper.AppSettings.DashboardPageRecordCount;

            var skip = pageNum * recordCount;

            return AccessorFactory.CreateAccessor<IRunAccessor>().FindRuns(filter, skip, recordCount);
        }

        public List<RunSummaryResponseModel> GetRuns()
        {
            var runs = AccessorFactory.CreateAccessor<IRunAccessor>().GetRuns();

            return runs.Select(x => new RunSummaryResponseModel
            {
                CreatedDate = x.CreatedDate,
                RunId = x.Id,
                RunName = x.Name,
                Status = x.Status.GetDisplayName()
            }).ToList();
        }

        public int FindRunsCount(RunFilter filter)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().FindRunsCount(filter);
        }

        public bool DeleteRun(int runId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().DeleteRun(runId);
        }

        public Run DuplicateRun(int runId)
        {
            var accessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var run = accessor.FindRun(runId);

            if (run == null)
            {
                throw new Exception($"Invalid run id: {runId}");
            }

            if (!AccessorFactory.CreateAccessor<IModelAccessor>().FindAllModels()
                .Select(m => m.Id).Contains(run.ModelId))
            {
                throw new Exception($"Can't create run for model {run.ModelId}. Model could not be found.");
            }

            var originalLocator = run.FileStorageLocator;

            run.Id = 0; //id set to default will force a create instead of update
            run.Status = RunStatus.Created;

            var newName = run.Name;

            if (newName.Contains("- Copy"))
            {
                newName = newName.Substring(0, newName.IndexOf("- Copy"));
            }

            newName = $"{newName} - Copy {DateTime.UtcNow:MM/dd/yy H:mm}";

            run.Name = newName;
            run.CreatedDate = DateTime.UtcNow;
            run.FileStorageLocator = Guid.NewGuid().ToString();
            run.ImageId = null;
            run.ProcessingStartDate = null;
            run.ProcessingEndDate = null;
            run.InputVolumeType = run.InputVolumeType;
            run.OutputVolumeType = run.OutputVolumeType;

            run = accessor.CreateOrUpdateRun(run);

            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            //Copy canal file
            var parsedInputFileContent = fileAccessor.GetFile(ParsedCanalInputFilePathForRun(originalLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            if (parsedInputFileContent != null)
            {
                fileAccessor.SaveFile(ParsedCanalInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, parsedInputFileContent);
            }

            //copy map input file
            var parsedWellInputFileContent = fileAccessor.GetFile(ParsedWellInputFilePathForRun(originalLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            if (parsedWellInputFileContent != null)
            {
                fileAccessor.SaveFile(ParsedWellInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, parsedWellInputFileContent);
            }

            //copy zone input file
            var parsedZoneInputFileContent = fileAccessor.GetFile(ParsedZoneInputFilePathForRun(originalLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            if (parsedZoneInputFileContent != null)
            {
                fileAccessor.SaveFile(ParsedZoneInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, parsedZoneInputFileContent);
            }

            //copy map particle input file
            var parsedWellParticleInputFileContent = fileAccessor.GetFile(ParsedWellParticleInputFilePathForRun(originalLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            if (parsedWellParticleInputFileContent != null)
            {
                fileAccessor.SaveFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, parsedWellParticleInputFileContent);
            }

            return run;
        }

        public bool RenameRun(int runId, string newName)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().RenameRun(runId, newName);
        }

        public bool ChangeRunDescription(int runId, string newDescription)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().ChangeRunDescription(runId, newDescription);
        }

        public RunBucket FindRunBucket(int bucketId)
        {
            Logger.Info($"Finding action bucket with ID {bucketId}");
            var runBucket = AccessorFactory.CreateAccessor<IRunAccessor>().FindRunBucket(bucketId);
            runBucket.AvailableRunResults = new List<RunResultListItem>();

            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            foreach (var run in runBucket.Runs)
            {
                if (_finishedStatuses.Contains(run.Status))
                {
                    var files = blobFileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(run), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
                    run.AvailableRunResults = files.Select(a => FileNameParseRegEx.Match(a))
                               .Where(a => a.Success)
                               .Where(a => string.IsNullOrWhiteSpace(a.Groups["hidden"].Value))
                               .Select(a => new RunResultListItem
                               {
                                   RunResultId = int.Parse(a.Groups["id"].Value),
                                   RunResultName = a.Groups["name"].Value,
                                   RunResultFileExtension = a.Groups["extension"].Value
                               }).ToList();

                    foreach (var runResult in run.AvailableRunResults)
                    {
                        if (runBucket.AvailableRunResults.FindIndex(x => x.RunResultName == runResult.RunResultName) < 0)
                        {
                            runBucket.AvailableRunResults.Add(runResult);
                        }
                    }
                }
            }

            runBucket.AvailableRunResults = runBucket.AvailableRunResults.Where(x => x.RunResultName != "List File Output" &&
                x.RunResultName != "Water Level Change" &&
                x.RunResultName != "Water Level" &&
                x.RunResultName != "Drawdown").ToList();

            return runBucket;
        }

        public List<RunBucket> GetRunBuckets()
        {
            Logger.Info($"Finding action buckets");
            return AccessorFactory.CreateAccessor<IRunAccessor>().GetRunBuckets(); ;
        }


        public RunBucket CreateOrUpdateRunBucket(RunBucket runBucket)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().CreateOrUpdateRunBucket(runBucket); ;
        }

        public bool RenameRunBucket(int bucketId, string newName)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().RenameRunBucket(bucketId, newName);
        }

        public bool ChangeRunBucketDescription(int bucketId, string newDescription)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().ChangeRunBucketDescription(bucketId, newDescription);
        }

        public bool DeleteRunBucket(int bucketId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().DeleteRunBucket(bucketId); ;
        }

        public bool AddRunToRunBucket(int runId, int bucketId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().AddRunToRunBucket(runId, bucketId); ;
        }

        public bool RemoveRunFromRunBucket(int runId, int bucketId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().RemoveRunFromRunBucket(runId, bucketId); ;
        }

        public bool DuplicateRunBucket(int bucketId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().DuplicateRunBucket(bucketId);
        }

        public RunCanalInputParseResult ProcessRunInputFile(Run run, byte[] fileContent)
        {
            //parse and preview
            var parseResult = EngineFactory.CreateEngine<IRunDataParseEngine>().ParseCanalRunDataFromFile(fileContent, run.Model);

            //save serialized and parsed result
            if (parseResult.Success)
            {
                //update run with new file name
                AccessorFactory.CreateAccessor<IRunAccessor>().CreateOrUpdateRun(run);

                var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

                //save parsed file                
                fileAccessor.SaveFile(ParsedCanalInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parseResult.RunInputs)));
            }

            return parseResult;
        }

        public RunWellInputParseResult ProcessWellRunInputFile(Run run, byte[] fileContent)
        {
            //parse and preview
            var parseResult = EngineFactory.CreateEngine<IRunDataParseEngine>().ParseWellRunDataFromFile(fileContent, run.Model);

            //save serialized and parsed result
            if (parseResult.Success)
            {
                //update run with new file name
                AccessorFactory.CreateAccessor<IRunAccessor>().CreateOrUpdateRun(run);

                var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

                //save parsed file                
                fileAccessor.SaveFile(ParsedWellInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parseResult.RunInputs)));
            }

            return parseResult;
        }

        public RunWellParticleInputParseResult ProcessWellParticleRunInputFile(Run run, byte[] fileContent)
        {
            //parse and preview
            var parseResult = EngineFactory.CreateEngine<IRunDataParseEngine>().ParseWellParticleRunDataFromFile(fileContent, run.Model);

            //save serialized and parsed result
            if (parseResult.Success)
            {
                //update run with new file name
                AccessorFactory.CreateAccessor<IRunAccessor>().CreateOrUpdateRun(run);

                var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

                //save parsed file                
                fileAccessor.SaveFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(parseResult.RunInputs)));
            }

            return parseResult;
        }

        public bool UploadInputFile(Run run, string name, byte[] fileContent)
        {
            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            if (run.Scenario.Files.Any(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            {
                blobFileAccessor.SaveFile(StorageLocations.InputFilePathForRun(name, run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, fileContent);

                return true;
            }

            return false;
        }

        public bool DeleteInputFile(string fileLocator, string filename)
        {
            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            blobFileAccessor.DeleteFile(StorageLocations.InputFilePathForRun(filename, fileLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);

            return true;
        }

        public bool UpdateInputCanalData(Run run, RunCanalInput[] data)
        {
            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            //save parsed inputs                
            fileAccessor.SaveFile(ParsedCanalInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));

            return true;
        }

        public byte[] FindCanalRunInputFile(Run run)
        {
            Logger.Info($"Finding input file for run {run.Id}");

            var data = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedCanalInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);

            if (data == null)
            {
                return EngineFactory.CreateEngine<IRunDataParseEngine>().CanalRunDataToCsv(BuildCanalInputsForRun(run));
            }

            var csvData = JsonConvert.DeserializeObject<List<RunCanalInput>>(Encoding.UTF8.GetString(data));

            return EngineFactory.CreateEngine<IRunDataParseEngine>().CanalRunDataToCsv(csvData);
        }

        public byte[] FindWellRunInputFile(Run run)
        {
            Logger.Info($"Finding input file for run {run.Id}");

            var data = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);

            var csvData = JsonConvert.DeserializeObject<List<RunWellInput>>(Encoding.UTF8.GetString(data));

            return EngineFactory.CreateEngine<IRunDataParseEngine>().WellRunDataToCsv(csvData);
        }

        public byte[] FindWellParticleRunInputFile(Run run)
        {
            Logger.Info($"Finding input file for run {run.Id}");

            var data = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);

            var csvData = JsonConvert.DeserializeObject<List<RunWellParticleInput>>(Encoding.UTF8.GetString(data));

            return EngineFactory.CreateEngine<IRunDataParseEngine>().WellParticleRunDataToCsv(csvData);
        }

        public byte[] DownloadKmlFile(string fileStorageLocator, string filename, int runResultId)
        {
            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
            return fileAccessor.GetFile(StorageLocations.OutputFilePathForRun($"!{runResultId.ToString().PadLeft(3, '0')}-{filename}.kml", fileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
        }

        public RunResultDetails FindRunResultDetails(string fileStorageLocator, int runResultId)
        {
            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
            var files = fileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(fileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            var file = files.Select(a => new { Name = a, Match = FileNameParseRegEx.Match(a) })
                .FirstOrDefault(a => a.Match.Success &&
                int.Parse(a.Match.Groups["id"].Value) == runResultId &&
                a.Match.Groups["extension"].Value.Equals(".json", StringComparison.InvariantCultureIgnoreCase));

            if (file == null)
            {
                return null;
            }

            var fileData = fileAccessor.GetFile(OutputFilePathForRun(fileStorageLocator, file.Name), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);

            var runResultDetails = JsonConvert.DeserializeObject<RunResultDetails>(System.Text.Encoding.UTF8.GetString(fileData));

            if (runResultDetails.ResultSets != null && runResultDetails.ResultSets.Count > 0 && runResultDetails.ResultSets[0].MapData != null)
            {
                var containsKmlFile = files.Select(a => new { Name = a, Match = FileNameParseRegEx.Match(a) })
                    .Any(a => a.Match.Success &&
                    int.Parse(a.Match.Groups["id"].Value) == runResultId &&
                    a.Match.Groups["extension"].Value.Equals(".kml", StringComparison.InvariantCultureIgnoreCase));

                runResultDetails.ResultSets[0].MapData.ContainsKmlFile = containsKmlFile;
            }

            return runResultDetails;
        }

        public ActionBucketResultDetails FindAggregateRunResultDetails(List<RunResultDisplay> runResultsToDisplay)
        {
            var resultDetails = new ActionBucketResultDetails();
            resultDetails.ResultSets = new List<RunResultSet>();

            var allResults = new List<RunResultSet>();

            foreach (var runResult in runResultsToDisplay)
            {
                var resultSet = new RunResultSet();

                var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
                var files = fileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(runResult.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
                var file = files.Select(a => new { Name = a, Match = FileNameParseRegEx.Match(a) })
                    .FirstOrDefault(a => a.Match.Success &&
                    int.Parse(a.Match.Groups["id"].Value) == runResult.RunResultId &&
                    a.Match.Groups["extension"].Value.Equals(".json", StringComparison.InvariantCultureIgnoreCase));

                if (file == null)
                {
                    return null;
                }

                var fileData = fileAccessor.GetFile(OutputFilePathForRun(runResult.FileStorageLocator, file.Name), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
                var runResultDetails = JsonConvert.DeserializeObject<RunResultDetails>(System.Text.Encoding.UTF8.GetString(fileData));

                foreach (var resultSetDetails in runResultDetails.ResultSets)
                {
                    var matchingResultSet = resultDetails.ResultSets.SingleOrDefault(x => x.Name == resultSetDetails.Name);

                    foreach (var dataSeries in resultSetDetails.DataSeries)
                    {
                        dataSeries.Name = runResult.Name + ": " + dataSeries.Name;
                    }

                    if (matchingResultSet == null)
                    {
                        resultDetails.ResultSets.Add(resultSetDetails);
                    }
                    else
                    {
                        matchingResultSet.DataSeries.AddRange(resultSetDetails.DataSeries);
                    }
                }

                if (runResultDetails.RelatedResultOptions != null)
                {
                    resultDetails.RelatedResultOptions = resultDetails.RelatedResultOptions ?? new List<ActionBucketRelatedResultOption>();
                    foreach (var relatedResultOption in runResultDetails.RelatedResultOptions)
                    {
                        resultDetails.RelatedResultOptions.Add(new ActionBucketRelatedResultOption
                        {
                            ResultId = relatedResultOption.Id,
                            RelatedResultName = relatedResultOption.Label,
                            FileStorageLocator = runResult.FileStorageLocator,
                        });
                    }
                }

            }

            // Order by the data series name so that the same results will be next to each other.
            // This helps with defaulting on the first X lines in the UI.
            foreach (var resultSet in resultDetails.ResultSets)
            {
                resultSet.DataSeries = resultSet.DataSeries.OrderBy(x => x.Name.Split(new string[] { ": " }, StringSplitOptions.None)[1]).ToList();
            }

            return resultDetails;
        }

        public string GetRunResultData(string fileStorageLocator, int runResultId, string fileExtension = ".json")
        {
            var fileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
            var files = fileAccessor.GetFilesInDirectory(OutputDirectoryPathForRun(fileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            var file = files.Select(a => new { Name = a, Match = FileNameParseRegEx.Match(a) })
                .FirstOrDefault(a => a.Match.Success &&
                int.Parse(a.Match.Groups["id"].Value) == runResultId &&
                a.Match.Groups["extension"].Value.Equals(fileExtension, StringComparison.InvariantCultureIgnoreCase));

            if (file == null)
            {
                return null;
            }

            var fileData = fileAccessor.GetFile(OutputFilePathForRun(fileStorageLocator, file.Name), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);

            return System.Text.Encoding.UTF8.GetString(fileData);
        }

        public bool QueueRun(int runId, bool shouldCreateMaps)
        {
            Logger.Info($"Queuing run {runId}");

            var accessor = AccessorFactory.CreateAccessor<IRunAccessor>();

            var run = accessor.FindRun(runId);

            //set which image will execute this run
            var image = AccessorFactory.CreateAccessor<IModelAccessor>().FindImageForModel(run.ModelId);
            run.ImageId = image.Id;

            //update status
            run.Status = RunStatus.Queued;
            run.ShouldCreateMaps = shouldCreateMaps;

            var result = accessor.CreateOrUpdateRun(run) != null;

            var queueAccessor = AccessorFactory.CreateAccessor<IQueueAccessor>();
            queueAccessor.CreateGenerateInputsMessage(runId, null);

            return result;
        }

        public bool QueueGenerateOutput(int runId)
        {
            var queueAccessor = AccessorFactory.CreateAccessor<IQueueAccessor>();
            queueAccessor.CreateGenerateOutputsMessage(runId, null);

            return true;
        }

        public bool QueueRunAnalysis(int runId)
        {
            var queueAccessor = AccessorFactory.CreateAccessor<IQueueAccessor>();
            queueAccessor.CreateRunAnalysisMessage(runId, null);

            return true;
        }

        public async Task StartContainer(int runId, AgentProcessType processType)
        {
            var containerAccessor = AccessorFactory.CreateAccessor<IContainerAccessor>();
            var queueAccessor = AccessorFactory.CreateAccessor<IQueueAccessor>();

            if (!await containerAccessor.CanQueueNewContainer())
            {
                switch (processType)
                {
                    case AgentProcessType.Input:
                        queueAccessor.CreateGenerateInputsMessage(runId, TimeSpan.FromMinutes(5));
                        break;
                    case AgentProcessType.Analysis:
                        queueAccessor.CreateRunAnalysisMessage(runId, TimeSpan.FromMinutes(5));
                        break;
                    case AgentProcessType.Output:
                        queueAccessor.CreateGenerateOutputsMessage(runId, TimeSpan.FromMinutes(5));
                        break;
                }
                return;
            }

            var runAccessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var apiFunctionsAccessor = AccessorFactory.CreateAccessor<IAPIFunctionsAccessor>();
            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
            var run = runAccessor.FindRunWithImage(runId);

            Exception startException = null;
            var containerStarted = false;

            // run custom image
            if (processType == AgentProcessType.Input && run.Scenario.InputImage != null)
            {
                blobFileAccessor.CreateFileShare(run.FileStorageLocator);

                //move input files into file storage
                var files = blobFileAccessor.GetFilesInDirectory(StorageLocations.InputDirectoryPathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);

                foreach (var file in files)
                {
                    blobFileAccessor.CopyFromBlobStorageToFileShare(StorageLocations.InputFilePathForRun(file, run.FileStorageLocator),
                        ConfigurationHelper.AppSettings.BlobStorageModelDataFolder,
                        file,
                        run.FileStorageLocator);
                }

                try
                {
                    Dictionary<string, string> envVars = new Dictionary<string, string>
                    {
                        { "SOURCE_FOLDER", ConfigurationHelper.AppSettings.AzureContainerVolumeName },
                        { "ANALYSIS_URL", GetAnalysisUrl(runId) },
                        { "MODEL_ID", run.ModelId.ToString() }
                    };

                    containerAccessor.StartAzureContainer(run.FileStorageLocator,
                        run.Scenario.InputImage.Name,
                        run.Scenario.InputImage.CpuCoreCount ?? 1,
                        decimal.ToDouble(run.Scenario.InputImage.Memory ?? 3.5m),
                        envVars,
                        processType,
                        run.Scenario.InputImage.IsLinux);

                    containerStarted = true;
                }
                catch (Exception ex)
                {
                    startException = ex;
                    Logger.Error($"Error while starting container: {ex.AllExceptionMessages()}");
                }
            }
            // start regular docker container
            else
            {
                Dictionary<string, string> envVars = new Dictionary<string, string>
                    {
                        { "ANALYSIS_URL", ConfigurationHelper.AppSettings.RunAnalysisUrl },
                        { "API_FUNCTION_CODE", ConfigurationHelper.AppSettings.APIFunctionCode },
                        { "AZURESTORAGEACCOUNT", ConfigurationHelper.ConnectionStrings.AzureStorageAccount },
                        { "BLOBSTORAGEMODELDATAFOLDER", ConfigurationHelper.AppSettings.BlobStorageModelDataFolder },
                        { "BRAVOPRIMARYDATABASE", ConfigurationHelper.ConnectionStrings.BravoPrimaryConnectionString },
                        { "MODFLOWDATAFOLDER", ConfigurationHelper.AppSettings.ModflowDataFolder },
                        { "NOTIFICATION_URL", ConfigurationHelper.AppSettings.SendRunCompletedNotificationUrl },
                        { "OUTPUTS_URL", ConfigurationHelper.AppSettings.GenerateOutputsUrl },
                        { "PROCESSTYPE", ((int)processType).ToString()  },
                        { "RUN_ID", runId.ToString() },
                        { "MODEL_ID", run.ModelId.ToString() }
                    };

                try
                {
                    containerAccessor.StartAzureContainer(run.FileStorageLocator,
                       run.Image.Name,
                       run.Image.CpuCoreCount ?? 1,
                       decimal.ToDouble(run.Image.Memory ?? 3.5m),
                       envVars,
                       processType,
                       run.Image.IsLinux);

                    containerStarted = true;
                }
                catch (Exception ex)
                {
                    startException = ex;
                    Logger.Error($"Error while starting container: {ex.AllExceptionMessages()}");
                }
            }

            if (processType == AgentProcessType.Input)
            {
                run.ProcessingStartDate = DateTime.UtcNow;
            }

            if (containerStarted)
            {
                if (processType == AgentProcessType.Input)
                {
                    run.Status = RunStatus.ProcesingInputs;
                }
                else if (processType == AgentProcessType.Analysis)
                {
                    run.Status = RunStatus.RunningAnalysis;
                }
            }
            else
            {
                run.Status = RunStatus.SystemError;
                run.ProcessingEndDate = DateTime.UtcNow;
            }

            runAccessor.CreateOrUpdateRun(run);
        }

        public bool GenerateInputFiles(int runId)
        {
            Logger.Info($"Generating input files for run {runId}:");

            var fileAccessor = AccessorFactory.CreateAccessor<IFileAccessor>();
            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
            var initialFiles = fileAccessor.GetFilesInModflowDataFolder();

            var runAccessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var apiFunctionsAccessor = AccessorFactory.CreateAccessor<IAPIFunctionsAccessor>();
            var run = runAccessor.FindRunWithImage(runId);

            var wellParticleMapInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            if (wellParticleMapInputData != null)
            {
                run.RunWellParticleInputs = (JsonConvert.DeserializeObject<RunWellParticleInput[]>(Encoding.UTF8.GetString(wellParticleMapInputData))).ToList();
            }

            var modelInputOutputEngine = EngineFactory.CreateEngine<IModelInputOutputEngineFactory>().CreateModelInputOutputEngine(run);

            try
            {
                //Parse Inputs
                modelInputOutputEngine.GenerateInputFiles(run);
            }
            catch (InputDataInvalidException diex)
            {
                Logger.Error(diex);
                runAccessor.UpdateRunStatus(run.Id, RunStatus.InvalidInput, null);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                runAccessor.UpdateRunStatus(run.Id, RunStatus.SystemError, null);
                return false;
            }

            var updatedFiles = fileAccessor.GetFilesInModflowDataFolder();

            // upload any new or updated files from analysis into blob storage
            foreach (var file in updatedFiles)
            {
                if (!initialFiles.Any(x => x.Path.Equals(file.Path)) || initialFiles.Any(x => x.Path.Equals(file.Path) && x.ModDate != file.ModDate))
                {
                    blobFileAccessor.SaveFile(StorageLocations.GenerateInputOutputFilePath(file.Name, run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, file.Path);
                }
            }

            apiFunctionsAccessor.MakeFunctionCall(GetAnalysisUrl(runId));

            return true;
        }

        // planned to upload analysis output files to blob storage to be downloaded by generate outputs container
        // but encountered large files output (6.5gb)
        // MemoryStream upload is limited to 2gb
        // a library Microsoft.Azure.Storage.DataMovement supports transferring large files but timed out at 5.7gb after running for ~30 minutes
        // generate outputs will now be performed in the analysis container for now
        public bool RunAnalysis(int runId)
        {
            var fileAccessor = AccessorFactory.CreateAccessor<IFileAccessor>();
            var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();

            var runAccessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var run = runAccessor.FindRunWithImage(runId);

            var storageFiles = new List<string>();
            var storageFilesCopied = new List<string>();

            var usesFileStorage = run.Scenario.InputImage != null && run.Scenario.InputImage.IsLinux;

            // get files from generate input container
            if (usesFileStorage)
            {
                var modelFiles = fileAccessor.GetFilesInModflowDataFolder();

                storageFiles = blobFileAccessor.GetFilesInShareDirectory(run.FileStorageLocator);

                foreach (var file in storageFiles)
                {
                    if (modelFiles.Any(x => x.Name.Equals(file, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var destPath = Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, file);
                        fileAccessor.DeleteFile(destPath);
                        blobFileAccessor.GetSharedFile(file, run.FileStorageLocator, destPath);
                        storageFilesCopied.Add(file);
                    }
                }
            }
            else
            {
                storageFiles = blobFileAccessor.GetFilesInDirectory(StorageLocations.GenerateInputOutputFolderPath(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);

                foreach (var blobFile in storageFiles)
                {
                    var destPath = Path.Combine(ConfigurationHelper.AppSettings.ModflowDataFolder, blobFile);
                    fileAccessor.DeleteFile(destPath);
                    blobFileAccessor.GetFile(StorageLocations.GenerateInputOutputFilePath(blobFile, run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, destPath);
                }
            }

            //Run Modflow
            var runResult = EngineFactory.CreateEngine<IAnalysisEngine>().RunAnalysis(run);
            if (!runResult.Success)
            {
                Logger.Error("Analysis failed to complete successfully.  Still will attempt to generate outputs.");
            }

            runAccessor.UpdateRunStatus(run.Id, runResult.Success ? RunStatus.AnalysisSuccess : RunStatus.AnalysisFailed, runResult?.ConsoleOutput);

            if (usesFileStorage)
            {
                // move copied files into model outputs
                foreach (var file in storageFilesCopied)
                {
                    blobFileAccessor.CopyFromFileShareToBlobStorage(file, run.FileStorageLocator, StorageLocations.ModelOutputFolderPath(run.Image.Name, file), ConfigurationHelper.AppSettings.BlobStorageModelOutputsFolder);
                }

                // delete files from generate input
                blobFileAccessor.DeleteCloudFileShare(run.FileStorageLocator);
            }
            else
            {
                foreach (var blobFile in storageFiles)
                {
                    blobFileAccessor.DeleteFile(StorageLocations.GenerateInputOutputFilePath(blobFile, run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
                }
            }

            var apiFunctionsAccessor = AccessorFactory.CreateAccessor<IAPIFunctionsAccessor>();
            apiFunctionsAccessor.MakeFunctionCall(GetGenerateOutputsUrl(runId));

            return runResult.Success;
        }

        public bool GenerateOutputFiles(int runId)
        {
            var runAccessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var apiFunctionsAccessor = AccessorFactory.CreateAccessor<IAPIFunctionsAccessor>();
            var run = runAccessor.FindRunWithImage(runId);

            var modelInputOutputEngine = EngineFactory.CreateEngine<IModelInputOutputEngineFactory>().CreateModelInputOutputEngine(run);

            var wellParticleMapInputData = AccessorFactory.CreateAccessor<IBlobFileAccessor>().GetFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder);
            if (wellParticleMapInputData != null)
            {
                run.RunWellParticleInputs = (JsonConvert.DeserializeObject<RunWellParticleInput[]>(Encoding.UTF8.GetString(wellParticleMapInputData))).ToList();
            }

            try
            {
                //Parse Outputs
                modelInputOutputEngine.GenerateOutputFiles(run);
            }
            catch (OutputDataInvalidException diex)
            {
                Logger.Error(diex);
                runAccessor.UpdateRunStatus(run.Id, diex.Status, null);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                runAccessor.UpdateRunStatus(run.Id, RunStatus.SystemError, null);
                return false;
            }
            finally
            {
                #if !DEBUG
                var blobFileAccessor = AccessorFactory.CreateAccessor<IBlobFileAccessor>();
                blobFileAccessor.DeleteCloudFileShare(run.FileStorageLocator);
                #endif
            }

            if (run.Status != RunStatus.AnalysisFailed)
            {
                // preserve whatever message
                runAccessor.UpdateRunStatus(run.Id, RunStatus.Complete, null);
            }
            else
            {
                runAccessor.UpdateRunStatus(run.Id, RunStatus.SystemError, null);
            }

            return true;
        }

        public async Task CleanCompletedRuns()
        {
            var containerAccessor = AccessorFactory.CreateAccessor<IContainerAccessor>();
            var runAccessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var apiFunctionsAccessor = AccessorFactory.CreateAccessor<IAPIFunctionsAccessor>();
            var containers = await containerAccessor.GetAzureContainers();

            var fileStorageLocator = GetFileStorageLocators(containers.Select(x => x.GroupName).ToList());

            var runs = runAccessor.FindRunsByFileStorageLocators(fileStorageLocator);

            var cutOffDate = DateTime.UtcNow.AddDays(-ConfigurationHelper.AppSettings.ContainerRetentionPeriodInDays);

            var deleteContainerTasks = new List<Task>();
            foreach (var exitedContainer in containers)
            {
                var run = runs.SingleOrDefault(x => x.FileStorageLocator.Equals(GetFileStorageLocator(exitedContainer.GroupName)));

                if (exitedContainer.State.Equals("Succeeded") ||
                    exitedContainer.State.Equals("Stopped"))
                {
                    if (exitedContainer.Events != null && exitedContainer.Events.Count > 0)
                    {
                        var timestamp = exitedContainer.Events.OrderByDescending(x => x.LastTimeStamp).First().LastTimeStamp;

                        if (timestamp < cutOffDate &&
                            (run == null || run.IsDeleted || RunStatusToDelete.Contains(run.Status)))
                        {
                            deleteContainerTasks.Add(containerAccessor.DeleteAzureContainer(exitedContainer.Id));
                        }
                    }
                    else
                    {
                        RestartContainer(run, exitedContainer.Id, runAccessor, containerAccessor, apiFunctionsAccessor);
                    }
                }
                else if (exitedContainer.State.Equals("Failed") && !RunStatusToDelete.Contains(run.Status))
                {
                    RestartContainer(run, exitedContainer.Id, runAccessor, containerAccessor, apiFunctionsAccessor);
                }
            }
            await Task.WhenAll(deleteContainerTasks);
        }

        private List<string> GetFileStorageLocators(List<string> containerGroupNames)
        {
            var names = new List<string>();

            foreach (var name in containerGroupNames)
            {
                names.Add(GetFileStorageLocator(name));
            }

            return names;
        }

        private string GetFileStorageLocator(string containerGroupName)
        {
            return containerGroupName.Replace("-input", "").Replace("-analysis", "");
        }

        private void RestartContainer(Run run, string containerId, IRunAccessor runAccessor, IContainerAccessor containerAccessor, IAPIFunctionsAccessor apiFunctionsAccessor)
        {
            if (run.RestartCount < 1)
            {
                containerAccessor.RestartContainerAsync(containerId);

                run.RestartCount = run.RestartCount + 1;

                runAccessor.CreateOrUpdateRun(run);
            }
            else
            {
                if (!RunStatusToDelete.Contains(run.Status))
                {
                    run.Status = RunStatus.SystemError;

                    runAccessor.CreateOrUpdateRun(run);
                }

            }
        }

        public async Task FailLongProcessingRuns()
        {
            var containerAccessor = AccessorFactory.CreateAccessor<IContainerAccessor>();
            var runAccessor = AccessorFactory.CreateAccessor<IRunAccessor>();
            var containers = await containerAccessor.GetAzureContainers();

            var fileStorageLocator = GetFileStorageLocators(containers.Select(x => x.GroupName).ToList());

            var runs = runAccessor.FindRunsByFileStorageLocators(fileStorageLocator);

            var cutOffDate = DateTime.UtcNow.AddHours(-ConfigurationHelper.AppSettings.MaxRunProcessingTimeInHours);

            foreach (var container in containers)
            {
                var run = runs.SingleOrDefault(x => x.FileStorageLocator.Equals(GetFileStorageLocator(container.GroupName)));

                if (container.State.Equals("Running") &&
                    run != null &&
                    run.ProcessingStartDate < cutOffDate)
                {
                    Logger.Warn($"Stopping long running container [{container.Id}] [{run.Id}]");
                    await containerAccessor.StopContainerAsync(container.Id);

                    //flag it as error
                    run.ProcessingEndDate = DateTime.UtcNow;
                    run.Status = RunStatus.SystemError;

                    //save it
                    runAccessor.CreateOrUpdateRun(run);
                }
            }
        }

        public bool UpdateInputWellData(PivotedRunWellInput[] wellData, int runId)
        {
            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId);

            //PivotedRunWellInput[] to RunWellInput[]
            var runWellInputs = BuildWellInputData(wellData, run);

            //save parsed inputs                
            AccessorFactory.CreateAccessor<IBlobFileAccessor>().SaveFile(ParsedWellInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(runWellInputs)));

            return true;
        }

        public bool UpdateInputWellParticleData(RunWellParticleInput[] wellData, int runId)
        {
            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId);

            //save parsed inputs                
            AccessorFactory.CreateAccessor<IBlobFileAccessor>().SaveFile(ParsedWellParticleInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(wellData)));

            return true;
        }

        public bool UpdateInputZoneData(RunZoneInput[] zoneData, int runId)
        {
            var run = AccessorFactory.CreateAccessor<IRunAccessor>().FindRun(runId);

            //save parsed inputs                
            AccessorFactory.CreateAccessor<IBlobFileAccessor>().SaveFile(ParsedZoneInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(zoneData)));

            return true;
        }

        public RunStatus? GetRunStatus(int runId)
        {
            return AccessorFactory.CreateAccessor<IRunAccessor>().GetRunStatus(runId);
        }

        #region Private Methods
        private string ParsedCanalInputFilePathForRun(string fileLocator)
        {
            return StorageLocations.ParsedInputFilePathForRun(fileLocator);
        }

        private string ParsedWellInputFilePathForRun(string fileLocator)
        {
            return StorageLocations.ParsedWellInputFilePathForRun(fileLocator);
        }

        private string ParsedWellParticleInputFilePathForRun(string fileLocator)
        {
            return StorageLocations.ParsedWellParticleInputFilePathForRun(fileLocator);
        }

        private string ParsedZoneInputFilePathForRun(string fileLocator)
        {
            return StorageLocations.ParsedZoneInputFilePathForRun(fileLocator);
        }

        private string OutputFilePathForRun(string fileStorageLocator, string outputFileName)
        {
            return $"{OutputDirectoryPathForRun(fileStorageLocator)}/{outputFileName}";
        }

        private string OutputDirectoryPathForRun(Run run)
        {
            return StorageLocations.OutputDirectoryPathForRun(run.FileStorageLocator);
        }

        private string OutputDirectoryPathForRun(string fileStorageLocator)
        {
            return StorageLocations.OutputDirectoryPathForRun(fileStorageLocator);
        }

        private readonly RunStatus[] _finishedStatuses = { RunStatus.Complete, RunStatus.InvalidOutput, RunStatus.SystemError, RunStatus.InvalidInput, RunStatus.HasDryCells };

        private List<RunCanalInput> BuildCanalInputsForRun(Run run)
        {
            var inputs = new List<RunCanalInput>();
            
            if (string.IsNullOrWhiteSpace(run.Model.CanalData))
            {
                throw new Exception("Trying to build canal inputs but no canal data in the database.");
            }

            var canals = (run.Model.CanalData ?? "").Split(',');

            for (var i = 0; i < run.Model.NumberOfStressPeriods; i++)
            {
                var stressPeriodDate = run.Model.ModelStressPeriodCustomStartDates != null && run.Model.ModelStressPeriodCustomStartDates.Length > 0 ? run.Model.ModelStressPeriodCustomStartDates[i].StressPeriodStartDate : run.Model.StartDateTime.AddMonths(i);

                var input = new RunCanalInput()
                {
                    Month = stressPeriodDate.Month,
                    Year = stressPeriodDate.Year
                };

                input.Values = new List<FeatureValue>();

                foreach (var canal in canals)
                {
                    input.Values.Add(new FeatureValue()
                    {
                        Value = 0,
                        FeatureName = canal
                    });
                }

                inputs.Add(input);
            }

            return inputs;
        }

        private RunWellInput[] BuildWellInputData(PivotedRunWellInput[] data, Run run)
        {
            var result = new List<RunWellInput>();
            

            for (var i = 0; i < run.Model.NumberOfStressPeriods; i++)
            {
                var stressPeriodDate = run.Model.ModelStressPeriodCustomStartDates != null && run.Model.ModelStressPeriodCustomStartDates.Length > 0 ? run.Model.ModelStressPeriodCustomStartDates[i].StressPeriodStartDate : run.Model.StartDateTime.AddMonths(i);

                var stressPeriodInput = new RunWellInput()
                {
                    Month = stressPeriodDate.Month,
                    Year = stressPeriodDate.Year,
                    ManuallyAdded = data.Any(d => d.ManuallyAdded),
                    Values = new List<FeatureWithLocationValue>()
                };

                foreach (var well in data)
                {
                    stressPeriodInput.Values.Add(new FeatureWithLocationValue()
                    {
                        Value = well.AverageValue,
                        Lng = well.Lng,
                        Lat = well.Lat,
                        FeatureName = well.Name
                    });
                }

                result.Add(stressPeriodInput);
            }

            return result.ToArray();
        }

        private PivotedRunWellInput[] BuildWellPivotedInputData(RunWellInput[] inputs, Run run)
        {
            var result = new List<PivotedRunWellInput>();
            foreach (var well in inputs.First().Values)
            {
                var pivotedData = new PivotedRunWellInput()
                {
                    Lat = well.Lat,
                    Lng = well.Lng,
                    ManuallyAdded = inputs.First().ManuallyAdded,
                    Name = well.FeatureName,
                };

                var values = new List<StressPeriodValue>();

                foreach (var input in inputs)
                {
                    values.Add(new StressPeriodValue()
                    {
                        Month = input.Month,
                        Year = input.Year,
                        Value = input.Values.First(v => v.FeatureName == well.FeatureName).Value
                    });
                }

                pivotedData.StressPeriodValues = values;
                pivotedData.AverageValue = pivotedData.StressPeriodValues.Average(spv => spv.Value);

                result.Add(pivotedData);
            }

            return result.ToArray();
        }

        private string GetAnalysisUrl(int runId)
        {
            return $"{ConfigurationHelper.AppSettings.RunAnalysisUrl}?code={ConfigurationHelper.AppSettings.APIFunctionCode}&RunId={runId.ToString()}";
        }

        private string GetGenerateOutputsUrl(int runId)
        {
            return $"{ConfigurationHelper.AppSettings.GenerateOutputsUrl}?code={ConfigurationHelper.AppSettings.APIFunctionCode}&RunId={runId.ToString()}";
        }

        private DateTime ParseDateTime(string str)
        {
            return new DateTime(int.Parse(str.Substring(0, 4)),
                int.Parse(str.Substring(4, 2)),
                int.Parse(str.Substring(6, 2)),
                int.Parse(str.Substring(8, 2)),
                int.Parse(str.Substring(10, 2)),
                int.Parse(str.Substring(12, 2)));
        }

        private bool IsCustomInput(Run run)
        {
            return run?.Scenario?.Files != null && run.Scenario.Files.Any();
        }

        #endregion

    }
}
