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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Hana;
using FluentMigrator.Tests.Helpers;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Hana
{
    [TestFixture]
    [Category("Integration")]
    [Category("Hana")]
    public class HanaSchemaExtensionsTests : BaseSchemaExtensionsTests
    {
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private HanaProcessor Processor { get; set; }

        [Test]
        public override void CallingColumnExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new HanaTestTable(Processor, "test'schema", "id int"))
                Processor.ColumnExists("test'schema", table.Name, "id").ShouldBeTrue();
        }

        [Test]
        public override void CallingConstraintExistsCanAcceptSchemaNameWithSingleQuote()
        {
            Assert.Ignore("Hana doesn't support check constraints");
        }

        [Test]
        public override void CallingIndexExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new HanaTestTable(Processor, "test'schema", "\"id\" int"))
            {
                var indexName = table.WithIndexOn("id");
                Processor.IndexExists("test'schema", table.Name, indexName).ShouldBeTrue();
            }
        }

        [Test]
        public override void CallingSchemaExistsCanAcceptSchemaNameWithSingleQuote()
        {
            Assert.Ignore("Schemas aren't supported by this SAP Hana runner");
        }

        [Test]
        public override void CallingTableExistsCanAcceptSchemaNameWithSingleQuote()
        {
            using (var table = new HanaTestTable(Processor, "test'schema", "id int"))
                Processor.TableExists("test'schema", table.Name).ShouldBeTrue();
        }

        [Test]
        public void CallingDefaultValueExistsCanAcceptSchemaNameWithSingleQuote()
        {
            Assert.Ignore("Hana doesn't support changing a columns default constraint");
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            IntegrationTestOptions.Hana.IgnoreIfNotEnabled();

            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(builder => builder.AddHana())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Hana.ConnectionString));
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
            Processor = ServiceScope.ServiceProvider.GetRequiredService<HanaProcessor>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }
    }
}
