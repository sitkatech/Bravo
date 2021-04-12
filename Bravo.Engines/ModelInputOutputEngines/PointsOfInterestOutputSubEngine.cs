using log4net;
using Bravo.Accessors.FileIO;
using Bravo.Common.DataContracts.Models;
using Bravo.Common.DataContracts.Runs;
using Bravo.Common.Shared.Enums;
using Bravo.Common.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Bravo.Engines.ModelInputOutputEngines
{
    internal interface IPointsOfInterestOutputSubEngine
    {
        List<RunResultDetails> GeneratePointsOfInterestGraphOutput(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, int currResultId, bool isDifferential);
    }

    internal class PointsOfInterestOutputSubEngine : IPointsOfInterestOutputSubEngine
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(ListFileOutputSubEngine));
        public PointsOfInterestOutputSubEngine(Model model)
        {
            Model = model;
        }

        private Model Model { get; }

        public List<RunResultDetails> GeneratePointsOfInterestGraphOutput(IModelFileAccessor modflowFileAccessor, List<StressPeriod> stressPeriods, int currResultId, bool isDifferential)
        {
            Logger.Info("Generating points of interest output.");

            if (stressPeriods == null || !stressPeriods.Any())
            {
                Logger.Warn("Not generating points of interest output because no stress period data found.");
                return new List<RunResultDetails>(); ;
            }

            var pointsOfInterest = modflowFileAccessor.GetPointsOfInterest();
            if (pointsOfInterest == null)
            {
                Logger.Warn("Not generating points of interest output because no points of interest file found.");
                return new List<RunResultDetails>(); ;
            }

            var cells = GetLocationCellDictionary(pointsOfInterest.ToList(), modflowFileAccessor);

            var result = CreateRunResultSetWithPointsOfInterest(pointsOfInterest.Select(x => x.Name).ToList());

            if (isDifferential)
            {
                Logger.Info("Run is differential -- comparing to baseline.");
                AddFlowDeltas(result, modflowFileAccessor.GetBaselineMapData(), modflowFileAccessor.GetOutputMapData(), stressPeriods, cells);
            }
            else
            {
                Logger.Info("Run is non-differential -- ignoring baseline data even if present.");
                AddFlowData(result, modflowFileAccessor.GetOutputMapData(), stressPeriods, cells);
            }

            AddObservedData(result, modflowFileAccessor, isDifferential);

            var resultDetails = new RunResultDetails
            {
                RunResultName = "Points of Interest",
                ResultSets = new List<RunResultSet>(),
            };
            resultDetails.ResultSets.Add(result);

            Logger.Info("List file output results generated.");
            return new List<RunResultDetails>() { resultDetails };
        }

        private Dictionary<string, List<string>> GetLocationCellDictionary(List<PointOfInterest> pointsOfInterest, IModelFileAccessor modflowFileAccessor)
        {
            var cells = new Dictionary<string, List<string>>();
            foreach (var point in pointsOfInterest)
            {
                var cellLocation = modflowFileAccessor.FindLocationCell(point.Coordinate.Lat, point.Coordinate.Lng).Location.Replace(' ', '|');

                if (cells.ContainsKey(cellLocation))
                {
                    cells[cellLocation].Add(point.Name);
                }
                else
                {
                    cells.Add(cellLocation, new List<string> { point.Name });
                }
            }

            return cells;
        }

        private void AddObservedData(RunResultSet result, IModelFileAccessor modflowFileAccessor, bool isDifferential)
        {
            var observedData = modflowFileAccessor.GetObservedPointsOfInterest(isDifferential);
            if (observedData == null)
            {
                Logger.Debug("Observed data is not present -- skipping adding it to chart.");
                return;
            }

            foreach (var observedLocation in observedData.GroupBy(x => x.LocationSeriesName))
            {
                if (observedLocation.Any(x => x.Period > Model.NumberOfStressPeriods))
                {
                    Logger.Debug($"{observedLocation.Key} has data from a period outside the model duration.");
                    throw new OutputDataInvalidException("Too many stress periods in observed data.");
                }

                result.DataSeries.Add(new DataSeries
                {
                    Name = observedLocation.Key,
                    IsDefaultDisplayed = false,
                    DataPoints = observedLocation.Select(CalculateObservedDataPoint).ToList(),
                    IsObserved = true
                });
            }
        }

        private RunResultSet CreateRunResultSetWithPointsOfInterest(List<string> pointsOfInterest)
        {
            var result = new RunResultSet()
            {
                Name = "Points of Interest",
                DisplayType = RunResultDisplayType.LineChart,
                DataSeries = new List<DataSeries>(),
                DataType = "Elevation (feet)",
            };

            foreach (var point in pointsOfInterest)
            {
                result.DataSeries.Add(new DataSeries()
                {
                    Name = point,
                    IsDefaultDisplayed = false,
                    DataPoints = new List<RunResultSetDataPoint>(),
                    IsObserved = false
                });
            }

            return result;
        }

        private void AddFlowDeltas(RunResultSet result, IEnumerable<MapOutputData> baseline, IEnumerable<MapOutputData> run, List<StressPeriod> stressPeriods, Dictionary<string, List<string>> locations)
        {
            using (var runEnumerator = run.GetEnumerator())
            {
                foreach (var baselineData in baseline)
                {
                    if (!runEnumerator.MoveNext() || runEnumerator.Current == null)
                    {
                        throw new OutputDataInvalidException("Not enough rows in map output data.");
                    }

                    var runData = runEnumerator.Current;
                    var stressPeriod = stressPeriods[baselineData.StressPeriod - 1];

                    if (baselineData.StressPeriod != runData.StressPeriod || baselineData.TimeStep != runData.TimeStep || baselineData.Location != runData.Location)
                    {
                        throw new OutputDataInvalidException("Mismatched map output data.");
                    }

                    if (baselineData.StressPeriod > stressPeriods.Count)
                    {
                        throw new OutputDataInvalidException("Stress period not found.");
                    }

                    if (stressPeriod.NumberOfTimeSteps == baselineData.TimeStep && locations.ContainsKey(baselineData.Location))
                    {
                        var pointsOfInterest = locations[baselineData.Location];
                        var date = Model.StartDateTime.AddMonths(baselineData.StressPeriod - 1);

                        foreach (var pointOfInterest in pointsOfInterest)
                        {
                            AddDelta(runData, baselineData, date, result.DataSeries.Where(x => x.Name == pointOfInterest).SingleOrDefault());
                        }
                    }
                }
            }
        }


        private void AddFlowData(RunResultSet result, IEnumerable<MapOutputData> run, List<StressPeriod> stressPeriods, Dictionary<string, List<string>> locations)
        {
            foreach (var runData in run)
            {
                var stressPeriod = stressPeriods[runData.StressPeriod - 1];

                if (runData.StressPeriod > stressPeriods.Count)
                {
                    throw new OutputDataInvalidException("Stress period not found.");
                }

                if (stressPeriod.NumberOfTimeSteps == runData.TimeStep && locations.ContainsKey(runData.Location))
                {
                    var date = Model.StartDateTime.AddMonths(runData.StressPeriod - 1);
                    var pointOfInterests = locations[runData.Location];
                    foreach (var pointOfInterest in pointOfInterests)
                    {
                        AddData(runData, date, result.DataSeries.Where(x => x.Name == pointOfInterest).SingleOrDefault());
                    }
                }
            }
        }


        private static void AddDelta(MapOutputData runData, MapOutputData baselineData, DateTime date, DataSeries result)
        {
            double difference = 0;

            if (runData.Value != null && baselineData.Value != null)
            {
                difference = runData.Value.Value - baselineData.Value.Value;
            }

            result.DataPoints.Add(new RunResultSetDataPoint()
            {
                Date = date,
                Value = difference
            });
        }

        private static void AddData(MapOutputData runData, DateTime date, DataSeries result)
        {
            result.DataPoints.Add(new RunResultSetDataPoint()
            {
                Date = date,
                Value = runData.Value ?? 0
            });
        }

        private RunResultSetDataPoint CalculateObservedDataPoint(ObservedPointOfInterest point)
        {
            var date = Model.StartDateTime.AddMonths(point.Period - 1);

            return new RunResultSetDataPoint
            {
                Date = date,
                Value = point.ValueInCubicFeet
            };
        }

    }
}
