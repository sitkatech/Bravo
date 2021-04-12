namespace Bravo.Accessors.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ScenarioFile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ScenarioFile()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int ScenarioId { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Required { get; set; }

        public virtual Scenario Scenario { get; set; }
    }
}
