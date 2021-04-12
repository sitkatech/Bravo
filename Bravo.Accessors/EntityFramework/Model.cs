namespace Bravo.Accessors.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Model
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Model()
        {
            ModelScenarios = new HashSet<ModelScenario>();
            Runs = new HashSet<Run>();
            Scenarios = new HashSet<Scenario>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        public int ImageId { get; set; }

        [Required]
        [StringLength(50)]
        public string NamFileName { get; set; }

        public DateTime StartDateTime { get; set; }

        [StringLength(50)]
        public string RunFileName { get; set; }
        
        [StringLength(50)]
        public string ModflowExeName { get; set; }

        [StringLength(50)]
        public string ModpathExeName { get; set; }

        [StringLength(50)]
        public string ZoneBudgetExeName { get; set; }

        [StringLength(50)]
        public string MapRunFileName { get; set; }

        [StringLength(50)]
        public string MapDrawdownFileName { get; set; }

        public double? AllowablePercentDiscrepancy { get; set; }

        [StringLength(1024)]
        public string MapSettings { get; set; }

        public string MapModelArea { get; set; }

        public bool IsDoubleSizeHeatMapOutput { get; set; }

        public string InputZoneData { get; set; }

        public int NumberOfStressPeriods { get; set; }

        public string SimulationFileName { get; set; }

        public string CanalData { get; set; }

        public string BuddyGroup { get; set; }

        public string ListFileName { get; set; }
        
        public string OutputZoneData { get; set; }

        public int? BaseflowTableProcessingConfigurationID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ModelScenario> ModelScenarios { get; set; }

        public virtual Image Image { get; set; }

        public virtual BaseflowTableProcessingConfiguration BaseflowTableProcessingConfiguration { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Run> Runs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Scenario> Scenarios { get; set; }
    }
}
