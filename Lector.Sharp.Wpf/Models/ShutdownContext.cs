using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lector.Sharp.Wpf.Models
{
    public class ShutdownContext : DbContext
    {
        public ShutdownContext(string server, string catalog) 
            : base(BuildConnectionString(server, catalog))
        {
        }

        private static string BuildConnectionString(string dataSource, string database)
        {
            // Construcción de connectionString
            //var connectionString = $"server={dataSource};user id=fisiotes_admin;password=123456;persistsecurityinfo=True;database={database}; Allow Zero Datetime=True; Convert Zero Datetime=True";
            var connectionString = $"server={dataSource};user id=plector;password=Njmm_851;persistsecurityinfo=True;database={database}; Allow Zero Datetime=True; Convert Zero Datetime=True";
            var metadata = "res://*/Models.SisFarmaModel.csdl|res://*/Models.SisFarmaModel.ssdl|res://*/Models.SisFarmaModel.msl";
            var provider = "MySql.Data.MySqlClient";

            // Build the MetaData... feel free to copy/paste it from the connection string in the config file.
            EntityConnectionStringBuilder esb = new EntityConnectionStringBuilder();
            esb.Metadata = metadata;
            esb.Provider = provider;
            esb.ProviderConnectionString = connectionString;

            // Generate the full string and return it
            var str = esb.ToString();
            return str;
        }
    }
}
