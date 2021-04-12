using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Common.DataContracts.Files
{
    public class FileModel
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public DateTime ModDate { get; set; }
    }
}
