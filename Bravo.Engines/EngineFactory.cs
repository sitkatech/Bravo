using Bravo.Accessors;
using Bravo.Common.Shared;
using Bravo.Engines.RunDataParse;

namespace Bravo.Engines
{
    public class EngineFactory : FactoryBase
    {
        private AccessorFactory _accessorFactory;

        public EngineFactory() : this(null)
        {

        }

        public EngineFactory(AccessorFactory accessorFactory)
        {
            _accessorFactory = accessorFactory ?? new AccessorFactory();

            AddType<IRunDataParseEngine>(typeof(RunDataParseEngine));
            AddType<ModelInputOutputEngines.IModelInputOutputEngineFactory>(typeof(ModelInputOutputEngines.ModelInputOutputEngineFactory));
            AddType<IAnalysisEngine>(typeof(AnalysisEngine));
        }

        public T CreateEngine<T>() where T : class
        {
            return CreateEngine<T>(null);
        }

        public T CreateEngine<T>(AccessorFactory accessorFactory) where T : class
        {
            _accessorFactory = accessorFactory ?? _accessorFactory;

            T result = GetInstanceForType<T>();

            // configure the context and the accessor factory if the result is not a mock
            if (result is BaseEngine)
            {
                (result as BaseEngine).AccessorFactory = _accessorFactory;
            }

            return result;
        }
    }
}
