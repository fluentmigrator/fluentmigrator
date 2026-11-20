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

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.DB2;
using FluentMigrator.Tests.Helpers;

using IBM.Data.Db2;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.Db2
{
    [TestFixture]
    [Category("Integration")]
    [Category("Db2")]
    public class Db2ProcessorTests
    {
        private const string CheckSchema = "CHECK_TEST_SCHEMA";
        private const string CheckName = "POSITIVE_ID";

        static Db2ProcessorTests()
        {
            try { EnsureReference(); } catch { /* ignore */ }
        }

        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private Db2Processor Processor { get; set; }

        [Test]
        public void CallingColumnExistsReturnsFalseIfColumnExistsInDifferentSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.ColumnExists("DNE", table.Name, "ID").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingConstraintExistsReturnsFalseIfConstraintExistsInDifferentSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT NOT NULL"))
            {
                table.WithUniqueConstraintOn("ID", "c1");
                Processor.ConstraintExists("DNE", table.Name, "c1").ShouldBeFalse();
            }
        }

        [Test]
        public void CallingTableExistsReturnsFalseIfTableExistsInDifferentSchema()
        {
            using (var table = new Db2TestTable(Processor, "TstSchma", "ID INT"))
            {
                Processor.TableExists("DNE", table.Name).ShouldBeFalse();
            }
        }

        [Test]
        public void ACheckCreatedDisabledIsExcludedFromQueryOptimisation()
        {
            using var table = new Db2TestTable(Processor, CheckSchema, "ID INT");
            new CreateCheckConstraintExpression
            {
                Constraint =
                {
                    Name = CheckName,
                    TableName = table.Name,
                    SchemaName = CheckSchema,
                    Condition = "ID > 0",
                    Enabled = false,
                },
            }.ExecuteWith(Processor);

            AssertCheckEnforcementAndOptimisation(table, "N");
        }

        [TestCase(CheckConstraintAction.Disable, "N")]
        [TestCase(CheckConstraintAction.Enable, "Y")]
        public void ChangingEnforcementAlsoChangesQueryOptimisation(CheckConstraintAction action, string expected)
        {
            using var table = new Db2TestTable(Processor, CheckSchema, "ID INT");
            new CreateCheckConstraintExpression
            {
                Constraint =
                {
                    Name = CheckName,
                    TableName = table.Name,
                    SchemaName = CheckSchema,
                    Condition = "ID > 0",
                    Enabled = action == CheckConstraintAction.Disable,
                },
            }.ExecuteWith(Processor);

            new AlterCheckConstraintExpression
            {
                Action = action,
                Constraint = { Name = CheckName, TableName = table.Name, SchemaName = CheckSchema },
            }.ExecuteWith(Processor);

            AssertCheckEnforcementAndOptimisation(table, expected);
        }

        [Test]
        public void EnablingACheckValidatesExistingRows()
        {
            using var table = new Db2TestTable(Processor, CheckSchema, "ID INT");
            new CreateCheckConstraintExpression
            {
                Constraint =
                {
                    Name = CheckName,
                    TableName = table.Name,
                    SchemaName = CheckSchema,
                    Condition = "ID > 0",
                    Enabled = false,
                },
            }.ExecuteWith(Processor);
            Processor.Execute($"INSERT INTO {table.NameWithSchema} (ID) VALUES (0)");

            var expression = new AlterCheckConstraintExpression
            {
                Action = CheckConstraintAction.Enable,
                Constraint = { Name = CheckName, TableName = table.Name, SchemaName = CheckSchema },
            };

            var exception = Should.Throw<DB2Exception>(() => expression.ExecuteWith(Processor));
            exception.Errors[0].SQLState.ShouldBe("23512");
        }

        [Test]
        public void ChangingAConditionReplacesTheCheckWithinTheMigrationTransaction()
        {
            using var table = new Db2TestTable(Processor, CheckSchema, "ID INT");
            new CreateCheckConstraintExpression
            {
                Constraint =
                {
                    Name = CheckName,
                    TableName = table.Name,
                    SchemaName = CheckSchema,
                    Condition = "ID > 0",
                },
            }.ExecuteWith(Processor);

            Processor.BeginTransaction();
            try
            {
                new AlterCheckConstraintExpression
                {
                    Action = CheckConstraintAction.ChangeCondition,
                    Constraint =
                    {
                        Name = CheckName,
                        TableName = table.Name,
                        SchemaName = CheckSchema,
                        Condition = "ID < 0",
                    },
                }.ExecuteWith(Processor);

                Processor.Execute($"INSERT INTO {table.NameWithSchema} (ID) VALUES (-1)");
                var exception = Should.Throw<DB2Exception>(
                    () => Processor.Execute($"INSERT INTO {table.NameWithSchema} (ID) VALUES (1)"));
                exception.Errors[0].SQLState.ShouldBe("23513");
            }
            finally
            {
                Processor.RollbackTransaction();
            }
        }

        private void AssertCheckEnforcementAndOptimisation(Db2TestTable table, string expected)
        {
            using var result = Processor.Read(
                "SELECT ENFORCED, ENABLEQUERYOPT FROM SYSCAT.TABCONST WHERE TABSCHEMA = '{0}' AND TABNAME = '{1}' AND CONSTNAME = '{2}' AND TYPE = 'K'",
                CheckSchema,
                table.Name.ToUpperInvariant(),
                CheckName);
            var row = result.Tables[0].Rows[0];
            Assert.That(((string)row["ENFORCED"], (string)row["ENABLEQUERYOPT"]), Is.EqualTo((expected, expected)));
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
