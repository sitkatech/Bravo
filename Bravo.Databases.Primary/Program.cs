using Bravo.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Databases.Primary
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Environment.Exit(new DatabaseMigrator(args.Length > 0 ? args[0] : ConfigurationHelper.ConnectionStrings.BravoPrimaryConnectionString).RunMigrations());
        }
    }
}
