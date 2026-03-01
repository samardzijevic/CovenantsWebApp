using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Covenants.Common;
using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.DAL.Repositories
{
    // -----------------------------------------------------------------------
    // REPOSITORY — THE DATA ACCESS LAYER (DAL)
    // -----------------------------------------------------------------------
    // This class contains ALL SQL for the Covenants table. No SQL should appear
    // anywhere else (not in services, not in pages).
    //
    // ADO.NET PATTERN used in every method:
    //
    //   using (var conn = ConnectionFactory.Create())      // 1. create connection
    //   using (var cmd  = new SqlCommand(sql, conn))       // 2. create command
    //   {
    //       cmd.Parameters.AddWithValue("@Param", value);  // 3. add parameters
    //       conn.Open();                                    // 4. open connection
    //       var reader = cmd.ExecuteReader();               // 5. run query
    //       while (reader.Read()) { ... Map(reader) ... }  // 6. read rows
    //   }                                                  // 7. auto-close (using)
    //
    // WHY PARAMETERS (cmd.Parameters.AddWithValue)?
    //   Never concatenate user input into SQL strings! That causes SQL Injection.
    //   Parameters are sent separately — the database treats them as DATA not CODE.
    //   Example of WRONG (vulnerable): "WHERE Id = " + id
    //   Example of RIGHT (safe):       "WHERE Id = @Id"  + cmd.Parameters.AddWithValue("@Id", id)
    //
    // -----------------------------------------------------------------------

    public class CovenantRepository : ICovenantRepository
    {
        // Reusable SELECT column list — defined once to avoid repeating it in every query.
        // We JOIN CovenantTypes so that CovenantTypeName is available without a second trip to the DB.
        private const string SelectColumns = @"
            c.Id, c.CovenantTypeId, ct.Name AS CovenantTypeName,
            c.Title, c.Description, c.ProcessingDate,
            c.Value, c.Currency, c.Status,
            c.IsDeleted, c.DeletedAt, c.DeletedBy,
            c.CreatedAt, c.CreatedBy, c.UpdatedAt, c.UpdatedBy";

        // The FROM + JOIN is also reused — one covenant always has one type.
        private const string FromJoin = @"
            FROM Covenants c
            INNER JOIN CovenantTypes ct ON c.CovenantTypeId = ct.Id";

        // ---------------------------------------------------------------
        // READ OPERATIONS
        // ---------------------------------------------------------------

        public IEnumerable<Covenant> GetAll()
        {
            // Returns everything including soft-deleted records.
            // The UI layer decides what to show/hide (e.g. greyed-out deleted rows).
            var list = new List<Covenant>();
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand($"SELECT {SelectColumns} {FromJoin} ORDER BY c.CreatedAt DESC", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r)); // Map() converts each row to a Covenant object
            }
            return list;
        }

        public IEnumerable<Covenant> GetActive()
        {
            // Active = not deleted AND not yet completed.
            // Sorted by ProcessingDate so the most urgent appears at the top.
            var list = new List<Covenant>();
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(
                $"SELECT {SelectColumns} {FromJoin} WHERE c.IsDeleted = 0 AND c.Status != 'Completed' ORDER BY c.ProcessingDate ASC", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public IEnumerable<Covenant> GetCompleted()
        {
            var list = new List<Covenant>();
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(
                $"SELECT {SelectColumns} {FromJoin} WHERE c.IsDeleted = 0 AND c.Status = 'Completed' ORDER BY c.UpdatedAt DESC", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public Covenant GetById(int id)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand($"SELECT {SelectColumns} {FromJoin} WHERE c.Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    // r.Read() returns true if a row was found; Map() converts it.
                    // If no row found, returns null — the caller must handle null.
                    return r.Read() ? Map(r) : null;
            }
        }

        public IEnumerable<Covenant> GetApproachingProcessingDate(int daysThreshold)
        {
            // DATEADD(DAY, 7, GETUTCDATE()) = "now + 7 days" — used by notification engine.
            var list = new List<Covenant>();
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand($@"
                SELECT {SelectColumns} {FromJoin}
                WHERE c.IsDeleted = 0
                  AND c.Status != 'Completed'
                  AND c.ProcessingDate > GETUTCDATE()
                  AND c.ProcessingDate <= DATEADD(DAY, @Days, GETUTCDATE())
                ORDER BY c.ProcessingDate ASC", conn))
            {
                cmd.Parameters.AddWithValue("@Days", daysThreshold);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        // ---------------------------------------------------------------
        // WRITE OPERATIONS
        // ---------------------------------------------------------------

        public int Insert(Covenant c)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                INSERT INTO Covenants
                    (CovenantTypeId, Title, Description, ProcessingDate, Value, Currency, Status, IsDeleted, CreatedAt, CreatedBy)
                VALUES
                    (@CovenantTypeId, @Title, @Description, @ProcessingDate, @Value, @Currency, @Status, 0, GETUTCDATE(), @CreatedBy);
                SELECT SCOPE_IDENTITY();", conn))
            {
                // (object)c.Description ?? DBNull.Value — this pattern handles nullable strings.
                // SQL Server cannot receive C# null — it needs DBNull.Value for NULL columns.
                // The cast to (object) is needed because ?? compares types; DBNull.Value is object.
                cmd.Parameters.AddWithValue("@CovenantTypeId", c.CovenantTypeId);
                cmd.Parameters.AddWithValue("@Title",          c.Title);
                cmd.Parameters.AddWithValue("@Description",    (object)c.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProcessingDate", c.ProcessingDate);
                cmd.Parameters.AddWithValue("@Value",          (object)c.Value       ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Currency",       (object)c.Currency    ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status",         c.Status ?? Constants.CovenantStatuses.Active);
                cmd.Parameters.AddWithValue("@CreatedBy",      (object)c.CreatedBy   ?? DBNull.Value);
                conn.Open();

                // ExecuteScalar() runs the query and returns the first column of the first row.
                // SELECT SCOPE_IDENTITY() returns the Id that was just auto-generated by IDENTITY(1,1).
                // Convert.ToInt32 because SCOPE_IDENTITY returns decimal in SQL Server.
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(Covenant c)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                UPDATE Covenants
                SET CovenantTypeId = @CovenantTypeId,
                    Title          = @Title,
                    Description    = @Description,
                    ProcessingDate = @ProcessingDate,
                    Value          = @Value,
                    Currency       = @Currency,
                    Status         = @Status,
                    UpdatedAt      = GETUTCDATE(),  -- always stamp who/when changed it
                    UpdatedBy      = @UpdatedBy
                WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@CovenantTypeId", c.CovenantTypeId);
                cmd.Parameters.AddWithValue("@Title",          c.Title);
                cmd.Parameters.AddWithValue("@Description",    (object)c.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProcessingDate", c.ProcessingDate);
                cmd.Parameters.AddWithValue("@Value",          (object)c.Value       ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Currency",       (object)c.Currency    ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status",         c.Status);
                cmd.Parameters.AddWithValue("@UpdatedBy",      (object)c.UpdatedBy   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id",             c.Id);
                conn.Open();
                // ExecuteNonQuery() runs INSERT/UPDATE/DELETE — returns rows affected, which we ignore here.
                cmd.ExecuteNonQuery();
            }
        }

        public void SoftDelete(int id, string deletedBy)
        {
            // SOFT DELETE: we do NOT remove the row from the database.
            // Instead we flip IsDeleted = 1 and record who deleted it and when.
            // The row is invisible to most queries (they filter WHERE IsDeleted = 0)
            // but can be found and restored at any time.
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                UPDATE Covenants
                SET IsDeleted = 1, DeletedAt = GETUTCDATE(), DeletedBy = @DeletedBy
                WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@DeletedBy", (object)deletedBy ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Restore(int id, string restoredBy)
        {
            // RESTORE: undo the soft delete by clearing the deleted fields.
            // We also set UpdatedAt/UpdatedBy so history reflects the restoration.
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                UPDATE Covenants
                SET IsDeleted = 0, DeletedAt = NULL, DeletedBy = NULL,
                    UpdatedAt = GETUTCDATE(), UpdatedBy = @RestoredBy
                WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@RestoredBy", (object)restoredBy ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ---------------------------------------------------------------
        // MAP — converts one database row into a C# Covenant object
        // ---------------------------------------------------------------
        // This is called once per row inside while(reader.Read()).
        // reader.GetOrdinal("ColumnName") gets the column index by name — faster
        // than using the string name on every property access.
        // reader.IsDBNull(ordinal) must be checked before reading nullable columns
        // or you get an InvalidCastException at runtime.
        private static Covenant Map(SqlDataReader r)
        {
            int ordValue    = r.GetOrdinal("Value");
            int ordCurrency = r.GetOrdinal("Currency");
            return new Covenant
            {
                Id               = r.GetInt32(r.GetOrdinal("Id")),
                CovenantTypeId   = r.GetInt32(r.GetOrdinal("CovenantTypeId")),
                CovenantTypeName = r.GetString(r.GetOrdinal("CovenantTypeName")), // from the JOIN
                Title            = r.GetString(r.GetOrdinal("Title")),
                Description      = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
                ProcessingDate   = r.GetDateTime(r.GetOrdinal("ProcessingDate")),
                Value            = r.IsDBNull(ordValue)    ? (decimal?)null : r.GetDecimal(ordValue),
                Currency         = r.IsDBNull(ordCurrency) ? null            : r.GetString(ordCurrency),
                Status           = r.GetString(r.GetOrdinal("Status")),
                IsDeleted        = r.GetBoolean(r.GetOrdinal("IsDeleted")),
                DeletedAt        = r.IsDBNull(r.GetOrdinal("DeletedAt")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("DeletedAt")),
                DeletedBy        = r.IsDBNull(r.GetOrdinal("DeletedBy")) ? null : r.GetString(r.GetOrdinal("DeletedBy")),
                CreatedAt        = r.GetDateTime(r.GetOrdinal("CreatedAt")),
                CreatedBy        = r.IsDBNull(r.GetOrdinal("CreatedBy")) ? null : r.GetString(r.GetOrdinal("CreatedBy")),
                UpdatedAt        = r.IsDBNull(r.GetOrdinal("UpdatedAt")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("UpdatedAt")),
                UpdatedBy        = r.IsDBNull(r.GetOrdinal("UpdatedBy")) ? null : r.GetString(r.GetOrdinal("UpdatedBy"))
            };
        }
    }
}
