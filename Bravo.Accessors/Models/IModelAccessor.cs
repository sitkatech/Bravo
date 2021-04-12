using Bravo.Common.DataContracts.Models;

namespace Bravo.Accessors.Models
{
    public interface IModelAccessor
    {
        Model[] FindAllModels();
        ModelWithScenario FindModelWithScenario(int modelId, int scenarioId);

        Image FindImageForModel(int modelId);
    }
}
