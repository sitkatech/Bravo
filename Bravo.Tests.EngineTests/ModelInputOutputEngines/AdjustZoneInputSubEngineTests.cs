﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bravo.Accessors.FileIO;
using Bravo.Common.DataContracts.Models;
using Telerik.JustMock;
using Bravo.Engines.ModelInputOutputEngines;
using Telerik.JustMock.Helpers;
using Newtonsoft.Json;
using Bravo.Common.DataContracts.Runs;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Bravo.Tests.EngineTests.ModelInputOutputEngines
{
    [TestClass]
    public class AdjustZoneInputSubEngineTests
    {
        private readonly IModelFileAccessor _modflowFileAccessorMock = Mock.Create<IModelFileAccessor>(Behavior.Strict);
        private readonly IBlobFileAccessor _fileAccessorMock = Mock.Create<IBlobFileAccessor>(Behavior.Strict);

        private readonly Model _model = new Model
        {
            StartDateTime = new DateTime(2011, 1, 1)
        };

        private Run _run = new Run {
            FileStorageLocator = "fakeLocator",
            Scenario = new Scenario
            {
                ShouldSwitchSign = false
            }
        };

        private AdjustZoneInputSubEngine CreateCanalCsvInputSubEngine()
        {
            var modflowFileAccessorFactory = Mock.Create<IModelFileAccessorFactory>();
            modflowFileAccessorFactory.Arrange(a => a.CreateModflowFileAccessor(Arg.IsAny<Model>()))
                .Returns(_modflowFileAccessorMock);

            var sut = new AdjustZoneInputSubEngine(_model);
            return sut;
        }

        [TestMethod]
        public void UpdateFlowInputs_IsInOneZone()
        {
            ConfigurationManager.AppSettings["BlobStorageModelDataFolder"] = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate
                            {
                                Location = "1",
                                Rate = 2.22
                            }
                        }
                    }
                }
            };

            _modflowFileAccessorMock.Arrange(a => a.GetInputLocationZones("1"))
                .Returns(new List<string> { "abc" });

            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/mapzoneinputs.json", "fakeModelDataFolder"))
                .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunZoneInput>
                {
                    new RunZoneInput{ Adjustment = .5, ZoneNumber = "abc" }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(1.11, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate, .001);
        }

        [TestMethod]
        public void UpdateFlowInputs_IsInNoZones()
        {
            ConfigurationManager.AppSettings["BlobStorageModelDataFolder"] = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate
                            {
                                Location = "1",
                                Rate = 2.22
                            }
                        }
                    }
                }
            };

            _modflowFileAccessorMock.Arrange(a => a.GetInputLocationZones("1"))
                .Returns(new List<string>());

            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/mapzoneinputs.json", "fakeModelDataFolder"))
                .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunZoneInput>
                {
                    new RunZoneInput{ Adjustment = .5, ZoneNumber = "abc" }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(2.22, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate, .001);
        }

        [TestMethod]
        public void UpdateFlowInputs_IsInMultipleZones()
        {
            ConfigurationManager.AppSettings["BlobStorageModelDataFolder"] = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate
                            {
                                Location = "1",
                                Rate = 2.22
                            }
                        }
                    }
                }
            };

            _modflowFileAccessorMock.Arrange(a => a.GetInputLocationZones("1"))
                .Returns(new List<string> { "abc", "def" });

            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/mapzoneinputs.json", "fakeModelDataFolder"))
                .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunZoneInput>
                {
                    new RunZoneInput{ Adjustment = .5, ZoneNumber = "abc" },
                    new RunZoneInput{ Adjustment = 1.5, ZoneNumber = "def" }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(1.665, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate, .001);
        }

        [TestMethod]
        public void UpdateFlowInputs_MultipleZonesNotAllAdjusted()
        {
            ConfigurationManager.AppSettings["BlobStorageModelDataFolder"] = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate
                            {
                                Location = "1",
                                Rate = 2.22
                            }
                        }
                    }
                }
            };

            _modflowFileAccessorMock.Arrange(a => a.GetInputLocationZones("1"))
                .Returns(new List<string> { "abc", "def" });

            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/mapzoneinputs.json", "fakeModelDataFolder"))
                .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunZoneInput>
                {
                    new RunZoneInput{ Adjustment = .5, ZoneNumber = "abc" }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(1.11, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate, .001);
        }

        [TestMethod]
        public void UpdateFlowInputs_MultipleZonesNoneAdjusted()
        {
            ConfigurationManager.AppSettings["BlobStorageModelDataFolder"] = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate
                            {
                                Location = "1",
                                Rate = 2.22
                            }
                        }
                    }
                }
            };

            _modflowFileAccessorMock.Arrange(a => a.GetInputLocationZones("1"))
                .Returns(new List<string> { "abc", "def" });

            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/mapzoneinputs.json", "fakeModelDataFolder"))
                .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunZoneInput>
                {
                    new RunZoneInput{ Adjustment = .5, ZoneNumber = "xyz" }
                })));
            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(2.22, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate, .001);
        }

        [TestMethod]
        public void UpdateFlowInputs_IsInOneZone_ShouldSwitchSign()
        {
            ConfigurationManager.AppSettings["BlobStorageModelDataFolder"] = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate
                            {
                                Location = "1",
                                Rate = 2.22
                            }
                        }
                    }
                }
            };

            _modflowFileAccessorMock.Arrange(a => a.GetInputLocationZones("1"))
                .Returns(new List<string> { "abc" });

            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/mapzoneinputs.json", "fakeModelDataFolder"))
                .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunZoneInput>
                {
                    new RunZoneInput{ Adjustment = .5, ZoneNumber = "abc" }
                })));

            _run.Scenario.ShouldSwitchSign = true;

            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(3.33, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate, .001);
        }

        [TestMethod]
        public void UpdateFlowInputs_IsInOneZone_ShouldSwitchSign_Smaller()
        {
            ConfigurationManager.AppSettings["BlobStorageModelDataFolder"] = "fakeModelDataFolder";
            var existingFlows = new StressPeriodsLocationRates
            {
                StressPeriods = new List<StressPeriodLocationRates>
                {
                    new StressPeriodLocationRates
                    {
                        LocationRates = new List<LocationRate>
                        {
                            new LocationRate
                            {
                                Location = "1",
                                Rate = 4.44
                            }
                        }
                    }
                }
            };

            _modflowFileAccessorMock.Arrange(a => a.GetInputLocationZones("1"))
                .Returns(new List<string> { "abc" });

            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/mapzoneinputs.json", "fakeModelDataFolder"))
                .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunZoneInput>
                {
                    new RunZoneInput{ Adjustment = 1.75, ZoneNumber = "abc" }
                })));

            _run.Scenario.ShouldSwitchSign = true;

            var sut = CreateCanalCsvInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(1.11, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate, .001);
        }
    }
}
