using Bravo.Accessors;
using Bravo.Common.Shared;
using Bravo.Engines;
using Bravo.Managers.Models;
using Bravo.Managers.Runs;

namespace Bravo.Managers
{
    public class ManagerFactory : FactoryBase
    {
        private AccessorFactory _accessorFactory;
        private EngineFactory _engineFactory;

        public ManagerFactory() : this(null, null)
        {
        }

        public ManagerFactory(AccessorFactory accessorFactory, EngineFactory engineFactory)
        {
            _accessorFactory = accessorFactory ?? new AccessorFactory();
            _engineFactory = engineFactory ?? new EngineFactory(_accessorFactory);

            AddType<IModelManager>(typeof(ModelManager));
            AddType<IRunManager>(typeof(RunManager));
        }

        public T CreateManager<T>() where T : class
        {
            return CreateManager<T>(null, null);
        }

        public T CreateManager<T>(AccessorFactory accessorFactory, EngineFactory engineFactory) where T : class
        {
            _accessorFactory = accessorFactory ?? _accessorFactory;
            _engineFactory = _engineFactory ?? new EngineFactory(_accessorFactory);

            T result = GetInstanceForType<T>();

            // configure the context and the accessor factory if the result is not a mock
            if (result is BaseManager)
            {
                (result as BaseManager).AccessorFactory = _accessorFactory;
                (result as BaseManager).EngineFactory = _engineFactory;
                (result as BaseManager).ManagerFactory = this;
            }

            return result;
        }
    }
}
