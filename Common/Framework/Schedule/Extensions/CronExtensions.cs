using Framework.Schedule.Core;
using Framework.Schedule.Enums;
using System.Text.RegularExpressions;

namespace Framework.Schedule.Extensions
{
    public static class CronExtensions
    {
        private static readonly Regex CronPartPattern = new(@"^(\*|\d+|\d+-\d+|\d+(\/\d+)?|\w+)$", RegexOptions.IgnoreCase);

        public static bool IsValidCron(this string cron)
        {
            if (string.IsNullOrWhiteSpace(cron))
                return false;

            var parts = cron.Split(' ');
            if (parts.Length != 5)
                return false;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                if (!CronPartPattern.IsMatch(part) && part != "?" && !part.Contains("#"))
                    return false;

                if (i == 0 && !IsValidNumberOrRange(part, 0, 59)) return false; // Minute
                if (i == 1 && !IsValidNumberOrRange(part, 0, 23)) return false; // Hour
                if (i == 2 && !IsValidNumberOrRange(part, 1, 31)) return false; // DayOfMonth
                if (i == 3 && !IsValidNumberOrRange(part, 1, 12)) return false; // Month
                if (i == 4 && !IsValidNumberOrRange(part, 0, 6)) return false;  // DayOfWeek
            }

            return true;
        }

        public static string ToFriendlyString(this JobSchedule schedule)
        {
            switch (schedule.Frequency)
            {
                case Frequency.Once:
                    return $"One-time at {schedule.Hour:D2}:{schedule.Minute:D2}";

                case Frequency.Daily:
                    if (schedule.DailyType == DailyType.Once)
                        return $"Every day at {schedule.Hour:D2}:{schedule.Minute:D2}";
                    else
                    {
                        if (!schedule.RecurringStartTime.HasValue || !schedule.RecurringEndTime.HasValue)
                            return "Every day (invalid recurring schedule)";

                        string startTime = schedule.RecurringStartTime.Value.ToString(@"hh\:mm");
                        string endTime = schedule.RecurringEndTime.Value.ToString(@"hh\:mm");

                        string hourText = schedule.RecurringEveryHours > 0 ? $"{schedule.RecurringEveryHours} hour(s)" : "";
                        string minuteText = schedule.RecurringEveryMinutes > 0 ? $"{schedule.RecurringEveryMinutes} minute(s)" : "";

                        string everyText = "";
                        if (hourText != "" && minuteText != "")
                            everyText = $"every {hourText} and {minuteText}";
                        else if (hourText != "")
                            everyText = $"every {hourText}";
                        else if (minuteText != "")
                            everyText = $"every {minuteText}";

                        return $"Every day from {startTime} to {endTime} {everyText}".Trim();
                    }

                case Frequency.Weekly:
                    var days = Enum.GetValues(typeof(DaysOfWeek))
                                   .Cast<DaysOfWeek>()
                                   .Where(d => d != DaysOfWeek.None && d != DaysOfWeek.All && schedule.DaysOfWeek.HasFlag(d))
                                   .Select(d => d.ToString());
                    string daysText = days.Any() ? string.Join(", ", days) : "Unknown days";
                    return $"Every week on {daysText} at {schedule.Hour:D2}:{schedule.Minute:D2}";

                case Frequency.Monthly:
                    if (schedule.MonthlyType == MonthlyType.DayOfMonth)
                        return $"Every month on day {schedule.DayOfMonth} at {schedule.Hour:D2}:{schedule.Minute:D2}";
                    else
                    {
                        string ordinal = GetOrdinal(schedule.NthWeek);
                        return $"Every month on the {ordinal} {schedule.WeekDay} at {schedule.Hour:D2}:{schedule.Minute:D2}";
                    }

                default:
                    return "Unknown schedule";
            }
        }

