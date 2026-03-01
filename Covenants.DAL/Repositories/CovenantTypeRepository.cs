using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Covenants.Common;
using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.DAL.Repositories
{
    public class CovenantTypeRepository : ICovenantTypeRepository
    {
        public IEnumerable<CovenantType> GetAll()
        {
            var list = new List<CovenantType>();
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand("SELECT Id, Name, Description, IsActive, CreatedAt, CreatedBy FROM CovenantTypes ORDER BY Name", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public IEnumerable<CovenantType> GetAllActive()
        {
            var list = new List<CovenantType>();
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand("SELECT Id, Name, Description, IsActive, CreatedAt, CreatedBy FROM CovenantTypes WHERE IsActive = 1 ORDER BY Name", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public CovenantType GetById(int id)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand("SELECT Id, Name, Description, IsActive, CreatedAt, CreatedBy FROM CovenantTypes WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? Map(r) : null;
            }
        }

        public int Insert(CovenantType type)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                INSERT INTO CovenantTypes (Name, Description, IsActive, CreatedAt, CreatedBy)
                VALUES (@Name, @Description, @IsActive, GETUTCDATE(), @CreatedBy);
                SELECT SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@Name", type.Name);
                cmd.Parameters.AddWithValue("@Description", (object)type.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", type.IsActive);
                cmd.Parameters.AddWithValue("@CreatedBy", (object)type.CreatedBy ?? DBNull.Value);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(CovenantType type)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                UPDATE CovenantTypes
                SET Name = @Name, Description = @Description, IsActive = @IsActive
                WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Name", type.Name);
                cmd.Parameters.AddWithValue("@Description", (object)type.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", type.IsActive);
                cmd.Parameters.AddWithValue("@Id", type.Id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void SetActive(int id, bool isActive)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand("UPDATE CovenantTypes SET IsActive = @IsActive WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@IsActive", isActive);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static CovenantType Map(SqlDataReader r) => new CovenantType
        {
            Id          = r.GetInt32(r.GetOrdinal("Id")),
            Name        = r.GetString(r.GetOrdinal("Name")),
            Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
            IsActive    = r.GetBoolean(r.GetOrdinal("IsActive")),
            CreatedAt   = r.GetDateTime(r.GetOrdinal("CreatedAt")),
            CreatedBy   = r.IsDBNull(r.GetOrdinal("CreatedBy")) ? null : r.GetString(r.GetOrdinal("CreatedBy"))
        };
    }
}
