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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Logging;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Loggers
{
    [TestFixture]
    public class TextWriterAnnouncerTests
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
            _options = new SqlScriptFluentMigratorLoggerOptions();
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new SqlScriptFluentMigratorLoggerProvider(_stringWriter, _options));
            _logger = _loggerFactory.CreateLogger("Test");
        }

        [Test]
        public void CanAnnounceAndPadWithEquals()
        {
            _logger.LogHeader("Test");
            Output.ShouldBe("/* Test ====================================================================== */" + Environment.NewLine + Environment.NewLine);
        }

        [Test]
        public void CanSay()
        {
            _logger.LogSay("Create table");
            Output.ShouldBe("/* Create table */" + Environment.NewLine);
        }

        [Test]
        public void CanSaySql()
        {
            _logger.LogSql("DELETE Blah");
            Output.ShouldBe("DELETE Blah" + Environment.NewLine);
        }

        [Test]
        public void CanSayTimeSpan()
        {
            _options.ShowElapsedTime = true;
            _logger.LogElapsedTime(new TimeSpan(0, 0, 5));
            Output.ShouldBe("/* => 5s */" + Environment.NewLine + Environment.NewLine);
        }
    }
}
