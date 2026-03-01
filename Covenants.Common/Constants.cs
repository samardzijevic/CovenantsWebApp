namespace Covenants.Common
{
    // -----------------------------------------------------------------------
    // CONSTANTS
    // -----------------------------------------------------------------------
    // Magic strings and numbers scattered through code are hard to maintain.
    // If you store a status as "Active" in 10 places and then rename it to
    // "Enabled", you have to hunt down every occurrence.
    //
    // Solution: define all fixed values in ONE place.
    //   - Use 'const string' for string values — they become compile-time
    //     constants (zero runtime cost, IDE finds all usages).
    //   - Nest related constants in inner static classes for organisation.
    //
    // These strings MUST match exactly what is stored in the database columns.
    // -----------------------------------------------------------------------

    public static class Constants
    {
        /// <summary>
        /// How many days before the ProcessingDate we start sending notifications.
        /// E.g. 7 means "notify 1 week before the deadline".
        /// </summary>
        public const int NotificationDaysThreshold = 7;

        /// <summary>How often the SchedulerEngine checks for due schedules (milliseconds).</summary>
        public const int SchedulerIntervalMs = 60_000;       // 60 seconds

        /// <summary>How often the NotificationEngine checks for approaching dates (milliseconds).</summary>
        public const int NotificationIntervalMs = 3_600_000; // 60 minutes

        // -------------------------------------------------------------------
        // Covenant schedule types — stored in CovenantSchedules.ScheduleType
        // -------------------------------------------------------------------
        public static class ScheduleTypes
        {
            public const string Once    = "Once";    // runs exactly one time
            public const string Daily   = "Daily";   // every N days
            public const string Weekly  = "Weekly";  // every N weeks on selected days
            public const string Monthly = "Monthly"; // every N months on a specific day
            public const string Yearly  = "Yearly";  // every N years on a specific month+day
        }

        // -------------------------------------------------------------------
        // Covenant status values — stored in Covenants.Status
        // -------------------------------------------------------------------
        public static class CovenantStatuses
        {
            public const string Active    = "Active";    // covenant is active and ongoing
            public const string Pending   = "Pending";   // not yet started or under review
            public const string Completed = "Completed"; // finished; shown in the completed panel
        }

        // -------------------------------------------------------------------
        // Follow-up status values — stored in CovenantFollowUps.Status
        // The lifecycle is: Pending → InProgress → Completed / CompletedLate / Cancelled
        // -------------------------------------------------------------------
        public static class FollowUpStatuses
        {
            public const string Pending       = "Pending";       // created, not yet started
            public const string InProgress    = "InProgress";    // Start button clicked
            public const string Completed     = "Completed";     // finished on time
            public const string CompletedLate = "CompletedLate"; // finished after the EndDate deadline
            public const string Cancelled     = "Cancelled";     // discarded without completion
        }

        // -------------------------------------------------------------------
        // History action labels — stored in CovenantHistory.Action
        // Every meaningful change to a covenant writes a history row.
        // -------------------------------------------------------------------
        public static class HistoryActions
        {
            public const string Created         = "Created";
            public const string Updated         = "Updated";
            public const string Deleted         = "Deleted";
            public const string Restored        = "Restored";
            public const string FollowUpAdded   = "FollowUpAdded";
            public const string ScheduleChanged = "ScheduleChanged";
        }

        // -------------------------------------------------------------------
        // Notification type labels — stored in Notifications.Type
        // -------------------------------------------------------------------
        public static class NotificationTypes
        {
            public const string ProcessingDateApproaching = "ProcessingDateApproaching"; // deadline near
            public const string FollowUpDue               = "FollowUpDue";               // follow-up overdue
            public const string ScheduleTriggered         = "ScheduleTriggered";         // auto-followup created
        }
    }
}
