using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Covenants.Common;
using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.DAL.Repositories
{
    public class CovenantRepository : ICovenantRepository
    {
        private const string SelectColumns = @"
            c.Id, c.CovenantTypeId, ct.Name AS CovenantTypeName,
            c.Title, c.Description, c.ProcessingDate,
            c.Value, c.Currency, c.Status,
            c.IsDeleted, c.DeletedAt, c.DeletedBy,
            c.CreatedAt, c.CreatedBy, c.UpdatedAt, c.UpdatedBy";

        private const string FromJoin = @"
            FROM Covenants c
            INNER JOIN CovenantTypes ct ON c.CovenantTypeId = ct.Id";

        public IEnumerable<Covenant> GetAll()
        {
            var list = new List<Covenant>();
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand($"SELECT {SelectColumns} {FromJoin} ORDER BY c.CreatedAt DESC", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public IEnumerable<Covenant> GetActive()
        {
            var list = new List<Covenant>();
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand($"SELECT {SelectColumns} {FromJoin} WHERE c.IsDeleted = 0 AND c.Status != 'Completed' ORDER BY c.ProcessingDate ASC", conn))
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
            using (var cmd = new SqlCommand($"SELECT {SelectColumns} {FromJoin} WHERE c.IsDeleted = 0 AND c.Status = 'Completed' ORDER BY c.UpdatedAt DESC", conn))
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
                    return r.Read() ? Map(r) : null;
            }
        }

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
                cmd.Parameters.AddWithValue("@CovenantTypeId", c.CovenantTypeId);
                cmd.Parameters.AddWithValue("@Title", c.Title);
                cmd.Parameters.AddWithValue("@Description", (object)c.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProcessingDate", c.ProcessingDate);
                cmd.Parameters.AddWithValue("@Value", (object)c.Value ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Currency", (object)c.Currency ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", c.Status ?? Constants.CovenantStatuses.Active);
                cmd.Parameters.AddWithValue("@CreatedBy", (object)c.CreatedBy ?? DBNull.Value);
                conn.Open();
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
                    UpdatedAt      = GETUTCDATE(),
                    UpdatedBy      = @UpdatedBy
                WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@CovenantTypeId", c.CovenantTypeId);
                cmd.Parameters.AddWithValue("@Title", c.Title);
                cmd.Parameters.AddWithValue("@Description", (object)c.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProcessingDate", c.ProcessingDate);
                cmd.Parameters.AddWithValue("@Value", (object)c.Value ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Currency", (object)c.Currency ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", c.Status);
                cmd.Parameters.AddWithValue("@UpdatedBy", (object)c.UpdatedBy ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", c.Id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void SoftDelete(int id, string deletedBy)
        {
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

        public IEnumerable<Covenant> GetApproachingProcessingDate(int daysThreshold)
        {
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

        private static Covenant Map(SqlDataReader r)
        {
            int ordValue    = r.GetOrdinal("Value");
            int ordCurrency = r.GetOrdinal("Currency");
            return new Covenant
            {
                Id               = r.GetInt32(r.GetOrdinal("Id")),
                CovenantTypeId   = r.GetInt32(r.GetOrdinal("CovenantTypeId")),
                CovenantTypeName = r.GetString(r.GetOrdinal("CovenantTypeName")),
                Title            = r.GetString(r.GetOrdinal("Title")),
                Description      = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
                ProcessingDate   = r.GetDateTime(r.GetOrdinal("ProcessingDate")),
                Value            = r.IsDBNull(ordValue) ? (decimal?)null : r.GetDecimal(ordValue),
                Currency         = r.IsDBNull(ordCurrency) ? null : r.GetString(ordCurrency),
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
