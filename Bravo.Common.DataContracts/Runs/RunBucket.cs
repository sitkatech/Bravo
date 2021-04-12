using Bravo.Common.DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Common.DataContracts.Runs
{
    public class RunBucket
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public List<Run> Runs { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember]
        public string Description { get; set; }

        public List<RunResultListItem> AvailableRunResults { get; set; }
    }
}
