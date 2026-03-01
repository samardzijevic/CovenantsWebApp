namespace Covenants.Scheduler
{
    // -----------------------------------------------------------------------
    // APP SCHEDULER — FAÇADE FOR ALL BACKGROUND ENGINES
    // -----------------------------------------------------------------------
    // This class is the single entry point that Global.asax talks to.
    // It follows the FAÇADE PATTERN: instead of the caller knowing about
    // SchedulerEngine AND NotificationEngine, it only needs to know AppScheduler.
    //
    // FAÇADE PATTERN:
    //   A simple interface that hides the complexity of multiple subsystems.
    //   "Start everything" and "Stop everything" are the only two operations
    //   the caller ever needs.
    //
    // HOW IT IS USED (Global.asax):
    //   Application_Start → AppScheduler.Start()   ← starts both engines
    //   Application_End   → AppScheduler.Stop()    ← stops both engines cleanly
    //
    // WHY IN-PROCESS (not a Windows Service)?
    //   A Windows Service would be more robust, but requires separate deployment.
    //   Running the scheduler inside the ASP.NET worker process is simpler —
    //   it starts when the web app starts, and stops when it stops.
    //   Trade-off: if IIS recycles the app pool, the timers restart too.
    //   For this application that trade-off is acceptable.
    // -----------------------------------------------------------------------

    /// <summary>
    /// Entry point for all background engines.
    /// Call Start() from Global.asax Application_Start
    /// and Stop() from Application_End.
    /// </summary>
    public static class AppScheduler
    {
        /// <summary>
        /// Starts the SchedulerEngine (creates follow-ups every 60s)
        /// and the NotificationEngine (checks deadlines every 60min).
        /// </summary>
        public static void Start()
        {
            SchedulerEngine.Start();
            NotificationEngine.Start();
        }

        /// <summary>
        /// Gracefully stops both engines by disposing their timers.
        /// Called when IIS is shutting down the application pool.
        /// </summary>
        public static void Stop()
        {
            SchedulerEngine.Stop();
            NotificationEngine.Stop();
        }
    }
}
