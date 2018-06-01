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
using System.Collections.Generic;
using System.IO;

using FluentMigrator.Runner;
using FluentMigrator.Tests.Logging;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Loggers
{
    [TestFixture]
    public class AnnouncerTests
    {
        private ILoggerFactory _loggerFactory;

        private ILogger _logger;

        private FluentMigratorLoggerOptions _options;

        private StringWriter _output;

        [SetUp]
        public void Setup()
        {
            _options = new FluentMigratorLoggerOptions();
            _output = new StringWriter();
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new TextWriterLoggerProvider(_output, _options));
            _logger = _loggerFactory.CreateLogger("Test");
        }

        [Test]
        public void ElapsedTime_Should_Not_Write_When_ShowElapsedTime_Is_False()
        {
            var time = new TimeSpan(0, 1, 40);

            _logger.LogElapsedTime(time);

            Assert.IsEmpty(_output.ToString());
        }

        [Test]
        public void ElapsedTime_Should_Write_When_ShowElapsedTime_Is_True()
        {
            var time = new TimeSpan(0, 1, 40);

            _options.ShowElapsedTime = true;

            _logger.LogElapsedTime(time);

            Assert.AreEqual("=> 100s", _output.ToString().Trim());
        }

        [Test]
        public void Error_Should_Write()
        {
            var message = "TheMessage";

            _logger.LogError(message);

            Assert.AreEqual($"!!! {message}", _output.ToString().Trim());
        }

        [Test]
        public void Heading_Should_Write()
        {
            var message = "TheMessage";

            _logger.LogHeader(message);

            var lines = GetLines();
            Assert.GreaterOrEqual(3, lines.Count);

            Assert.AreEqual(message, lines[1]);
        }

        [Test]
        public void Say_Should_Write()
        {
            var message = "TheMessage";

            _logger.LogSay(message);

            Assert.AreEqual(message, _output.ToString().Trim());
        }

        [Test]
        public void Sql_Should_Not_Write_When_Show_Sql_Is_False()
        {
            var sql = "INSERT INTO table(Id,Name) VALUES (1, 'Test');";

            _logger.LogSql(sql);

            Assert.IsEmpty(_output.ToString());
        }

        [Test]
        public void Sql_Should_Write_When_Show_Sql_Is_True()
        {
            var sql = "INSERT INTO table(Id,Name) VALUES (1, 'Test');";

            _options.ShowSql = true;

            _logger.LogSql(sql);

            Assert.AreEqual(sql, _output.ToString().Trim());
        }

        [Test]
        public void Sql_Should_Write_When_Show_Sql_Is_True_And_Sql_Is_Empty()
        {
            var sql = string.Empty;

            _options.ShowSql = true;

            _logger.LogSql(sql);

            Assert.AreEqual("No SQL statement executed.", _output.ToString().Trim());
        }

        private IReadOnlyList<string> GetLines()
        {
            var lines = new List<string>();

            using (var reader = new StringReader(_output.ToString()))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }
    }
}
