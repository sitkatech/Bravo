using Bravo.Common.DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Common.DataContracts.Runs
{
    public class ParticleLocation
    {
        public int PaticleId { get; set; }

        public List<ParticleTimeLocations> TimeLocations { get; set; }
    }

    public class ParticleTimeLocations
    {
        public double TimeSinceBeginingOfModelInDays { get; set; }

        public Coordinate Coordinate { get; set; }
    }
}
