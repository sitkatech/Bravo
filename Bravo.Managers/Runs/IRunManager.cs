using Bravo.Common.DataContracts.APIFunctionModels;
using Bravo.Common.DataContracts.Models;
using Bravo.Common.DataContracts.Runs;
using Bravo.Common.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bravo.Managers.Runs
{
    public interface IRunManager
    {
        List<Run> FindRuns(RunFilter filter, int pageNum = 0);

        List<RunSummaryResponseModel> GetRuns();

        int FindRunsCount(RunFilter filter);

        Run FindRun(int runId, bool includeHiddenFiles = false);

        Run CreateOrUpdateRun(Run run);

        Run DuplicateRun(int runId);

        bool DeleteRun(int runId);

        bool RenameRun(int runId, string newName);

        bool ChangeRunDescription(int runId, string newDescription);

        RunBucket FindRunBucket(int bucketId);

        List<RunBucket> GetRunBuckets();

        RunBucket CreateOrUpdateRunBucket(RunBucket runBucket);

        bool RenameRunBucket(int bucketId, string newName);

        bool ChangeRunBucketDescription(int bucketId, string newDescription);

        bool AddRunToRunBucket(int runId, int bucketId);

        bool RemoveRunFromRunBucket(int runId, int bucketId);

        bool DuplicateRunBucket(int bucketId);

        bool DeleteRunBucket(int bucketId);

        RunCanalInputParseResult ProcessRunInputFile(Run run, byte[] fileContent);

        RunWellInputParseResult ProcessWellRunInputFile(Run run, byte[] fileContent);

        RunWellParticleInputParseResult ProcessWellParticleRunInputFile(Run run, byte[] fileContent);

        bool UpdateInputCanalData(Run run, RunCanalInput[] data);

        bool UpdateInputWellData(PivotedRunWellInput[] wellData, int runId);

        bool UpdateInputWellParticleData(RunWellParticleInput[] wellData, int runId);

        bool UpdateInputZoneData(RunZoneInput[] zoneData, int runId);

        RunResultDetails FindRunResultDetails(string fileStorageLocator, int runResultId);

        ActionBucketResultDetails FindAggregateRunResultDetails(List<RunResultDisplay> runResultsToDisplay);

        string GetRunResultData(string fileStorageLocator, int runResultId, string fileExtension = ".json");

        bool QueueRun(int runId, bool shouldCreateMaps);

        byte[] FindCanalRunInputFile(Run run);

        byte[] FindWellRunInputFile(Run run);

        byte[] FindWellParticleRunInputFile(Run run);

        bool GenerateInputFiles(int runId);

        bool RunAnalysis(int runId);

        bool GenerateOutputFiles(int runId);

        Task CleanCompletedRuns();

        Task FailLongProcessingRuns();

        Task StartContainer(int runId, AgentProcessType processType);

        bool QueueGenerateOutput(int runId);

        bool QueueRunAnalysis(int runId);

        RunStatus? GetRunStatus(int runId);

        byte[] DownloadKmlFile(string fileStorageLocator, string filename, int runResultId);

        bool UploadInputFile(Run run, string name, byte[] fileContent);

        bool DeleteInputFile(string fileLocator, string filename);

        List<AvailableRunResult> FindAvailableRunResults(int runId);

        RunResultResponseModel GetRunResult(int runId, string fileName, string subType, string fileType);
    }
}
