using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using log4net;
using Bravo.Common.DataContracts.Runs;
using Bravo.Common.Shared.Enums;
using Bravo.Common.Utilities;
using Run = Bravo.Common.DataContracts.Runs.Run;

namespace Bravo.Accessors.Runs
{
    internal class RunAccessor : BaseTableAccessor, IRunAccessor
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(RunAccessor));
        public Run CreateOrUpdateRun(Run run)
        {
            return base.CreateOrUpdate<Run, EntityFramework.Run, PrimaryDBContext>(run);
        }

        public bool DeleteRun(int runId)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = (from r in db.Runs
                           where r.Id == runId
                           select r).FirstOrDefault();

                run.IsDeleted = true;

                return db.SaveChanges() == 1;
            }
        }

        private readonly int[] _processingStatuses = { (int)RunStatus.Processing };

        public Run FindRun(int runId)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = (from r in db.Runs.Include("Model").Include("Model.ModelStressPeriodCustomStartDates").Include("Scenario").Include("Scenario.Files")
                    where r.Id == runId && !r.IsDeleted
                    select r).FirstOrDefault();

                return DTOMapper.Mapper.Map<Run>(run);
            }
        }

        public Run FindRunWithImage(int runId)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = (from r in db.Runs.Include("Model").Include("Model.ModelStressPeriodCustomStartDates").Include("Scenario").Include("Image").Include("Scenario.InputImage").Include("Scenario.Files").Include("Model.BaseflowTableProcessingConfiguration")
                           where r.Id == runId && !r.IsDeleted
                           select r).FirstOrDefault();

                return DTOMapper.Mapper.Map<Run>(run);
            }
        }

        public List<Run> FindRuns(RunFilter filter, int skip, int take)
        {
            var hasStatusFilter = (filter.Statuses != null && filter.Statuses.Count > 0);

            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runs = (from r in db.Runs.Include("Model").Include("Scenario").Include("User").Include("Model.Image").Include("Scenario.InputImage")
                            where (filter.NameSearch == null || filter.NameSearch.Trim() == string.Empty || r.Name.Contains(filter.NameSearch))
                            && (!filter.ModelId.HasValue || filter.ModelId.Value == r.ModelId)
                            && (!filter.ScenarioId.HasValue || filter.ScenarioId.Value == r.ScenarioId)
                            && (!hasStatusFilter || (filter.Statuses.Select(s => (int)s).Contains(r.Status)))
                            && !r.IsDeleted
                            orderby r.CreatedDate descending
                            select new { r = r, m = r.Model, i = r.Model.Image, s = r.Scenario })
                            .Skip(skip)
                            .Take(take)
                            .ToArray();

                var result = DTOMapper.Mapper.Map<Run[]>(runs.Select(run => run.r)).ToList();
                
                return result;
            }
        }

        public List<Run> GetRuns()
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runs = (from r in db.Runs
                            select r).ToList();

                return DTOMapper.Mapper.Map<Run[]>(runs.Select(run => run)).ToList();
            }
        }

        public int FindRunsCount(RunFilter filter)
        {
            var hasStatusFilter = (filter.Statuses != null && filter.Statuses.Count > 0);

            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runCount = (from r in db.Runs.Include("Model").Include("Scenario").Include("Model.Image").Include("Scenario.InputImage")
                                where (filter.NameSearch == null || filter.NameSearch.Trim() == string.Empty || r.Name.Contains(filter.NameSearch))
                                && (!filter.ModelId.HasValue || filter.ModelId.Value == r.ModelId)
                                && (!filter.ScenarioId.HasValue || filter.ScenarioId.Value == r.ScenarioId)
                                && (!hasStatusFilter || (filter.Statuses.Select(s => (int)s).Contains(r.Status)))
                                && !r.IsDeleted
                                select r.Id).Count();

                return runCount;
            }
        }

        public List<Run> FindRunsByFileStorageLocators(List<string> fileStorageLocators)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runs = (from r in db.Runs
                            where fileStorageLocators.Contains(r.FileStorageLocator)
                            select new { r }).ToArray();

                return DTOMapper.Mapper.Map<Run[]>(runs.Select(run => run.r)).ToList();
            }
        }

        public bool RenameRun(int runId, string newName)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = (from r in db.Runs
                           where r.Id == runId
                           select r).FirstOrDefault();

                run.Name = newName;

                return db.SaveChanges() == 1;
            }
        }

        public bool ChangeRunDescription(int runId, string newDescription)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = (from r in db.Runs
                           where r.Id == runId
                           select r).FirstOrDefault();

                run.Description = newDescription;

                return db.SaveChanges() == 1;
            }
        }

        private readonly RunStatus[] _finishedStatuses = { RunStatus.Complete, RunStatus.InvalidOutput, RunStatus.SystemError, RunStatus.InvalidInput, RunStatus.HasDryCells };

        public bool UpdateRunStatus(int runId, RunStatus status, string output)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = (from r in db.Runs
                           where r.Id == runId
                           select r).FirstOrDefault();

                run.Status = (int)status;
                if (_finishedStatuses.Contains(status))
                {
                    run.ProcessingEndDate = DateTime.UtcNow;
                    run.Output = output;
                }

                return db.SaveChanges() == 1;
            }
        }

        public RunStatus? GetRunStatus(int runId)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var run = (from r in db.Runs
                           where r.Id == runId
                           select r).FirstOrDefault();

                if (run != null)
                {
                    return (RunStatus)run.Status;
                }

                return null;
            }
        }

        public RunBucket FindRunBucket(int bucketId)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBucket = db.RunBuckets
                    .Where(x => x.Id == bucketId)
                    .Select(x => new RunBucket()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        CreatedDate = x.CreatedDate,
                        Runs = x.RunBucketRuns.Where(y => y.Run.IsDeleted == false)
                        .Select(y => new Run
                        {
                            Id = y.RunId,
                            Name = y.Run.Name,
                            Status = (RunStatus)y.Run.Status,
                            FileStorageLocator = y.Run.FileStorageLocator,
                            Model = new Common.DataContracts.Models.Model
                            {
                                Id = y.Run.ModelId,
                                Name = y.Run.Model.Name,
                                BuddyGroup = y.Run.Model.BuddyGroup
                            },
                            Scenario = new Common.DataContracts.Models.Scenario{ 
                                Id = y.Run.Scenario.Id,
                                Name = y.Run.Scenario.Name,
                                InputControlType = (Common.DataContracts.Models.InputControlType)y.Run.Scenario.InputControlType,
                                ShouldSwitchSign = y.Run.Scenario.ShouldSwitchSign,
                                InputImageId = y.Run.Scenario.InputImageId
                            }
                        }).ToList(),
                        Description = x.Description
                    });
                return runBucket.SingleOrDefault();
            }
        }

        public List<RunBucket> GetRunBuckets()
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBuckets = db.RunBuckets
                    .Select(x => new RunBucket()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        CreatedDate = x.CreatedDate,
                        Runs = x.RunBucketRuns.Where(y => y.Run.IsDeleted == false)
                        .Select(y => new Run
                        {
                            Id = y.RunId,
                            Name = y.Run.Name,
                            OutputVolumeType = (VolumeType)y.Run.OutputVolumeUnit,
                            IsDifferential = y.Run.IsDifferential,
                            Description = y.Run.Description,
                            Model = new Common.DataContracts.Models.Model
                            {
                                Id = y.Run.ModelId,
                                Name = y.Run.Model.Name,
                                BuddyGroup = y.Run.Model.BuddyGroup
                            }
                        }).ToList(),
                        Description = x.Description
                    })
                    .ToList();

                return runBuckets;
            }
        }

        public RunBucket CreateOrUpdateRunBucket(RunBucket runBucket)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                return base.CreateOrUpdate<RunBucket, EntityFramework.RunBucket, PrimaryDBContext>(runBucket);
            }
        }

        public bool RenameRunBucket(int bucketId, string newName)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBucket = (from r in db.RunBuckets
                                where r.Id == bucketId
                                select r).FirstOrDefault();

                runBucket.Name = newName;

                return db.SaveChanges() == 1;
            }
        }

        public bool ChangeRunBucketDescription(int bucketId, string newDescription)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBucket = (from r in db.RunBuckets
                                where r.Id == bucketId
                                select r).FirstOrDefault();

                runBucket.Description = newDescription;

                return db.SaveChanges() == 1;
            }
        }

        public bool DeleteRunBucket(int bucketId)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                db.RunBucketRuns.RemoveRange(from r in db.RunBucketRuns
                                             where r.RunBucketId == bucketId
                                             select r);

                db.RunBuckets.RemoveRange(from r in db.RunBuckets
                                          where r.Id == bucketId
                                          select r);

                return db.SaveChanges() == 1;
            }
        }

        public bool DuplicateRunBucket(int bucketId)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBucket = (from r in db.RunBuckets
                                 where r.Id == bucketId
                                 select r).FirstOrDefault();

                var newBucket = db.RunBuckets.Add(new EntityFramework.RunBucket()
                {
                    Name = runBucket.Name + " - Copy @" + DateTime.Now.ToString(),
                    CreatedDate = DateTime.Now
                });

                db.SaveChanges();

                var runBucketRuns = (from r in db.RunBucketRuns
                                    where r.RunBucketId == bucketId
                                    select r).ToList();
                
                foreach(var run in runBucketRuns)
                {
                    db.RunBucketRuns.Add(new EntityFramework.RunBucketRun()
                    {
                        RunId = run.RunId,
                        RunBucketId = newBucket.Id
                    });
                    db.SaveChanges();
                }

                return true;
            }
        }

        public bool AddRunToRunBucket(int runId, int bucketId)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBucketExists = (from r in db.RunBuckets where r.Id == bucketId select 1).Any();

                if (runBucketExists)
                {
                    db.RunBucketRuns.Add(new EntityFramework.RunBucketRun()
                    {
                        RunId = runId,
                        RunBucketId = bucketId
                    });

                    return db.SaveChanges() == 1;
                }

                return false;
            }
        }

        public bool RemoveRunFromRunBucket(int runId, int bucketId)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var runBucket = (from r in db.RunBucketRuns
                                 where r.RunId == runId && r.RunBucketId == bucketId
                                 select r).FirstOrDefault();

                if(runBucket != null)
                {
                    db.RunBucketRuns.Remove(runBucket);

                    return db.SaveChanges() == 1;
                }

                return false;
            }
        }
    }
}
