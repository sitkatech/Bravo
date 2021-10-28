using System;

namespace Bravo.Accessors.EntityFramework
{
    public partial class ModelStressPeriodCustomStartDate
    {
        public int ModelStressPeriodCustomStartDateID { get; set; }

        public int ModelId { get; set; }

        public int StressPeriod { get; set; }

        public DateTime StressPeriodStartDate { get; set; }

        public virtual Model Model { get; set; }

    }
}