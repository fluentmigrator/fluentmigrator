#region License

// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Runners
{
    [TestFixture]
    public class MigratorConsoleTests
    {
        private const string Database = "SQLite";
        private const string Connection = "Data Source=:memory:;Version=3;New=True;";
        private const string Target = "FluentMigrator.Tests.dll";

        [Test]
        [Category("NotWorkingOnMono")]
        public void CanInitMigratorConsoleWithValidArguments()
        {
            var console = new MigratorConsole(
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
        [Category("NotWorkingOnMono")]
        public void ConsoleAnnouncerHasMoreOutputWhenVerbose()
        {
            var sbNonVerbose = new StringBuilder();
            var stringWriterNonVerbose = new StringWriter(sbNonVerbose);
            System.Console.SetOut(stringWriterNonVerbose);

            new MigratorConsole(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/namespace", "FluentMigrator.Tests.Integration.Migrations",
                "/task", "migrate:up",
                "/version", "1");

            var sbVerbose = new StringBuilder();
            var stringWriterVerbose = new StringWriter(sbVerbose);
            System.Console.SetOut(stringWriterVerbose);

            new MigratorConsole(
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
            new MigratorConsole(
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
        [Category("NotWorkingOnMono")]
        public void ConsoleAnnouncerHasOutputEvenIfMarkedAsPreviewOnly()
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);

            System.Console.SetOut(stringWriter);

            new MigratorConsole(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/namespace", "FluentMigrator.Tests.Unit.Runners.Migrations",
                "/verbose",
                "/task", "migrate:up",
                "/preview");

            var output = sb.ToString();
            Assert.That(output.Contains("PREVIEW-ONLY MODE"));
            Assert.AreNotEqual(0, output.Length);
        }

        [Test]
        public void FileAnnouncerHasOutputToDefaultOutputFile()
        {
            const string outputFileName = Target + ".sql";
            if (File.Exists(outputFileName)) File.Delete(outputFileName);

            Assert.IsFalse(File.Exists(outputFileName));

            new MigratorConsole(
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
            const string outputFileName = "output.sql";
            if (File.Exists(outputFileName)) File.Delete(outputFileName);

            Assert.IsFalse(File.Exists(outputFileName));

            new MigratorConsole(
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
            new MigratorConsole("/db", Database);
            Assert.That(Environment.ExitCode == 1);
        }

        [Test]
        public void MustInitializeConsoleWithDatabaseArgument()
        {
            new MigratorConsole("/connection", Connection);
            Assert.That(Environment.ExitCode == 1);
        }

        [Test, Ignore("implement this test")]
        public void OrderOfConsoleArgumentsShouldNotMatter()
        {
        }

        [Test]
        public void TagsPassedToRunnerContextOnExecuteMigrations()
        {
            var migratorConsole = new MigratorConsole(
                "/db", Database,
                "/connection", Connection,
                "/verbose", "1",
                "/target", Target,
                "/namespace", "FluentMigrator.Tests.Integration.Migrations",
                "/task", "migrate:up",
                "/version", "1",
                "/tag", "uk",
                "/tag", "production");

            var expectedTags = new string[] { "uk", "production" };

            CollectionAssert.AreEquivalent(expectedTags, migratorConsole.RunnerContext.Tags);   
        }

        [Test]
        public void TransactionPerSessionShouldBeSetOnRunnerContextWithShortSwitch()
        {
            var console = new MigratorConsole(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/task", "migrate:up",
                "/tps");

            console.TransactionPerSession.ShouldBeTrue();
            console.RunnerContext.TransactionPerSession.ShouldBeTrue();
        }

        [Test]
        public void TransactionPerSessionShouldBeSetOnRunnerContextWithLongSwitch()
        {
            var console = new MigratorConsole(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/task", "migrate:up",
                "/transaction-per-session");

            console.TransactionPerSession.ShouldBeTrue();
            console.RunnerContext.TransactionPerSession.ShouldBeTrue();
        }

        [Test]
        public void ProviderSwitchesPassedToRunnerContextOnExecuteMigrations()
        {
            var migratorConsole = new MigratorConsole(
                "/db", Database,
                "/connection", Connection,
                "/target", Target,
                "/output",
                "/namespace", "FluentMigrator.Tests.Unit.Runners.Migrations",
                "/task", "migrate:up",
                "/version", "0",
                "/providerswitches", "QuotedIdentifiers=true");

            const string expectedProviderSwitces = "QuotedIdentifiers=true";

            CollectionAssert.AreEquivalent(expectedProviderSwitces, migratorConsole.RunnerContext.ProviderSwitches);
        }
    }
}
