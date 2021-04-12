using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Bravo.Accessors;
using Bravo.Accessors.FileIO;
using Bravo.Accessors.Runs;
using Bravo.Common.DataContracts.Runs;
using Bravo.Managers.Runs;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Bravo.Accessors.Containers;
using Bravo.Common.DataContracts.Models;
using Bravo.Engines;
using Bravo.Managers;
using Bravo.Engines.ModelInputOutputEngines;
using Bravo.Accessors.APIFunctions;
using Bravo.Accessors.Models;
using Bravo.Common.DataContracts.Files;
using Bravo.Common.Shared.Enums;
using Bravo.Accessors.Queue;

namespace Bravo.Tests.ManagerTests
{
    [TestClass]
    public class RunManagerTests
    {
        private readonly IBlobFileAccessor _blobFileAccessorMock = Mock.Create<IBlobFileAccessor>(Behavior.Strict);
        private readonly IModelAccessor _modelAccessorMock = Mock.Create<IModelAccessor>(Behavior.Strict);
        private readonly IFileAccessor _fileAccessorMock = Mock.Create<IFileAccessor>(Behavior.Strict);
        private readonly IAnalysisEngine _analysisEngineMock = Mock.Create<IAnalysisEngine>(Behavior.Strict);
        private readonly IRunAccessor _runAccessorMock = Mock.Create<IRunAccessor>(Behavior.Strict);
        private readonly IContainerAccessor _containerAccessorMock = Mock.Create<IContainerAccessor>(Behavior.Strict);
        private readonly IAPIFunctionsAccessor _apiFunctionsAccessorMock = Mock.Create<IAPIFunctionsAccessor>(Behavior.Strict);
        private readonly IModelInputOutputEngineFactory _modelInputOutputEngineFactoryMock = Mock.Create<IModelInputOutputEngineFactory>(Behavior.Strict);
        private readonly IModelInputOutputEngine _modelInputOutputEngineMock = Mock.Create<IModelInputOutputEngine>(Behavior.Strict);
        private readonly IQueueAccessor _queueAccessorMock = Mock.Create<IQueueAccessor>(Behavior.Strict);


