using System.Configuration;
using System.Data.SqlClient;

namespace Covenants.Common
{
    public static class ConnectionFactory
    {
        private const string ConnectionName = "CovenantsDB";

        public static SqlConnection Create()
        {
            string cs = ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString;
            return new SqlConnection(cs);
        }
    }
}
