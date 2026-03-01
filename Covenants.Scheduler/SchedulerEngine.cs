using System;
using System.Threading;
using Covenants.BLL.Services;
using Covenants.Common;
using Covenants.DAL.Repositories;

namespace Covenants.Scheduler
{
    /// <summary>
    /// Fires every 60 seconds, finds due schedules, and auto-creates follow-ups.
    /// </summary>
    public static class SchedulerEngine
    {
        private static Timer _timer;

        public static void Start()
        {
            _timer = new Timer(Tick, null, Constants.SchedulerIntervalMs, Constants.SchedulerIntervalMs);
        }

        public static void Stop()
        {
            _timer?.Dispose();
        }

        private static void Tick(object state)
        {
            try
            {
                var scheduleRepo = new CovenantScheduleRepository();
                var followUpRepo = new CovenantFollowUpRepository();
                var historyRepo  = new CovenantHistoryRepository();

                var historyService  = new HistoryService(historyRepo);
                var followUpService = new FollowUpService(followUpRepo, historyService);
                var scheduleService = new ScheduleService(scheduleRepo, historyService);

                var dueSchedules = scheduleRepo.GetDueSchedules(DateTime.UtcNow);

                foreach (var schedule in dueSchedules)
                {
                    followUpService.CreateAutoFollowUp(schedule);
                    scheduleService.UpdateAfterRun(schedule);
                }
            }
            catch (Exception ex)
            {
                // Log to Windows Event Log or a file — do not crash the web app
                System.Diagnostics.Trace.TraceError($"[SchedulerEngine] Error: {ex}");
            }
        }
    }
}
