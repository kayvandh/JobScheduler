using Framework.Schedule.Enums;
using Framework.Schedule.Extensions;

namespace Framework.Schedule.Core
{
    public class JobSchedule
    {
        public JobSchedule()
        {
        }

        public JobSchedule(string cron)
        {
            if (!cron.IsValidCron())
                throw new ArgumentException($"Invalid Cron string: {cron}", nameof(cron));

            this.FromCron(cron);
        }

        // ----------- Schedule Properties -----------
        public Frequency Frequency { get; set; } = Frequency.Daily;

        // Daily
        public DailyType DailyType { get; set; } = DailyType.Once;

        public int Hour { get; set; } = 0;
        public int Minute { get; set; } = 0;
        public int RecurringEveryHours { get; set; } = 0;
        public int RecurringEveryMinutes { get; set; } = 0;
        public TimeSpan? RecurringStartTime { get; set; }
        public TimeSpan? RecurringEndTime { get; set; }

        // Weekly
        public DaysOfWeek DaysOfWeek { get; set; } = DaysOfWeek.Monday;

        // Monthly
        public MonthlyType MonthlyType { get; set; } = MonthlyType.DayOfMonth;

        public int DayOfMonth { get; set; } = 1;
        public int NthWeek { get; set; } = 1; // 1st, 2nd, 3rd, 4th, Last
        public DaysOfWeek WeekDay { get; set; } = DaysOfWeek.Monday;

        // ----------- Cron Generation -----------
        public string ToCron()
        {
            string cronMinute = "*";
            string cronHour = "*";
            string dayOfMonth = "*";
            string month = "*";
            string dayOfWeek = "?";

            switch (Frequency)
            {
                case Frequency.Once:
                    cronMinute = Minute.ToString();
                    cronHour = Hour.ToString();
                    break;

                case Frequency.Daily:
                    if (DailyType == DailyType.Once)
                    {
                        cronMinute = Minute.ToString();
                        cronHour = Hour.ToString();
                    }
                    else if (DailyType == DailyType.Recurring)
                    {
                        if (RecurringStartTime == null || RecurringEndTime == null)
                            throw new InvalidOperationException("Recurring start/end time must be set");

                        int startHour = RecurringStartTime.Value.Hours;
                        int startMinute = RecurringStartTime.Value.Minutes;
                        int endHour = RecurringEndTime.Value.Hours;

                        // Hour part
                        cronHour = RecurringEveryHours > 0 ? $"{startHour}-{endHour}/{RecurringEveryHours}" : $"{startHour}-{endHour}";

                        // Minute part
                        cronMinute = RecurringEveryMinutes > 0 ? $"{startMinute}-59/{RecurringEveryMinutes}" : startMinute.ToString();
                    }
                    dayOfWeek = "*";
                    break;

                case Frequency.Weekly:
                    cronMinute = Minute.ToString();
                    cronHour = Hour.ToString();
                    dayOfWeek = string.Join(",", GetSelectedDays());
                    break;

                case Frequency.Monthly:
                    cronMinute = Minute.ToString();
                    cronHour = Hour.ToString();
                    if (MonthlyType == MonthlyType.DayOfMonth)
                    {
                        dayOfMonth = DayOfMonth.ToString();
                        dayOfWeek = "?";
                    }
                    else
                    {
                        dayOfWeek = $"{DayOfWeekToCronNumber(WeekDay)}#{NthWeek}";
                        dayOfMonth = "?";
                    }
                    break;
            }

            return $"{cronMinute} {cronHour} {dayOfMonth} {month} {dayOfWeek}";
        }

        // ----------- Parse Cron -----------
        public void FromCron(string cron)
        {
            var parts = cron.Split(' ');
            if (parts.Length != 5)
                throw new ArgumentException("Invalid cron expression");

            string minutePart = parts[0];
            string hourPart = parts[1];
            string dayOfMonthPart = parts[2];
            string dayOfWeekPart = parts[4];

            // Reset
            Frequency = Frequency.Daily;
            DailyType = DailyType.Once;
            RecurringStartTime = null;
            RecurringEndTime = null;
            RecurringEveryHours = 0;
            RecurringEveryMinutes = 0;

            if (dayOfMonthPart != "*" && dayOfMonthPart != "?")
            {
                Frequency = Frequency.Monthly;
                MonthlyType = MonthlyType.DayOfMonth;
                DayOfMonth = int.Parse(dayOfMonthPart);
            }
            else if (dayOfWeekPart.Contains("#"))
            {
                Frequency = Frequency.Monthly;
                MonthlyType = MonthlyType.NthWeekday;
                var arr = dayOfWeekPart.Split('#');
                WeekDay = (DaysOfWeek)(1 << int.Parse(arr[0]));
                NthWeek = int.Parse(arr[1]);
            }
            else if (dayOfWeekPart != "*" && dayOfWeekPart != "?")
            {
                Frequency = Frequency.Weekly;
                DaysOfWeek = ParseDaysOfWeek(dayOfWeekPart);
            }
            else
            {
                // Check for Recurring
                if (minutePart.Contains("/") || hourPart.Contains("/"))
                {
                    DailyType = DailyType.Recurring;

                    // Hour
                    if (hourPart.Contains("-"))
                    {
                        var hRange = hourPart.Split('-');
                        RecurringStartTime = TimeSpan.FromHours(int.Parse(hRange[0]));
                        int endHour = int.Parse(hRange[1].Split('/')[0]);
                        RecurringEndTime = TimeSpan.FromHours(endHour);
                        RecurringEveryHours = hourPart.Contains("/") ? int.Parse(hRange[1].Split('/')[1]) : 0;
                    }
                    else
                    {
                        RecurringStartTime = TimeSpan.FromHours(int.Parse(hourPart.Split('/')[0]));
                        RecurringEndTime = RecurringStartTime;
                    }

                    // Minute
                    if (minutePart.Contains("/"))
                    {
                        var mParts = minutePart.Split('/');
                        int startMinute = int.Parse(mParts[0].Split('-')[0]);
                        RecurringEveryMinutes = int.Parse(mParts[1]);
                        RecurringStartTime = RecurringStartTime.Value.Add(TimeSpan.FromMinutes(startMinute));
                    }
                    else
                    {
                        Minute = int.Parse(minutePart);
                        Hour = RecurringStartTime?.Hours ?? 0;
                    }
                }
                else
                {
                    DailyType = DailyType.Once;
                    Minute = int.Parse(minutePart);
                    Hour = int.Parse(hourPart);
                }
            }
        }

