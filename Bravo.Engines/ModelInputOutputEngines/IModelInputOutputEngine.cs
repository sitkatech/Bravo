using Bravo.Common.DataContracts.Runs;

namespace Bravo.Engines.ModelInputOutputEngines
{
    public interface IModelInputOutputEngine
    {
        void GenerateInputFiles(Run run);
        void GenerateOutputFiles(Run run);
    }
}