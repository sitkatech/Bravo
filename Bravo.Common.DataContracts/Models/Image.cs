using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Common.DataContracts.Models
{
    public class Image
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Server { get; set; }

        public bool IsLinux { get; set; }

        public int? CpuCoreCount { get; set; }

        public decimal? Memory { get; set; }
    }
}
