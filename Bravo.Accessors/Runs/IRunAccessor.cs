using Bravo.Common.DataContracts.Runs;
using System.Collections.Generic;
using Run = Bravo.Common.DataContracts.Runs.Run;

namespace Bravo.Accessors.Runs
{
    public interface IRunAccessor
    {
        List<Run> FindRuns(RunFilter filter, int skip, int take);

        List<Run> GetRuns();

        int FindRunsCount(RunFilter filter);

        Run FindRun(int runId);

        Run FindRunWithImage(int runId);

        Run CreateOrUpdateRun(Run run);

        bool DeleteRun(int runId);

        bool RenameRun(int runId, string newName);

        bool ChangeRunDescription(int runId, string newDescription);

        bool UpdateRunStatus(int runId, RunStatus status, string output);

        RunStatus? GetRunStatus(int runId);

        List<Run> FindRunsByFileStorageLocators(List<string> fileStorageLocators);

        RunBucket FindRunBucket(int bucketId);

        List<RunBucket> GetRunBuckets();

        RunBucket CreateOrUpdateRunBucket(RunBucket runBucket);

        bool RenameRunBucket(int bucketId, string newName);

        bool ChangeRunBucketDescription(int bucketId, string newDescription);

        bool DeleteRunBucket(int bucketId);

        bool DuplicateRunBucket(int bucketId);

        bool AddRunToRunBucket(int runId, int bucketId);

        bool RemoveRunFromRunBucket(int runId, int bucketId);
    }
}
