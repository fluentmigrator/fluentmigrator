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
using System.Text.RegularExpressions;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Integration.CreationScript.ConditionalCreate;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.CreationScript
{
    /// <summary>
    /// Issue #2444: <c>Create.Table(...).IfNotExists()</c>. On the databases that have no <c>CREATE TABLE IF NOT EXISTS</c>
    /// (SQL Server, Oracle, Firebird) the generators used to return no SQL in the default LOOSE compatibility mode, so
    /// the table was never created and the migration was still recorded as applied. On every database, the indexes and
    /// foreign keys declared on the columns were separate statements, which failed when the table already had them.
    /// </summary>
    /// <remarks>
    /// Each test scripts <see cref="ConditionalCreateMigrations"/> the way
    /// <c>dotnet fm migrate --no-connection --preview --preview-from-nothing --output</c> does, and runs the script on a
    /// real database the way the database's own tool would: once on an empty database, and once on a database that
    /// already has the tables, with their indexes and foreign key. The live runner is checked the same way.
    /// </remarks>
    [TestFixture]
    [Category("Integration")]
    [Category("CreationScript")]
    public class CreateTableIfNotExistsScriptTests
    {
        private const string ExistingDescription = "Created before the migration";

        private static IEnumerable<TestCaseData> Targets() =>
            CreationScriptTarget.All
                .Where(t => ExistingTable.ContainsKey(t.Name))
                .Select(t => new TestCaseData(t).SetArgDisplayNames(t.Name).SetCategory(t.Name));

        [TestCaseSource(nameof(Targets))]
        public void ScriptCreatesTheMissingTable(CreationScriptTarget target)
        {
            var serverOptions = target.ServerOptions;
            serverOptions.IgnoreIfNotEnabled();

            var script = GenerateScript(target);

            try
            {
                Assert.That(
                    Regex.IsMatch(script, $@"CREATE TABLE (IF NOT EXISTS )?[^\s(]*{ConditionalCreateMigrations.Table}\b"),
                    Is.True,
                    "The script has no CREATE TABLE for the table");

                using var database = target.CreateEmptyDatabase(serverOptions.ConnectionString);

                target.RunScript(database.ConnectionString, script);

                Verify(target, database.ConnectionString, tableExisted: false);
            }
            catch
            {
                TestContext.Out.WriteLine($"The {target.Name} script:");
                TestContext.Out.WriteLine(script);
                throw;
            }
        }

        [TestCaseSource(nameof(Targets))]
        public void ScriptKeepsAnExistingTable(CreationScriptTarget target)
        {
            var serverOptions = target.ServerOptions;
            serverOptions.IgnoreIfNotEnabled();

            var script = GenerateScript(target);

            try
            {
                using var database = target.CreateEmptyDatabase(serverOptions.ConnectionString);
                target.RunScript(database.ConnectionString, ExistingTable[target.Name]);

                target.RunScript(database.ConnectionString, script);

                Verify(target, database.ConnectionString, tableExisted: true);
            }
            catch
            {
                TestContext.Out.WriteLine($"The {target.Name} script:");
                TestContext.Out.WriteLine(script);
                throw;
            }
        }

        [TestCaseSource(nameof(Targets))]
        public void LiveRunCreatesTheMissingTable(CreationScriptTarget target)
        {
            var serverOptions = target.ServerOptions;
            serverOptions.IgnoreIfNotEnabled();

            using var database = target.CreateEmptyDatabase(serverOptions.ConnectionString);

            MigrateUp(target, database.ConnectionString);

            Verify(target, database.ConnectionString, tableExisted: false);
        }

        [TestCaseSource(nameof(Targets))]
        public void LiveRunKeepsAnExistingTable(CreationScriptTarget target)
        {
            var serverOptions = target.ServerOptions;
            serverOptions.IgnoreIfNotEnabled();

            using var database = target.CreateEmptyDatabase(serverOptions.ConnectionString);
            target.RunScript(database.ConnectionString, ExistingTable[target.Name]);

            MigrateUp(target, database.ConnectionString);

            Verify(target, database.ConnectionString, tableExisted: true);
        }

        /// <summary>
        /// The tables as someone else made them. The widget table has the index, unique index and foreign key the
        /// migration declares, so creating any of them again would fail, and also an extra column, a row and a table
        /// description, none of which the migration would create. Descriptions that ran anyway would fail (SQL Server)
        /// or overwrite this one (Oracle).
        /// </summary>
        private static readonly Dictionary<string, string> ExistingTable = new()
        {
            ["SqlServer2016"] = string.Join(
                Environment.NewLine + "GO" + Environment.NewLine,
                "CREATE TABLE [dbo].[FmWidgetKind] ([Id] INT NOT NULL PRIMARY KEY)",
                "CREATE TABLE [dbo].[FmWidget] ([Id] INT NOT NULL PRIMARY KEY, [Name] NVARCHAR(100) NOT NULL, [KindId] INT NULL, [Marker] NVARCHAR(20) NULL)",
                "CREATE UNIQUE INDEX [UX_FmWidget_Name] ON [dbo].[FmWidget] ([Name])",
                "CREATE INDEX [IX_FmWidget_KindId] ON [dbo].[FmWidget] ([KindId])",
                "ALTER TABLE [dbo].[FmWidget] ADD CONSTRAINT [FK_FmWidget_Kind] FOREIGN KEY ([KindId]) REFERENCES [dbo].[FmWidgetKind] ([Id])",
                $"EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{ExistingDescription}', @level0type=N'SCHEMA', @level0name='dbo', @level1type=N'TABLE', @level1name='FmWidget'",
                "INSERT INTO [dbo].[FmWidget] ([Id], [Name], [Marker]) VALUES (1, N'Already there', N'kept')"),
            ["Postgres15_0"] = string.Join(
                ";" + Environment.NewLine,
                "CREATE TABLE \"public\".\"FmWidgetKind\" (\"Id\" integer NOT NULL PRIMARY KEY)",
                "CREATE TABLE \"public\".\"FmWidget\" (\"Id\" integer NOT NULL PRIMARY KEY, \"Name\" varchar(100) NOT NULL, \"KindId\" integer NULL, \"Marker\" varchar(20) NULL)",
                "CREATE UNIQUE INDEX \"UX_FmWidget_Name\" ON \"public\".\"FmWidget\" (\"Name\")",
                "CREATE INDEX \"IX_FmWidget_KindId\" ON \"public\".\"FmWidget\" (\"KindId\")",
                "ALTER TABLE \"public\".\"FmWidget\" ADD CONSTRAINT \"FK_FmWidget_Kind\" FOREIGN KEY (\"KindId\") REFERENCES \"public\".\"FmWidgetKind\" (\"Id\")",
                "INSERT INTO \"public\".\"FmWidget\" (\"Id\", \"Name\", \"Marker\") VALUES (1, 'Already there', 'kept')") + ";",
            ["MySql8"] = string.Join(
                ";" + Environment.NewLine,
                "CREATE TABLE `FmWidgetKind` (`Id` INT NOT NULL PRIMARY KEY) ENGINE = INNODB",
                "CREATE TABLE `FmWidget` (`Id` INT NOT NULL PRIMARY KEY, `Name` VARCHAR(100) NOT NULL, `KindId` INT NULL, `Marker` VARCHAR(20) NULL, "
              + "UNIQUE INDEX `UX_FmWidget_Name` (`Name`), INDEX `IX_FmWidget_KindId` (`KindId`), "
              + "CONSTRAINT `FK_FmWidget_Kind` FOREIGN KEY (`KindId`) REFERENCES `FmWidgetKind` (`Id`)) "
              + $"ENGINE = INNODB COMMENT '{ExistingDescription}'",
                "INSERT INTO `FmWidget` (`Id`, `Name`, `Marker`) VALUES (1, 'Already there', 'kept')") + ";",
            ["Oracle12CManaged"] = string.Join(
                ";" + Environment.NewLine,
                "CREATE TABLE FmWidgetKind (Id NUMBER(10) NOT NULL PRIMARY KEY)",
                "CREATE TABLE FmWidget (Id NUMBER(10) NOT NULL PRIMARY KEY, Name NVARCHAR2(100) NOT NULL, KindId NUMBER(10), Marker NVARCHAR2(20))",
                "CREATE UNIQUE INDEX UX_FmWidget_Name ON FmWidget (Name)",
                "CREATE INDEX IX_FmWidget_KindId ON FmWidget (KindId)",
                "ALTER TABLE FmWidget ADD CONSTRAINT FK_FmWidget_Kind FOREIGN KEY (KindId) REFERENCES FmWidgetKind (Id)",
                $"COMMENT ON TABLE FmWidget IS '{ExistingDescription}'",
                "INSERT INTO FmWidget (Id, Name, Marker) VALUES (1, 'Already there', 'kept')") + ";",
            ["Firebird"] = string.Join(
                ";" + Environment.NewLine,
                "CREATE TABLE FmWidgetKind (Id INTEGER NOT NULL PRIMARY KEY)",
                "CREATE TABLE FmWidget (Id INTEGER NOT NULL PRIMARY KEY, Name VARCHAR(100) NOT NULL, KindId INTEGER, Marker VARCHAR(20))",
                "CREATE UNIQUE INDEX UX_FmWidget_Name ON FmWidget (Name)",
                "CREATE INDEX IX_FmWidget_KindId ON FmWidget (KindId)",
                "ALTER TABLE FmWidget ADD CONSTRAINT FK_FmWidget_Kind FOREIGN KEY (KindId) REFERENCES FmWidgetKind (Id)",
                "INSERT INTO FmWidget (Id, Name, Marker) VALUES (1, 'Already there', 'kept')") + ";",
        };

        /// <summary>
        /// The table's description as the database stores it. The PostgreSQL and Firebird generators write no table
        /// descriptions in <c>CREATE TABLE</c>.
        /// </summary>
        private static readonly Dictionary<string, string> DescriptionQuery = new()
        {
            ["SqlServer2016"] = "SELECT CAST(value AS NVARCHAR(4000)) FROM sys.extended_properties WHERE major_id = OBJECT_ID(N'dbo.FmWidget') AND minor_id = 0 AND name = N'MS_Description'",
            ["MySql8"] = "SELECT TABLE_COMMENT FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'FmWidget'",
            ["Oracle12CManaged"] = "SELECT COMMENTS FROM USER_TAB_COMMENTS WHERE TABLE_NAME = 'FMWIDGET'",
        };

        private static string GenerateScript(CreationScriptTarget target) =>
            ConnectionlessScript.Generate(target, CreateServices(target, connectionString: null));

        private static void MigrateUp(CreationScriptTarget target, string connectionString)
        {
            using var serviceProvider = CreateServices(target, connectionString).BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
        }

        private static void Verify(CreationScriptTarget target, string connectionString, bool tableExisted)
        {
            using var serviceProvider = CreateServices(target, connectionString).BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var processor = scope.ServiceProvider.GetRequiredService<IProcessorAccessor>().Processor;
            var versionLoader = scope.ServiceProvider.GetRequiredService<IVersionLoader>();
            var table = ConditionalCreateMigrations.Table;

            var ids = processor.ReadTableData(null, table).Tables[0].Rows
                .Cast<DataRow>()
                .Select(row => Convert.ToInt32(row[0]))
                .OrderBy(id => id)
                .ToList();

            Assert.Multiple(
                () =>
                {
                    Assert.That(processor.TableExists(null, table), Is.True, "table");
                    Assert.That(processor.ColumnExists(null, table, "Id"), Is.True, "Id");
                    Assert.That(processor.ColumnExists(null, table, "Name"), Is.True, "Name");

                    Assert.That(processor.ColumnExists(null, table, "KindId"), Is.True, "KindId");
                    Assert.That(processor.TableExists(null, ConditionalCreateMigrations.KindTable), Is.True, "kind table");

                    // Created along with a new table; already there, and not created again, with an existing one.
                    Assert.That(processor.IndexExists(null, table, ConditionalCreateMigrations.UniqueIndex), Is.True, "unique index");
                    Assert.That(processor.IndexExists(null, table, ConditionalCreateMigrations.Index), Is.True, "index");
                    Assert.That(processor.ConstraintExists(null, table, ConditionalCreateMigrations.ForeignKey), Is.True, "foreign key");

                    // The extra column shows that the existing table was left as it was.
                    Assert.That(processor.ColumnExists(null, table, "Marker"), Is.EqualTo(tableExisted), "Marker");
                    Assert.That(ids, Is.EqualTo(tableExisted ? new[] { 1, 2 } : new[] { 2 }), "rows");

                    if (DescriptionQuery.TryGetValue(target.Name, out var query))
                    {
                        var rows = processor.Read(query).Tables[0].Rows;
                        Assert.That(
                            rows.Count == 0 ? null : rows[0][0] as string,
                            Is.EqualTo(tableExisted ? ExistingDescription : ConditionalCreateMigrations.Description),
                            "table description");
                    }

                    Assert.That(
                        versionLoader.VersionInfo.AppliedMigrations(),
                        Is.EquivalentTo(ConditionalCreateMigrations.Versions),
                        "applied migrations");
                });
        }

        private static IServiceCollection CreateServices(CreationScriptTarget target, string connectionString)
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .AddSingleton<IAssemblySourceItem>(new AssemblySourceItem(typeof(ConditionalCreateMigrations).Assembly))
                .Configure<TypeFilterOptions>(
                    options =>
                    {
                        options.Namespace = typeof(ConditionalCreateMigrations).Namespace;
                        options.NestedNamespaces = false;
                    })
                .Configure<RunnerOptions>(options => options.Tags = [ConditionalCreateMigrations.Tag])
                .ConfigureRunner(
                    builder =>
                    {
                        target.AddProcessor(builder);

                        if (connectionString != null)
                        {
                            builder.WithGlobalConnectionString(connectionString);
                        }
                    });
        }
    }
}
