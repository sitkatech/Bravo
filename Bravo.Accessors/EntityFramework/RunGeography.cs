using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Spatial;

namespace Bravo.Accessors.EntityFramework
{
    public partial class RunGeography
    {
        public int Id { get; set; }

        public int RunId { get; set; }

        public int StressPeriod { get; set; }

        [Required]
        [StringLength(7)]
        public string Color { get; set; }

        public DbGeography Geography { get; set; }

        public virtual Run Run { get; set; }
    }
}