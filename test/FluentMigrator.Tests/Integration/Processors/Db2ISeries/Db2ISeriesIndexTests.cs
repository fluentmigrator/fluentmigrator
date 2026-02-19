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

#if !NETFRAMEWORK
using System.Diagnostics;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.DB2.iSeries;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Db2ISeries
{
    [TestFixture]
    [Category("Integration")]
    [Category("DB2 iSeries")]
    public class Db2ISeriesIndexTests : BaseIndexTests
    {
        static Db2ISeriesIndexTests()
        {
            try { EnsureReference(); } catch { /* ignore */ }
        }

        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private Db2ISeriesProcessor Processor { get; set; }

        [Test]
        public override void CallingIndexExistsCanAcceptIndexNameWithSingleQuote()
        {
            using (var table = new Db2ISeriesTestTable(Processor, null, "ID INT"))
            {
                table.WithIndexOn("ID", "UI'id");
                Processor.IndexExists(null, table.Name, "UI'id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsCanAcceptTableNameWithSingleQuote()
        {
            using (var table = new Db2ISeriesTestTable("Test'Table", Processor, null, "ID INT"))
            {
                table.WithIndexOn("ID", "UI_id");
                Processor.IndexExists(null, table.Name, "UI_id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExist()
        {
            using (var table = new Db2ISeriesTestTable(Processor, null, "ID INT"))
            {
                Processor.IndexExists(null, table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfIndexDoesNotExistWithSchema()
        {
            using (var table = new Db2ISeriesTestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.IndexExists("TstSchma", table.Name, "DoesNotExist").ShouldBeFalse();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExist()
        {
            Processor.IndexExists(null, "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsFalseIfTableDoesNotExistWithSchema()
        {
            Processor.IndexExists("TstSchma", "DoesNotExist", "DoesNotExist").ShouldBeFalse();
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExists()
        {
            using (var table = new Db2ISeriesTestTable(Processor, null, "ID INT"))
            {
                table.WithIndexOn("ID", "UI_id");
                Processor.IndexExists(null, table.Name, "UI_id").ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingIndexExistsReturnsTrueIfIndexExistsWithSchema()
        {
            using (var table = new Db2ISeriesTestTable(Processor, "TstSchma", "ID INT"))
            {
                table.WithIndexOn("ID", "UI_id");
                Processor.IndexExists("TstSchma", table.Name, "UI_id").ShouldBeTrue();
            }
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            IntegrationTestOptions.Db2ISeries.IgnoreIfNotEnabled();

            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(builder => builder.AddDb2ISeries())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Db2ISeries.ConnectionString));
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
            Processor = ServiceScope.ServiceProvider.GetRequiredService<Db2ISeriesProcessor>();
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
            Debug.WriteLine(typeof(IBM.Data.Db2.DB2Factory));
        }
    }
}
#endif
