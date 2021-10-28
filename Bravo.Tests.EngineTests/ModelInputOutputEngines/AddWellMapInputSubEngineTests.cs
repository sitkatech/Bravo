﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Bravo.Accessors.FileIO;
using Bravo.Common.DataContracts.Models;
using Bravo.Common.DataContracts.Runs;
using Bravo.Common.Shared.Enums;
using Bravo.Engines.ModelInputOutputEngines;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Bravo.Tests.EngineTests.ModelInputOutputEngines
{
    [TestClass]
    public class AddWellMapInputSubEngineTests
    {
        private readonly IModelFileAccessor _modflowFileAccessorMock = Mock.Create<IModelFileAccessor>(Behavior.Strict);
        private readonly IBlobFileAccessor _fileAccessorMock = Mock.Create<IBlobFileAccessor>(Behavior.Strict);

        private readonly Model _model = new Model
        {
            StartDateTime = new DateTime(2011, 1, 1),
            NumberOfStressPeriods = 1
        };

        private Run _run = new Run
        {
            FileStorageLocator = "fakeLocator",
            Scenario = new Scenario
            {
                ShouldSwitchSign = false
            },
            InputVolumeType = VolumeType.Gallon,
            OutputVolumeType = VolumeType.AcreFeet
        };

        private AddWellMapInputSubEngine CreateAddWellMapInputSubEngine()
        {
            var modflowFileAccessorFactory = Mock.Create<IModelFileAccessorFactory>();
            modflowFileAccessorFactory.Arrange(a => a.CreateModflowFileAccessor(Arg.IsAny<Model>()))
                .Returns(_modflowFileAccessorMock);

            _modflowFileAccessorMock.Arrange(a => a.GetAsrDataNameMap())
                .Returns(new List<AsrDataMap>());

            var sut = new AddWellMapInputSubEngine(_model);
            return sut;
        }

        [TestMethod]
        public void UpdateFlowInputs_OneLocation()
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
                                Location = "2",
                                Rate = 2.22
                            }
                        }
                    }
                }
            };

            _modflowFileAccessorMock.Arrange(a => a.FindWellLocations(40, -100))
                .Returns(new List<LocationPumpingProportion>
                {
                    new LocationPumpingProportion{ Location = "1", Proportion = 1}
                });

            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/mapinputs.json", "fakeModelDataFolder"))
                .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunWellInput>
                {
                    new RunWellInput
                    {
                        Year = 2011,
                        Month = 1,
                        Values = new List<FeatureWithLocationValue>
                        {
                            new FeatureWithLocationValue
                            {
                                Lat = 40,
                                Lng = -100,
                                Value = 10
                            }
                        }
                    }
                })));
            var sut = CreateAddWellMapInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(-1925.0, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate, .1);
            Assert.AreEqual("2", updateNodeRatesResult.StressPeriods[0].LocationRates[1].Location);
            Assert.AreEqual(2.22, updateNodeRatesResult.StressPeriods[0].LocationRates[1].Rate, .001);
        }

        [TestMethod]
        public void UpdateFlowInputs_OneLocation_ShouldSwitchSign()
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
                                Location = "2",
                                Rate = 2.22
                            }
                        }
                    }
                }
            };

            _modflowFileAccessorMock.Arrange(a => a.FindWellLocations(40, -100))
                .Returns(new List<LocationPumpingProportion>
                {
                    new LocationPumpingProportion{ Location = "1", Proportion = 1}
                });

            _fileAccessorMock.Arrange(a => a.GetFile("fakeLocator/inputs/mapinputs.json", "fakeModelDataFolder"))
                .Returns(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new List<RunWellInput>
                {
                    new RunWellInput
                    {
                        Year = 2011,
                        Month = 1,
                        Values = new List<FeatureWithLocationValue>
                        {
                            new FeatureWithLocationValue
                            {
                                Lat = 40,
                                Lng = -100,
                                Value = 10
                            }
                        }
                    }
                })));

            _run.Scenario.ShouldSwitchSign = true;

            var sut = CreateAddWellMapInputSubEngine();
            var updateNodeRatesResult = sut.UpdateFlowInputs(_modflowFileAccessorMock, _fileAccessorMock, existingFlows, _run);

            Assert.IsNotNull(updateNodeRatesResult);
            Assert.AreEqual(1, updateNodeRatesResult.StressPeriods.Count);
            Assert.AreEqual(2, updateNodeRatesResult.StressPeriods[0].LocationRates.Count);
            Assert.AreEqual("1", updateNodeRatesResult.StressPeriods[0].LocationRates[0].Location);
            Assert.AreEqual(1925.0, updateNodeRatesResult.StressPeriods[0].LocationRates[0].Rate, .1);
            Assert.AreEqual("2", updateNodeRatesResult.StressPeriods[0].LocationRates[1].Location);
            Assert.AreEqual(2.22, updateNodeRatesResult.StressPeriods[0].LocationRates[1].Rate, .001);
        }
    }
}
