using System;

namespace FluentMigrator
{
    /// <summary>
    /// Creates a MigrationAttribute which executes in order based on the given date and time.
    /// </summary>
    public class TimestampedMigrationAttribute : MigrationAttribute
    {
        private const long TicksPerDay = TicksPerHour * 24;

        private const long TicksPerHour = TicksPerMinute * 60;

        private const long TicksPerMillisecond = 10000;

        private const long TicksPerMinute = TicksPerSecond * 60;

        private const long TicksPerSecond = TicksPerMillisecond * 1000;

        private static readonly int[] DaysToMonth365 =
            {
                0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365
            };

        private static readonly int[] DaysToMonth366 =
            {
                0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampedMigrationAttribute"/> class whose version is based on date and time information.
        /// </summary>
        /// <param name="year">The year the migration was created.</param>
        /// <param name="month">The month the migration was created.</param>
        /// <param name="day">The day the migration was created.</param>
        /// <param name="hour">The hour the migration was created.</param>
        /// <param name="minute">The minute the migration was created.</param>
        /// <param name="second">The second the migration was created.</param>
        public TimestampedMigrationAttribute(ushort year, ushort month, ushort day, ushort hour, ushort minute, ushort second)
            : base(DateTimeToTicks(year, month, day, hour, minute, second))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampedMigrationAttribute"/> class whose version is based on date and time information.
        /// </summary>
        /// <param name="year">The year the migration was created.</param>
        /// <param name="month">The month the migration was created.</param>
        /// <param name="day">The day the migration was created.</param>
        /// <param name="hour">The hour the migration was created.</param>
        /// <param name="minute">The minute the migration was created.</param>
        public TimestampedMigrationAttribute(ushort year, ushort month, ushort day, ushort hour, ushort minute)
            : base(DateTimeToTicks(year, month, day, hour, minute, 0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampedMigrationAttribute"/> class whose version is based on date and time information.
        /// </summary>
        /// <param name="year">The year the migration was created.</param>
        /// <param name="month">The month the migration was created.</param>
        /// <param name="day">The day the migration was created.</param>
        /// <param name="hour">The hour the migration was created.</param>
        /// <param name="minute">The minute the migration was created.</param>
        /// <param name="description">A description for the migration.</param>
        public TimestampedMigrationAttribute(ushort year, ushort month, ushort day, ushort hour, ushort minute, string description)
            : base(DateTimeToTicks(year, month, day, hour, minute, 0), description)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampedMigrationAttribute"/> class whose version is based on date and time information.
        /// </summary>
        /// <param name="year">The year the migration was created.</param>
        /// <param name="month">The month the migration was created.</param>
        /// <param name="day">The day the migration was created.</param>
        /// <param name="hour">The hour the migration was created.</param>
        /// <param name="minute">The minute the migration was created.</param>
        /// <param name="second">The second the migration was created.</param>
        /// <param name="description">A description for the migration.</param>
        public TimestampedMigrationAttribute(ushort year, ushort month, ushort day, ushort hour, ushort minute, ushort second, string description)
            : base(DateTimeToTicks(year, month, day, hour, minute, second), description)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampedMigrationAttribute"/> class whose version is based on date and time information.
        /// </summary>
        /// <param name="year">The year the migration was created.</param>
        /// <param name="month">The month the migration was created.</param>
        /// <param name="day">The day the migration was created.</param>
        /// <param name="hour">The hour the migration was created.</param>
        /// <param name="minute">The minute the migration was created.</param>
        /// <param name="transactionBehavior">The <see cref="FluentMigrator.TransactionBehavior"/> the migration will use.</param>
        public TimestampedMigrationAttribute(ushort year, ushort month, ushort day, ushort hour, ushort minute, TransactionBehavior transactionBehavior)
            : base(DateTimeToTicks(year, month, day, hour, minute, 0), transactionBehavior)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampedMigrationAttribute"/> class whose version is based on date and time information.
        /// </summary>
        /// <param name="year">The year the migration was created.</param>
        /// <param name="month">The month the migration was created.</param>
        /// <param name="day">The day the migration was created.</param>
        /// <param name="hour">The hour the migration was created.</param>
        /// <param name="minute">The minute the migration was created.</param>
        /// <param name="second">The second the migration was created.</param>
        /// <param name="transactionBehavior">The <see cref="FluentMigrator.TransactionBehavior"/> the migration will use.</param>
        public TimestampedMigrationAttribute(ushort year, ushort month, ushort day, ushort hour, ushort minute, ushort second, TransactionBehavior transactionBehavior)
            : base(DateTimeToTicks(year, month, day, hour, minute, second), transactionBehavior)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampedMigrationAttribute"/> class whose version is based on date and time information.
        /// </summary>
        /// <param name="year">The year the migration was created.</param>
        /// <param name="month">The month the migration was created.</param>
        /// <param name="day">The day the migration was created.</param>
        /// <param name="hour">The hour the migration was created.</param>
        /// <param name="minute">The minute the migration was created.</param>
        /// <param name="transactionBehavior">The <see cref="FluentMigrator.TransactionBehavior"/> the migration will use.</param>
        /// <param name="description">A description for the migration.</param>
        public TimestampedMigrationAttribute(ushort year, ushort month, ushort day, ushort hour, ushort minute, TransactionBehavior transactionBehavior, string description)
            : base(DateTimeToTicks(year, month, day, hour, minute, 0), transactionBehavior, description)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampedMigrationAttribute"/> class whose version is based on date and time information.
        /// </summary>
        /// <param name="year">The year the migration was created.</param>
        /// <param name="month">The month the migration was created.</param>
        /// <param name="day">The day the migration was created.</param>
        /// <param name="hour">The hour the migration was created.</param>
        /// <param name="minute">The minute the migration was created.</param>
        /// <param name="second">The second the migration was created.</param>
        /// <param name="transactionBehavior">The <see cref="FluentMigrator.TransactionBehavior"/> the migration will use.</param>
        /// <param name="description">A description for the migration.</param>
        public TimestampedMigrationAttribute(ushort year, ushort month, ushort day, ushort hour, ushort minute, ushort second, TransactionBehavior transactionBehavior, string description)
            : base(DateTimeToTicks(year, month, day, hour, minute, second), transactionBehavior, description)
        {
        }

        private static long DateTimeToTicks(int year, int month, int day, int hour, int minute, int second)
        {
            return DateToTicks(year, month, day) + TimeToTicks(hour, minute, second);
        }

        private static long DateToTicks(int year, int month, int day)
        {
            if (!IsValidDate(year, month, day))
            {
                throw new ArgumentOutOfRangeException(null, "Year, Month, and Day parameters describe an un-representable DateTime.");
            }

            var daysToMonth = GetDaysToMonth(year);
            var yearIndex = year - 1;
            var daysSinceTheBeginningOfTime = (yearIndex * 365) + (yearIndex / 4) - (yearIndex / 100) + (yearIndex / 400) + daysToMonth[month - 1] + day - 1;

            return daysSinceTheBeginningOfTime * TicksPerDay;
        }

        private static int[] GetDaysToMonth(int year)
        {
            return DateTime.IsLeapYear(year) ? DaysToMonth366 : DaysToMonth365;
        }

        private static bool IsValidDate(int year, int month, int day)
        {
            if (!IsValidYear(year) || !IsValidMonth(month))
            {
                return false;
            }

            var daysToMonth = GetDaysToMonth(year);
            return day >= 1 && day <= daysToMonth[month] - daysToMonth[month - 1];
        }

        private static bool IsValidHour(int hour)
        {
            return hour >= 0 && hour < 24;
        }

        private static bool IsValidMinute(int minute)
        {
            return minute >= 0 && minute < 60;
        }

        private static bool IsValidMonth(int month)
        {
            return month >= 1 && month <= 12;
        }

        private static bool IsValidSecond(int second)
        {
            return second >= 0 && second < 60;
        }

        private static bool IsValidTime(int hour, int minute, int second)
        {
            return IsValidHour(hour) && IsValidMinute(minute) && IsValidSecond(second);
        }

        private static bool IsValidYear(int year)
        {
            return year >= 1 && year <= 9999;
        }

        private static long TimeToTicks(int hour, int minute, int second)
        {
            if (!IsValidTime(hour, minute, second))
            {
                throw new ArgumentOutOfRangeException(null, "Hour, Minute, and Second parameters describe an un-representable DateTime.");
            }

            long totalSeconds = (hour * 3600) + (minute * 60) + second;
            return totalSeconds * TicksPerSecond;
        }
    }
}
