using Bravo.Common.DataContracts.Runs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using Bravo.Common.DataContracts.Models;

namespace Bravo.Accessors.FileIO
{
    internal class UnstructuredModflowFileAccessor : ModelFileAccessor
    {
        public UnstructuredModflowFileAccessor(Model model) : base(model)
        {
        
        }

        private sealed class UnstructuredProportionMapper : LocationProportionMapper
        {
            public UnstructuredProportionMapper()
                : base(new[] { "node" })
            {
                Map(m => m.Location).Name("node");
            }
        }

        protected override Type LocationProportionMapperType => typeof(UnstructuredProportionMapper);
        protected override string DisFileKey => UnstructuredDisFileKey;
        protected override int NumberOfStressPeriodsColumnInDisFileIndex => 4;
        protected override int FlowToAquiferColumnInOutputIndex => 4;
        protected override int SegmentNumberColumnInOutputIndex => 1;
        protected override int ReachNumberColumnInOutputIndex => 2;                  
    }

    internal class UnstructuredLocationMapPositionRecord
    {
        public string Node { get; set; }
        public string WellPumpingNodes { get; set; }
    }

    internal class UnstructuredLocationZone
    {
        public string Node { get; set; }
        public string Zone { get; set; }
    }
}
