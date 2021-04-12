using Bravo.Common.Shared;
using Bravo.Accessors.Models;
using Bravo.Accessors.Runs;
using Bravo.Accessors.FileIO;
using Bravo.Accessors.Containers;
using Bravo.Accessors.Modflow;
using Bravo.Accessors.Queue;
using Bravo.Accessors.APIFunctions;

namespace Bravo.Accessors
{
    public class AccessorFactory : FactoryBase
    {
        public const string LocalFileAccessorKey = "Local";
        public const string RemoteFileAccessorKey = "Local";
        public AccessorFactory()
        {
            AddType<IModelAccessor>(typeof(ModelAccessor));
            AddType<IRunAccessor>(typeof(RunAccessor));
            AddType<IBlobFileAccessor>(typeof(BlobFileAccessor));
            AddType<IContainerAccessor>(typeof(ContainerAccessor));
            AddType<IModelFileAccessorFactory>(typeof(ModelFileAccessorFactory));
            AddType<IModflowAccessor>(typeof(ModflowAccessor));
            AddType<IQueueAccessor>(typeof(QueueAccessor));
            AddType<IAPIFunctionsAccessor>(typeof(APIFunctionsAccessor));
            AddType<IFileAccessor>(typeof(FileAccessor));
        }

        public T CreateAccessor<T>() where T : class
        {
            T result = base.GetInstanceForType<T>();

            return result;
        }

    }
}