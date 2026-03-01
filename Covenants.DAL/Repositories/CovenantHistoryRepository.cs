using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Covenants.Common;
using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.DAL.Repositories
{
    public class CovenantHistoryRepository : ICovenantHistoryRepository
    {
        public IEnumerable<CovenantHistory> GetByCovenantId(int covenantId)
        {
            var list = new List<CovenantHistory>();
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                SELECT Id, CovenantId, Action, FieldName, OldValue, NewValue, ChangedAt, ChangedBy, Notes
                FROM CovenantHistory
                WHERE CovenantId = @CovenantId
                ORDER BY ChangedAt DESC", conn))
            {
                cmd.Parameters.AddWithValue("@CovenantId", covenantId);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public void Insert(CovenantHistory h)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                INSERT INTO CovenantHistory (CovenantId, Action, FieldName, OldValue, NewValue, ChangedAt, ChangedBy, Notes)
                VALUES (@CovenantId, @Action, @FieldName, @OldValue, @NewValue, GETUTCDATE(), @ChangedBy, @Notes)", conn))
            {
                cmd.Parameters.AddWithValue("@CovenantId", h.CovenantId);
                cmd.Parameters.AddWithValue("@Action",     h.Action);
                cmd.Parameters.AddWithValue("@FieldName",  (object)h.FieldName  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OldValue",   (object)h.OldValue   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NewValue",   (object)h.NewValue   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ChangedBy",  (object)h.ChangedBy  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Notes",      (object)h.Notes      ?? DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static CovenantHistory Map(SqlDataReader r) => new CovenantHistory
        {
            Id         = r.GetInt32(r.GetOrdinal("Id")),
            CovenantId = r.GetInt32(r.GetOrdinal("CovenantId")),
            Action     = r.GetString(r.GetOrdinal("Action")),
            FieldName  = r.IsDBNull(r.GetOrdinal("FieldName"))  ? null : r.GetString(r.GetOrdinal("FieldName")),
            OldValue   = r.IsDBNull(r.GetOrdinal("OldValue"))   ? null : r.GetString(r.GetOrdinal("OldValue")),
            NewValue   = r.IsDBNull(r.GetOrdinal("NewValue"))   ? null : r.GetString(r.GetOrdinal("NewValue")),
            ChangedAt  = r.GetDateTime(r.GetOrdinal("ChangedAt")),
            ChangedBy  = r.IsDBNull(r.GetOrdinal("ChangedBy"))  ? null : r.GetString(r.GetOrdinal("ChangedBy")),
            Notes      = r.IsDBNull(r.GetOrdinal("Notes"))      ? null : r.GetString(r.GetOrdinal("Notes"))
        };
    }
}
