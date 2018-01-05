#region License
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class VersionOrderInvalidExceptionTests
    {
        [Test]
        public void InvalidMigrationsPopulated()
        {
            var migrations = new[]
                                 {
                                     new KeyValuePair<long,IMigrationInfo>(1, new MigrationInfo(1, TransactionBehavior.Default, false, new TestMigration1())),
                                     new KeyValuePair<long,IMigrationInfo>(2, new MigrationInfo(2, TransactionBehavior.Default, false, new TestMigration2()))
                                 };


            var exception = new VersionOrderInvalidException(migrations);

            exception.InvalidMigrations.ShouldBe(migrations);
        }

        [Test]
        public void ExceptionMessageListsInvalidMigrations()
        {
            var migrations = new[]
                                 {
                                     new KeyValuePair<long,IMigrationInfo>(1, new MigrationInfo(1, TransactionBehavior.Default, false, new TestMigration1())),
                                     new KeyValuePair<long,IMigrationInfo>(2, new MigrationInfo(2, TransactionBehavior.Default, false, new TestMigration2()))
                                 };

            var exception = new VersionOrderInvalidException(migrations);

            var expectedMessage = "Unapplied migrations have version numbers that are less than the greatest version number of applied migrations:"
                + Environment.NewLine + "1 - TestMigration1"
                + Environment.NewLine + "2 - TestMigration2";

            System.Console.WriteLine(exception.Message);

            exception.Message.ShouldBe(expectedMessage);
        }
   }


    class TestMigration1 : Migration
    {
        public override void Up() { }
        public override void Down() { }
    }

    class TestMigration2 : Migration
    {
        public override void Up() { }
        public override void Down() { }
    }
}