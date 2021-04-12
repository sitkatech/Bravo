using System.ComponentModel.DataAnnotations;

namespace Bravo.Common.Shared.Enums
{
    public enum VolumeType
    {
        [Display(Name = "Unknown")]
        Unknown = 0,

        [Display(Name = "Acre-Feet")]
        AcreFeet = 1,

        [Display(Name = "Cubic Feet")]
        CubicFeet = 2,

        [Display(Name = "Cubic Yard")]
        CubicYard = 3,

        [Display(Name = "Cubic Meter")]
        CubicMeter = 4,

        [Display(Name = "Gallon")]
        Gallon = 5,

        [Display(Name = "Million Gallon")]
        MillionGallon = 6
    }
}
