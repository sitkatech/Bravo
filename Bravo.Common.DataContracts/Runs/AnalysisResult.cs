﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Common.DataContracts.Runs
{
    public class AnalysisResult
    {
        public bool Success { get; set; }

        public string ConsoleOutput { get; set; }
    }
}
