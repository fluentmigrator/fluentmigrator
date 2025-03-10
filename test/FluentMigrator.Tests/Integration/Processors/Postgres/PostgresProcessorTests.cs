#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using System.Data;
using System.IO;

using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Logging;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Tests.Helpers;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Postgres
{
    [TestFixture]
    [Category("Integration")]
    [Category("Postgres")]
    public class PostgresProcessorTests
    {
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private PostgresProcessor Processor { get; set; }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnExistsInDifferentSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema1", "id int"))
                Processor.ColumnExists("TestSchema2", table.Name, "id").ShouldBeFalse();
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintExistsInDifferentSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema1", "id int",
                "wibble int CONSTRAINT c1 CHECK(wibble > 0)"))
                Processor.ConstraintExists("TestSchema2", table.Name, "c1").ShouldBeFalse();
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableExistsInDifferentSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema1", "id int"))
                Processor.TableExists("TestSchema2", table.Name).ShouldBeFalse();
        }

        [Test]
        public void CanReadData()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
            {
                AddTestData(table);

                DataSet ds = Processor.Read("SELECT * FROM \"{0}\"", table.Name);

                ds.ShouldNotBeNull();
                ds.Tables.Count.ShouldBe(1);
                ds.Tables[0].Rows.Count.ShouldBe(3);
                ds.Tables[0].Rows[2][0].ShouldBe(2);
            }
        }

        [Test]
        public void CanReadTableData()
        {
            using (var table = new PostgresTestTable(Processor, null, "id int"))
            {
                AddTestData(table);

                DataSet ds = Processor.ReadTableData(null, table.Name);

                ds.ShouldNotBeNull();
                ds.Tables.Count.ShouldBe(1);
                ds.Tables[0].Rows.Count.ShouldBe(3);
                ds.Tables[0].Rows[2][0].ShouldBe(2);
            }
        }

        private void AddTestData(PostgresTestTable table)
        {
            for (int i = 0; i < 3; i++)
            {
                var cmd = table.Connection.CreateCommand();
                cmd.Transaction = table.Transaction;
                cmd.CommandText = $"INSERT INTO {table.NameWithSchema} (id) VALUES ({i})";
                cmd.ExecuteNonQuery();
            }
        }


        [Test]
        public void CanReadDataWithSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
            {
                AddTestData(table);

                DataSet ds = Processor.Read("SELECT * FROM {0}", table.NameWithSchema);

                ds.ShouldNotBeNull();
                ds.Tables.Count.ShouldBe(1);
                ds.Tables[0].Rows.Count.ShouldBe(3);
                ds.Tables[0].Rows[2][0].ShouldBe(2);
            }
        }

        [Test]
        public void CanReadTableDataWithSchema()
        {
            using (var table = new PostgresTestTable(Processor, "TestSchema", "id int"))
            {
                AddTestData(table);

                DataSet ds = Processor.ReadTableData("TestSchema", table.Name);

                ds.ShouldNotBeNull();
                ds.Tables.Count.ShouldBe(1);
                ds.Tables[0].Rows.Count.ShouldBe(3);
                ds.Tables[0].Rows[2][0].ShouldBe(2);
            }
        }

        [Test]
        public void CallingProcessWithPerformDbOperationExpressionWhenInPreviewOnlyModeWillNotMakeDbChanges()
        {
            var output = new StringWriter();

            var sp = CreateProcessorServices(
                services => services
                    .AddSingleton<ILoggerProvider>(new SqlScriptFluentMigratorLoggerProvider(output))
                    .ConfigureRunner(r => r.AsGlobalPreview()));
            using (sp)
            {
                using (var scope = sp.CreateScope())
                {
                    var processor = scope.ServiceProvider.GetRequiredService<IMigrationProcessor>();

                    bool tableExists;

                    try
                    {
                        var expression =
                            new PerformDBOperationExpression
                            {
                                Operation = (con, trans) =>
                                {
                                    var command = con.CreateCommand();
                                    command.CommandText = "CREATE TABLE processtesttable (test int NULL) ";
                                    command.Transaction = trans;

                                    command.ExecuteNonQuery();
                                }
                            };

                        processor.BeginTransaction();
                        processor.Process(expression);

                        tableExists = processor.TableExists("public", "processtesttable");
                    }
                    finally
                    {
                        processor.RollbackTransaction();
                    }

                    tableExists.ShouldBeFalse();
                    output.ToString().ShouldBe(
                        @"/* Beginning Transaction */
/* Performing DB Operation */
/* Rolling back transaction */
");
                }
            }
        }

        private ServiceProvider CreateProcessorServices([CanBeNull] Action<IServiceCollection> initAction)
        {
            IntegrationTestOptions.Postgres.IgnoreIfNotEnabled();

            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(r => r.AddPostgres())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Postgres.ConnectionString));

            initAction?.Invoke(serivces);

            return serivces.BuildServiceProvider();
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            ServiceProvider = CreateProcessorServices(initAction: null);
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
            Processor = ServiceScope.ServiceProvider.GetRequiredService<PostgresProcessor>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }
    }
}
