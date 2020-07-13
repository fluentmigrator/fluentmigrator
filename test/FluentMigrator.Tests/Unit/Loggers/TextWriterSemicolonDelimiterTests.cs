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
    public class TextWriterSemicolonDelimiterTests
    {
        private StringWriter stringWriter;
        private SqlScriptFluentMigratorLoggerOptions options;
        private ILoggerFactory loggerFactory;
        private ILogger logger;

        private string Output => stringWriter.ToString();

        [SetUp]
        public void SetUp() => stringWriter = new StringWriter();

        [Test]
        public void WhenEnabledSqlShouldHaveSemicolonDelimiter()
        {
            options = new SqlScriptFluentMigratorLoggerOptions() { OutputSemicolonDelimiter = true };
            loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new SqlScriptFluentMigratorLoggerProvider(stringWriter, options));
            logger = loggerFactory.CreateLogger("Test");

            logger.LogSql("DELETE Blah");
            Output.ShouldBe($"DELETE Blah;{Environment.NewLine}");
        }

        [Test]
        public void WhenDisabledSqlShouldNotHaveSemicolonDelimiter()
        {
            options = new SqlScriptFluentMigratorLoggerOptions() { OutputSemicolonDelimiter = false };
            loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new SqlScriptFluentMigratorLoggerProvider(stringWriter, options));
            logger = loggerFactory.CreateLogger("Test");

            logger.LogSql("DELETE Blah");
            Output.ShouldNotContain(";");
        }
    }
}
