﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Common.DataContracts.APIFunctionModels
{
    public class NotificationModel
    {
        public int? RunId { get; set; }

        public Exception Exception { get; set; }

        public bool IsSystemFailure { get; set; }
    }
}
