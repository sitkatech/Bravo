using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bravo.Common.Shared.Enums;
using Bravo.Engines.ModelInputOutputEngines;

namespace Bravo.Tests.EngineTests
{
    [TestClass]
    public class UnitConversionTests
    {
        [DataRow(1, VolumeType.AcreFeet, VolumeType.AcreFeet, 30, 1)]
        [DataRow(1, VolumeType.CubicFeet, VolumeType.AcreFeet, 30, 6.88705e-4)]
        [DataRow(1, VolumeType.CubicMeter, VolumeType.AcreFeet, 30, 0.0243214)]
        [DataRow(1, VolumeType.CubicYard, VolumeType.AcreFeet, 30, 0.0185950)]
        [DataRow(1, VolumeType.Gallon, VolumeType.AcreFeet, 30, 0.132576)]
        [DataRow(1, VolumeType.MillionGallon, VolumeType.AcreFeet, 30, 92.0665)]

        [DataRow(1, VolumeType.AcreFeet, VolumeType.AcreFeet, 28, 1)]
        [DataRow(1, VolumeType.CubicFeet, VolumeType.AcreFeet, 28, 6.42792e-4)]
        [DataRow(1, VolumeType.CubicMeter, VolumeType.AcreFeet, 28, 0.0227000)]
        [DataRow(1, VolumeType.CubicYard, VolumeType.AcreFeet, 28, 0.0173554)]
        [DataRow(1, VolumeType.Gallon, VolumeType.AcreFeet, 28, 0.123737)]
        [DataRow(1, VolumeType.MillionGallon, VolumeType.AcreFeet, 28, 85.9287)]

