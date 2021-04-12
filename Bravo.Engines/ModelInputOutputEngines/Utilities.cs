using Bravo.Common.DataContracts.Models;
using Bravo.Common.DataContracts.Runs;
using System.Collections.Generic;

namespace Bravo.Engines.ModelInputOutputEngines
{
    internal class Utilities
    {
        internal static StressPeriodLocationRates GetStressPeriod(int year, int month, Model model, List<StressPeriodLocationRates> stressPeriods)
        {
            var stressPeriodIndex = (year - model.StartDateTime.Year) * 12 + month - model.StartDateTime.Month;

            if (stressPeriodIndex >= stressPeriods.Count)
            {
                throw new InputDataInvalidException("Date too far in the future in the input file.");
            }

            if (stressPeriodIndex < 0)
            {
                throw new InputDataInvalidException("Invalid date in the input file.");
            }

            return stressPeriods[stressPeriodIndex];
        }
    }
}
