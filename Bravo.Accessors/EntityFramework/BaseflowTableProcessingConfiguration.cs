using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bravo.Accessors.EntityFramework
{
    public partial class BaseflowTableProcessingConfiguration
    {
        public BaseflowTableProcessingConfiguration()
        {
            Models = new HashSet<Model>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BaseflowTableProcessingConfigurationID { get; set; }

        [Required]
        [StringLength(200)]
        public string BaseflowTableIndicatorRegexPattern { get; set; }

        [Required]
        public int SegmentColumnNum { get; set; }

        [Required]
        public int FlowToAquiferColumnNum { get; set; }

        public int? ReachColumnNum { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Model> Models { get; set; }
    }
}