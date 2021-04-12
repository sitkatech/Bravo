using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bravo.Accessors.EntityFramework
{
    public partial class ModelScenario
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ModelId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ScenarioId { get; set; }

        public virtual Model Model { get; set; }

        public virtual Scenario Scenario { get; set; }
    }
}