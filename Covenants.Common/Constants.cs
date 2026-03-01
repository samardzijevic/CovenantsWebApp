namespace Covenants.Common
{
    public static class Constants
    {
        /// <summary>Number of days before ProcessingDate to start sending notifications.</summary>
        public const int NotificationDaysThreshold = 7;

        /// <summary>Scheduler engine tick interval in milliseconds (60 seconds).</summary>
        public const int SchedulerIntervalMs = 60_000;

        /// <summary>Notification engine tick interval in milliseconds (60 minutes).</summary>
        public const int NotificationIntervalMs = 3_600_000;

        public static class ScheduleTypes
        {
            public const string Once    = "Once";
            public const string Daily   = "Daily";
            public const string Weekly  = "Weekly";
            public const string Monthly = "Monthly";
            public const string Yearly  = "Yearly";
        }

        public static class CovenantStatuses
        {
            public const string Active    = "Active";
            public const string Pending   = "Pending";
            public const string Completed = "Completed";
        }

        public static class FollowUpStatuses
        {
            public const string Pending       = "Pending";
            public const string InProgress    = "InProgress";
            public const string Completed     = "Completed";
            public const string CompletedLate = "CompletedLate";
            public const string Cancelled     = "Cancelled";
        }

        public static class HistoryActions
        {
            public const string Created        = "Created";
            public const string Updated        = "Updated";
            public const string Deleted        = "Deleted";
            public const string Restored       = "Restored";
            public const string FollowUpAdded  = "FollowUpAdded";
            public const string ScheduleChanged = "ScheduleChanged";
        }

        public static class NotificationTypes
        {
            public const string ProcessingDateApproaching = "ProcessingDateApproaching";
            public const string FollowUpDue               = "FollowUpDue";
            public const string ScheduleTriggered         = "ScheduleTriggered";
        }
    }
}
