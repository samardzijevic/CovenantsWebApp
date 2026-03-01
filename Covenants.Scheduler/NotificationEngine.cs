using System;
using System.Threading;
using Covenants.BLL.Services;
using Covenants.Common;
using Covenants.DAL.Repositories;

namespace Covenants.Scheduler
{
    /// <summary>
    /// Fires every 60 minutes, checks for covenants with approaching ProcessingDate,
    /// and creates in-app notifications if none already exist.
    /// </summary>
    public static class NotificationEngine
    {
        private static Timer _timer;

        public static void Start()
        {
            // Fire immediately on start, then every hour
            _timer = new Timer(Tick, null, 0, Constants.NotificationIntervalMs);
        }

        public static void Stop()
        {
            _timer?.Dispose();
        }

        private static void Tick(object state)
        {
            try
            {
                var covenantRepo      = new CovenantRepository();
                var notificationRepo  = new NotificationRepository();
                var notificationSvc   = new NotificationService(notificationRepo);

                var approaching = covenantRepo.GetApproachingProcessingDate(Constants.NotificationDaysThreshold);

                foreach (var covenant in approaching)
                {
                    int daysLeft = (int)(covenant.ProcessingDate - DateTime.UtcNow).TotalDays;
                    string message = $"Covenant '{covenant.Title}' processing date is in {daysLeft} day(s) ({covenant.ProcessingDate:d}).";

                    // UserId = null → broadcast to all users
                    notificationSvc.Create(
                        covenant.Id,
                        userId:  null,
                        type:    Constants.NotificationTypes.ProcessingDateApproaching,
                        message: message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"[NotificationEngine] Error: {ex}");
            }
        }
    }
}
