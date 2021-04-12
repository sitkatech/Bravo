using Bravo.Common.DataContracts.Models;

namespace Bravo.Accessors.FileIO
{
    public interface IModelFileAccessorFactory
    {
        IModelFileAccessor CreateModflowFileAccessor(Model model);

        IFileFormatter CreateFileFormatterAccessor(ModelFileAccessor modflowFileAccessor);
    }
}