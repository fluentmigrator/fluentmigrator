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
                "/verbose", "true",
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
                "/verbose", "true",
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

        // ── NormalizeArgs backward-compatibility (regression) ─────────────────

        [Test]
        public void NormalizeArgs_SlashPrefixConvertedToDoubleHyphen()
        {
            var input  = new[] { "/db", "sqlite", "/connection", "conn" };
            var output = MigratorConsole.NormalizeArgs(input);

            Assert.That(output, Is.EqualTo(new[] { "--db", "sqlite", "--connection", "conn" }));
        }

        [Test]
        public void NormalizeArgs_SlashPrefixCaseNormalized()
        {
            var input  = new[] { "/DB", "sqlite", "/Connection", "conn", "/TASK", "migrate:up" };
            var output = MigratorConsole.NormalizeArgs(input);

            Assert.That(output, Is.EqualTo(new[] { "--db", "sqlite", "--connection", "conn", "--task", "migrate:up" }));
        }

        [Test]
        public void NormalizeArgs_FilePathNotConverted()
        {
            // A forward-slash path should NOT be changed to --
            var input  = new[] { "/target", "/path/to/assembly.dll" };
            var output = MigratorConsole.NormalizeArgs(input);

            Assert.That(output, Is.EqualTo(new[] { "--target", "/path/to/assembly.dll" }));
        }

        [Test]
        public void NormalizeArgs_SlashWithEqualsValuePreservesValue()
        {
            var input  = new[] { "/task=migrate:up" };
            var output = MigratorConsole.NormalizeArgs(input);

            Assert.That(output, Is.EqualTo(new[] { "--task=migrate:up" }));
        }

        [Test]
        public void NormalizeArgs_DoubleDashArgsPassedThrough()
        {
            var input  = new[] { "--provider", "sqlite", "--connection", "conn" };
            var output = MigratorConsole.NormalizeArgs(input);

            Assert.That(output, Is.EqualTo(new[] { "--provider", "sqlite", "--connection", "conn" }));
        }

        // ── New --option syntax (forward-compatibility) ───────────────────────

        [Test]
        public void CanInitMigratorConsoleWithNewStyleArguments()
        {
            var console = new MigratorConsole();
            console.Run(
                "--provider", Database,
                "--connection", Connection,
                "--assembly", Target,
                "--namespace", "FluentMigrator.Tests.Integration.Migrations",
                "--nested",
                "--task", "migrate:up",
                "--version", "1");

            console.Connection.ShouldBe(Connection);
            console.Namespace.ShouldBe("FluentMigrator.Tests.Integration.Migrations");
            console.NestedNamespaces.ShouldBeTrue();
            console.Task.ShouldBe("migrate:up");
            console.Version.ShouldBe(1);
        }

        [Test]
        public void CanInitMigratorConsoleUsingShortAlias()
        {
            // -a is the short alias for --assembly; -c for --connection
            var console = new MigratorConsole();
            console.Run(
                "--db", Database,
                "-c", Connection,
                "--target", Target,
                "--task", "migrate:up");

            console.Connection.ShouldBe(Connection);
            console.Task.ShouldBe("migrate:up");
        }

        [Test]
        public void VerboseFlagAcceptsTrueValue()
        {
            var console = new MigratorConsole();
            console.Run(
                "--db", Database,
                "--connection", Connection,
                "--assembly", Target,
                "--task", "migrate:up",
                "--verbose", "true");

            console.Verbose.ShouldBeTrue();
        }
    }
}
