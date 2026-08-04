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

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using FluentMigrator.Console;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Runners
{
    [TestFixture]
    [Category("Runner")]
    [Category("Console")]
    public class MigratorConsoleTests
    {
        private const string Database = ProcessorIdConstants.SQLite;
        private const string Connection = "Data Source=:memory:";
        private const string Target = "FluentMigrator.Tests.dll";

        [Test]
        public void CanInitMigratorConsoleWithValidArguments()
        {
            var console = new MigratorConsole();
            console.Run(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/namespace", "FluentMigrator.Tests.Integration.Migrations",
                "/nested",
                "/task", "migrate:up",
                "/version", "1");

            console.Connection.ShouldBe(Connection);
            console.Namespace.ShouldBe("FluentMigrator.Tests.Integration.Migrations");
            console.NestedNamespaces.ShouldBeTrue();
            console.Task.ShouldBe("migrate:up");
            console.Version.ShouldBe(1);
        }

        [Test]
        public void CanInitMigratorConsoleWithValidArgumentsRegardlessOfCase()
        {
            var console = new MigratorConsole();
            console.Run(
                "/db", Database,
                "/Connection", Connection,
                "/target", Target,
                "/namespace", "FluentMigrator.Tests.Integration.Migrations",
                "/nested",
                "/TASK", "migrate:up",
                "/vErSiOn", "1");

            console.Connection.ShouldBe(Connection);
            console.Namespace.ShouldBe("FluentMigrator.Tests.Integration.Migrations");
            console.NestedNamespaces.ShouldBeTrue();
            console.Task.ShouldBe("migrate:up");
            console.Version.ShouldBe(1);
        }

        [Test]
        public void IncludeUntaggedIsMigrationsOnlyByDefault()
        {
            var console = new MigratorConsole();
            console.IncludeUntaggedMigrations.ShouldBeTrue();
            console.IncludeUntaggedMaintenances.ShouldBeFalse();
        }

        [TestCase(null, true, true, TestName = "IncludeUntagged_Absent_EnablesBoth")]
        [TestCase("", true, true, TestName = "IncludeUntagged_Empty_EnablesBoth")]
        [TestCase(",", true, false, TestName = "IncludeUntagged_OnlySeparators_LeavesDefaults")]
        // migrations token, every suffix
        [TestCase("mi", true, false)]
        [TestCase("mi+", true, false)]
        [TestCase("mi-", false, false)]
        [TestCase("migrations", true, false)]
        [TestCase("migrations+", true, false)]
        [TestCase("migrations-", false, false)]
        // maintenance token, every suffix
        [TestCase("ma", true, true)]
        [TestCase("ma+", true, true)]
        [TestCase("ma-", true, false)]
        [TestCase("maintenance", true, true)]
        [TestCase("maintenance+", true, true)]
        [TestCase("maintenance-", true, false)]
        // every combination of the two toggles
        [TestCase("mi+,ma+", true, true)]
        [TestCase("mi+,ma-", true, false)]
        [TestCase("mi-,ma+", false, true)]
        [TestCase("mi-,ma-", false, false)]
        // order independence
        [TestCase("ma+,mi-", false, true)]
        // case-insensitive
        [TestCase("MI-", false, false)]
        [TestCase("MA+", true, true)]
        [TestCase("Migrations-,Maintenance+", false, true)]
        // whitespace trimming and empty entries removed
        [TestCase(" mi- , ma+ ", false, true)]
        [TestCase("mi-,,ma+", false, true)]
        // long-form combinations, every combination of the two signs
        [TestCase("migrations+,maintenance+", true, true)]
        [TestCase("migrations+,maintenance-", true, false)]
        [TestCase("migrations-,maintenance+", false, true)]
        [TestCase("migrations-,maintenance-", false, false)]
        // long-form order independence
        [TestCase("maintenance+,migrations-", false, true)]
        [TestCase("maintenance-,migrations+", true, false)]
        // long-form / short-form mixed
        [TestCase("migrations-,ma+", false, true)]
        [TestCase("mi+,maintenance-", true, false)]
        public void IncludeUntaggedParsesValue(string value, bool expectedMigrations, bool expectedMaintenances)
        {
            var (console, exitCode) = RunWithIncludeUntagged(value);

            exitCode.ShouldBe(0);
            console.IncludeUntaggedMigrations.ShouldBe(expectedMigrations);
            console.IncludeUntaggedMaintenances.ShouldBe(expectedMaintenances);
        }

        [TestCase("mo")]
        [TestCase("foo-")]
        [TestCase("z+")]
        [TestCase("mi-,zz")]
        public void IncludeUntaggedRejectsUnknownValue(string value)
        {
            var (_, exitCode) = RunWithIncludeUntagged(value);

            exitCode.ShouldBe(3);
        }

        // Smoke test for the --help output.
        //
        // It is intended as the home for verifying any option's help text:
        // when you add or change an option description in MigratorConsole, add another
        // `normalized.ShouldContain(...)` assertion here for that option.
        [Test]
        public void HelpOutputContainsOptionDescriptions()
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            System.Console.SetOut(stringWriter);
            System.Console.SetError(stringWriter);

            var exitCode = new MigratorConsole().Run(
                "/db", Database,
                "/target", Target,
                "/help");

            exitCode.ShouldBe(0);

            // WriteOptionDescriptions word-wraps long descriptions across lines,
            // so collapse whitespace before asserting the description is present.
            var normalized = Regex.Replace(sb.ToString(), @"\s+", " ");

            // --include-untagged
            normalized.ShouldContain(
                "Available values are: ma, maintenance, mi, migrations with an optional '+' or '-' "
                + "at the end to enable or disable the option. "
                + "Multiple values may be given when separated by a comma.");
        }

        private static (MigratorConsole Console, int ExitCode) RunWithIncludeUntagged(string value)
        {
            var stringWriter = new StringWriter(new StringBuilder());
            System.Console.SetOut(stringWriter);
            System.Console.SetError(stringWriter);

            var includeUntagged = value == null ? "/include-untagged" : "/include-untagged=" + value;
            var console = new MigratorConsole();

            var exitCode = console.Run(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/namespace", "FluentMigrator.Tests.Unit.Runners.Migrations",
                "/task", "migrate:up",
                "/version", "0",
                includeUntagged);

            return (console, exitCode);
        }

        [Test]
        public void ConsoleAnnouncerHasMoreOutputWhenVerbose()
        {
            var sbNonVerbose = new StringBuilder();
            var stringWriterNonVerbose = new StringWriter(sbNonVerbose);

            System.Console.SetOut(stringWriterNonVerbose);
            System.Console.SetError(stringWriterNonVerbose);

            new MigratorConsole().Run(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/namespace", "FluentMigrator.Tests.Integration.Migrations",
                "/task", "migrate:up",
                "/version", "1");

            var sbVerbose = new StringBuilder();
            var stringWriterVerbose = new StringWriter(sbVerbose);
            System.Console.SetOut(stringWriterVerbose);

            new MigratorConsole().Run(
                "/db", Database,
                "/connection", Connection,
                "/verbose", "1",
                "/target", Target,
                "/namespace", "FluentMigrator.Tests.Integration.Migrations",
                "/task", "migrate:up",
                "/version", "1");

            Assert.That(sbVerbose.ToString(), Has.Length.GreaterThan(sbNonVerbose.ToString().Length));
        }

        [Test]
        public void ConsoleAnnouncerHasOutput()
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);

            System.Console.SetOut(stringWriter);
            System.Console.SetError(stringWriter);

            new MigratorConsole().Run(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/namespace", "FluentMigrator.Tests.Unit.Runners.Migrations",
                "/task", "migrate:up",
                "/version", "0");

            var output = sb.ToString();
            Assert.That(output, Is.Not.Empty);
        }

        [Test]
        public void ConsoleAnnouncerHasOutputEvenIfMarkedAsPreviewOnlyMigrateUp()
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);

            System.Console.SetOut(stringWriter);
            System.Console.SetError(stringWriter);

            new MigratorConsole().Run(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/namespace", "FluentMigrator.Tests.Unit.Runners.Migrations",
                "/verbose", "true",
                "/task", "migrate:up",
                "/preview");

            var output = sb.ToString();
            Assert.Multiple(() =>
            {
                Assert.That(output, Does.Contain("PREVIEW-ONLY MODE"));
                Assert.That(output, Is.Not.Empty);
            });
        }

        [Test]
        public void ConsoleAnnouncerHasOutputEvenIfMarkedAsPreviewOnlyMigrateDown()
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);

            System.Console.SetOut(stringWriter);
            System.Console.SetError(stringWriter);

            new MigratorConsole().Run(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/namespace", "FluentMigrator.Tests.Unit.Runners.Migrations",
                "/verbose", "true",
                "/task", "migrate:down",
                "/version", "2",
                "/preview");

            var output = sb.ToString();
            Assert.Multiple(() =>
            {
                Assert.That(output, Does.Contain("PREVIEW-ONLY MODE"));
                Assert.That(output, Is.Not.Empty);
            });
        }

        [Test]
        public void FileAnnouncerHasOutputToDefaultOutputFile()
        {
            var outputFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Target + ".sql");
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }

            Assert.That(File.Exists(outputFileName), Is.False);

            new MigratorConsole().Run(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/output",
                "/namespace", "FluentMigrator.Tests.Unit.Runners.Migrations",
                "/task", "migrate:up",
                "/version", "0");

            Assert.That(File.Exists(outputFileName));
            File.Delete(outputFileName);
        }

        [Test]
        public void FileAnnouncerHasOutputToSpecifiedOutputFile()
        {
            var outputFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output.sql");
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }

            Assert.That(File.Exists(outputFileName), Is.False);

            new MigratorConsole().Run(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/output",
                "/outputFilename", outputFileName,
                "/namespace", "FluentMigrator.Tests.Unit.Runners.Migrations",
                "/task", "migrate:up",
                "/version", "0");

            Assert.That(File.Exists(outputFileName));
            File.Delete(outputFileName);
        }

        [Test]
        public void MustInitializeConsoleWithConnectionArgument()
        {
            var exitCode = new MigratorConsole().Run("/db", Database);
            Assert.That(exitCode, Is.EqualTo(1));
        }

        [Test]
        public void MustInitializeConsoleWithDatabaseArgument()
        {
            var exitCode = new MigratorConsole().Run("/connection", Connection);
            Assert.That(exitCode, Is.EqualTo(1));
        }

        [Test]
        public void TagsPassedToRunnerContextOnExecuteMigrations()
        {
            var migratorConsole = new MigratorConsole();
            migratorConsole.Run(
                "/db", Database,
                "/connection", Connection,
                "/verbose", "1",
                "/target", Target,
                "/namespace", "FluentMigrator.Tests.Integration.Migrations",
                "/task", "migrate:up",
                "/version", "1",
                "/tag", "uk",
                "/tag", "production");

            var expectedTags = new[] { "uk", "production" };

            Assert.That(migratorConsole.Tags, Is.EquivalentTo(expectedTags));
        }

        [Test]
        public void TransactionPerSessionShouldBeSetOnRunnerContextWithShortSwitch()
        {
            var console = new MigratorConsole();
            console.Run(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/task", "migrate:up",
                "/tps");

            console.TransactionPerSession.ShouldBeTrue();
        }

        [Test]
        public void TransactionPerSessionShouldBeSetOnRunnerContextWithLongSwitch()
        {
            var console = new MigratorConsole();
            console.Run(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/task", "migrate:up",
                "/transaction-per-session");

            console.TransactionPerSession.ShouldBeTrue();
        }

        [Test]
        public void ProviderSwitchesPassedToRunnerContextOnExecuteMigrations()
        {
            var migratorConsole = new MigratorConsole();
            migratorConsole.Run(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/output",
                "/namespace", "FluentMigrator.Tests.Unit.Runners.Migrations",
                "/task", "migrate:up",
                "/version", "0",
                "/providerswitches", "QuotedIdentifiers=true");

            const string expectedProviderSwitces = "QuotedIdentifiers=true";

            Assert.That(migratorConsole.ProviderSwitches, Is.EquivalentTo(expectedProviderSwitces));
        }
    }
}
