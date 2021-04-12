using Newtonsoft.Json;
using Bravo.Accessors.FileIO;
using Bravo.Common.DataContracts.Runs;
using Bravo.Common.Shared;
using Bravo.Common.Utilities;
using System.Text;

namespace Bravo.Engines.ModelInputOutputEngines
{
    public abstract class BaseInputOutputEngine : BaseEngine
    {
        protected static void WriteOuputFile(Run run, IBlobFileAccessor fileAccessor, RunResultDetails result)
        {
            WriteOuputFile(run, fileAccessor, result, false, result.RunResultName);
        }

        protected static void WriteOuputFile(Run run, IBlobFileAccessor fileAccessor, RunResultDetails result, bool hidden, string name)
        {
            result.Version = "1.0";
            fileAccessor.SaveFile(StorageLocations.OutputFilePathForRun($"{(hidden ? "!" : "")}{result.RunResultId.ToString().PadLeft(3, '0')}-{name}.json", run.FileStorageLocator), ConfigurationHelper.AppSettings.BlobStorageModelDataFolder, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result)));
        }

        protected static void WriteKmlFile(Run run, IBlobFileAccessor fileAccessor, RunResultDetails result, bool hidden, string name)
        {
            var kmlContent = Encoding.UTF8.GetBytes(result.ResultSets[0].MapData.KmlString);

            fileAccessor.SaveFile(StorageLocations.OutputFilePathForRun($"{(hidden ? "!" : "")}{result.RunResultId.ToString().PadLeft(3, '0')}-{name}.kml",
                run.FileStorageLocator),
                ConfigurationHelper.AppSettings.BlobStorageModelDataFolder,
                kmlContent,
                "application/vnd.google-earth.kml+xml"
                );
        }
    }
}
