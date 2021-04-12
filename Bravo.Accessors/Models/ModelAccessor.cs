using System.Linq;
using Bravo.Common.DataContracts.Models;

namespace Bravo.Accessors.Models
{
    class ModelAccessor : BaseTableAccessor, IModelAccessor
    {
        public Model[] FindAllModels()
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var entity = from m in db.Models select m;

                return DTOMapper.Mapper.Map<Model[]>(entity);
            }
        }

        public ModelWithScenario FindModelWithScenario(int modelId, int scenarioId)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var modelScenario = (from ms in db.ModelScenarios
                                     where ms.ModelId == modelId
                                     && ms.ScenarioId == scenarioId
                                     select ms).FirstOrDefault();

                if (modelScenario != null)
                {
                    var model = (from m in db.Models
                        where m.Id == modelId
                        select m).First();

                    var scenario = (from s in db.Scenarios.Include("Files")
                        where s.Id == scenarioId
                        select s).First();

                    return new ModelWithScenario()
                    {
                        ModelId = model.Id,
                        ModelName = model.Name,
                        Scenarios = new Scenario[]
                        {
                            new Scenario
                            {
                                Id = scenario.Id,
                                Name = scenario.Name,
                                InputControlType = (InputControlType)scenario.InputControlType,
                                Files = DTOMapper.Mapper.Map<ScenarioFile[]>(scenario.Files)
                            }
                        }
                    };
                }

                return null;
            }
        }

        public Image FindImageForModel(int modelId)
        {
            using (var db = DatabaseFactory.Create<PrimaryDBContext>())
            {
                var image = (from i in db.Images
                            join m in db.Models on i.Id equals m.ImageId
                            where m.Id == modelId
                            select i).FirstOrDefault();

                return DTOMapper.Mapper.Map<Image>(image);
            }
        }
    }
}
