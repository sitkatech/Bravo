namespace Bravo.Accessors.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Run
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Run()
        {
            RunBucketRun = new HashSet<RunBucketRun>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string FileStorageLocator { get; set; }

        public int? ImageId { get; set; }

        public int ModelId { get; set; }

        public int ScenarioId { get; set; }

        public int Status { get; set; }

        public bool IsDeleted { get; set; }

        public string Output { get; set; }

        [StringLength(256)]
        public string InputFileName { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ProcessingStartDate { get; set; }

        public DateTime? ProcessingEndDate { get; set; }

        public bool ShouldCreateMaps { get; set; }

        public int RestartCount { get; set; }

        public virtual Image Image { get; set; }

        public virtual Model Model { get; set; }

        public virtual Scenario Scenario { get; set; }

        public int InputVolumeUnit { get; set; }

        public int OutputVolumeUnit { get; set; }

        public bool IsDifferential { get; set; }

        public string Description { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RunGeography> Geographies { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RunBucketRun> RunBucketRun { get; set; }

    }
}
