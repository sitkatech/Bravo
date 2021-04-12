﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Common.DataContracts.APIFunctionModels
{
    public class RetrieveResultModel
    {
        public int? RunId { get; set; }

        public string FileName { get; set; }

        public string FileDate { get; set; }

        public string SubType { get; set; }

        public string FileExtension { get; set; }
    }
}
