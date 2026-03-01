using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Covenants.Common;
using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.DAL.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private const string SelectAll = @"
            SELECT n.Id, n.CovenantId, c.Title AS CovenantTitle,
                   n.UserId, n.Type, n.Message, n.IsRead, n.ReadAt, n.CreatedAt, n.DismissedAt
            FROM Notifications n
            INNER JOIN Covenants c ON n.CovenantId = c.Id";

        public IEnumerable<Notification> GetByUserId(string userId)
        {
            var list = new List<Notification>();
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand($@"
                {SelectAll}
                WHERE (n.UserId = @UserId OR n.UserId IS NULL)
                  AND n.DismissedAt IS NULL
                ORDER BY n.IsRead ASC, n.CreatedAt DESC", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", (object)userId ?? DBNull.Value);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public int GetUnreadCount(string userId)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM Notifications
                WHERE (UserId = @UserId OR UserId IS NULL)
                  AND IsRead = 0
                  AND DismissedAt IS NULL", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", (object)userId ?? DBNull.Value);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public bool ExistsUnread(int covenantId, string userId, string type)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM Notifications
                WHERE CovenantId = @CovenantId
                  AND (UserId = @UserId OR UserId IS NULL)
                  AND Type = @Type
                  AND IsRead = 0
                  AND DismissedAt IS NULL", conn))
            {
                cmd.Parameters.AddWithValue("@CovenantId", covenantId);
                cmd.Parameters.AddWithValue("@UserId",     (object)userId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Type",       type);
                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public int Insert(Notification n)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                INSERT INTO Notifications (CovenantId, UserId, Type, Message, IsRead, CreatedAt)
                VALUES (@CovenantId, @UserId, @Type, @Message, 0, GETUTCDATE());
                SELECT SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@CovenantId", n.CovenantId);
                cmd.Parameters.AddWithValue("@UserId",     (object)n.UserId  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Type",       n.Type);
                cmd.Parameters.AddWithValue("@Message",    n.Message);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void MarkRead(int id)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand("UPDATE Notifications SET IsRead = 1, ReadAt = GETUTCDATE() WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void MarkAllRead(string userId)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                UPDATE Notifications SET IsRead = 1, ReadAt = GETUTCDATE()
                WHERE (UserId = @UserId OR UserId IS NULL) AND IsRead = 0", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", (object)userId ?? DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Dismiss(int id)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand("UPDATE Notifications SET DismissedAt = GETUTCDATE() WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static Notification Map(SqlDataReader r) => new Notification
        {
            Id             = r.GetInt32(r.GetOrdinal("Id")),
            CovenantId     = r.GetInt32(r.GetOrdinal("CovenantId")),
            CovenantTitle  = r.IsDBNull(r.GetOrdinal("CovenantTitle")) ? null : r.GetString(r.GetOrdinal("CovenantTitle")),
            UserId         = r.IsDBNull(r.GetOrdinal("UserId"))        ? null : r.GetString(r.GetOrdinal("UserId")),
            Type           = r.GetString(r.GetOrdinal("Type")),
            Message        = r.GetString(r.GetOrdinal("Message")),
            IsRead         = r.GetBoolean(r.GetOrdinal("IsRead")),
            ReadAt         = r.IsDBNull(r.GetOrdinal("ReadAt"))        ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("ReadAt")),
            CreatedAt      = r.GetDateTime(r.GetOrdinal("CreatedAt")),
            DismissedAt    = r.IsDBNull(r.GetOrdinal("DismissedAt"))   ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("DismissedAt"))
        };
    }
}
