#region License

// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
    public class MigratorConsoleTests
    {
        private const string Database = ProcessorId.SQLite;
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
                "/verbose", "1",
                "/target", Target,
                "/namespace", "FluentMigrator.Tests.Integration.Migrations",
                "/task", "migrate:up",
                "/version", "1");

            Assert.Greater(sbVerbose.ToString().Length, sbNonVerbose.ToString().Length);
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
            Assert.AreNotEqual(0, output.Length);
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
            Assert.That(output.Contains("PREVIEW-ONLY MODE"));
            Assert.AreNotEqual(0, output.Length);
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
            Assert.That(output.Contains("PREVIEW-ONLY MODE"));
            Assert.AreNotEqual(0, output.Length);
        }

        [Test]
        public void FileAnnouncerHasOutputToDefaultOutputFile()
        {
            var outputFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Target + ".sql");
            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }

            Assert.IsFalse(File.Exists(outputFileName));

            new MigratorConsole().Run(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/output",
                "/namespace", "FluentMigrator.Tests.Unit.Runners.Migrations",
                "/task", "migrate:up",
                "/version", "0");

            Assert.IsTrue(File.Exists(outputFileName));
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

            Assert.IsFalse(File.Exists(outputFileName));

            new MigratorConsole().Run(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/output",
                "/outputFilename", outputFileName,
                "/namespace", "FluentMigrator.Tests.Unit.Runners.Migrations",
                "/task", "migrate:up",
                "/version", "0");

            Assert.IsTrue(File.Exists(outputFileName));
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

            CollectionAssert.AreEquivalent(expectedTags, migratorConsole.Tags);
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

            CollectionAssert.AreEquivalent(expectedProviderSwitces, migratorConsole.ProviderSwitches);
        }
    }
}
