using Bravo.Common.DataContracts.Models;
using Bravo.Common.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Common.DataContracts.Runs
{
    [DataContract]
    public class Run
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string FileStorageLocator { get; set; }

        [DataMember]
        public int? ImageId { get; set; }

        [DataMember]
        public int ModelId { get; set; }

        [DataMember]
        public int ScenarioId { get; set; }

        [DataMember]
        public string InputFileName { get; set; }

        [DataMember]
        public RunStatus Status { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public DateTime? ProcessingStartDate { get; set; }

        [DataMember]
        public DateTime? ProcessingEndDate { get; set; }

        [DataMember]
        public bool ShouldCreateMaps { get; set; }

        [DataMember]
        public int RestartCount { get; set; }

        [DataMember]
        public string Output { get; set; }

        [DataMember]
        public Model Model { get; set; }

        [DataMember]
        public Scenario Scenario { get; set; }

        [DataMember]
        public Image Image { get; set; }

        [DataMember]
        public VolumeType InputVolumeType { get; set; }

        [DataMember]
        public VolumeType OutputVolumeType { get; set; }

        [DataMember]
        public bool IsDifferential { get; set; }

        [DataMember]
        public string Description { get; set; }

        public List<RunResultListItem> AvailableRunResults { get; set; }

        public List<RunCanalInput> CanalRunInputs { get; set; }

        public List<RunWellInput> WellMapInputs { get; set; }

        public List<PivotedRunWellInput> PivotedWellMapInputs { get; set; }

        public List<RunZoneInput> RunZoneInputs { get; set; }

        public List<RunWellParticleInput> RunWellParticleInputs { get; set; }

        public List<RunBucket> RunBuckets { get; set; }
    }

    [DataContract]
    public class RunResultListItem
    {
        [DataMember]
        public int RunResultId { get; set; }

        [DataMember]
        public string RunResultName { get; set; }

        [DataMember]
        public string RunResultFileExtension { get; set; }
    }
}
