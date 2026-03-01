using System;
using System.Threading;
using Covenants.BLL.Services;
using Covenants.Common;
using Covenants.DAL.Repositories;

namespace Covenants.Scheduler
{
    // -----------------------------------------------------------------------
    // NOTIFICATION ENGINE — APPROACHING DEADLINE ALERTS
    // -----------------------------------------------------------------------
    // This engine runs every 60 minutes and creates in-app notifications for
    // covenants whose ProcessingDate is within the next 7 days (configurable
    // via Constants.NotificationDaysThreshold).
    //
    // WHY EVERY HOUR instead of every 60 seconds (like SchedulerEngine)?
    //   Notification checks hit the database but produce no time-critical work.
    //   A 60-minute window is accurate enough for deadline alerts.
    //   Running it more often would waste resources without user benefit.
    //
    // IDEMPOTENCY (running the same thing many times safely):
    //   The engine fires 24 times per day. Without deduplication, one 7-day
    //   window would create 24 × 7 = 168 notifications for a single covenant.
    //   NotificationService.Create() calls ExistsUnread() first — if an unread
    //   notification already exists for that covenant+type, it skips insertion.
    //   This makes the engine safe to run as many times as needed.
    //
    // dueTime = 0 (fires IMMEDIATELY on app start) so users see notifications
    // right away rather than waiting 60 minutes after the web app restarts.
    //
    // UserId = null means the notification is broadcast to ALL users.
    // If you add per-user permissions later, this is where you'd filter.
    // -----------------------------------------------------------------------

    /// <summary>
    /// Fires every 60 minutes, checks for covenants with approaching ProcessingDate,
    /// and creates in-app notifications if none already exist.
    /// </summary>
    public static class NotificationEngine
    {
        private static Timer _timer;

        /// <summary>
        /// Starts the notification timer. dueTime=0 means fire immediately on first start.
        /// </summary>
        public static void Start()
        {
            // dueTime = 0 → fires right away on application start
            // period  = NotificationIntervalMs (3,600,000 ms = 60 minutes)
            _timer = new Timer(Tick, null, 0, Constants.NotificationIntervalMs);
        }

        /// <summary>Stops the timer on application shutdown.</summary>
        public static void Stop()
        {
            _timer?.Dispose();
        }

        // ---------------------------------------------------------------
        // TICK — runs every 60 minutes on a ThreadPool thread
        // ---------------------------------------------------------------
        private static void Tick(object state)
        {
            try
            {
                // Fresh instances per tick — same pattern as SchedulerEngine
                var covenantRepo     = new CovenantRepository();
                var notificationRepo = new NotificationRepository();
                var notificationSvc  = new NotificationService(notificationRepo);

                // GetApproachingProcessingDate returns only non-deleted, non-completed
                // covenants whose ProcessingDate falls in the next N days from now (UTC).
                var approaching = covenantRepo.GetApproachingProcessingDate(Constants.NotificationDaysThreshold);

                foreach (var covenant in approaching)
                {
                    // Calculate how many full days remain until the deadline.
                    // TotalDays gives a fractional double; (int) truncates toward zero.
                    int daysLeft = (int)(covenant.ProcessingDate - DateTime.UtcNow).TotalDays;

                    // Build a human-readable message shown in the notification list.
                    // :d is the short date format (e.g. "01/31/2025" or "31/01/2025" depending on locale).
                    string message = $"Covenant '{covenant.Title}' processing date is in {daysLeft} day(s) ({covenant.ProcessingDate:d}).";

                    // userId = null → "broadcast" — NotificationService stores a row
                    // with UserId=NULL which the notification list shows to all users.
                    // ExistsUnread() inside Create() prevents duplicates.
                    notificationSvc.Create(
                        covenant.Id,
                        userId:  null,
                        type:    Constants.NotificationTypes.ProcessingDateApproaching,
                        message: message);
                }
            }
            catch (Exception ex)
            {
                // Same pattern as SchedulerEngine — never crash the worker process
                System.Diagnostics.Trace.TraceError($"[NotificationEngine] Error: {ex}");
            }
        }
    }
}
