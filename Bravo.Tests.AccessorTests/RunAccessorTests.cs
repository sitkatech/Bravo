using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bravo.Accessors;
using Bravo.Accessors.Runs;
using Bravo.Common.DataContracts.Runs;
using FluentAssertions;

namespace Bravo.Tests.AccessorTests
{
    [TestClass]
    public class RunAccessorTests : BaseAccessorTest
    {
        IRunAccessor _runAccessor = new AccessorFactory().CreateAccessor<IRunAccessor>();

        [TestMethod]
        public void RunAccessorTests_FindRun_NonDifferential()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, IsDifferential = false, Description = "test description" });

            run1.Should().NotBeNull();

            var findResult = _runAccessor.FindRunWithImage(run1.Id);

            findResult.Should().NotBeNull();
            findResult.Id.Should().Equals(run1.Id);
            findResult.IsDifferential.Should().Be(false);
        }

        [TestMethod]
        public void RunAccessorTests_FindRun()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, IsDifferential = true, Description = "test description" });

            run1.Should().NotBeNull();

            var findResult = _runAccessor.FindRunWithImage(run1.Id);

            findResult.Should().NotBeNull();
            findResult.Id.Should().Equals(run1.Id);
        }

        [TestMethod]
        public void RunAccessorTests_RenameRun()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, IsDifferential = true, Description = "test description" });

            var newName = "new name";
            _runAccessor.RenameRun(run1.Id, newName);

            var findResult = _runAccessor.FindRunWithImage(run1.Id);

            findResult.Name.Should().Be(newName);
        }

        [TestMethod]
        public void RunAccessorTests_ChangeRunDescription()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, IsDifferential = true, Description = "test description" });

            var newDescription = "new description";
            _runAccessor.ChangeRunDescription(run1.Id, newDescription);

            var findResult = _runAccessor.FindRunWithImage(run1.Id);

            findResult.Description.Should().Be(newDescription);
        }

        [TestMethod]
        public void RunAccessorTests_UpdateRunStatus()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, IsDifferential = true, Description = "test description" });

            var newStatus = RunStatus.Queued;
            _runAccessor.UpdateRunStatus(run1.Id, newStatus, null);

            var findResult = _runAccessor.FindRunWithImage(run1.Id);

            findResult.Status.Should().Be(newStatus);
        }

        [TestMethod]
        public void RunAccessorTests_UpdateRunStatus_HasDryCellsUpdatesProcessEndTime()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, IsDifferential = true, Description = "test description" });

            var newStatus = RunStatus.HasDryCells;
            _runAccessor.UpdateRunStatus(run1.Id, newStatus, null);

            var findResult = _runAccessor.FindRunWithImage(run1.Id);

            findResult.Status.Should().Be(newStatus);
            findResult.ProcessingEndDate.Should().NotBeNull();
        }

        [TestMethod]
        public void RunAccessorTests_FindRuns()
        {
            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test1",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, IsDifferential = true, Description = "test1 description" });
            var run2 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test2",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, IsDifferential = true, Description = "test2 description" });
            var run3 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test3", FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, IsDifferential = true, Description = "test3 description" });

            var filter = new RunFilter();

            var findResult = _runAccessor.FindRuns(filter, 0, 20);
            findResult.Should().NotBeNull();
            findResult.Count.Should().Be(3);
        }

        [TestMethod]
        public void RunAccessorTests_GetRunStatus_Success()
        {
            var run = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test1",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, IsDifferential = true, Description = "test description" });

            var result = _runAccessor.GetRunStatus(run.Id);
            result.Should().NotBeNull();
            result.Value.Should().Be(RunStatus.Created);
        }

        [TestMethod]
        public void RunAccessorTests_GetRunStatus_Failure()
        {
            var run = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test1",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, IsDifferential = true, Description = "test description" });

            var result = _runAccessor.GetRunStatus(run.Id + 1);
            result.Should().BeNull();
        }

        [TestMethod]
        public void RunAccessorTests_GetRunBuckets_NoBuckets()
        {
            var result = _runAccessor.GetRunBuckets();
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public void RunAccessorTests_GetRunBuckets()
        {
            _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                Name = "TestBucket 1",
                CreatedDate = DateTime.Now,
                Runs = new List<Run>() { }
            });
             
            _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                Name = "TestBucket 2",
                CreatedDate = DateTime.Now,
                Runs = new List<Run>() { }
            });

            var result1 = _runAccessor.GetRunBuckets();
            result1.Count.Should().Be(2);
            result1[0].Name.Should().Be("TestBucket 1");
            result1[0].Runs.Count.Should().Be(0);
            result1[1].Name.Should().Be("TestBucket 2");
            result1[1].Runs.Count.Should().Be(0);
        }
        
        [TestMethod]
        public void RunAccessorTests_GetRunBuckets_Runs()
        {
            var actionBucket1 = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                Name = "TestBucket 1",
                CreatedDate = DateTime.Now,
                Runs = new List<Run>() { }
            });
            
            var actionBucket2 = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                Name = "TestBucket 1",
                CreatedDate = DateTime.Now,
                Runs = new List<Run>() { }
            });

            var result1 = _runAccessor.GetRunBuckets();
            result1.Count.Should().Be(2);
            result1[0].Runs.Count.Should().Be(0);

            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test1",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, Description = "test1 description" });
            var run2 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test1",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, Description = "test1 description" });

            _runAccessor.AddRunToRunBucket(run1.Id, actionBucket1.Id);
            _runAccessor.AddRunToRunBucket(run2.Id, actionBucket1.Id);
            _runAccessor.AddRunToRunBucket(run2.Id, actionBucket2.Id);

            var result2 = _runAccessor.GetRunBuckets();
            result2.Count.Should().Be(2);
            result2[0].Runs.Count.Should().Be(2);
        }

        [TestMethod]
        public void RunAccessorTests_FindRunBucket_Empty()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                Name = "TestBucket 1",
                CreatedDate = DateTime.Now,
                Runs = new List<Run>() { }
            });

            var result = _runAccessor.FindRunBucket(bucket.Id);
            result.Name.Should().Be("TestBucket 1");
            result.Runs.Count.Should().Be(0);
        }

        [TestMethod]
        public void RunAccessorTests_FindRunBucket_Runs()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                Name = "TestBucket 1",
                CreatedDate = DateTime.Now,
                Runs = new List<Run>() { }
            });

            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test1",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, Description = "test1 description" });
            var run2 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test1",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, Description = "test1 description" });

            _runAccessor.AddRunToRunBucket(run1.Id, bucket.Id);
            _runAccessor.AddRunToRunBucket(run2.Id, bucket.Id);

            var result = _runAccessor.FindRunBucket(bucket.Id);
            result.Name.Should().Be("TestBucket 1");
            result.Runs.Count.Should().Be(2);
        }

        [TestMethod]
        public void RunAccessorTests_RenameRunBucket()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                Name = "TestBucket 1",
                CreatedDate = DateTime.Now,
                Runs = new List<Run>() { }
            });

            _runAccessor.RenameRunBucket(bucket.Id, "TestBucket New Name");

            var result = _runAccessor.FindRunBucket(bucket.Id);
            result.Name.Should().Be("TestBucket New Name");
        }

        [TestMethod]
        public void RunAccessorTests_ChangeRunBucketDescription()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                Name = "TestBucket 1",
                CreatedDate = DateTime.Now,
                Runs = new List<Run>() { },
                Description = "Test Description"
            });

            _runAccessor.ChangeRunBucketDescription(bucket.Id, "TestBucket New Description");

            var result = _runAccessor.FindRunBucket(bucket.Id);
            result.Description.Should().Be("TestBucket New Description");
        }
        
        [TestMethod]
        public void RunAccessorTests_DeleteRunBucket()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                Name = "TestBucket 1",
                CreatedDate = DateTime.Now,
                Runs = new List<Run>() { }
            });

            _runAccessor.DeleteRunBucket(bucket.Id);

            var result = _runAccessor.FindRunBucket(bucket.Id);
            result.Should().Be(null);
        }
        
        [TestMethod]
        public void RunAccessorTests_DuplicateRunBucket()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                Name = "TestBucket 1",
                CreatedDate = DateTime.Now,
                Runs = new List<Run>() { }
            });

            _runAccessor.DuplicateRunBucket(bucket.Id);

            var result = _runAccessor.GetRunBuckets();
            result.Count.Should().Be(2);
        }

        [TestMethod]
        public void RunAccessorTests_AddRunToRunBucket()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                Name = "TestBucket 1",
                CreatedDate = DateTime.Now,
                Runs = new List<Run>() { }
            });

            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test1",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, Description = "test1 description" });
            var run2 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test1",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, Description = "test1 description" });

            _runAccessor.AddRunToRunBucket(run1.Id, bucket.Id);
            _runAccessor.AddRunToRunBucket(run2.Id, bucket.Id);

            var result = _runAccessor.FindRunBucket(bucket.Id);
            result.Name.Should().Be("TestBucket 1");
            result.Runs.Count.Should().Be(2);
            foreach(var run in result.Runs)
            {
                run.Name.Should().Be("test1");
            }
        }

        [TestMethod]
        public void RunAccessorTests_RemoveRunFromRunBucket()
        {
            var bucket = _runAccessor.CreateOrUpdateRunBucket(new RunBucket()
            {
                Name = "TestBucket 1",
                CreatedDate = DateTime.Now,
                Runs = new List<Run>() { }
            });

            var run1 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test1",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, Description = "test description" });
            var run2 = _runAccessor.CreateOrUpdateRun(new Run() { CreatedDate = DateTime.UtcNow, Name = "test1",  FileStorageLocator = Guid.NewGuid().ToString(), ImageId = 1, ModelId = 1, ScenarioId = 1, Status = RunStatus.Created, Description = "test description" });

            _runAccessor.AddRunToRunBucket(run1.Id, bucket.Id);
            _runAccessor.AddRunToRunBucket(run2.Id, bucket.Id);

            var result1 = _runAccessor.FindRunBucket(bucket.Id);
            result1.Name.Should().Be("TestBucket 1");
            result1.Runs.Count.Should().Be(2);
            foreach (var run in result1.Runs)
            {
                run.Name.Should().Be("test1");
            }

            _runAccessor.RemoveRunFromRunBucket(run1.Id, bucket.Id);

            var result2 = _runAccessor.FindRunBucket(bucket.Id);
            result2.Runs.Count.Should().Be(1);
            foreach (var run in result2.Runs)
            {
                run.Id.Should().NotBe(run1.Id);
            }
        }
    }
}
