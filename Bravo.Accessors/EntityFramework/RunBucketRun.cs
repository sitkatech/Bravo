using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Accessors.EntityFramework
{
    public partial class RunBucketRun
    {
        public int Id { get; set; }
        public int RunId { get; set; }
        public int RunBucketId { get; set; }
        public virtual Run Run { get; set; }
        public virtual RunBucket RunBucket { get; set; }
    }
}