        public static DateTime NextRunTime(this JobSchedule schedule, DateTime fromTime)
        {
            switch (schedule.Frequency)
            {
                case Frequency.Once:
                    var onceTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, schedule.Hour, schedule.Minute, 0);
                    if (onceTime < fromTime)
                        onceTime = onceTime.AddDays(1);
                    return onceTime;

                case Frequency.Daily:
                    if (schedule.DailyType == DailyType.Once)
                    {
                        var dailyTime = new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, schedule.Hour, schedule.Minute, 0);
                        if (dailyTime < fromTime)
                            dailyTime = dailyTime.AddDays(1);
                        return dailyTime;
                    }
                    else
                    {
                        if (!schedule.RecurringStartTime.HasValue || !schedule.RecurringEndTime.HasValue)
                            throw new InvalidOperationException("Recurring start/end time not set");

                        var start = schedule.RecurringStartTime.Value;
                        var end = schedule.RecurringEndTime.Value;
                        var currentTime = fromTime.TimeOfDay;

                        var nextTime = new TimeSpan(start.Hours, start.Minutes, 0);
                        while (nextTime < currentTime)
                        {
                            nextTime = nextTime.Add(new TimeSpan(schedule.RecurringEveryHours, schedule.RecurringEveryMinutes, 0));
                        }

                        if (nextTime <= end)
                        {
                            return new DateTime(fromTime.Year, fromTime.Month, fromTime.Day, nextTime.Hours, nextTime.Minutes, 0);
                        }
                        else
                        {
                            nextTime = start;
                            var nextDay = fromTime.AddDays(1);
                            return new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, nextTime.Hours, nextTime.Minutes, 0);
                        }
                    }

                case Frequency.Weekly:
                    for (int i = 0; i <= 7; i++)
                    {
                        var candidate = fromTime.AddDays(i);
                        DaysOfWeek dayFlag = (DaysOfWeek)(1 << (int)candidate.DayOfWeek);
                        if (schedule.DaysOfWeek.HasFlag(dayFlag))
                        {
                            var candidateTime = new DateTime(candidate.Year, candidate.Month, candidate.Day, schedule.Hour, schedule.Minute, 0);
                            if (candidateTime >= fromTime)
                                return candidateTime;
                        }
                    }
                    throw new InvalidOperationException("No valid next run time found for Weekly schedule");

                case Frequency.Monthly:
                    DateTime nextMonth;
                    if (schedule.MonthlyType == MonthlyType.DayOfMonth)
                    {
                        nextMonth = new DateTime(fromTime.Year, fromTime.Month, 1, schedule.Hour, schedule.Minute, 0);
                        if (fromTime.Day > schedule.DayOfMonth)
                        {
                            nextMonth = nextMonth.AddMonths(1);
                        }
                        return new DateTime(nextMonth.Year, nextMonth.Month, schedule.DayOfMonth, schedule.Hour, schedule.Minute, 0);
                    }
                    else
                    {
                        nextMonth = new DateTime(fromTime.Year, fromTime.Month, 1, schedule.Hour, schedule.Minute, 0);
                        DateTime candidate = GetNthWeekdayOfMonth(nextMonth, schedule.WeekDay, schedule.NthWeek);
                        if (candidate < fromTime)
                        {
                            nextMonth = nextMonth.AddMonths(1);
                            candidate = GetNthWeekdayOfMonth(nextMonth, schedule.WeekDay, schedule.NthWeek);
                        }
                        return candidate;
                    }

                default:
                    throw new InvalidOperationException("Unknown schedule frequency");
            }
        }

        public static JobSchedule ToJobSchedule(this string cron)
        {
            var schedule = new JobSchedule();
            schedule.FromCron(cron);
            return schedule;
        }

        public static string ToCronString(this JobSchedule schedule)
        {
            return schedule.ToCron();
        }

        private static string GetOrdinal(int number)
        {
            return number switch
            {
                1 => "first",
                2 => "second",
                3 => "third",
                4 => "fourth",
                5 => "last",
                _ => number.ToString()
            };
        }

        private static bool IsValidNumberOrRange(string part, int min, int max)
        {
            try
            {
                if (part.Contains("/")) part = part.Split('/')[0];
                if (part.Contains("-"))
                {
                    var range = part.Split('-');
                    int start = int.Parse(range[0]);
                    int end = int.Parse(range[1]);
                    return start >= min && start <= max && end >= min && end <= max && start <= end;
                }

                if (part == "*" || part == "?") return true;

                int value = int.Parse(part);
                return value >= min && value <= max;
            }
            catch
            {
                return false;
            }
        }

        private static DateTime GetNthWeekdayOfMonth(DateTime monthStart, DaysOfWeek day, int nth)
        {
            int dayOfWeek = (int)day;
            DateTime firstDay = new DateTime(monthStart.Year, monthStart.Month, 1);
            int offset = ((dayOfWeek - (int)firstDay.DayOfWeek + 7) % 7);
            DateTime result = firstDay.AddDays(offset + (nth - 1) * 7);

            if (nth == 5)
            {
                DateTime last = result.AddDays(7);
                if (last.Month != monthStart.Month)
                    return result;
                else
                    return last;
            }

            return result;
        }
    }
}