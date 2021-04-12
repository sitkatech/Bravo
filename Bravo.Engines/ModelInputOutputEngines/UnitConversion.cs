using System;
using Bravo.Common.DataContracts.Runs;
using Bravo.Common.Shared.Enums;

namespace Bravo.Engines.ModelInputOutputEngines
{
    internal static class UnitConversion
    {
        private const double Million = 1e6;

        private const double CubicFeetPerAcreFoot = 43560;
        private const double CubicFeetPerCubicYard = 27;
        private const double CubicFeetPerCubicMeter = 35.314666721489;
        private const double CubicFeetPerUsGallon = 0.13368055555556;

        public static double CalculateVolumePerTimeStep(double volume, StressPeriod stressPeriod)
        {
            return volume * (stressPeriod.Days / stressPeriod.NumberOfTimeSteps);
        }

        public static double ConvertFlow(double value, VolumeType currentType, VolumeType newType, int daysInMonth)
        {
            double convertedVolume = ConvertVolume(value, currentType, newType);
            return convertedVolume * (GetDefaultFlowPeriod(newType, daysInMonth).TotalMilliseconds / GetDefaultFlowPeriod(currentType, daysInMonth).TotalMilliseconds);
        }

        public static double ConvertVolume(double value, VolumeType currentType, VolumeType newType)
        {
            var cubicFeet = ToCubicFeet(value, currentType);
            switch (newType)
            {
                case VolumeType.AcreFeet:
                    return ToAcreFeet(cubicFeet);
                case VolumeType.CubicFeet:
                    return ToCubicFeet(cubicFeet);
                case VolumeType.CubicYard:
                    return ToCubicYards(cubicFeet);
                case VolumeType.CubicMeter:
                    return ToCubicMeters(cubicFeet);
                case VolumeType.Gallon:
                    return ToUsGallons(cubicFeet);
                case VolumeType.MillionGallon:
                    return ToMillionUsGallons(cubicFeet);
                default:
                    throw new NotSupportedException($"No conversion exists for cubic feet to {newType}");
            }
        }

        private static TimeSpan GetDefaultFlowPeriod(VolumeType type, int daysInMonth)
        {
            switch (type)
            {
                case VolumeType.AcreFeet:
                    return TimeSpan.FromDays(daysInMonth);
                case VolumeType.CubicFeet:
                    return TimeSpan.FromDays(1);
                case VolumeType.CubicYard:
                    return TimeSpan.FromDays(1);
                case VolumeType.CubicMeter:
                    return TimeSpan.FromDays(1);
                case VolumeType.Gallon:
                    return TimeSpan.FromMinutes(1);
                case VolumeType.MillionGallon:
                    return TimeSpan.FromDays(1);
                default:
                    throw new NotSupportedException($"No conversion exists for cubic feet to {type}");
            }
        }
        private static double ToCubicFeet(double cubicFeet)
        {
            return cubicFeet;
        }
        private static double ToCubicMeters(double cubicFeet)
        {
            return cubicFeet * (1 / CubicFeetPerCubicMeter);
        }
        private static double ToAcreFeet(double cubicFeet)
        {
            return cubicFeet * (1 / CubicFeetPerAcreFoot);
        }
        private static double ToUsGallons(double cubicFeet)
        {
            return cubicFeet * (1 / CubicFeetPerUsGallon);
        }
        private static double ToMillionUsGallons(double cubicFeet)
        {
            return ToUsGallons(cubicFeet) / Million;
        }
        private static double ToCubicYards(double cubicFeet)
        {
            return cubicFeet * (1 / CubicFeetPerCubicYard);
        }
        private static double ToCubicFeet(double value, VolumeType currentType)
        {
            switch (currentType)
            {
                case VolumeType.AcreFeet:
                    return value * CubicFeetPerAcreFoot;
                case VolumeType.CubicFeet:
                    return value;
                case VolumeType.CubicYard:
                    return value * CubicFeetPerCubicYard;
                case VolumeType.CubicMeter:
                    return value * CubicFeetPerCubicMeter;
                case VolumeType.Gallon:
                    return value * CubicFeetPerUsGallon;
                case VolumeType.MillionGallon:
                    return value * CubicFeetPerUsGallon * Million;
                default:
                    throw new NotSupportedException($"No conversion exists for volume type {currentType} to cubic feet");
            }
        }
    }
}