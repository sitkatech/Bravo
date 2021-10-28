using System;
using System.Runtime.Serialization;

namespace Bravo.Common.DataContracts.Models
{
    public class Model
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ImageId { get; set; }

        public string NamFileName { get; set; }
        
        public DateTime StartDateTime { get; set; }

        public string RunFileName { get; set; }

        public string ListFileName { get; set; }

        public string ModflowExeName { get; set; }

        public string ModpathExeName { get; set; }

        public string ZoneBudgetExeName { get; set; }

        public virtual Scenario[] Scenarios { get; set; }

        public double? AllowablePercentDiscrepancy { get; set; }

        public string MapSettings { get; set; }

        public string MapRunFileName { get; set; }

        public string MapDrawdownFileName { get; set; }

        public string SimulationFileName { get; set; }

        public string MapModelArea { get; set; }

        public bool IsDoubleSizeHeatMapOutput { get; set; }

        public string InputZoneData { get; set; }

        public string OutputZoneData { get; set; }

        public int NumberOfStressPeriods { get; set; }

        public string CanalData { get; set; }

        public string BuddyGroup { get; set; }
        public int? BaseflowTableProcessingConfigurationID { get; set; }
        public virtual BaseflowTableProcessingConfiguration BaseflowTableProcessingConfiguration { get; set; }

        public bool IsModflowModel { get { return string.IsNullOrWhiteSpace(ModpathExeName); } }
        public virtual ModelStressPeriodCustomStartDate[] ModelStressPeriodCustomStartDates { get; set; }

    }
}
