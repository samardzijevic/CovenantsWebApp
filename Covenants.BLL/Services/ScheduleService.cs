using System;
using Covenants.Common;
using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.BLL.Services
{
    // -----------------------------------------------------------------------
    // SCHEDULE SERVICE — BUSINESS RULES FOR COVENANT SCHEDULES
    // -----------------------------------------------------------------------
    // A CovenantSchedule controls WHEN the scheduler engine automatically
    // creates follow-ups. Each covenant can have at most ONE active schedule
    // at a time.
    //
    // Key business rules enforced here:
    //   1. When a new schedule is created, any previous active schedule for
    //      that covenant is first deactivated (CreateOrReplace).
    //   2. After a scheduled follow-up is generated, NextRunAt is recalculated
    //      and LastRunAt is stamped (UpdateAfterRun).
    //   3. "Once" schedules automatically deactivate after their single run
    //      because DateHelper returns null for NextRunAt once lastRunAt is set.
    //
    // The actual date math lives in DateHelper — this service just orchestrates
    // the calls to the repository and history writer.
    // -----------------------------------------------------------------------

    public class ScheduleService
    {
        private readonly ICovenantScheduleRepository _repo;
        private readonly HistoryService _history;

        // Both dependencies are injected — this class never creates "new Repository()"
        // itself, keeping it testable and decoupled from the database.
        public ScheduleService(ICovenantScheduleRepository repo, HistoryService history)
        {
            _repo    = repo;
            _history = history;
        }

        // Simple read-through: no business logic needed for reads.
        public CovenantSchedule GetActive(int covenantId) =>
            _repo.GetActiveByCovenantId(covenantId);

        /// <summary>
        /// Creates a new schedule for the covenant, deactivating any existing
        /// active schedule first.
        /// </summary>
        /// <param name="schedule">The new schedule to persist.</param>
        /// <param name="createdBy">Username of the person making the change.</param>
        public Result<int> CreateOrReplace(CovenantSchedule schedule, string createdBy)
        {
            try
            {
                // RULE: only one active schedule per covenant.
                // DeactivateAllForCovenant flips IsActive=0 on any existing rows.
                _repo.DeactivateAllForCovenant(schedule.CovenantId, createdBy);

                // Stamp who created it
                schedule.CreatedBy = createdBy;

                // Calculate the first NextRunAt.
                // lastRunAt = null means "has never run" → DateHelper returns startDate.
                schedule.NextRunAt = DateHelper.CalculateNextRunDate(
                    schedule.ScheduleType,
                    schedule.StartDate,
                    lastRunAt:   null,           // null = first time
                    schedule.EndDate,
                    schedule.Interval,
                    schedule.DaysOfWeek,
                    schedule.DayOfMonth,
                    schedule.MonthOfYear);

                int id = _repo.Insert(schedule);

                // Write a history row so the History tab shows "Schedule changed"
                // with the human-readable description (e.g. "Every 3 days, starting Jan 5").
                _history.Write(schedule.CovenantId, Constants.HistoryActions.ScheduleChanged, createdBy,
                    notes: string.Format("Schedule set: {0}, starting {1:d}.",
                        schedule.Description, schedule.StartDate));

                return Result<int>.Ok(id);
            }
            catch (Exception ex)
            {
                return Result<int>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Called by the scheduler engine AFTER it has created a follow-up for
        /// this schedule. Updates LastRunAt and recalculates NextRunAt.
        /// If NextRunAt comes back null (Once schedule, or past EndDate),
        /// the repository sets IsActive=0 automatically.
        /// </summary>
        /// <param name="schedule">The schedule that just fired.</param>
        /// <param name="systemUser">Username written to UpdatedBy (defaults to "SYSTEM").</param>
        public void UpdateAfterRun(CovenantSchedule schedule, string systemUser = "SYSTEM")
        {
            // Calculate when the schedule should fire NEXT.
            // lastRunAt = DateTime.UtcNow because we just ran it now.
            DateTime? next = DateHelper.CalculateNextRunDate(
                schedule.ScheduleType,
                schedule.StartDate,
                lastRunAt:   DateTime.UtcNow,   // just ran now
                schedule.EndDate,
                schedule.Interval,
                schedule.DaysOfWeek,
                schedule.DayOfMonth,
                schedule.MonthOfYear);

            // Persist: set LastRunAt = now, NextRunAt = next (could be null → engine deactivates)
            _repo.UpdateAfterRun(schedule.Id, DateTime.UtcNow, next, systemUser);
        }
    }
}
