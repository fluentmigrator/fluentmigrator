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

using FluentMigrator.DotNet.Cli;
using FluentMigrator.DotNet.Cli.Commands;

using McMaster.Extensions.CommandLineUtils;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Commands
{
    [TestFixture]
    public class BaseCommandTests
    {
        [Test]
        public void MigrationFailureReturnsNonZeroExitCode()
        {
            var console = new TestConsole();
            var command = new TestCommand { Failure = new InvalidOperationException("migration failed") };
            var exitCode = command.Execute(new MigratorOptions(), console);

            exitCode.ShouldBe(1);
            console.ErrorText.ShouldContain("Error executing migrations:");
            console.ErrorText.ShouldContain("migration failed");
        }

        [Test]
        public void SuccessfulMigrationReturnsZeroExitCode()
        {
            var console = new TestConsole();
            var command = new TestCommand();
            var exitCode = command.Execute(new MigratorOptions(), console);

            exitCode.ShouldBe(0, console.ErrorText);
        }

        private sealed class TestCommand : BaseCommand
        {
            public Exception Failure { get; init; }

            public int Execute(MigratorOptions options, IConsole console)
            {
                return ExecuteMigrations(options, console);
            }

            protected override void RunMigrations(MigratorOptions options, IConsole console)
            {
                if (Failure is not null)
                {
                    throw Failure;
                }
            }
        }

        private sealed class TestConsole : IConsole
        {
            public TextWriter Out { get; } = new StringWriter();

            public TextWriter Error { get; } = new StringWriter();

            public TextReader In { get; } = new StringReader(string.Empty);

            public bool IsInputRedirected => true;

            public bool IsOutputRedirected => true;

            public bool IsErrorRedirected => true;

            public ConsoleColor ForegroundColor { get; set; }

            public ConsoleColor BackgroundColor { get; set; }

            public event ConsoleCancelEventHandler CancelKeyPress
            {
                add { }
                remove { }
            }

            public string ErrorText => Error.ToString();

            public void ResetColor()
            {
            }
        }
    }
}
