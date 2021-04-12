using Bravo.Common.DataContracts.Models;

namespace Bravo.Managers.Models
{
    public interface IModelManager
    {
        Model[] FindAllModels();
        ModelWithScenario FindModelWithScenario(int modelId, int scenarioId);
    }
}
