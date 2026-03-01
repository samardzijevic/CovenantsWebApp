using System;
using System.Collections.Generic;

namespace Covenants.Common
{
    // -----------------------------------------------------------------------
    // DATE HELPER — SCHEDULE CALCULATION ENGINE
    // -----------------------------------------------------------------------
    // This class answers ONE question: "given that a schedule last ran at X,
    // when should it run next?"
    //
    // Why is this in Common and not in BLL?
    //   Because it is pure math — no database, no business rules, just dates.
    //   Putting it in Common lets both BLL (ScheduleService) and any future
    //   console tools or tests use it without pulling in the whole BLL layer.
    //
    // All methods are STATIC — there is no state, so you never need to
    // "new DateHelper()". The 'static' class modifier enforces this.
    // -----------------------------------------------------------------------

    public static class DateHelper
    {
        /// <summary>
        /// Calculates the next run date for a schedule.
        /// Returns null when the schedule should no longer run
        /// (Once already executed, or EndDate exceeded).
        /// </summary>
        /// <param name="scheduleType">One of: Once, Daily, Weekly, Monthly, Yearly</param>
        /// <param name="startDate">The very first date the schedule could fire.</param>
        /// <param name="lastRunAt">When it last fired. Null = never run yet.</param>
        /// <param name="endDate">Stop scheduling after this date. Null = run forever.</param>
        /// <param name="interval">Run every N units (e.g. every 3 days, every 2 weeks).</param>
        /// <param name="daysOfWeek">Weekly only: comma-separated day numbers 0-6 (0=Sunday).</param>
        /// <param name="dayOfMonth">Monthly/Yearly: which day of the month (1-31).</param>
        /// <param name="monthOfYear">Yearly only: which month (1-12).</param>
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
            // Guard: interval must be at least 1 to avoid infinite loops
            if (interval < 1) interval = 1;

            // baseDate = the reference point we calculate forward FROM.
            // If we've never run: subtract 1 second from startDate so the
            // first calculated next-run equals startDate itself (not startDate + interval).
            DateTime baseDate = lastRunAt ?? startDate.AddSeconds(-1);

            DateTime next;

            // -----------------------------------------------------------
            // ONCE: fire exactly once on startDate, then never again.
            // -----------------------------------------------------------
            if (scheduleType == Constants.ScheduleTypes.Once)
            {
                // If lastRunAt is set, it already fired — return null to signal "done".
                if (lastRunAt.HasValue) return null;
                next = startDate;
            }
            // -----------------------------------------------------------
            // DAILY: fire every N days from baseDate.
            // Example: interval=3 → Mon, Thu, Sun, Wed, ...
            // -----------------------------------------------------------
            else if (scheduleType == Constants.ScheduleTypes.Daily)
            {
                // .Date strips the time component so we always fire at midnight.
                // AddDays(interval) moves forward exactly N calendar days.
                next = baseDate.Date.AddDays(interval);
            }
            // -----------------------------------------------------------
            // WEEKLY: fire every N weeks, on one or more specific weekdays.
            // Example: interval=2, daysOfWeek="1,3,5" → every 2 weeks on Mon/Wed/Fri.
            // The complex block-alignment logic is inside GetNextWeeklyRun().
            // -----------------------------------------------------------
            else if (scheduleType == Constants.ScheduleTypes.Weekly)
            {
                // ParseDaysOfWeek converts "1,3,5" → int[] { 1, 3, 5 }
                int[] days = ParseDaysOfWeek(daysOfWeek);
                next = GetNextWeeklyRun(baseDate, startDate, interval, days);
            }
            // -----------------------------------------------------------
            // MONTHLY: fire every N months on a specific day of the month.
            // Example: interval=5, dayOfMonth=15 → 5th month from now on the 15th.
            // -----------------------------------------------------------
            else if (scheduleType == Constants.ScheduleTypes.Monthly)
            {
                // Default to 1st if no day specified
                int dom = dayOfMonth ?? 1;
                next = GetNextMonthlyRun(baseDate, interval, dom);
            }
            // -----------------------------------------------------------
            // YEARLY: fire every N years on a specific month+day.
            // Example: interval=2, monthOfYear=3, dayOfMonth=15 → every 2 years on March 15.
            // -----------------------------------------------------------
            else if (scheduleType == Constants.ScheduleTypes.Yearly)
            {
                int month = monthOfYear ?? 1;
                int day   = dayOfMonth  ?? 1;
                next = GetNextYearlyRun(baseDate, interval, month, day);
            }
            else
            {
                // Unknown schedule type — signal "don't run"
                return null;
            }

            // If we crossed the EndDate, the schedule is expired — return null.
            // The caller (ScheduleService.UpdateAfterRun) will deactivate it.
            if (endDate.HasValue && next > endDate.Value)
                return null;

            return next;
        }

