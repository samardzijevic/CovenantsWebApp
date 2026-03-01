namespace Covenants.Scheduler
{
    /// <summary>
    /// Entry point for all background engines.
    /// Call Start() from Global.asax Application_Start
    /// and Stop() from Application_End.
    /// </summary>
    public static class AppScheduler
    {
        public static void Start()
        {
            SchedulerEngine.Start();
            NotificationEngine.Start();
        }

        public static void Stop()
        {
            SchedulerEngine.Stop();
            NotificationEngine.Stop();
        }
    }
}
