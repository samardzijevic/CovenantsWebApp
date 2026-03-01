using System;
using System.Threading;
using Covenants.BLL.Services;
using Covenants.Common;
using Covenants.DAL.Repositories;

namespace Covenants.Scheduler
{
    // -----------------------------------------------------------------------
    // SCHEDULER ENGINE — AUTO FOLLOW-UP GENERATION
    // -----------------------------------------------------------------------
    // This class runs a background timer that fires every 60 seconds.
    // On each tick it:
    //   1. Queries the DB for schedules whose NextRunAt <= now (i.e., "due").
    //   2. For each due schedule, creates an auto-generated follow-up.
    //   3. Advances the schedule's NextRunAt to the next occurrence.
    //
    // THREADING CONCEPTS:
    //   System.Threading.Timer fires on a ThreadPool thread (not the UI thread).
    //   This means the Tick() method runs in the background — the web request
    //   being served is completely unaffected.
    //
    //   Important: Timer's callback runs even if a previous tick is still running.
    //   For safety we new up fresh repository/service instances inside each Tick()
    //   rather than sharing state across ticks.
    //
    // ERROR HANDLING:
    //   The try/catch inside Tick() is CRITICAL. If an unhandled exception escapes
    //   a ThreadPool callback in .NET 4.x it can crash the entire worker process
    //   (taking down the website). We log the error and carry on instead.
    //
    // TIMER PARAMETERS:
    //   new Timer(callback, state, dueTime, period)
    //     callback = method to call on each tick
    //     state    = arbitrary object passed to callback (we don't use it → null)
    //     dueTime  = delay before FIRST tick (SchedulerIntervalMs = 60,000ms = 60s)
    //     period   = interval between subsequent ticks (same 60s)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Fires every 60 seconds, finds due schedules, and auto-creates follow-ups.
    /// </summary>
    public static class SchedulerEngine
    {
        // _timer is the handle to the background timer.
        // 'static' because the engine itself is static (one instance for the whole app).
        private static Timer _timer;

        /// <summary>
        /// Starts the background timer. Called once from AppScheduler.Start().
        /// </summary>
        public static void Start()
        {
            // dueTime = SchedulerIntervalMs (60 seconds) → first tick after 60s
            // period  = SchedulerIntervalMs (60 seconds) → then every 60s thereafter
            _timer = new Timer(Tick, null, Constants.SchedulerIntervalMs, Constants.SchedulerIntervalMs);
        }

        /// <summary>
        /// Disposes the timer, stopping future ticks.
        /// The ?. (null-conditional) is safe in case Stop() is called before Start().
        /// </summary>
        public static void Stop()
        {
            _timer?.Dispose();
        }

        // ---------------------------------------------------------------
        // TICK — the work performed on every timer interval
        // ---------------------------------------------------------------
        private static void Tick(object state)
        {
            try
            {
                // Create fresh instances on every tick.
                // We don't reuse instances across ticks because SqlConnection is NOT
                // thread-safe and ADO.NET connection pooling handles the actual recycling.
                var scheduleRepo = new CovenantScheduleRepository();
                var followUpRepo = new CovenantFollowUpRepository();
                var historyRepo  = new CovenantHistoryRepository();

                // Wire up services (manual dependency injection — no IoC container)
                var historyService  = new HistoryService(historyRepo);
                var followUpService = new FollowUpService(followUpRepo, historyService);
                var scheduleService = new ScheduleService(scheduleRepo, historyService);

                // GetDueSchedules returns only active schedules where NextRunAt <= DateTime.UtcNow.
                // Using UTC consistently avoids timezone issues across servers.
                var dueSchedules = scheduleRepo.GetDueSchedules(DateTime.UtcNow);

                foreach (var schedule in dueSchedules)
                {
                    // 1. Create the follow-up (auto-generated, linked to this schedule)
                    followUpService.CreateAutoFollowUp(schedule);

                    // 2. Advance the schedule: sets LastRunAt=now, calculates new NextRunAt.
                    //    If NextRunAt comes back null (Once schedule or past EndDate),
                    //    the repository deactivates the schedule (IsActive=0).
                    scheduleService.UpdateAfterRun(schedule);
                }
            }
            catch (Exception ex)
            {
                // NEVER let an exception propagate out of a timer callback.
                // Trace.TraceError writes to the ASP.NET trace and Windows event log.
                // In production you'd replace this with a proper logging framework (NLog, Serilog).
                System.Diagnostics.Trace.TraceError($"[SchedulerEngine] Error: {ex}");
            }
        }
    }
}
