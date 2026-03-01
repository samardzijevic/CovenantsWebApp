using System;
using System.Collections.Generic;

namespace Covenants.Common
{
    public static class DateHelper
    {
        /// <summary>
        /// Calculates the next run date for a schedule.
        /// Returns null when the schedule should no longer run
        /// (Once already executed, or EndDate exceeded).
        /// </summary>
        public static DateTime? CalculateNextRunDate(
            string    scheduleType,
            DateTime  startDate,
            DateTime? lastRunAt,
            DateTime? endDate,
            int       interval,      // every N units; minimum 1
            string    daysOfWeek,    // weekly: comma-separated 0-6, e.g. "1,3,5"
            int?      dayOfMonth,    // monthly/yearly: 1–31
            int?      monthOfYear)   // yearly: 1–12
        {
            if (interval < 1) interval = 1;

            DateTime baseDate = lastRunAt ?? startDate.AddSeconds(-1);
            DateTime next;

            if (scheduleType == Constants.ScheduleTypes.Once)
            {
                if (lastRunAt.HasValue) return null;
                next = startDate;
            }
            else if (scheduleType == Constants.ScheduleTypes.Daily)
            {
                next = baseDate.Date.AddDays(interval);
            }
            else if (scheduleType == Constants.ScheduleTypes.Weekly)
            {
                int[] days = ParseDaysOfWeek(daysOfWeek);
                next = GetNextWeeklyRun(baseDate, startDate, interval, days);
            }
            else if (scheduleType == Constants.ScheduleTypes.Monthly)
            {
                int dom = dayOfMonth ?? 1;
                next = GetNextMonthlyRun(baseDate, interval, dom);
            }
            else if (scheduleType == Constants.ScheduleTypes.Yearly)
            {
                int month = monthOfYear ?? 1;
                int day   = dayOfMonth  ?? 1;
                next = GetNextYearlyRun(baseDate, interval, month, day);
            }
            else
            {
                return null;
            }

            if (endDate.HasValue && next > endDate.Value)
                return null;

            return next;
        }

        // -------------------------------------------------------
        // Weekly: every N weeks on the given days
        // -------------------------------------------------------
        private static DateTime GetNextWeeklyRun(DateTime baseDate, DateTime startDate, int intervalWeeks, int[] targetDays)
        {
            if (targetDays == null || targetDays.Length == 0)
                targetDays = new[] { 1 }; // default Monday

            int blockDays = intervalWeeks * 7;

            // The first candidate to consider is the day AFTER baseDate
            DateTime candidate = baseDate.Date.AddDays(1);

            // If candidate is before startDate, begin from startDate
            if (candidate < startDate.Date)
                candidate = startDate.Date;

            // Which N-week block does the candidate fall into?
            int daysFromStart = (int)(candidate.Date - startDate.Date).TotalDays;
            if (daysFromStart < 0) daysFromStart = 0;

            int blockNumber  = daysFromStart / blockDays;
            DateTime blockStart = startDate.Date.AddDays((long)blockNumber * blockDays);
            DateTime blockEnd   = blockStart.AddDays(blockDays);

            // Search from candidate to end of current block
            DateTime search = candidate < blockStart ? blockStart : candidate;
            while (search < blockEnd)
            {
                if (IsDayInArray((int)search.DayOfWeek, targetDays))
                    return search;
                search = search.AddDays(1);
            }

            // Not found — advance to the next block and return first matching day
            DateTime nextBlock = blockEnd;
            for (int d = 0; d < blockDays; d++)
            {
                if (IsDayInArray((int)nextBlock.AddDays(d).DayOfWeek, targetDays))
                    return nextBlock.AddDays(d);
            }

            // Unreachable with a non-empty targetDays, but provide a safe fallback
            return baseDate.Date.AddDays(blockDays);
        }

        // -------------------------------------------------------
        // Monthly: every N months on the given day
        // -------------------------------------------------------
        private static DateTime GetNextMonthlyRun(DateTime baseDate, int intervalMonths, int dayOfMonth)
        {
            // Advance N months from the first of baseDate's month
            var candidate = new DateTime(baseDate.Year, baseDate.Month, 1).AddMonths(intervalMonths);
            int maxDay = DateTime.DaysInMonth(candidate.Year, candidate.Month);
            return new DateTime(candidate.Year, candidate.Month, Math.Min(dayOfMonth, maxDay));
        }

        // -------------------------------------------------------
        // Yearly: every N years on the given month + day
        // -------------------------------------------------------
        private static DateTime GetNextYearlyRun(DateTime baseDate, int intervalYears, int month, int day)
        {
            int year = baseDate.Year;
            int safeDay = Math.Min(day, DateTime.DaysInMonth(year, month));
            DateTime candidate = new DateTime(year, month, safeDay);

            if (candidate <= baseDate.Date)
            {
                year += intervalYears;
                safeDay = Math.Min(day, DateTime.DaysInMonth(year, month));
                candidate = new DateTime(year, month, safeDay);
            }

            return candidate;
        }

        // -------------------------------------------------------
        // Helpers
        // -------------------------------------------------------
        private static int[] ParseDaysOfWeek(string daysOfWeek)
        {
            if (string.IsNullOrEmpty(daysOfWeek))
                return new[] { 1 }; // Monday

            var parts = daysOfWeek.Split(',');
            var result = new List<int>();
            foreach (var p in parts)
            {
                int d;
                if (int.TryParse(p.Trim(), out d) && d >= 0 && d <= 6)
                    result.Add(d);
            }
            if (result.Count == 0)
                result.Add(1);

            result.Sort();
            return result.ToArray();
        }

        private static bool IsDayInArray(int day, int[] arr)
        {
            foreach (int d in arr)
                if (d == day) return true;
            return false;
        }
    }
}