        [DataRow(1, VolumeType.CubicFeet, VolumeType.CubicFeet, 30, 1)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.CubicFeet, 30, 1452.00)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.CubicFeet, 28, 1555.71)]
        [DataRow(1, VolumeType.CubicMeter, VolumeType.CubicFeet, 30, 35.3147)]
        [DataRow(1, VolumeType.CubicYard, VolumeType.CubicFeet, 30, 27)]
        [DataRow(1, VolumeType.Gallon, VolumeType.CubicFeet, 30, 192.500)]
        [DataRow(1, VolumeType.MillionGallon, VolumeType.CubicFeet, 30, 133681)]

        [DataRow(1, VolumeType.CubicMeter, VolumeType.CubicMeter, 30, 1)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.CubicMeter, 30, 41.1161)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.CubicMeter, 28, 44.0529)]
        [DataRow(1, VolumeType.CubicFeet, VolumeType.CubicMeter, 30, 0.0283168)]
        [DataRow(1, VolumeType.CubicYard, VolumeType.CubicMeter, 30, 0.764555)]
        [DataRow(1, VolumeType.Gallon, VolumeType.CubicMeter, 30, 5.45099)]
        [DataRow(1, VolumeType.MillionGallon, VolumeType.CubicMeter, 30, 3785.41)]

        [DataRow(1, VolumeType.CubicYard, VolumeType.CubicYard, 30, 1)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.CubicYard, 30, 53.7778)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.CubicYard, 28, 57.6190)]
        [DataRow(1, VolumeType.CubicMeter, VolumeType.CubicYard, 30, 1.30795)]
        [DataRow(1, VolumeType.CubicFeet, VolumeType.CubicYard, 30, 0.037037)]
        [DataRow(1, VolumeType.Gallon, VolumeType.CubicYard, 30, 7.12963)]
        [DataRow(1, VolumeType.MillionGallon, VolumeType.CubicYard, 30, 4951.13)]

        [DataRow(1, VolumeType.Gallon, VolumeType.Gallon, 30, 1)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.Gallon, 30, 7.54286)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.Gallon, 28, 8.08163)]
        [DataRow(1, VolumeType.CubicMeter, VolumeType.Gallon, 30, 0.183453)]
        [DataRow(1, VolumeType.CubicFeet, VolumeType.Gallon, 30, 0.00519481)]
        [DataRow(1, VolumeType.CubicYard, VolumeType.Gallon, 30, 0.140260)]
        [DataRow(1, VolumeType.MillionGallon, VolumeType.Gallon, 30, 694.444)]

        [DataRow(1, VolumeType.MillionGallon, VolumeType.MillionGallon, 30, 1)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.MillionGallon, 30, 0.0108617)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.MillionGallon, 28, 0.0116376)]
        [DataRow(1, VolumeType.CubicMeter, VolumeType.MillionGallon, 30, 0.000264172)]
        [DataRow(1, VolumeType.CubicFeet, VolumeType.MillionGallon, 30, 0.00000748052)]
        [DataRow(1, VolumeType.CubicYard, VolumeType.MillionGallon, 30, 0.000201974)]
        [DataRow(1, VolumeType.Gallon, VolumeType.MillionGallon, 30, 0.00144000)]

        [DataRow(-3.123456, VolumeType.CubicFeet, VolumeType.CubicMeter, 30, -0.088446424389)]
        [DataRow(0, VolumeType.CubicFeet, VolumeType.AcreFeet, 30, 0)]

        [DataTestMethod]
        public void ConvertFlowTestToFifthSignificantNumber(double currentValue, VolumeType currentType, VolumeType newType, int daysInMonth, double expectedValue)
        {
            var convertedValue = UnitConversion.ConvertFlow(currentValue, currentType, newType, daysInMonth);

            TestUtilities.AssertAreEqualWithCalculatedDelta(expectedValue, convertedValue);
        }

        [DataRow(1, VolumeType.AcreFeet, VolumeType.AcreFeet, 1)]
        [DataRow(1, VolumeType.CubicFeet, VolumeType.AcreFeet, 2.2957e-5)]
        [DataRow(1, VolumeType.CubicMeter, VolumeType.AcreFeet, 0.000810714)]
        [DataRow(1, VolumeType.CubicYard, VolumeType.AcreFeet, 0.000619836)]
        [DataRow(1, VolumeType.Gallon, VolumeType.AcreFeet, 3.0689e-6)]
        [DataRow(1, VolumeType.MillionGallon, VolumeType.AcreFeet, 3.0688)]

        [DataRow(1, VolumeType.CubicFeet, VolumeType.CubicFeet, 1)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.CubicFeet, 43559.9)]
        [DataRow(1, VolumeType.CubicMeter, VolumeType.CubicFeet, 35.3147)]
        [DataRow(1, VolumeType.CubicYard, VolumeType.CubicFeet, 27)]
        [DataRow(1, VolumeType.Gallon, VolumeType.CubicFeet, 0.133681)]
        [DataRow(1, VolumeType.MillionGallon, VolumeType.CubicFeet, 133681)]

        [DataRow(1, VolumeType.CubicMeter, VolumeType.CubicMeter, 1)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.CubicMeter, 1233.48)]
        [DataRow(1, VolumeType.CubicFeet, VolumeType.CubicMeter, 0.0283168)]
        [DataRow(1, VolumeType.CubicYard, VolumeType.CubicMeter, 0.764555)]
        [DataRow(1, VolumeType.Gallon, VolumeType.CubicMeter, 0.00378541)]
        [DataRow(1, VolumeType.MillionGallon, VolumeType.CubicMeter, 3785.41)]

        [DataRow(1, VolumeType.CubicYard, VolumeType.CubicYard, 1)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.CubicYard, 1613.33)]
        [DataRow(1, VolumeType.CubicMeter, VolumeType.CubicYard, 1.30795)]
        [DataRow(1, VolumeType.CubicFeet, VolumeType.CubicYard, 0.037037)]
        [DataRow(1, VolumeType.Gallon, VolumeType.CubicYard, 0.00495113)]
        [DataRow(1, VolumeType.MillionGallon, VolumeType.CubicYard, 4951.13)]

        [DataRow(1, VolumeType.Gallon, VolumeType.Gallon, 1)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.Gallon, 325851)]
        [DataRow(1, VolumeType.CubicMeter, VolumeType.Gallon, 264.172)]
        [DataRow(1, VolumeType.CubicFeet, VolumeType.Gallon, 7.48052)]
        [DataRow(1, VolumeType.CubicYard, VolumeType.Gallon, 201.974)]
        [DataRow(1, VolumeType.MillionGallon, VolumeType.Gallon, 1e6)]

        [DataRow(1, VolumeType.MillionGallon, VolumeType.MillionGallon, 1)]
        [DataRow(1, VolumeType.AcreFeet, VolumeType.MillionGallon, .325851)]
        [DataRow(1, VolumeType.CubicMeter, VolumeType.MillionGallon, .000264172)]
        [DataRow(1, VolumeType.CubicFeet, VolumeType.MillionGallon, .00000748052)]
        [DataRow(1, VolumeType.CubicYard, VolumeType.MillionGallon, .000201974)]
        [DataRow(1, VolumeType.Gallon, VolumeType.MillionGallon, 0.000001)]

        [DataRow(-3.123456, VolumeType.CubicFeet, VolumeType.CubicMeter, -0.088446424389)]
        [DataRow(0, VolumeType.CubicFeet, VolumeType.AcreFeet, 0)]

        [DataTestMethod]
        public void ConvertVolumeTestToFifthSignificantNumber(double currentValue, VolumeType currentType, VolumeType newType, double expectedValue)
        {
            var convertedValue = UnitConversion.ConvertVolume(currentValue, currentType, newType);

            TestUtilities.AssertAreEqualWithCalculatedDelta(expectedValue, convertedValue);
        }
    }
}
