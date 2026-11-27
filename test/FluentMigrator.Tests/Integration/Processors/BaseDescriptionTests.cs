#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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
using System.Collections.Generic;
using System.Data;
using System.Linq;

using FluentMigrator.Exceptions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.SyntaxRoots;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors
{
    /// <summary>
    /// Every fluent syntax root that sets a table or column description, run on a real database, with the descriptions
    /// read back from its catalog (issue #2351).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The scenarios are the ones the generator contract (<c>BaseDescriptionContractTests</c>) checks without a
    /// database: one migration per syntax root (<see cref="DescriptionScenario"/>). Here each runs through the runner
    /// on the database, and the test reads what the catalog stored. That proves the SQL is valid on the database and
    /// that it stores the descriptions where the database keeps them.
    /// </para>
    /// <para>
    /// A subclass says which database to use and how to read its catalog. Its database starts from the test settings
    /// like the other processor integration tests, or in a container when <c>ContainerEnabled</c> is set. Each test
    /// drops its table before and after.
    /// </para>
    /// </remarks>
    [Category("Integration")]
    [Category("Description")]
    public abstract class BaseDescriptionTests
    {
        private const string KeptTableDescription = "Kept table description";

        private const string KeptColumnDescription = "Kept column description";

        private string _connectionString;

        /// <summary>
        /// The database's test settings.
        /// </summary>
        protected abstract IntegrationTestOptions.DatabaseServerOptions ServerOptions { get; }

        /// <summary>
        /// Adds the database to the runner, for example <c>builder.AddPostgres()</c>.
        /// </summary>
        protected abstract void ConfigureRunner(IMigrationRunnerBuilder builder);

        /// <summary>
        /// SQL that returns the table's description in its first column, or <c>null</c> where FluentMigrator has no
        /// way to store one on this database.
        /// </summary>
        protected abstract string TableDescriptionSql(string tableName);

        /// <summary>
        /// SQL that returns every description stored for the column, one per row, in the first column, or <c>null</c>
        /// where FluentMigrator has no way to store one on this database.
        /// </summary>
        protected abstract string ColumnDescriptionsSql(string tableName, string columnName);

        /// <summary>
        /// What the database does with the descriptions of <paramref name="root"/>. All are stored unless a subclass
        /// says otherwise.
        /// </summary>
        protected virtual DescriptionSupport Support(DescriptionRoot root) => DescriptionSupport.Written;

        /// <summary>
        /// A known bug, outside descriptions, that makes the database reject the SQL of <paramref name="root"/>, or
        /// <c>null</c>. The test then checks that the migration still fails, so fixing the bug turns into a failing test
        /// that says to remove it here.
        /// </summary>
        protected virtual string KnownFailure(DescriptionRoot root) => null;

        /// <summary>
        /// The connection string of the database the tests run on. By default the one in the test settings. A
        /// subclass that makes a database for the tests drops it in a <c>[OneTimeTearDown]</c> of its own.
        /// </summary>
        protected virtual string OpenDatabase() => ServerOptions.ConnectionString;

        public static IEnumerable<DescriptionScenario> Scenarios => DescriptionScenario.All;

        [OneTimeSetUp]
        public void IgnoreIfNotEnabled() => ServerOptions.IgnoreIfNotEnabled();

        /// <summary>
        /// Read when the first test runs, after the subclass's own <c>[OneTimeSetUp]</c>, if any.
        /// </summary>
        private string ConnectionString => _connectionString ??= OpenDatabase();

        [TestCaseSource(nameof(Scenarios))]
        public void TheDatabaseStoresEveryDescription(DescriptionScenario scenario)
        {
            DropTable(scenario.TableName);
            try
            {
                if (scenario.NeedsExistingTable)
                {
                    Run(scenario.Arrange());
                }

                if (Support(scenario.Root) == DescriptionSupport.RootNotSupported)
                {
                    Assert.Throws<DatabaseOperationNotSupportedException>(() => Run(scenario.Act()));
                    return;
                }

                var knownFailure = KnownFailure(scenario.Root);
                if (knownFailure != null)
                {
                    Assert.That(
                        () => Run(scenario.Act()),
                        Throws.Exception,
                        $"Known failure: {knownFailure}. If {scenario.Syntax} works now, remove it from {GetType().Name}.{nameof(KnownFailure)}.");
                    return;
                }

                Run(scenario.Act());

                var stored = string.Join(
                    Environment.NewLine,
                    ReadTableDescription(scenario.TableName).Concat(ReadColumnDescriptions(scenario.TableName, DescriptionScenario.Column)));

                Assert.Multiple(
                    () =>
                    {
                        foreach (var text in scenario.ExpectedTexts)
                        {
                            if (Support(scenario.Root) == DescriptionSupport.Written)
                            {
                                Assert.That(stored, Does.Contain(text), $"{scenario.Syntax} should store \"{text}\"");
                            }
                            else
                            {
                                Assert.That(stored, Does.Not.Contain(text), $"{scenario.Syntax} is declared NotWritten but stores \"{text}\"");
                            }
                        }
                    });
            }
            finally
            {
                DropTable(scenario.TableName);
            }
        }

        /// <summary>
        /// <c>IfNotExists()</c> leaves an existing table as it is, its descriptions included.
        /// </summary>
        [Test]
        public void IfNotExistsKeepsTheExistingTablesDescriptions()
        {
            var scenario = DescriptionScenario.All.Single(s => s.Root == DescriptionRoot.CreateTableIfNotExists);
            if (Support(scenario.Root) != DescriptionSupport.Written)
            {
                Assert.Ignore("The database stores no descriptions, so there are none to keep.");
            }

            DropTable(scenario.TableName);
            try
            {
                // The table as someone else made it, with descriptions of its own.
                Run(new DelegateMigration(
                    m => m.Create.Table(scenario.TableName).WithDescription(KeptTableDescription)
                        .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_" + scenario.TableName)
                        .WithColumn(DescriptionScenario.Column).AsString(100).Nullable().WithColumnDescription(KeptColumnDescription)));

                Run(scenario.Act());

                var table = string.Join(Environment.NewLine, ReadTableDescription(scenario.TableName));
                var column = string.Join(Environment.NewLine, ReadColumnDescriptions(scenario.TableName, DescriptionScenario.Column));
                Assert.Multiple(
                    () =>
                    {
                        Assert.That(table, Does.Contain(KeptTableDescription), "the table's description");
                        Assert.That(table, Does.Not.Contain(DescriptionScenario.TableDescription), "the table's description");
                        Assert.That(column, Does.Contain(KeptColumnDescription), "the column's description");
                        Assert.That(column, Does.Not.Contain(DescriptionScenario.ColumnDescription), "the column's description");
                    });
            }
            finally
            {
                DropTable(scenario.TableName);
            }
        }

        protected void Run(IMigration migration) =>
            SyntaxRootRunner.Live(ConfigureRunner, ConnectionString, arrange: null, act: migration);

        protected void DropTable(string tableName) =>
            Run(new DelegateMigration(
                m =>
                {
                    if (m.Schema.Table(tableName).Exists())
                    {
                        m.Delete.Table(tableName);
                    }
                }));

        private IEnumerable<string> ReadTableDescription(string tableName) => Read(TableDescriptionSql(tableName));

        private IEnumerable<string> ReadColumnDescriptions(string tableName, string columnName) => Read(ColumnDescriptionsSql(tableName, columnName));

        private IReadOnlyList<string> Read(string sql)
        {
            if (sql == null)
            {
                return Array.Empty<string>();
            }

            using var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder =>
                    {
                        ConfigureRunner(builder);
                        builder.WithGlobalConnectionString(ConnectionString);
                    })
                .BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IProcessorAccessor>().Processor;

            using var data = processor.Read(sql);
            return data.Tables[0].Rows.Cast<DataRow>()
                .Select(row => row[0] as string)
                .Where(value => !string.IsNullOrEmpty(value))
                .ToList();
        }
    }
}
