using log4net;
using Bravo.Accessors.Models;
using Bravo.Common.DataContracts.Models;
using Bravo.Common.Utilities;

namespace Bravo.Managers.Models
{
    public class ModelManager : BaseManager, IModelManager
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(ModelManager));

        public Model[] FindAllModels()
        {
            Logger.Info($"Finding all models");

            return AccessorFactory.CreateAccessor<IModelAccessor>().FindAllModels();
        }

        public ModelWithScenario FindModelWithScenario(int modelID, int scenarioID)
        {
            Logger.Info($"Finding Model:{modelID} with Scenario:{scenarioID}");

            return AccessorFactory.CreateAccessor<IModelAccessor>().FindModelWithScenario(modelID, scenarioID);
        }
    }
}
