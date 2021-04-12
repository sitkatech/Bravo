using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Accessors.EntityFramework
{
    public partial class RunBucket
    {
        public RunBucket()
        {
            RunBucketRuns = new HashSet<RunBucketRun>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Description { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RunBucketRun> RunBucketRuns { get; set; }
    }
}
