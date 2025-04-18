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

using System.Diagnostics;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.DB2;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.DB2;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Db2
{
    [TestFixture]
    [Category("Db2")]
    public class Db2ColumnTests : BaseColumnTests
    {
        static Db2ColumnTests()
        {
            try { EnsureReference(); } catch { /* ignore */ }
        }

        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private Db2Processor Processor { get; set; }
        private Db2Quoter Quoter { get; set; }

        [Test]
        public override void CallingColumnExistsCanAcceptColumnNameWithSingleQuote()
        {
            var columnName = Quoter.Quote("I'D") + " INT";
            using (var table = new Db2TestTable(Processor, null, columnName))
            {
                Processor.ColumnExists(null, table.Name, "I'D").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingColumnExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new Db2TestTable("Test'Table", Processor, null, "ID INT"))
            {
                Processor.ColumnExists(null, table.Name, "ID").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExist()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.ColumnExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfColumnDoesNotExistWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.ColumnExists("TstSchma", table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.ColumnExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.ColumnExists("TstSchma", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExists()
        {
            using (var table = new Db2TestTable(Processor, null, "ID INT"))
            {
                Processor.ColumnExists(null, table.Name, "ID").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingColumnExistsReturnsTrueIfColumnExistsWithSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.ColumnExists("TstSchma", table.Name, "ID").ShouldBeTrue();
            }
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            IntegrationTestOptions.Db2.IgnoreIfNotEnabled();

            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(builder => builder.AddDb2())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Db2.ConnectionString));
            ServiceProvider = serivces.BuildServiceProvider();
        }

        [OneTimeTearDown]
        public void ClassTearDown()
        {
            ServiceProvider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            ServiceScope = ServiceProvider.CreateScope();
            Processor = ServiceScope.ServiceProvider.GetRequiredService<Db2Processor>();
            Quoter = ServiceScope.ServiceProvider.GetRequiredService<Db2Quoter>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }

        private static void EnsureReference()
        {
            // This is here to avoid the removal of the referenced assembly
            Debug.WriteLine(typeof(IBM.Data.DB2.DB2Factory));
        }
    }
}
