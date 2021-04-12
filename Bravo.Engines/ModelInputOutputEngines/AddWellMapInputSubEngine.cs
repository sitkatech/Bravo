﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Bravo.Accessors.FileIO;
using Bravo.Common.DataContracts.Models;
using Bravo.Common.DataContracts.Runs;
using Bravo.Common.Shared;
using Bravo.Common.Shared.Enums;
using Bravo.Common.Utilities;

namespace Bravo.Engines.ModelInputOutputEngines
{
    internal interface IAddWellMapInputSubEngine
    {
        StressPeriodsLocationRates UpdateFlowInputs(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, StressPeriodsLocationRates existingFlows, Run run);
    }

    internal class AddWellMapInputSubEngine : IAddWellMapInputSubEngine
    {
        public AddWellMapInputSubEngine(Model model)
        {
            Model = model;
        }
        private Model Model { get; }

        private List<RunWellInput> GetInputFileData(IBlobFileAccessor fileAccessor, Run run)
        {
            return JsonConvert.DeserializeObject<List<RunWellInput>>(Encoding.UTF8.GetString(fileAccessor.GetFile(StorageLocations.ParsedWellInputFilePathForRun(run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder)));
        }

        public StressPeriodsLocationRates UpdateFlowInputs(IModelFileAccessor modflowFileAccessor, IBlobFileAccessor fileAccessor, StressPeriodsLocationRates existingFlows, Run run)
        {
            var mapPointsInputs = GetInputFileData(fileAccessor, run);

            var groupedData = mapPointsInputs.SelectMany(a => a.Values.Where(b => b.Value.IsNotEqual(0)).Select(b => new { Date = (a.Year, a.Month), Value = b })).GroupBy(a => (a.Value.Lat, a.Value.Lng));
            foreach (var latLngData in groupedData)
            {
                var wellLocations = modflowFileAccessor.FindWellLocations(latLngData.Key.Item1, latLngData.Key.Item2);
                foreach (var latLngValue in latLngData)
                {
                    var val = latLngValue.Value.Value;
                    if (!run.Scenario.ShouldSwitchSign) //this seems backwards but add a well should go in as a negative.  Thus is we shouldn't switch sign we want to multiply by -1, otherwise leave it.
                    {
                        val *= -1.0;
                    }
                    var daysInMonth = DateTime.DaysInMonth(latLngValue.Date.Item1, latLngValue.Date.Item2);
                    var stressPeriod = Utilities.GetStressPeriod(latLngValue.Date.Item1, latLngValue.Date.Item2, Model, existingFlows.StressPeriods);
                    var flowInCubicFeetPerDay = UnitConversion.ConvertFlow(val, run.InputVolumeType, VolumeType.CubicFeet, daysInMonth);

                    foreach (var locationPumpingProportion in wellLocations)
                    {
                        stressPeriod.LocationRates.Insert(0, new LocationRate { Location = locationPumpingProportion.Location, Rate = flowInCubicFeetPerDay * locationPumpingProportion.Proportion });
                    }
                }
            }

            return existingFlows;
        }
    }
}