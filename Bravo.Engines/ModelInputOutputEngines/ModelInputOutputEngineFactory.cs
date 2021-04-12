using System;
using Bravo.Common.DataContracts.Models;
using Bravo.Common.DataContracts.Runs;

namespace Bravo.Engines.ModelInputOutputEngines
{
    public class ModelInputOutputEngineFactory : IModelInputOutputEngineFactory
    {
        public IModelInputOutputEngine CreateModelInputOutputEngine(Run run)
        {
            if (run.Model.IsModflowModel)
            {
                return new ModflowModelInputOutputEngine(run.Model);
            }
            else
            {
                return new ModpathModelInputOutputEngine();
            }
        }
    }
}