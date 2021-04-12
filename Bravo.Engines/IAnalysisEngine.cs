using Bravo.Common.DataContracts.Runs;

namespace Bravo.Engines
{
    public interface IAnalysisEngine
    {
        AnalysisResult RunAnalysis(Run run);
    }
}