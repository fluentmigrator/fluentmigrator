#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Globalization;

namespace FluentMigrator
{
    /// <summary>
    /// Creates a MigrationAttribute which executes in order based on the given date and time.
    /// </summary>
    public class TimestampedMigrationAttribute : MigrationAttribute
    {
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
            : base(DateTimeToFormattedInt(year, month, day, hour, minute, second))
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
            : base(DateTimeToFormattedInt(year, month, day, hour, minute, 0))
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
            : base(DateTimeToFormattedInt(year, month, day, hour, minute, 0), description)
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
            : base(DateTimeToFormattedInt(year, month, day, hour, minute, second), description)
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
            : base(DateTimeToFormattedInt(year, month, day, hour, minute, 0), transactionBehavior)
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
            : base(DateTimeToFormattedInt(year, month, day, hour, minute, second), transactionBehavior)
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
            : base(DateTimeToFormattedInt(year, month, day, hour, minute, 0), transactionBehavior, description)
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
            : base(DateTimeToFormattedInt(year, month, day, hour, minute, second), transactionBehavior, description)
        {
        }

        private static long DateTimeToFormattedInt(int year, int month, int day, int hour, int minute, int second)
        {
            var dt = new DateTime(year, month, day, hour, minute, second);
            var timestampAsString = dt.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            return long.Parse(timestampAsString, CultureInfo.InvariantCulture);
        }
    }
}
