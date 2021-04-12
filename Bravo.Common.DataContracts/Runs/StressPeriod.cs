using System.Runtime.Serialization;

namespace Bravo.Common.DataContracts.Runs
{
    [DataContract]
    public class StressPeriod
    {
        [DataMember]
        public double Days { get; set; }

        [DataMember]
        public int NumberOfTimeSteps { get; set; }
    }
}