#region License

// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System.CommandLine;

using FluentMigrator.Hosting.Commands;
using FluentMigrator.Hosting.Commands.CommandBuilders;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Runners
{
    /// <summary>
    /// Regression tests for the System.CommandLine command tree built by
    /// <see cref="FluentMigratorCommandBuilder"/>. Tests verify that option values
    /// are correctly parsed from command-line arguments without executing migrations.
    /// </summary>
    [TestFixture]
    [Category("Runner")]
    [Category("DotNetCli")]
    public class FluentMigratorCommandBuilderTests
    {
        private static RootCommand Root => FluentMigratorCommandBuilder.BuildRootCommand();

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Parses the given args and asserts no parse errors occurred.</summary>
        private static ParseResult ParseWithoutErrors(string[] args)
        {
            var result = Root.Parse(args);
            result.Errors.ShouldBeEmpty($"Unexpected parse errors for args: {string.Join(" ", args)}");
            return result;
        }

        // ── Root structure ────────────────────────────────────────────────────

        [Test]
        public void RootCommandHasMigrateSubcommand()
        {
            var root = Root;
            root.Subcommands.ShouldContain(c => c.Name == "migrate");
        }

        [Test]
        public void RootCommandHasRollbackSubcommand()
        {
            var root = Root;
            root.Subcommands.ShouldContain(c => c.Name == "rollback");
        }

        [Test]
        public void RootCommandHasValidateSubcommand()
        {
            var root = Root;
            root.Subcommands.ShouldContain(c => c.Name == "validate");
        }

        [Test]
        public void RootCommandHasListSubcommand()
        {
            var root = Root;
            root.Subcommands.ShouldContain(c => c.Name == "list");
        }

        // ── migrate: required options ─────────────────────────────────────────

        [Test]
        public void MigrateRequiresAssemblyOption()
        {
            var result = Root.Parse(new[] { "migrate", "-p", "sqlite", "-c", "conn" });
            result.Errors.ShouldNotBeEmpty("--assembly is required");
        }

        [Test]
        public void MigrateRequiresProcessorOption()
        {
            var result = Root.Parse(new[] { "migrate", "-a", "test.dll", "-c", "conn" });
            result.Errors.ShouldNotBeEmpty("--processor is required");
        }

        // ── migrate: short-form aliases ───────────────────────────────────────

        [Test]
        public void MigrateShortAliasesParsedCorrectly()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "MyApp.dll", "-p", "sqlserver2016", "-c", "Server=."
            });

            result.GetValue(FluentMigratorCommandBuilder.AssemblyOption).ShouldBe(new[] { "MyApp.dll" });
            result.GetValue(FluentMigratorCommandBuilder.ProcessorOption).ShouldBe("sqlserver2016");
            result.GetValue(FluentMigratorCommandBuilder.ConnectionOption).ShouldBe("Server=.");
        }

        // ── migrate: long-form options ────────────────────────────────────────

        [Test]
        public void MigrateLongFormOptionsParsedCorrectly()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate",
                "--assembly", "MyApp.dll",
                "--processor", "sqlite",
                "--connection", "Data Source=:memory:",
                "--namespace", "MyApp.Migrations",
                "--nested"
            });

            result.GetValue(FluentMigratorCommandBuilder.AssemblyOption).ShouldBe(new[] { "MyApp.dll" });
            result.GetValue(FluentMigratorCommandBuilder.ProcessorOption).ShouldBe("sqlite");
            result.GetValue(FluentMigratorCommandBuilder.ConnectionOption).ShouldBe("Data Source=:memory:");
            result.GetValue(FluentMigratorCommandBuilder.NamespaceOption).ShouldBe("MyApp.Migrations");
            result.GetValue(FluentMigratorCommandBuilder.NestedOption).ShouldBeTrue();
        }

        // ── migrate: --strip default ──────────────────────────────────────────

        [Test]
        public void StripCommentsDefaultsToTrue()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite"
            });

            result.GetValue(FluentMigratorCommandBuilder.StripCommentsOption).ShouldBeTrue();
        }

        [Test]
        public void StripCommentsCanBeDisabled()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite", "--strip", "false"
            });

            result.GetValue(FluentMigratorCommandBuilder.StripCommentsOption).ShouldBeFalse();
        }

        // ── migrate: --include-untagged-migrations default ────────────────────

        [Test]
        public void IncludeUntaggedMigrationsDefaultsToTrue()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite"
            });

            result.GetValue(FluentMigratorCommandBuilder.IncludeUntaggedMigrationsOption).ShouldBeTrue();
        }

        [Test]
        public void IncludeUntaggedMigrationsCanBeDisabled()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite",
                "--include-untagged-migrations", "false"
            });

            result.GetValue(FluentMigratorCommandBuilder.IncludeUntaggedMigrationsOption).ShouldBeFalse();
        }

        // ── migrate: --output ZeroOrOne arity ─────────────────────────────────

        [Test]
        public void OutputAbsent_BothOutputAndFilenameAreNull()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite"
            });

            result.GetResult(FluentMigratorCommandBuilder.OutputOption).ShouldBeNull();
            result.GetValue(FluentMigratorCommandBuilder.OutputOption).ShouldBeNull();
        }

        [Test]
        public void OutputFlagAloneEnablesOutputWithNullFilename()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite", "--output"
            });

            result.GetResult(FluentMigratorCommandBuilder.OutputOption).ShouldNotBeNull();
            result.GetValue(FluentMigratorCommandBuilder.OutputOption).ShouldBeNull();
        }

        [Test]
        public void OutputWithFilenameEnablesOutputAndSetsFilename()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite", "--output", "out.sql"
            });

            result.GetResult(FluentMigratorCommandBuilder.OutputOption).ShouldNotBeNull();
            result.GetValue(FluentMigratorCommandBuilder.OutputOption).ShouldBe("out.sql");
        }

        // ── migrate: --tag (multiple) ─────────────────────────────────────────

        [Test]
        public void MultipleTagsParsedCorrectly()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite",
                "--tag", "production", "--tag", "uk"
            });

            result.GetValue(FluentMigratorCommandBuilder.TagsOption)
                  .ShouldBe(new[] { "production", "uk" });
        }

        // ── migrate: --transaction-mode ───────────────────────────────────────

        [Test]
        public void TransactionModeDefaultsToMigration()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite"
            });

            result.GetValue(FluentMigratorCommandBuilder.TransactionModeOption)
                  .ShouldBe(TransactionMode.Migration);
        }

        [Test]
        public void TransactionModeSessionParsedCorrectly()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite",
                "--transaction-mode", "Session"
            });

            result.GetValue(FluentMigratorCommandBuilder.TransactionModeOption)
                  .ShouldBe(TransactionMode.Session);
        }

        // ── migrate up ────────────────────────────────────────────────────────

        [Test]
        public void MigrateUpParsesSuccessfully()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "up", "-a", "test.dll", "-p", "sqlite"
            });

            result.Errors.ShouldBeEmpty();
        }

        // ── migrate down ──────────────────────────────────────────────────────

        [Test]
        public void MigrateDownRequiresTargetOption()
        {
            var result = Root.Parse(new[] { "migrate", "down", "-a", "test.dll", "-p", "sqlite" });
            result.Errors.ShouldNotBeEmpty("--target is required for migrate down");
        }

        [Test]
        public void MigrateDownWithTargetParsesSuccessfully()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "down", "-a", "test.dll", "-p", "sqlite", "--target", "20240101000000"
            });

            result.Errors.ShouldBeEmpty();
        }

        // ── rollback ──────────────────────────────────────────────────────────

        [Test]
        public void RollbackByParsesStepsCorrectly()
        {
            var result = ParseWithoutErrors(new[] {
                "rollback", "by", "3",
                "-a", "test.dll", "-p", "sqlite", "-c", "conn"
            });

            result.Errors.ShouldBeEmpty();
        }

        [Test]
        public void RollbackToRequiresVersionOption()
        {
            var result = Root.Parse(new[] {
                "rollback", "to", "-a", "test.dll", "-p", "sqlite", "-c", "conn"
            });

            result.Errors.ShouldNotBeEmpty("--version is required for rollback to");
        }

        [Test]
        public void RollbackToWithVersionParsesSuccessfully()
        {
            var result = ParseWithoutErrors(new[] {
                "rollback", "to",
                "-a", "test.dll", "-p", "sqlite", "-c", "conn",
                "--version", "20240101000000"
            });

            result.Errors.ShouldBeEmpty();
        }

        [Test]
        public void RollbackAllParsesSuccessfully()
        {
            var result = ParseWithoutErrors(new[] {
                "rollback", "all",
                "-a", "test.dll", "-p", "sqlite", "-c", "conn"
            });

            result.Errors.ShouldBeEmpty();
        }

        // ── validate ──────────────────────────────────────────────────────────

        [Test]
        public void ValidateVersionsParsesSuccessfully()
        {
            var result = ParseWithoutErrors(new[] {
                "validate", "versions",
                "-a", "test.dll", "-p", "sqlite", "-c", "conn"
            });

            result.Errors.ShouldBeEmpty();
        }

        // ── list ──────────────────────────────────────────────────────────────

        [Test]
        public void ListProcessorsParsesSuccessfully()
        {
            var result = ParseWithoutErrors(new[] { "list", "processors" });
            result.Errors.ShouldBeEmpty();
        }

        [Test]
        public void ListMigrationsParsesSuccessfully()
        {
            var result = ParseWithoutErrors(new[] {
                "list", "migrations",
                "-a", "test.dll", "-p", "sqlite", "-c", "conn"
            });

            result.Errors.ShouldBeEmpty();
        }

        // ── multiple assemblies ───────────────────────────────────────────────

        [Test]
        public void MultipleAssembliesParsedCorrectly()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate",
                "-a", "First.dll", "-a", "Second.dll",
                "-p", "sqlite"
            });

            result.GetValue(FluentMigratorCommandBuilder.AssemblyOption)
                  .ShouldBe(new[] { "First.dll", "Second.dll" });
        }

        // ── preview, verbose, timeout ─────────────────────────────────────────

        [Test]
        public void PreviewFlagParsedCorrectly()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite", "--preview"
            });

            result.GetValue(FluentMigratorCommandBuilder.PreviewOption).ShouldBeTrue();
        }

        [Test]
        public void VerboseFlagParsedCorrectly()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite", "--verbose"
            });

            result.GetValue(FluentMigratorCommandBuilder.VerboseOption).ShouldBeTrue();
        }

        [Test]
        public void TimeoutOptionParsedCorrectly()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite", "--timeout", "120"
            });

            result.GetValue(FluentMigratorCommandBuilder.TimeoutOption).ShouldBe(120);
        }

        // ── --default-schema-name ─────────────────────────────────────────────

        [Test]
        public void DefaultSchemaNameParsedCorrectly()
        {
            var result = ParseWithoutErrors(new[] {
                "migrate", "-a", "test.dll", "-p", "sqlite",
                "--default-schema-name", "dbo"
            });

            result.GetValue(FluentMigratorCommandBuilder.SchemaNameOption).ShouldBe("dbo");
        }
    }
}
