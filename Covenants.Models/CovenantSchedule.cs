using System;
using System.Collections.Generic;

namespace Covenants.Models
{
    public class CovenantSchedule
    {
        public int       Id           { get; set; }
        public int       CovenantId   { get; set; }
        public string    ScheduleType { get; set; }   // Once | Daily | Weekly | Monthly | Yearly
        public int       Interval     { get; set; } = 1;  // every N days/weeks/months/years
        public DateTime  StartDate    { get; set; }
        public DateTime? EndDate      { get; set; }
        public string    DaysOfWeek   { get; set; }   // weekly multi-day: "0,1,3,5" = Sun,Mon,Wed,Fri
        public int?      DayOfMonth   { get; set; }   // monthly / yearly: 1–31
        public int?      MonthOfYear  { get; set; }   // yearly: 1–12
        public bool      IsActive     { get; set; }
        public DateTime? LastRunAt    { get; set; }
        public DateTime? NextRunAt    { get; set; }
        public DateTime  CreatedAt    { get; set; }
        public string    CreatedBy    { get; set; }
        public DateTime? UpdatedAt    { get; set; }
        public string    UpdatedBy    { get; set; }

        // -------------------------------------------------------
        // Human-readable description, e.g.
        //   "Every 3 days"
        //   "Every 2 weeks on Mon, Wed, Fri"
        //   "Every 5 months on the 15th"
        //   "Every 2 years on March 15"
        // -------------------------------------------------------
        public string Description
        {
            get
            {
                if (ScheduleType == "Once")
                    return string.Format("Once on {0:dd MMM yyyy}", StartDate);

                string unit;
                if (ScheduleType == "Daily")
                    unit = Interval == 1 ? "day" : "days";
                else if (ScheduleType == "Weekly")
                    unit = Interval == 1 ? "week" : "weeks";
                else if (ScheduleType == "Monthly")
                    unit = Interval == 1 ? "month" : "months";
                else if (ScheduleType == "Yearly")
                    unit = Interval == 1 ? "year" : "years";
                else
                    unit = "";

                string prefix = Interval == 1
                    ? string.Format("Every {0}", unit)
                    : string.Format("Every {0} {1}", Interval, unit);

                if (ScheduleType == "Weekly" && !string.IsNullOrEmpty(DaysOfWeek))
                {
                    string[] dayNames = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
                    var parts = DaysOfWeek.Split(',');
                    var names = new List<string>();
                    foreach (var p in parts)
                    {
                        int d;
                        if (int.TryParse(p.Trim(), out d) && d >= 0 && d <= 6)
                            names.Add(dayNames[d]);
                    }
                    if (names.Count > 0)
                        return string.Format("{0} on {1}", prefix, string.Join(", ", names.ToArray()));
                }

                if (ScheduleType == "Monthly" && DayOfMonth.HasValue)
                    return string.Format("{0} on the {1}", prefix, Ordinal(DayOfMonth.Value));

                if (ScheduleType == "Yearly" && MonthOfYear.HasValue && DayOfMonth.HasValue)
                {
                    string[] monthNames = new[] { "", "January", "February", "March", "April",
                        "May", "June", "July", "August", "September", "October",
                        "November", "December" };
                    return string.Format("{0} on {1} {2}", prefix,
                        monthNames[MonthOfYear.Value], DayOfMonth.Value);
                }

                return prefix;
            }
        }

        private static string Ordinal(int n)
        {
            if (n <= 0) return n.ToString();
            int mod100 = n % 100;
            if (mod100 >= 11 && mod100 <= 13) return n + "th";
            int mod10 = n % 10;
            if (mod10 == 1) return n + "st";
            if (mod10 == 2) return n + "nd";
            if (mod10 == 3) return n + "rd";
            return n + "th";
        }
    }
}
