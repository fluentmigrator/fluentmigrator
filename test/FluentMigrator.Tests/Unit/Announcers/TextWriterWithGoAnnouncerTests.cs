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

using FluentMigrator.Runner.Announcers;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Announcers
{
    [TestFixture]
    [Obsolete]
    public class TextWriterWithGoAnnouncerTests
    {
        private StringWriter _stringWriter;
        private TextWriterWithGoAnnouncer _announcer;

        [SetUp]
        public void TestSetup()
        {
            _stringWriter = new StringWriter();
            _announcer = new TextWriterWithGoAnnouncer(_stringWriter)
            {
                ShowElapsedTime = true,
                ShowSql = true
            };
        }


        [TearDown]
        public void TearDown()
        {
            _stringWriter?.Dispose();
        }

        [Test]
        public void Adds_Go_StatementAfterSqlAnouncement()
        {
            _announcer.Sql("DELETE Blah");
            Output.ShouldBe("DELETE Blah" + Environment.NewLine +
                "GO" + Environment.NewLine);
        }

        [Test]
        public void Sql_Should_Not_Write_When_Show_Sql_Is_False()
        {
            _announcer.ShowSql = false;

            _announcer.Sql("SQL");
            Output.ShouldBe(string.Empty);
        }

        [Test]
        public void Sql_Should_Not_Write_Go_When_Sql_Is_Empty()
        {
            _announcer.Sql("");
            Assert.That(Output, Does.Not.Contain("GO"));
        }

        public string Output => _stringWriter.GetStringBuilder().ToString();
    }
}
