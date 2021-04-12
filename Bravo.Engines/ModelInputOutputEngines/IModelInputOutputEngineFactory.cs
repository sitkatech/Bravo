using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bravo.Common.DataContracts.Runs;

namespace Bravo.Engines.ModelInputOutputEngines
{
    public interface IModelInputOutputEngineFactory
    {
        IModelInputOutputEngine CreateModelInputOutputEngine(Run run);
    }
}
