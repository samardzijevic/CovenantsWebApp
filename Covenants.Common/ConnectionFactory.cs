using System.Configuration;
using System.Data.SqlClient;

namespace Covenants.Common
{
    // -----------------------------------------------------------------------
    // CONNECTION FACTORY
    // -----------------------------------------------------------------------
    // This class has one job: create a new SqlConnection using the connection
    // string stored in Web.config.
    //
    // Why a factory instead of writing "new SqlConnection(...)" everywhere?
    //   1. Single place to change the connection string key if needed.
    //   2. Every repository calls ConnectionFactory.Create() — if you later
    //      want to add connection pooling configuration or logging, you only
    //      change it here.
    //
    // IMPORTANT: This returns a NEW connection each time — it does NOT keep
    // a single shared connection open. ADO.NET has built-in connection pooling,
    // so "new SqlConnection" is cheap — the pool reuses physical connections.
    //
    // Usage pattern in every repository:
    //   using (var conn = ConnectionFactory.Create())   // create
    //   using (var cmd  = new SqlCommand(sql, conn))    // attach command
    //   {
    //       conn.Open();          // open (actually borrows from pool)
    //       cmd.ExecuteReader();  // run query
    //   }                         // 'using' closes + returns to pool automatically
    // -----------------------------------------------------------------------

    public static class ConnectionFactory
    {
        // The name must match the <add name="CovenantsDB" ...> in Web.config.
        private const string ConnectionName = "CovenantsDB";

        /// <summary>
        /// Returns a new, closed SqlConnection configured for CovenantsDB.
        /// The caller is responsible for opening and disposing it (use 'using').
        /// </summary>
        public static SqlConnection Create()
        {
            // ConfigurationManager reads from Web.config <connectionStrings> section.
            string cs = ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString;
            return new SqlConnection(cs);
        }
    }
}
