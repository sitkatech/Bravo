using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bravo.Common.DataContracts.Models;
using Bravo.Common.DataContracts.Runs;

namespace Bravo.Accessors.FileIO
{
    class ModelFileAccessorFactory : IModelFileAccessorFactory
    {
        public IFileFormatter CreateFileFormatterAccessor(ModelFileAccessor modflowFileAccessor)
        {
            switch (modflowFileAccessor.FileFormat)
            {
                case FileFormat.Delimited:
                    return new DelimitedFileFormatter(modflowFileAccessor);
                case FileFormat.FixedWidth:
                    return new FixedWidthFileFormatter(modflowFileAccessor);
                case FileFormat.ModflowSixStructured:
                    return new ModflowSixFileFormatter(modflowFileAccessor);
                case FileFormat.ModflowSixUnstructured:
                    return new ModflowSixFileFormatter(modflowFileAccessor);
                default:
                    throw new Exception("Unknown file format");
            }
        }

        public IModelFileAccessor CreateModflowFileAccessor(Model model)
        {
            if (model.IsModflowModel)
            {
                var modelType = (IsStructured: ModelFileAccessor.IsStructuredFile(model), IsModFlow6: ModelFileAccessor.IsModFlow6(model));
                if (modelType.IsStructured)
                {
                    return modelType.IsModFlow6 ? (IModelFileAccessor)new StructuredModflowSixFileAccessor(model) : new StructuredModflowFileAccessor(model);
                }
                else
                {
                    return modelType.IsModFlow6 ? (IModelFileAccessor)new UnstructuredModflowSixFileAccessor(model) : new UnstructuredModflowFileAccessor(model);
                }
            }
            else // Modpath is structurned for now
            {
                return new StructuredModflowFileAccessor(model);
            }
        }
    }
}