using Bravo.Accessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Engines
{
    public abstract class BaseEngine
    {
        public AccessorFactory AccessorFactory { get; set; }
    }
}