        [TestMethod]
        public void FindRunResultDetails_Exists()
        {
            const string fileStorageLocator = "FakeFileStorageLocator";
            _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory($"{fileStorageLocator}/outputs", Arg.AnyString))
                .Returns(new List<string>
                {
                    "11-Fake file title.json"
                });
            _blobFileAccessorMock.Arrange(a =>
                    a.GetFile($"{fileStorageLocator}/outputs/11-Fake file title.json", Arg.AnyString))
                .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new RunResultDetails
                {
                    RunResultId = 11,
                    RunResultName = "fake stuff"
                })));

            var sut = CreateRunManager();
            var result = sut.FindRunResultDetails(fileStorageLocator, 11);

            Assert.AreEqual(11, result.RunResultId);
            Assert.AreEqual("fake stuff", result.RunResultName);

            _blobFileAccessorMock.AssertAll();

        }

        [TestMethod]
        public void FindRunResultDetails_MultipleExist()
        {
            const string fileStorageLocator = "FakeFileStorageLocator";
            _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory($"{fileStorageLocator}/outputs", Arg.AnyString))
                .Returns(new List<string>
                {
                    "10-Something else.json",
                    "11-Fake file title.json"
                });
            _blobFileAccessorMock.Arrange(a =>
                    a.GetFile($"{fileStorageLocator}/outputs/11-Fake file title.json", Arg.AnyString))
                .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new RunResultDetails
                {
                    RunResultId = 11,
                    RunResultName = "fake stuff"
                })));

            var sut = CreateRunManager();
            var result = sut.FindRunResultDetails(fileStorageLocator, 11);

            Assert.AreEqual(11, result.RunResultId);
            Assert.AreEqual("fake stuff", result.RunResultName);

            _blobFileAccessorMock.AssertAll();

        }

        [TestMethod]
        public void FindRun_Completed_HasFiles()
        {
            var fakeMapData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new[] { new RunWellInput()
            {
                Values = new List<FeatureWithLocationValue>()
            } }));

            const string fileStorageLocator = "FakeFileStorageLocator";
            _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory($"{fileStorageLocator}/outputs", Arg.AnyString))
                .Returns(new List<string>
                {
                    "10-Something else.json",
                    "11-Fake file title.json"
                });
            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(fakeMapData);

            _runAccessorMock.Arrange(a => a.FindRunWithImage(12))
                .Returns(new Run
                {
                    Status = RunStatus.Complete,
                    FileStorageLocator = fileStorageLocator,
                    Scenario = new Scenario()
                });

            var sut = CreateRunManager();
            var result = sut.FindRun(12);

            Assert.AreEqual(RunStatus.Complete, result.Status);
            Assert.IsNotNull(result.AvailableRunResults);
            Assert.AreEqual(2, result.AvailableRunResults.Count);
            var run10 = result.AvailableRunResults.Single(a => a.RunResultId == 10);
            Assert.AreEqual("Something else", run10.RunResultName);
            var run11 = result.AvailableRunResults.Single(a => a.RunResultId == 11);
            Assert.AreEqual("Fake file title", run11.RunResultName);
            Assert.AreEqual(1, result.WellMapInputs.Count);

            _runAccessorMock.AssertAll();
            _blobFileAccessorMock.AssertAll();

        }

        [TestMethod]
        public void FindRunBucket()
        {
            _runAccessorMock.Arrange(a => a.GetRunBuckets())
                .Returns(new List<RunBucket>
                {
                    new RunBucket()
                    {
                        Id = 1,
                        Name = "Test 1",
                        Runs = new List<Run>()
                        {
                            new Run()
                            {
                                Name = "Run 1"
                            },
                            new Run()
                            {
                                Name = "Run 2"
                            }
                        }
                    },
                    new RunBucket()
                    {
                        Id = 2,
                        Name = "Test 2",
                        Runs = new List<Run>(),
                    }
                });

            var sut = CreateRunManager();
            var result = sut.GetRunBuckets();

            Assert.AreEqual(result.Count(), 2);
            Assert.AreEqual(result[0].Id, 1);
            Assert.AreEqual(result[0].Name, "Test 1");
            Assert.AreEqual(result[0].Runs.Count(), 2);
            Assert.AreEqual(result[0].Runs[0].Name, "Run 1");
            Assert.AreEqual(result[0].Runs[1].Name, "Run 2");

            Assert.AreEqual(result[1].Id, 2);
            Assert.AreEqual(result[1].Name, "Test 2");
            Assert.IsNotNull(result[1].Runs);
            Assert.AreEqual(result[1].Runs.Count(), 0);

            _runAccessorMock.AssertAll();
        }

        [TestMethod]
        public void Duplicate()
        {
            var existingRun = new Run()
            {
                Id = 1,
                Name = "og",
                FileStorageLocator = Guid.NewGuid().ToString(),
                InputFileName = "test.csv",
                ModelId = 2
            };

            _runAccessorMock.Arrange(a => a.FindRunWithImage(Arg.AnyInt)).Returns(existingRun);
            _runAccessorMock.Arrange(a => a.CreateOrUpdateRun(Arg.IsAny<Run>())).Returns(existingRun);

            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(Encoding.UTF8.GetBytes("test"));
            _blobFileAccessorMock.Arrange(a => a.SaveFile(Arg.AnyString, Arg.AnyString, Arg.IsAny<byte[]>(), null));

            _modelAccessorMock.Arrange(a => a.FindAllModels())
                .Returns(new[] { new Model() { Id = 2 } });

            var mgr = CreateRunManager();
            var result = mgr.DuplicateRun(1);

            _blobFileAccessorMock.Assert(a => a.GetFile(Arg.AnyString, Arg.AnyString), Occurs.Exactly(4));
            _runAccessorMock.AssertAll();
            _blobFileAccessorMock.AssertAll();
            _modelAccessorMock.AssertAll();
        }

        [TestMethod]
        [Timeout(5000)]
        public void GenerateInputFiles_Successful()
        {
            const int runId = 1234;
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    }
                });
            _modelInputOutputEngineMock.Arrange(a => a.GenerateInputFiles(Arg.Matches<Run>(b => b.Id == runId)));
            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(default(byte[]));
            _apiFunctionsAccessorMock.Arrange(a => a.MakeFunctionCall(Arg.AnyString));
            _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());

            var sut = CreateRunManager();
            sut.GenerateInputFiles(runId);

            _modelInputOutputEngineMock.Assert(a => a.GenerateInputFiles(Arg.Matches<Run>(b => b.Id == runId)), Occurs.Once());
            _apiFunctionsAccessorMock.Assert(a => a.MakeFunctionCall(Arg.AnyString), Occurs.Once());
            _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Exactly(2));
        }

        [TestMethod]
        [Timeout(5000)]
        public void GenerateInputFiles_InputDataError()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    }
                });
            var exception = new InputDataInvalidException("Fake message");
            _modelInputOutputEngineMock.Arrange(a => a.GenerateInputFiles(Arg.Matches<Run>(b => b.Id == runId)))
                .Throws(exception);
            _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, RunStatus.InvalidInput, Arg.AnyString))
                .Returns(true);
            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(default(byte[]));
            _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());

            var sut = CreateRunManager();
            sut.GenerateInputFiles(runId);

            _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, RunStatus.InvalidInput, null), Occurs.Once());
            _modelInputOutputEngineMock.Assert(a => a.GenerateInputFiles(Arg.Matches<Run>(b => b.Id == runId)), Occurs.Once());
            _apiFunctionsAccessorMock.Assert(a => a.MakeFunctionCall(Arg.AnyString), Occurs.Never());
            _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Once());
        }

        [TestMethod]
        [Timeout(5000)]
        public void GenerateInputFiles_InputNonDataError()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    }
                });
            var exception = new Exception("Fake message");
            _modelInputOutputEngineMock.Arrange(a => a.GenerateInputFiles(Arg.Matches<Run>(b => b.Id == runId)))
                .Throws(exception);
            _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, RunStatus.SystemError, Arg.AnyString))
                .Returns(true);
            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(default(byte[]));
            _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());

            var sut = CreateRunManager();
            sut.GenerateInputFiles(runId);

            _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, RunStatus.SystemError, null), Occurs.Once());
            _modelInputOutputEngineMock.Assert(a => a.GenerateInputFiles(Arg.Matches<Run>(b => b.Id == runId)), Occurs.Once());
            _apiFunctionsAccessorMock.Assert(a => a.MakeFunctionCall(Arg.AnyString), Occurs.Never());
            _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Once());
        }

        [TestMethod]
        [Timeout(5000)]
        public void RunAnalysis_Classic_Successful()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Scenario = new Scenario()
                });
            _analysisEngineMock.Arrange(a => a.RunAnalysis(Arg.IsAny<Run>()))
               .Returns(new AnalysisResult() { Success = true });
            _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, RunStatus.AnalysisSuccess, Arg.AnyString))
             .Returns(true);
            _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());
            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(default(byte[]));
            _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString)).Returns(new List<string>());

            var sut = CreateRunManager();
            sut.RunAnalysis(runId);

            _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, RunStatus.AnalysisSuccess, null), Occurs.Once());
            _analysisEngineMock.Assert(a => a.RunAnalysis(Arg.IsAny<Run>()), Occurs.Once());
            _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Never());
            _blobFileAccessorMock.Assert(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }

        [TestMethod]
        [Timeout(5000)]
        public void RunAnalysis_Classic_Failure()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Scenario = new Scenario()
                });
            _analysisEngineMock.Arrange(a => a.RunAnalysis(Arg.IsAny<Run>()))
               .Returns(new AnalysisResult() { Success = false });
            _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, RunStatus.AnalysisFailed, Arg.AnyString))
             .Returns(true);
            _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());
            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(default(byte[]));
            _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString)).Returns(new List<string>());

            var sut = CreateRunManager();
            sut.RunAnalysis(runId);

            _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, RunStatus.AnalysisFailed, null), Occurs.Once());
            _analysisEngineMock.Assert(a => a.RunAnalysis(Arg.IsAny<Run>()), Occurs.Once());
            _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Never());
            _blobFileAccessorMock.Assert(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString), Occurs.Once());
        }

        [TestMethod]
        [Timeout(5000)]
        public void RunAnalysis_Custom_Successful()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Scenario = new Scenario
                    {
                        InputImage = new Image { CpuCoreCount = 1, IsLinux = true, Memory = 4, Name = "some name" }
                    }
                });
            _analysisEngineMock.Arrange(a => a.RunAnalysis(Arg.IsAny<Run>()))
               .Returns(new AnalysisResult() { Success = true });
            _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, RunStatus.AnalysisSuccess, Arg.AnyString))
             .Returns(true);
            _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());
            _blobFileAccessorMock.Arrange(a => a.GetFilesInShareDirectory(Arg.AnyString)).Returns(new List<string>());
            _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));

            var sut = CreateRunManager();
            sut.RunAnalysis(runId);

            _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, RunStatus.AnalysisSuccess, null), Occurs.Once());
            _analysisEngineMock.Assert(a => a.RunAnalysis(Arg.IsAny<Run>()), Occurs.Once());
            _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.GetFilesInShareDirectory(Arg.AnyString), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        }

        [TestMethod]
        [Timeout(5000)]
        public void RunAnalysis_Custom_Failure()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Scenario = new Scenario
                    {
                        InputImage = new Image { CpuCoreCount = 1, IsLinux = true, Memory = 4, Name = "some name" }
                    }
                });
            _analysisEngineMock.Arrange(a => a.RunAnalysis(Arg.IsAny<Run>()))
               .Returns(new AnalysisResult() { Success = false });
            _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, RunStatus.AnalysisFailed, Arg.AnyString))
             .Returns(true);
            _fileAccessorMock.Arrange(a => a.GetFilesInModflowDataFolder()).Returns(new List<FileModel>());
            _blobFileAccessorMock.Arrange(a => a.GetFilesInShareDirectory(Arg.AnyString)).Returns(new List<string>());
            _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));

            var sut = CreateRunManager();
            sut.RunAnalysis(runId);

            _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, RunStatus.AnalysisFailed, null), Occurs.Once());
            _analysisEngineMock.Assert(a => a.RunAnalysis(Arg.IsAny<Run>()), Occurs.Once());
            _fileAccessorMock.Assert(a => a.GetFilesInModflowDataFolder(), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.GetFilesInShareDirectory(Arg.AnyString), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        }

        [TestMethod]
        [Timeout(5000)]
        public void GenerateOutputFiles_AnalysisSuccess_Successful()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Status = RunStatus.AnalysisSuccess
                });
            _modelInputOutputEngineMock.Arrange(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.Id == runId)));
            _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, RunStatus.Complete, Arg.AnyString))
                .Returns(true);
            _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));
            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns((byte[])null);

            var sut = CreateRunManager();
            sut.GenerateOutputFiles(runId);

            _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, RunStatus.Complete, null), Occurs.Once());
            _modelInputOutputEngineMock.Assert(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.Id == runId)), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        }

        [TestMethod]
        [Timeout(5000)]
        public void GenerateOutputFiles_AnalysisFailure_Successful()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Status = RunStatus.AnalysisFailed
                });
            var exception = new Exception("Modflow failed to run successfully.");
            _modelInputOutputEngineMock.Arrange(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.Id == runId)));
            _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, RunStatus.SystemError, Arg.AnyString))
                .Returns(true);
            _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));

            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns((byte[])null);

            var sut = CreateRunManager();
            sut.GenerateOutputFiles(runId);

            _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, RunStatus.SystemError, null), Occurs.Once());
            _modelInputOutputEngineMock.Assert(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.Id == runId)), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        }

        [TestMethod]
        [Timeout(5000)]
        public void GenerateOutputFiles_OutputDataError()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Status = RunStatus.AnalysisSuccess
                });
            var exception = new OutputDataInvalidException("Fake message");
            _modelInputOutputEngineMock.Arrange(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.Id == runId)))
              .Throws(exception);
            _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, RunStatus.InvalidOutput, Arg.AnyString))
                .Returns(true);
            _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));
            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns((byte[])null);

            var sut = CreateRunManager();
            sut.GenerateOutputFiles(runId);

            _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, RunStatus.InvalidOutput, null), Occurs.Once());
            _modelInputOutputEngineMock.Assert(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.Id == runId)), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        }

        [TestMethod]
        [Timeout(5000)]
        public void GenerateOutputFiles_OutputNonDataError()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Status = RunStatus.AnalysisSuccess
                });
            var exception = new Exception("Fake message");
            _modelInputOutputEngineMock.Arrange(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.Id == runId)))
              .Throws(exception);
            _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, RunStatus.SystemError, Arg.AnyString))
                .Returns(true);
            _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));
            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns((byte[])null);

            var sut = CreateRunManager();
            sut.GenerateOutputFiles(runId);

            _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, RunStatus.SystemError, null), Occurs.Once());
            _modelInputOutputEngineMock.Assert(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.Id == runId)), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        }

        [TestMethod]
        [Timeout(5000)]
        public void GenerateOutputFiles_HasDryCells()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Status = RunStatus.AnalysisSuccess
                });

            var exception = new OutputDataInvalidException("Fake message", RunStatus.HasDryCells);
            _modelInputOutputEngineMock.Arrange(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.Id == runId)))
              .Throws(exception);
            _runAccessorMock.Arrange(a => a.UpdateRunStatus(runId, RunStatus.HasDryCells, Arg.AnyString))
               .Returns(true);
            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns(default(byte[]));
            _blobFileAccessorMock.Arrange(a => a.DeleteCloudFileShare(Arg.AnyString));
            _blobFileAccessorMock.Arrange(a => a.GetFile(Arg.AnyString, Arg.AnyString)).Returns((byte[])null);

            var sut = CreateRunManager();
            sut.GenerateOutputFiles(runId);

            _runAccessorMock.Assert(a => a.UpdateRunStatus(runId, RunStatus.HasDryCells, null), Occurs.Once());
            _modelInputOutputEngineMock.Assert(a => a.GenerateOutputFiles(Arg.Matches<Run>(b => b.Id == runId)), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.DeleteCloudFileShare(Arg.AnyString), Occurs.Once());
        }

        [TestMethod]
        public async Task StartContainer_Default_Success()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Image = new Image
                    {
                        Name = "someimagename",
                        Server = null,
                        CpuCoreCount = 1,
                        Memory = 4.0m,
                        IsLinux = false
                    },
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Scenario = new Scenario()
                });
            _containerAccessorMock.Arrange(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool));
            _runAccessorMock.Arrange(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.Id == runId))).Returns(default(Run));
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(true));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Input);

            _runAccessorMock.Assert(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.Id == runId)), Occurs.Once());
            _containerAccessorMock.Assert(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool), Occurs.Once());
        }

        [TestMethod]
        public async Task StartContainer_Default_CannotStart_Input()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Image = new Image
                    {
                        Name = "someimagename",
                        Server = null,
                        CpuCoreCount = 1,
                        Memory = 4.0m,
                        IsLinux = false
                    },
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Scenario = new Scenario()
                });
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(false));
            _queueAccessorMock.Arrange(a => a.CreateGenerateInputsMessage(runId, TimeSpan.FromMinutes(5)));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Input);

            _queueAccessorMock.Assert(a => a.CreateGenerateInputsMessage(runId, TimeSpan.FromMinutes(5)), Occurs.Once());
        }

        [TestMethod]
        public async Task StartContainer_Default_CannotStart_Analysis()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Image = new Image
                    {
                        Name = "someimagename",
                        Server = null,
                        CpuCoreCount = 1,
                        Memory = 4.0m,
                        IsLinux = false
                    },
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Scenario = new Scenario()
                });
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(false));
            _queueAccessorMock.Arrange(a => a.CreateRunAnalysisMessage(runId, TimeSpan.FromMinutes(5)));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Analysis);

            _queueAccessorMock.Assert(a => a.CreateRunAnalysisMessage(runId, TimeSpan.FromMinutes(5)), Occurs.Once());
        }

        [TestMethod]
        public async Task StartContainer_Default_CannotStart_Output()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Image = new Image
                    {
                        Name = "someimagename",
                        Server = null,
                        CpuCoreCount = 1,
                        Memory = 4.0m,
                        IsLinux = false
                    },
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Scenario = new Scenario()
                });
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(false));
            _queueAccessorMock.Arrange(a => a.CreateGenerateOutputsMessage(runId, TimeSpan.FromMinutes(5)));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Output);

            _queueAccessorMock.Assert(a => a.CreateGenerateOutputsMessage(runId, TimeSpan.FromMinutes(5)), Occurs.Once());
        }

        [TestMethod]
        public async Task StartContainer_Default_Failure()
        {
            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Image = new Image
                    {
                        Name = "someimagename",
                        Server = "someserver"
                    },
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Scenario = new Scenario()
                });
            var exception = new Exception("Unable to start container.");
            _containerAccessorMock.Arrange(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool))
                .Throws(exception);
            _runAccessorMock.Arrange(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.Id == runId))).Returns(default(Run));
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(true));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Input);

            _runAccessorMock.Assert(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.Id == runId)), Occurs.Once());
            _containerAccessorMock.Assert(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), false), Occurs.Once());
        }

        [TestMethod]
        public async Task StartContainer_Custom_Success()
        {
            ConfigurationManager.AppSettings["AzureContainerCpuCoreCount"] = "1";
            ConfigurationManager.AppSettings["AzureContainerMemory"] = "2";

            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Image = new Image
                    {
                        Name = "someimagename",
                        Server = "someserver"
                    },
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Scenario = new Scenario
                    {
                        InputImage = new Image { CpuCoreCount = 1, IsLinux = true, Memory = 4, Name = "some name" }
                    }
                });
            var inputFiles = new List<string> { "somefile.csv", "anotherfile.csv" };
            _blobFileAccessorMock.Arrange(a => a.CreateFileShare(Arg.AnyString));
            _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString)).Returns(inputFiles);
            _blobFileAccessorMock.Arrange(a => a.CopyFromBlobStorageToFileShare(Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyBool));
            _containerAccessorMock.Arrange(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool));
            _runAccessorMock.Arrange(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.Id == runId))).Returns(default(Run));
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(true));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Input);

            _blobFileAccessorMock.Assert(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.CopyFromBlobStorageToFileShare(Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyBool), Occurs.Exactly(inputFiles.Count()));
            _runAccessorMock.Assert(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.Id == runId)), Occurs.Once());
            _containerAccessorMock.Assert(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.CreateFileShare(Arg.AnyString), Occurs.Once());
        }

        [TestMethod]
        public async Task StartContainer_Custom_Failure()
        {
            ConfigurationManager.AppSettings["AzureContainerCpuCoreCount"] = "1";
            ConfigurationManager.AppSettings["AzureContainerMemory"] = "2";

            const int runId = 1234;
            
            _runAccessorMock.Arrange(a => a.FindRunWithImage(runId))
                .Returns(new Run
                {
                    Id = runId,
                    
                    Image = new Image
                    {
                        Name = "someimagename",
                        Server = "someserver"
                    },
                    Model = new Model
                    {
                        ModflowExeName = "fake.exe",
                        NamFileName = "fake.nam"
                    },
                    Scenario = new Scenario
                    {
                        InputImage = new Image { CpuCoreCount = 1, IsLinux = true, Memory = 4, Name = "some name" }
                    }
                });
            var inputFiles = new List<string> { "somefile.csv" };
            var exception = new Exception("azure container error");
            _blobFileAccessorMock.Arrange(a => a.CreateFileShare(Arg.AnyString));
            _blobFileAccessorMock.Arrange(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString)).Returns(inputFiles);
            _blobFileAccessorMock.Arrange(a => a.CopyFromBlobStorageToFileShare(Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyBool));
           _containerAccessorMock.Arrange(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool)).Throws(exception);
            _runAccessorMock.Arrange(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.Id == runId))).Returns(default(Run));
            _containerAccessorMock.Arrange(a => a.CanQueueNewContainer()).Returns(Task.FromResult(true));

            var sut = CreateRunManager();
            await sut.StartContainer(runId, AgentProcessType.Input);

            _runAccessorMock.Assert(a => a.CreateOrUpdateRun(Arg.Matches<Run>(b => b.Id == runId)), Occurs.Once());
            _containerAccessorMock.Assert(a => a.StartAzureContainer(Arg.AnyString, Arg.AnyString, Arg.AnyDouble, Arg.AnyDouble, Arg.Matches<Dictionary<string, string>>(x => x.Count > 0), Arg.IsAny<AgentProcessType>(), Arg.AnyBool), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.CreateFileShare(Arg.AnyString), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.GetFilesInDirectory(Arg.AnyString, Arg.AnyString), Occurs.Once());
            _blobFileAccessorMock.Assert(a => a.CopyFromBlobStorageToFileShare(Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyString, Arg.AnyBool), Occurs.Exactly(inputFiles.Count()));
        }


        #region Private Methods
        private RunManager CreateRunManager()
        {
            _modelInputOutputEngineFactoryMock.Arrange(a => a.CreateModelInputOutputEngine(Arg.IsAny<Run>()))
                .Returns(() => _modelInputOutputEngineMock);
            var sut = new RunManager();
            sut.AccessorFactory = new AccessorFactory();
            sut.AccessorFactory.AddOverride(_blobFileAccessorMock);
            sut.AccessorFactory.AddOverride(_modelAccessorMock);
            sut.AccessorFactory.AddOverride(_fileAccessorMock);
            sut.AccessorFactory.AddOverride(_runAccessorMock);
            sut.AccessorFactory.AddOverride(_containerAccessorMock);
            sut.AccessorFactory.AddOverride(_apiFunctionsAccessorMock);
            sut.AccessorFactory.AddOverride(_queueAccessorMock);
            sut.EngineFactory = new EngineFactory();
            sut.EngineFactory.AddOverride(_modelInputOutputEngineFactoryMock);
            sut.EngineFactory.AddOverride(_analysisEngineMock);
            return sut;
        }
        #endregion
    }
}