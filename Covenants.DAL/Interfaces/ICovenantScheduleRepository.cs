using System;
using System.Collections.Generic;
using Covenants.Models;

namespace Covenants.DAL.Interfaces
{
    public interface ICovenantScheduleRepository
    {
        CovenantSchedule GetById(int id);
        CovenantSchedule GetActiveByCovenantId(int covenantId);
        IEnumerable<CovenantSchedule> GetAllByCovenantId(int covenantId);
        int Insert(CovenantSchedule schedule);
        void Update(CovenantSchedule schedule);
        void Deactivate(int id, string updatedBy);
        void DeactivateAllForCovenant(int covenantId, string updatedBy);
        /// <summary>Returns all active schedules where NextRunAt &lt;= utcNow.</summary>
        IEnumerable<CovenantSchedule> GetDueSchedules(DateTime utcNow);
        void UpdateAfterRun(int id, DateTime lastRunAt, DateTime? nextRunAt, string updatedBy);
    }
}
