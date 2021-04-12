using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Common.DataContracts.Runs
{
    public class RunFilter
    {
        public RunFilter()
        {
            Statuses = new List<RunStatus>();
        }

        public string NameSearch { get; set; }

        public List<RunStatus> Statuses { get; set; }

        public int? ModelId { get; set; }

        public int? ScenarioId { get; set; }
    }
}
