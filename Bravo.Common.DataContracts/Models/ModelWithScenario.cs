namespace Bravo.Common.DataContracts.Models
{
    public class ModelWithScenario
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; }
        public virtual Scenario[] Scenarios { get; set; }
    }
}