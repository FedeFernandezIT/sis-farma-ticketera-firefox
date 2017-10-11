using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lector.Sharp.Wpf.Models
{
    public partial class SisFarmaEntities : DbContext
    {
        public SisFarmaEntities(string server, string catalog) 
            : this()
        {
        }

        private static string BuildConnectionString(string dataSource, string database)
        {
            // Construcción de connectionString
            var connectionString = $"&quot;server={dataSource};user id=plector;password=Njmm_851;persistsecurityinfo=True;database={database}&quot;";
            var metadata = "res://*/Models.SisFarmaModel.csdl|res://*/Models.SisFarmaModel.ssdl|res://*/Models.SisFarmaModel.msl";
            var provider = "MySql.Data.MySqlClient";
            // Build the connection string from the provided datasource and database
            //String connString = @"data source=" + DataSource + ";initial catalog=" +
            //Database + ";integrated security=True;MultipleActiveResultSets=True;App=EntityFramework;";

            // Build the MetaData... feel free to copy/paste it from the connection string in the config file.
            EntityConnectionStringBuilder esb = new EntityConnectionStringBuilder();
            esb.Metadata = metadata;
            esb.Provider = provider;
            esb.ProviderConnectionString = connectionString;

            // Generate the full string and return it
            return esb.ToString();
        }
    }
    
}
