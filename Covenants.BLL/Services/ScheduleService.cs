using System;
using Covenants.Common;
using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.BLL.Services
{
    public class ScheduleService
    {
        private readonly ICovenantScheduleRepository _repo;
        private readonly HistoryService _history;

        public ScheduleService(ICovenantScheduleRepository repo, HistoryService history)
        {
            _repo    = repo;
            _history = history;
        }

        public CovenantSchedule GetActive(int covenantId) =>
            _repo.GetActiveByCovenantId(covenantId);

        /// <summary>
        /// Creates a new schedule for the covenant, deactivating any existing active schedule first.
        /// </summary>
        public Result<int> CreateOrReplace(CovenantSchedule schedule, string createdBy)
        {
            try
            {
                _repo.DeactivateAllForCovenant(schedule.CovenantId, createdBy);

                schedule.CreatedBy = createdBy;
                schedule.NextRunAt = DateHelper.CalculateNextRunDate(
                    schedule.ScheduleType,
                    schedule.StartDate,
                    null,
                    schedule.EndDate,
                    schedule.Interval,
                    schedule.DaysOfWeek,
                    schedule.DayOfMonth,
                    schedule.MonthOfYear);

                int id = _repo.Insert(schedule);

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

        /// <summary>Called by the scheduler engine after a follow-up is generated.</summary>
        public void UpdateAfterRun(CovenantSchedule schedule, string systemUser = "SYSTEM")
        {
            DateTime? next = DateHelper.CalculateNextRunDate(
                schedule.ScheduleType,
                schedule.StartDate,
                DateTime.UtcNow,
                schedule.EndDate,
                schedule.Interval,
                schedule.DaysOfWeek,
                schedule.DayOfMonth,
                schedule.MonthOfYear);

            _repo.UpdateAfterRun(schedule.Id, DateTime.UtcNow, next, systemUser);
        }
    }
}
