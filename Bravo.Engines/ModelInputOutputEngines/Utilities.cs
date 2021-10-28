using Bravo.Common.DataContracts.Models;
using Bravo.Common.DataContracts.Runs;
using System.Collections.Generic;

namespace Bravo.Engines.ModelInputOutputEngines
{
    internal class Utilities
    {
        internal static StressPeriodLocationRates GetStressPeriod(int year, int month, Model model, List<StressPeriodLocationRates> stressPeriodLocationRates)
        {
            if (model.ModelStressPeriodCustomStartDates != null && model.ModelStressPeriodCustomStartDates.Length > 0)
            {
                for (int i = 0; i < model.ModelStressPeriodCustomStartDates.Length; i++)
                {
                    var stressPeriodDate = model.ModelStressPeriodCustomStartDates[i].StressPeriodStartDate;
                    if (stressPeriodDate.Month == month && stressPeriodDate.Year == year)
                    {
                        return stressPeriodLocationRates[i];
                    }
                }

                throw new InputDataInvalidException("Invalid date in the input file.");
            }


            var stressPeriodIndex = (year - model.StartDateTime.Year) * 12 + month - model.StartDateTime.Month;

            if (stressPeriodIndex >= stressPeriodLocationRates.Count)
            {
                throw new InputDataInvalidException("Date too far in the future in the input file.");
            }

            if (stressPeriodIndex < 0)
            {
                throw new InputDataInvalidException("Invalid date in the input file.");
            }

            return stressPeriodLocationRates[stressPeriodIndex];
        }
    }
}
