namespace Bravo.Common.DataContracts.Models
{
    public class ScenarioFile
    {
        public int Id { get; set; }

        public int ScenarioId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool Required { get; set; }

        public bool Uploaded { get; set; }

        public Scenario Scenario { get; set; }
    }
}