        // -----------------------------------------------------------------------
        // WEEKLY BLOCK ALIGNMENT
        // -----------------------------------------------------------------------
        // The tricky part: "every 2 weeks" must stay aligned to the original
        // startDate, not drift based on when it last ran.
        //
        // Example: startDate = Monday Jan 6, interval = 2, days = Mon+Wed
        //   Block 1: Jan 6–Jan 19  → fires Jan 6 (Mon) and Jan 8 (Wed)
        //   Block 2: Jan 20–Feb 2  → fires Jan 20 (Mon) and Jan 22 (Wed)
        //   Block 3: Feb 3–Feb 16  → fires Feb 3 (Mon) and Feb 5 (Wed)
        //
        // We figure out WHICH N-week block the candidate falls in, look for a
        // matching weekday IN that block, and if none remain, advance to the
        // first match in the NEXT block.
        // -----------------------------------------------------------------------
        private static DateTime GetNextWeeklyRun(DateTime baseDate, DateTime startDate, int intervalWeeks, int[] targetDays)
        {
            // Default to Monday if no days were configured
            if (targetDays == null || targetDays.Length == 0)
                targetDays = new[] { 1 }; // 1 = Monday (0=Sunday)

            // How many calendar days make up one "block"
            int blockDays = intervalWeeks * 7;

            // The first candidate to consider is the day AFTER baseDate
            // (we already ran on baseDate, so we want the NEXT occurrence)
            DateTime candidate = baseDate.Date.AddDays(1);

            // If we haven't reached startDate yet (e.g. first run before schedule begins),
            // clamp to startDate so we never fire before it
            if (candidate < startDate.Date)
                candidate = startDate.Date;

            // Find which N-week block the candidate belongs to.
            // daysFromStart = how many days since startDate
            // blockNumber   = which block we're in (0, 1, 2, ...)
            int daysFromStart = (int)(candidate.Date - startDate.Date).TotalDays;
            if (daysFromStart < 0) daysFromStart = 0;

            int blockNumber  = daysFromStart / blockDays;
            DateTime blockStart = startDate.Date.AddDays((long)blockNumber * blockDays);
            DateTime blockEnd   = blockStart.AddDays(blockDays);

            // Scan from candidate to end of current block, looking for a target weekday
            DateTime search = candidate < blockStart ? blockStart : candidate;
            while (search < blockEnd)
            {
                // DayOfWeek returns 0=Sunday, 1=Monday, ..., 6=Saturday
                if (IsDayInArray((int)search.DayOfWeek, targetDays))
                    return search;
                search = search.AddDays(1);
            }

            // No match in current block — advance to the NEXT block and return the
            // first matching weekday in it
            DateTime nextBlock = blockEnd;
            for (int d = 0; d < blockDays; d++)
            {
                if (IsDayInArray((int)nextBlock.AddDays(d).DayOfWeek, targetDays))
                    return nextBlock.AddDays(d);
            }

            // Unreachable as long as targetDays is non-empty, but safe fallback
            return baseDate.Date.AddDays(blockDays);
        }

        // -----------------------------------------------------------------------
        // MONTHLY: advance N months, then clamp day to the month's last valid day.
        // -----------------------------------------------------------------------
        // Why clamp? February has 28/29 days. If the schedule is "every month on the
        // 31st", we can't use Feb 31 — so we use the last day of February instead.
        // Math.Min(dayOfMonth, DaysInMonth) does that safely.
        //
        // We always advance from the FIRST of the current month, so the interval
        // doesn't drift if the previous month had fewer days.
        // -----------------------------------------------------------------------
        private static DateTime GetNextMonthlyRun(DateTime baseDate, int intervalMonths, int dayOfMonth)
        {
            // Start from the first of baseDate's month, then add N months
            var candidate = new DateTime(baseDate.Year, baseDate.Month, 1).AddMonths(intervalMonths);

            // Clamp dayOfMonth so we don't ask for Feb 31, Apr 31, etc.
            int maxDay = DateTime.DaysInMonth(candidate.Year, candidate.Month);
            return new DateTime(candidate.Year, candidate.Month, Math.Min(dayOfMonth, maxDay));
        }

        // -----------------------------------------------------------------------
        // YEARLY: same month+day, but N years later.
        // -----------------------------------------------------------------------
        // The "Feb 29 problem": if dayOfMonth is 29 and it's not a leap year,
        // we clamp to Feb 28 — same Math.Min trick as monthly.
        // -----------------------------------------------------------------------
        private static DateTime GetNextYearlyRun(DateTime baseDate, int intervalYears, int month, int day)
        {
            int year = baseDate.Year;

            // Build candidate date in the CURRENT year
            int safeDay = Math.Min(day, DateTime.DaysInMonth(year, month));
            DateTime candidate = new DateTime(year, month, safeDay);

            // If that date is in the past (or today), advance N years
            if (candidate <= baseDate.Date)
            {
                year += intervalYears;
                // Re-clamp in case the new year has a different number of days
                // (e.g., Feb 29 in a non-leap year)
                safeDay   = Math.Min(day, DateTime.DaysInMonth(year, month));
                candidate = new DateTime(year, month, safeDay);
            }

            return candidate;
        }

        // -----------------------------------------------------------------------
        // HELPERS
        // -----------------------------------------------------------------------

        // Converts a stored string like "1,3,5" into an integer array [1, 3, 5].
        // int.TryParse is used (not int.Parse) so bad data in the DB doesn't crash.
        // The result is sorted so the weekly algorithm can iterate days in order.
        private static int[] ParseDaysOfWeek(string daysOfWeek)
        {
            if (string.IsNullOrEmpty(daysOfWeek))
                return new[] { 1 }; // default: Monday

            var parts  = daysOfWeek.Split(',');
            var result = new List<int>();
            foreach (var p in parts)
            {
                int d;
                // TryParse: if it fails (empty string, letters, etc.) we just skip it
                if (int.TryParse(p.Trim(), out d) && d >= 0 && d <= 6)
                    result.Add(d);
            }

            // Ensure we always have at least one day
            if (result.Count == 0)
                result.Add(1);

            // Sort so days appear in calendar order (Sun < Mon < ... < Sat)
            result.Sort();
            return result.ToArray();
        }

        // Linear search over a small array — faster than HashSet for 1–7 elements.
        private static bool IsDayInArray(int day, int[] arr)
        {
            foreach (int d in arr)
                if (d == day) return true;
            return false;
        }
    }
}
