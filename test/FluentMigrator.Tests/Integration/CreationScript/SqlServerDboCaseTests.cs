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
using System.IO;
using System.Linq;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Logging;
using FluentMigrator.Runner.Processors;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.CreationScript
{
    /// <summary>
    /// A creation script whose default schema is <c>DBO</c>: the built-in <c>dbo</c> in a case-insensitive database,
    /// a schema of its own in a case-sensitive one. The script can't know which, so the database decides.
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    [Category("CreationScript")]
    [Category("SqlServer2016")]
    public class SqlServerDboCaseTests
    {
        [TestCase("SQL_Latin1_General_CP1_CI_AS", "dbo")]
        [TestCase("Latin1_General_100_CS_AS", "DBO")]
        public void ScriptPutsTheTablesInTheSchemaTheCollationFinds(string collation, string expectedSchema)
        {
            var serverOptions = IntegrationTestOptions.SqlServer2016;
            serverOptions.IgnoreIfNotEnabled();

            var target = CreationScriptTarget.All.Single(t => t.Name == "SqlServer2016");
            var script = GenerateScript(target);

            var name = "fmdbo" + Guid.NewGuid().ToString("N").Substring(0, 12);
            Execute(serverOptions.ConnectionString, $"CREATE DATABASE [{name}] COLLATE {collation}");

            try
            {
                var connectionString = new SqlConnectionStringBuilder(serverOptions.ConnectionString) { InitialCatalog = name }.ConnectionString;
                target.RunScript(connectionString, script);

                using var connection = new SqlConnection(connectionString);
                connection.Open();

                Assert.Multiple(
                    () =>
                    {
                        Assert.That(SchemaOf(connection, "VersionInfo"), Is.EqualTo(expectedSchema), script);
                        Assert.That(SchemaOf(connection, "Widget"), Is.EqualTo(expectedSchema), script);
                    });
            }
            finally
            {
                SqlConnection.ClearAllPools();
                Execute(
                    serverOptions.ConnectionString,
                    $"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                    $"DROP DATABASE [{name}]");
            }
        }

        // The name SQL Server stored, not the one the script used.
        private static string SchemaOf(SqlConnection connection, string table)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT SCHEMA_NAME(schema_id) FROM sys.tables WHERE name = '{table}'";
            return (string)command.ExecuteScalar();
        }

        private static void Execute(string connectionString, params string[] statements)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            foreach (var statement in statements)
            {
                using var command = connection.CreateCommand();
                command.CommandText = statement;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// What <c>dotnet fm migrate --no-connection --preview --preview-from-nothing --default-schema-name=DBO --output</c>
        /// writes.
        /// </summary>
        private static string GenerateScript(CreationScriptTarget target)
        {
            var outputFile = Path.Combine(Path.GetTempPath(), $"fm-dbo-{Guid.NewGuid():N}.sql");

            try
            {
                var services = new ServiceCollection()
                    .AddFluentMigratorCore()
                    .AddSingleton<IConventionSet>(new DefaultConventionSet("DBO", null))
                    .Configure<TypeFilterOptions>(options => options.Namespace = typeof(DboInAnotherCase.CreateWidget).Namespace)
                    .ConfigureRunner(
                        builder =>
                        {
                            target.AddProcessor(builder);
                            builder.WithMigrationsIn(typeof(DboInAnotherCase.CreateWidget).Assembly);
                        })
                    .AddSingleton<ILoggerProvider, LogFileFluentMigratorLoggerProvider>()
                    .Configure<LogFileFluentMigratorLoggerOptions>(
                        options =>
                        {
                            options.OutputFileName = outputFile;
                            options.OutputGoBetweenStatements = target.GoBetweenStatements;
                            options.ShowSql = true;
                        })
                    .Configure<RunnerOptions>(
                        options =>
                        {
                            options.NoConnection = true;
                            options.PreviewFromNothing = true;
                        })
                    .Configure<ProcessorOptions>(options => options.PreviewOnly = true);

                // Disposing the provider closes the output file.
                using (var serviceProvider = services.BuildServiceProvider())
                using (var scope = serviceProvider.CreateScope())
                {
                    scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
                }

                return File.ReadAllText(outputFile);
            }
            finally
            {
                File.Delete(outputFile);
            }
        }
    }
}

namespace FluentMigrator.Tests.Integration.CreationScript.DboInAnotherCase
{
    [Migration(1)]
    public class CreateWidget : Migration
    {
        public override void Up()
        {
            Create.Table("Widget")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey();
        }

        public override void Down()
        {
            Delete.Table("Widget");
        }
    }
}
