using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Covenants.Common;
using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.DAL.Repositories
{
    public class CovenantScheduleRepository : ICovenantScheduleRepository
    {
        private const string SelectAll = @"
            SELECT Id, CovenantId, ScheduleType, [Interval], DaysOfWeek,
                   StartDate, EndDate, DayOfMonth, MonthOfYear, IsActive,
                   LastRunAt, NextRunAt, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
            FROM CovenantSchedules";

        public CovenantSchedule GetById(int id)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(SelectAll + " WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? Map(r) : null;
            }
        }

        public CovenantSchedule GetActiveByCovenantId(int covenantId)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(SelectAll + " WHERE CovenantId = @CovenantId AND IsActive = 1", conn))
            {
                cmd.Parameters.AddWithValue("@CovenantId", covenantId);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? Map(r) : null;
            }
        }

        public IEnumerable<CovenantSchedule> GetAllByCovenantId(int covenantId)
        {
            var list = new List<CovenantSchedule>();
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(SelectAll + " WHERE CovenantId = @CovenantId ORDER BY CreatedAt DESC", conn))
            {
                cmd.Parameters.AddWithValue("@CovenantId", covenantId);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public int Insert(CovenantSchedule s)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                INSERT INTO CovenantSchedules
                    (CovenantId, ScheduleType, [Interval], DaysOfWeek,
                     StartDate, EndDate, DayOfMonth, MonthOfYear,
                     IsActive, NextRunAt, CreatedAt, CreatedBy)
                VALUES
                    (@CovenantId, @ScheduleType, @Interval, @DaysOfWeek,
                     @StartDate, @EndDate, @DayOfMonth, @MonthOfYear,
                     1, @NextRunAt, GETUTCDATE(), @CreatedBy);
                SELECT SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@CovenantId",   s.CovenantId);
                cmd.Parameters.AddWithValue("@ScheduleType", s.ScheduleType);
                cmd.Parameters.AddWithValue("@Interval",     s.Interval < 1 ? 1 : s.Interval);
                cmd.Parameters.AddWithValue("@DaysOfWeek",   (object)s.DaysOfWeek  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StartDate",    s.StartDate);
                cmd.Parameters.AddWithValue("@EndDate",      (object)s.EndDate     ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DayOfMonth",   (object)s.DayOfMonth  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MonthOfYear",  (object)s.MonthOfYear ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NextRunAt",    (object)s.NextRunAt   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy",    (object)s.CreatedBy   ?? DBNull.Value);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(CovenantSchedule s)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                UPDATE CovenantSchedules
                SET ScheduleType = @ScheduleType,
                    [Interval]   = @Interval,
                    DaysOfWeek   = @DaysOfWeek,
                    StartDate    = @StartDate,
                    EndDate      = @EndDate,
                    DayOfMonth   = @DayOfMonth,
                    MonthOfYear  = @MonthOfYear,
                    NextRunAt    = @NextRunAt,
                    UpdatedAt    = GETUTCDATE(),
                    UpdatedBy    = @UpdatedBy
                WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@ScheduleType", s.ScheduleType);
                cmd.Parameters.AddWithValue("@Interval",     s.Interval < 1 ? 1 : s.Interval);
                cmd.Parameters.AddWithValue("@DaysOfWeek",   (object)s.DaysOfWeek  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StartDate",    s.StartDate);
                cmd.Parameters.AddWithValue("@EndDate",      (object)s.EndDate     ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DayOfMonth",   (object)s.DayOfMonth  ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MonthOfYear",  (object)s.MonthOfYear ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NextRunAt",    (object)s.NextRunAt   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UpdatedBy",    (object)s.UpdatedBy   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id",           s.Id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Deactivate(int id, string updatedBy)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                UPDATE CovenantSchedules SET IsActive = 0,
                    UpdatedAt = GETUTCDATE(), UpdatedBy = @UpdatedBy
                WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@UpdatedBy", (object)updatedBy ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeactivateAllForCovenant(int covenantId, string updatedBy)
        {
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                UPDATE CovenantSchedules SET IsActive = 0,
                    UpdatedAt = GETUTCDATE(), UpdatedBy = @UpdatedBy
                WHERE CovenantId = @CovenantId AND IsActive = 1", conn))
            {
                cmd.Parameters.AddWithValue("@UpdatedBy", (object)updatedBy ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CovenantId", covenantId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<CovenantSchedule> GetDueSchedules(DateTime utcNow)
        {
            var list = new List<CovenantSchedule>();
            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(SelectAll + " WHERE IsActive = 1 AND NextRunAt <= @UtcNow", conn))
            {
                cmd.Parameters.AddWithValue("@UtcNow", utcNow);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        public void UpdateAfterRun(int id, DateTime lastRunAt, DateTime? nextRunAt, string updatedBy)
        {
            string sql = nextRunAt.HasValue
                ? @"UPDATE CovenantSchedules
                    SET LastRunAt = @LastRunAt, NextRunAt = @NextRunAt,
                        UpdatedAt = GETUTCDATE(), UpdatedBy = @UpdatedBy
                    WHERE Id = @Id"
                : @"UPDATE CovenantSchedules
                    SET LastRunAt = @LastRunAt, NextRunAt = NULL, IsActive = 0,
                        UpdatedAt = GETUTCDATE(), UpdatedBy = @UpdatedBy
                    WHERE Id = @Id";

            using (var conn = ConnectionFactory.Create())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@LastRunAt", lastRunAt);
                if (nextRunAt.HasValue)
                    cmd.Parameters.AddWithValue("@NextRunAt", nextRunAt.Value);
                cmd.Parameters.AddWithValue("@UpdatedBy", (object)updatedBy ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static CovenantSchedule Map(SqlDataReader r)
        {
            return new CovenantSchedule
            {
                Id           = r.GetInt32(r.GetOrdinal("Id")),
                CovenantId   = r.GetInt32(r.GetOrdinal("CovenantId")),
                ScheduleType = r.GetString(r.GetOrdinal("ScheduleType")),
                Interval     = r.IsDBNull(r.GetOrdinal("Interval"))     ? 1    : r.GetInt32(r.GetOrdinal("Interval")),
                DaysOfWeek   = r.IsDBNull(r.GetOrdinal("DaysOfWeek"))   ? null : r.GetString(r.GetOrdinal("DaysOfWeek")),
                StartDate    = r.GetDateTime(r.GetOrdinal("StartDate")),
                EndDate      = r.IsDBNull(r.GetOrdinal("EndDate"))      ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("EndDate")),
                DayOfMonth   = r.IsDBNull(r.GetOrdinal("DayOfMonth"))   ? (int?)null : r.GetInt32(r.GetOrdinal("DayOfMonth")),
                MonthOfYear  = r.IsDBNull(r.GetOrdinal("MonthOfYear"))  ? (int?)null : r.GetInt32(r.GetOrdinal("MonthOfYear")),
                IsActive     = r.GetBoolean(r.GetOrdinal("IsActive")),
                LastRunAt    = r.IsDBNull(r.GetOrdinal("LastRunAt"))    ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("LastRunAt")),
                NextRunAt    = r.IsDBNull(r.GetOrdinal("NextRunAt"))    ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("NextRunAt")),
                CreatedAt    = r.GetDateTime(r.GetOrdinal("CreatedAt")),
                CreatedBy    = r.IsDBNull(r.GetOrdinal("CreatedBy"))    ? null : r.GetString(r.GetOrdinal("CreatedBy")),
                UpdatedAt    = r.IsDBNull(r.GetOrdinal("UpdatedAt"))    ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("UpdatedAt")),
                UpdatedBy    = r.IsDBNull(r.GetOrdinal("UpdatedBy"))    ? null : r.GetString(r.GetOrdinal("UpdatedBy"))
            };
        }
    }
}
