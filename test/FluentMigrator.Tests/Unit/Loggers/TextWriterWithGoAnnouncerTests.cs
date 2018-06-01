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

using System;
using System.IO;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Logging;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Loggers
{
    [TestFixture]
    public class TextWriterWithGoAnnouncerTests
    {
        private ILoggerFactory _loggerFactory;

        private ILogger _logger;

        private SqlScriptFluentMigratorLoggerOptions _options;

        private StringWriter _stringWriter;

        private string Output => _stringWriter.ToString();

        [SetUp]
        public void SetUp()
        {
            _stringWriter = new StringWriter();
            _options = new SqlScriptFluentMigratorLoggerOptions() { OutputGoBetweenStatements = true };
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new SqlScriptFluentMigratorLoggerProvider(_stringWriter, _options));
            _logger = _loggerFactory.CreateLogger("Test");
        }

        [Test]
        public void Adds_Go_StatementAfterSqlAnouncement()
        {
            _logger.LogSql("DELETE Blah");
            Output.ShouldBe("DELETE Blah" + Environment.NewLine +
                "GO" + Environment.NewLine);
        }

        [Test]
        public void Sql_Should_Not_Write_When_Show_Sql_Is_False()
        {
            _options.ShowSql = false;

            _logger.LogSql("SQL");
            Output.ShouldBe(string.Empty);
        }

        [Test]
        public void Sql_Should_Not_Write_Go_When_Sql_Is_Empty()
        {
            _logger.LogSql("");
            Assert.IsFalse(Output.Contains("GO"));
        }
    }
}
