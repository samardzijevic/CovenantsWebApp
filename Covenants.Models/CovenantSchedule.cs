using System;
using System.Collections.Generic;

namespace Covenants.Models
{
    // -----------------------------------------------------------------------
    // COVENANT SCHEDULE MODEL — represents one row in CovenantSchedules table
    // -----------------------------------------------------------------------
    // A schedule controls WHEN the SchedulerEngine auto-creates follow-ups.
    // Each covenant can have one active schedule at a time.
    //
    // INTERVAL: the "every N" multiplier.
    //   Interval=1  + Daily  → every day
    //   Interval=3  + Daily  → every 3 days
    //   Interval=2  + Weekly → every 2 weeks
    //   Interval=5  + Monthly→ every 5 months
    //
    // DAYSOFWEEK: only used for Weekly schedules.
    //   Stored as a comma-separated string: "1,3,5" = Monday, Wednesday, Friday
    //   0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4=Thursday, 5=Friday, 6=Saturday
    //   Multiple days are supported: fire on ALL listed days within each N-week block.
    //
    // WHY STRING instead of an array/list?
    //   The database stores it as NVARCHAR. Keeping it as a string in the model
    //   avoids extra serialization/deserialization — the DateHelper.ParseDaysOfWeek()
    //   helper converts it to int[] when the calculation needs it.
    // -----------------------------------------------------------------------

    public class CovenantSchedule
    {
        public int       Id           { get; set; }
        public int       CovenantId   { get; set; }

        // One of: Once | Daily | Weekly | Monthly | Yearly
        // Matches the string constants in Constants.ScheduleTypes
        public string    ScheduleType { get; set; }

        // Every N days/weeks/months/years (default 1 = "every" unit)
        public int       Interval     { get; set; } = 1;

        // The schedule is valid from StartDate...
        public DateTime  StartDate    { get; set; }

        // ...until EndDate (null = runs forever)
        public DateTime? EndDate      { get; set; }

        // Weekly multi-day: "0,1,3,5" = Sun,Mon,Wed,Fri
        // null or empty for non-weekly schedules
        public string    DaysOfWeek   { get; set; }

        // Monthly: day number 1–31 on which to fire (e.g. 15 = 15th of the month)
        // Yearly: day of DayOfMonth within MonthOfYear
        public int?      DayOfMonth   { get; set; }

        // Yearly only: 1=January … 12=December
        public int?      MonthOfYear  { get; set; }

        // Whether this schedule is currently active.
        // IsActive=0 means it has been replaced, expired, or is a Once that already ran.
        public bool      IsActive     { get; set; }

        // Set by ScheduleService.UpdateAfterRun() each time the schedule fires
        public DateTime? LastRunAt    { get; set; }

        // The next DateTime when the SchedulerEngine should fire this schedule.
        // null = schedule is expired or deactivated (engine ignores it).
        public DateTime? NextRunAt    { get; set; }

        public DateTime  CreatedAt    { get; set; }
        public string    CreatedBy    { get; set; }
        public DateTime? UpdatedAt    { get; set; }
        public string    UpdatedBy    { get; set; }

        // ---------------------------------------------------------------
        // DESCRIPTION — computed property (not stored in DB)
        // ---------------------------------------------------------------
        // A computed property does NOT have a backing field; its value is
        // calculated on the fly from other properties every time it is accessed.
        // We use this for display only — it is never written to the database.
        //
        // Examples:
        //   "Once on 05 Jan 2025"
        //   "Every day"
        //   "Every 3 days"
        //   "Every week on Mon, Wed, Fri"
        //   "Every 2 weeks on Mon, Fri"
        //   "Every month on the 15th"
        //   "Every 5 months on the 1st"
        //   "Every year on January 1"
        //   "Every 2 years on March 15"
        // ---------------------------------------------------------------
        public string Description
        {
            get
            {
                // Special case: "Once" has no interval — just show the date
                if (ScheduleType == "Once")
                    return string.Format("Once on {0:dd MMM yyyy}", StartDate);

                // Pick the correct unit word, singular or plural
                string unit;
                if      (ScheduleType == "Daily")   unit = Interval == 1 ? "day"   : "days";
                else if (ScheduleType == "Weekly")  unit = Interval == 1 ? "week"  : "weeks";
                else if (ScheduleType == "Monthly") unit = Interval == 1 ? "month" : "months";
                else if (ScheduleType == "Yearly")  unit = Interval == 1 ? "year"  : "years";
                else                                unit = "";

                // "Every day" vs "Every 3 days"
                string prefix = Interval == 1
                    ? string.Format("Every {0}", unit)
                    : string.Format("Every {0} {1}", Interval, unit);

                // Weekly: append the weekday list if configured
                if (ScheduleType == "Weekly" && !string.IsNullOrEmpty(DaysOfWeek))
                {
                    // Map 0-6 → short day names
                    string[] dayNames = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
                    var parts = DaysOfWeek.Split(',');
                    var names = new List<string>();
                    foreach (var p in parts)
                    {
                        int d;
                        // TryParse guards against bad stored data (doesn't crash)
                        if (int.TryParse(p.Trim(), out d) && d >= 0 && d <= 6)
                            names.Add(dayNames[d]);
                    }
                    if (names.Count > 0)
                        return string.Format("{0} on {1}", prefix, string.Join(", ", names.ToArray()));
                }

                // Monthly: append "on the 15th" etc. using Ordinal helper
                if (ScheduleType == "Monthly" && DayOfMonth.HasValue)
                    return string.Format("{0} on the {1}", prefix, Ordinal(DayOfMonth.Value));

                // Yearly: append "on January 15" etc.
                if (ScheduleType == "Yearly" && MonthOfYear.HasValue && DayOfMonth.HasValue)
                {
                    // Index 0 is empty string so we can use monthNames[1] = "January" directly
                    string[] monthNames = new[] { "", "January", "February", "March", "April",
                        "May", "June", "July", "August", "September", "October",
                        "November", "December" };
                    return string.Format("{0} on {1} {2}", prefix,
                        monthNames[MonthOfYear.Value], DayOfMonth.Value);
                }

                // Fallback: just "Every 3 days" with no extra detail
                return prefix;
            }
        }

        // ---------------------------------------------------------------
        // ORDINAL — converts an integer to its English ordinal suffix
        // ---------------------------------------------------------------
        // Examples: 1→"1st", 2→"2nd", 3→"3rd", 4→"4th", 11→"11th", 21→"21st"
        //
        // The special 11th/12th/13th rule:
        //   Normally: 1st, 2nd, 3rd, 4th, ...
        //   But:      11th, 12th, 13th (NOT 11st, 12nd, 13rd) because of English grammar.
        //   mod100 catches those "teen" cases regardless of how large the number is.
        // ---------------------------------------------------------------
        private static string Ordinal(int n)
        {
            if (n <= 0) return n.ToString();

            int mod100 = n % 100;
            // 11, 12, 13 are exceptions — always "th" regardless of last digit
            if (mod100 >= 11 && mod100 <= 13) return n + "th";

            int mod10 = n % 10;
            if (mod10 == 1) return n + "st";   // 1st, 21st, 31st
            if (mod10 == 2) return n + "nd";   // 2nd, 22nd
            if (mod10 == 3) return n + "rd";   // 3rd, 23rd
            return n + "th";                   // 4th–20th, 24th–30th, etc.
        }
    }
}
