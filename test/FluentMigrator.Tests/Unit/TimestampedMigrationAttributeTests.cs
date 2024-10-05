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

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    [SetUICulture("en-US")]
    public class TimestampedMigrationAttributeTests
    {
        private const int DayOfMonth = 15;

        private const string Description = "Description";

        private const int Hour = 12;

        private const string InvalidDateExceptionMessage = "Year, Month, and Day parameters describe an un-representable DateTime.";

        private const string InvalidTimeExceptionMessage = "Hour, Minute, and Second parameters describe an un-representable DateTime.";

        private const int Minute = 30;

        private const int Month = 06;

        private const int Second = 30;

        private const int Year = 2000;

        [Test]
        public void CanCreateOneAccurateToTheMinute()
        {
            Assert.That(() => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute), Throws.Nothing);
        }

        [Test]
        public void CanCreateOneAccurateToTheMinuteWithDescription()
        {
            Assert.That(() => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, Description), Throws.Nothing);
        }

        [Test]
        public void CanCreateOneAccurateToTheMinuteWithTransactionBehavior()
        {
            Assert.That(() => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, TransactionBehavior.Default), Throws.Nothing);
        }

        [Test]
        public void CanCreateOneAccurateToTheMinuteWithTransactionBehaviorAndDescription()
        {
            Assert.That(() => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, TransactionBehavior.Default, Description), Throws.Nothing);
        }

        [Test]
        public void CanCreateOneAccurateToTheSecond()
        {
            Assert.That(() => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, Second), Throws.Nothing);
        }

        [Test]
        public void CanCreateOneAccurateToTheSecondWithDescription()
        {
            Assert.That(() => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, Second, Description), Throws.Nothing);
        }

        [Test]
        public void CanCreateOneAccurateToTheSecondWithTransactionBehavior()
        {
            Assert.That(
                () => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, Second, TransactionBehavior.Default),
                Throws.Nothing);
        }

        [Test]
        public void CanCreateOneAccurateToTheSecondWithTransactionBehaviorAndDescription()
        {
            Assert.That(
                () => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, Second, TransactionBehavior.Default, Description),
                Throws.Nothing);
        }

        [Test]
        public void CreatingOneSetsUnderlyingValues()
        {
            var attribute = new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, Second, TransactionBehavior.None, Description);
            Assert.Multiple(() =>
            {
                Assert.That(attribute.Description, Is.EqualTo(Description));
                Assert.That(attribute.TransactionBehavior, Is.EqualTo(TransactionBehavior.None));
                Assert.That(attribute.Version, Is.EqualTo(20000615123030));
            });
        }

        [Test]
        public void ExtendsMigrationAttribute()
        {
            var timestampedMigrationAttribute = new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute);
            Assert.That(timestampedMigrationAttribute, Is.InstanceOf<MigrationAttribute>());
        }

        [Test]
        public void TryingToCreateWithInvalidDayOfMonthResultsInArgumentOutOfRangeException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new TimestampedMigrationAttribute(Year, Month, 99, Hour, Minute));
            Assert.That(exception.Message, Is.EqualTo(InvalidDateExceptionMessage));
        }

        [Test]
        public void TryingToCreateWithInvalidHourResultsInArgumentOutOfRangeException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, 99, Minute));
            Assert.That(exception.Message, Is.EqualTo(InvalidTimeExceptionMessage));
        }

        [Test]
        public void TryingToCreateWithInvalidMinuteResultsInArgumentOutOfRangeException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, 99));
            Assert.That(exception.Message, Is.EqualTo(InvalidTimeExceptionMessage));
        }

        [Test]
        public void TryingToCreateWithInvalidMonthResultsInArgumentOutOfRangeException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new TimestampedMigrationAttribute(Year, 99, DayOfMonth, Hour, Minute));
            Assert.That(exception.Message, Is.EqualTo(InvalidDateExceptionMessage));
        }

        [Test]
        public void TryingToCreateWithInvalidSecondResultsInArgumentOutOfRangeException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new TimestampedMigrationAttribute(Year, Month, DayOfMonth, Hour, Minute, 99));
            Assert.That(exception.Message, Is.EqualTo(InvalidTimeExceptionMessage));
        }

        [Test]
        public void TryingToCreateWithInvalidYearResultsInArgumentOutOfRangeException()
        {
            // ReSharper disable once ObjectCreationAsStatement
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new TimestampedMigrationAttribute(0000, Month, DayOfMonth, Hour, Minute));
            Assert.That(exception.Message, Is.EqualTo(InvalidDateExceptionMessage));
        }
    }
}
