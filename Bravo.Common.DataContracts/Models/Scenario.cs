namespace Bravo.Common.DataContracts.Models
{
    public class Scenario
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public InputControlType InputControlType { get; set; }

        public bool ShouldSwitchSign { get; set; }

        public int? InputImageId { get; set; }

        public Image InputImage { get; set; }

        public ScenarioFile[] Files { get; set; }
    }
}