        private IEnumerable<int> GetSelectedDays()
        {
            foreach (DaysOfWeek day in Enum.GetValues(typeof(DaysOfWeek)))
            {
                if (day != DaysOfWeek.None && day != DaysOfWeek.All && DaysOfWeek.HasFlag(day))
                {
                    yield return DayOfWeekToCronNumber(day);
                }
            }
        }

        public ValidationResult Validate()
        {
            var result = new ValidationResult();

            switch (Frequency)
            {
                case Frequency.Once:
                    ValidateHourMinute(Hour, Minute, result.Errors, "Once");
                    break;

                case Frequency.Daily:
                    if (DailyType == DailyType.Once)
                        ValidateHourMinute(Hour, Minute, result.Errors, "Daily Once");
                    else
                    {
                        if (!RecurringStartTime.HasValue)
                            result.Errors.Add("RecurringStartTime must be set for Daily Recurring schedule.");
                        if (!RecurringEndTime.HasValue)
                            result.Errors.Add("RecurringEndTime must be set for Daily Recurring schedule.");
                        if (RecurringStartTime.HasValue && RecurringEndTime.HasValue && RecurringStartTime >= RecurringEndTime)
                            result.Errors.Add("RecurringStartTime must be before RecurringEndTime.");
                        if (RecurringEveryHours == 0 && RecurringEveryMinutes == 0)
                            result.Errors.Add("Recurring interval (hours or minutes) must be greater than zero.");
                    }
                    break;

                case Frequency.Weekly:
                    if (DaysOfWeek == DaysOfWeek.None)
                        result.Errors.Add("At least one day must be selected for Weekly schedule.");
                    ValidateHourMinute(Hour, Minute, result.Errors, "Weekly");
                    break;

                case Frequency.Monthly:
                    if (MonthlyType == MonthlyType.DayOfMonth)
                    {
                        if (DayOfMonth < 1 || DayOfMonth > 31)
                            result.Errors.Add("DayOfMonth must be between 1 and 31 for Monthly schedule.");
                        ValidateHourMinute(Hour, Minute, result.Errors, "Monthly DayOfMonth");
                    }
                    else
                    {
                        if (NthWeek < 1 || NthWeek > 5)
                            result.Errors.Add("NthWeek must be between 1 and 5 for Monthly NthWeekday schedule.");
                        if (WeekDay == DaysOfWeek.None)
                            result.Errors.Add("WeekDay must be selected for Monthly NthWeekday schedule.");
                        ValidateHourMinute(Hour, Minute, result.Errors, "Monthly NthWeekday");
                    }
                    break;

                default:
                    result.Errors.Add("Unknown schedule frequency.");
                    break;
            }

            return result;
        }

        private void ValidateHourMinute(int hour, int minute, List<string> errors, string context)
        {
            if (hour < 0 || hour > 23)
                errors.Add($"Hour must be between 0 and 23 for {context} schedule.");
            if (minute < 0 || minute > 59)
                errors.Add($"Minute must be between 0 and 59 for {context} schedule.");
        }

        private int DayOfWeekToCronNumber(DaysOfWeek day)
        {
            return day switch
            {
                DaysOfWeek.Sunday => 0,
                DaysOfWeek.Monday => 1,
                DaysOfWeek.Tuesday => 2,
                DaysOfWeek.Wednesday => 3,
                DaysOfWeek.Thursday => 4,
                DaysOfWeek.Friday => 5,
                DaysOfWeek.Saturday => 6,
                _ => throw new ArgumentException("Invalid day")
            };
        }

        private DaysOfWeek ParseDaysOfWeek(string cronDays)
        {
            var result = DaysOfWeek.None;
            var parts = cronDays.Split(',');
            foreach (var part in parts)
            {
                int dayNum = int.Parse(part);
                result |= dayNum switch
                {
                    0 => DaysOfWeek.Sunday,
                    1 => DaysOfWeek.Monday,
                    2 => DaysOfWeek.Tuesday,
                    3 => DaysOfWeek.Wednesday,
                    4 => DaysOfWeek.Thursday,
                    5 => DaysOfWeek.Friday,
                    6 => DaysOfWeek.Saturday,
                    _ => DaysOfWeek.None
                };
            }
            return result;
        }
    }
}