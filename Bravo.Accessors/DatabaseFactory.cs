using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using Bravo.Common.Utilities;

namespace Bravo.Accessors
{
    public static class DatabaseFactory
    {
        public static T Create<T>() where T : DbContext, new()
        {
            System.Data.Entity.Database.SetInitializer<T>(null);

            EntityConnectionStringBuilder connectionString = new EntityConnectionStringBuilder();
            connectionString.Provider = "System.Data.SqlClient";
            connectionString.ProviderConnectionString = ConfigurationHelper.ConnectionStrings.BravoPrimaryConnectionString;

            T ret = Activator.CreateInstance(typeof(T)) as T;
            ret.Database.Connection.ConnectionString = connectionString.ProviderConnectionString;

            return ret;
        }
    }
}
