#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class AnnouncerTests
    {
        private TextWriterAnnouncer _announcer;
        private StringWriter _stringWriter;

        [SetUp]
        public void SetUp()
        {
            _stringWriter = new StringWriter();
            _announcer = new TextWriterAnnouncer(_stringWriter)
                            {
                                ShowElapsedTime = true,
                                ShowSql = true
                            };
        }

        public string Output
        {
            get
            {
                return _stringWriter.GetStringBuilder().ToString();
            }
        }

        [Test]
        public void CanAnnounceAndPadWithEquals()
        {
            _announcer.Heading("Test");
            Output.ShouldBe("/* Test ====================================================================== */" + Environment.NewLine + Environment.NewLine);
        }

        [Test]
        public void CanSay()
        {
            _announcer.Say("Create table");
            Output.ShouldBe("/* Create table */" + Environment.NewLine);
        }

        [Test]
        public void CanSayTimeSpan()
        {
            _announcer.ElapsedTime(new TimeSpan(0, 0, 5));
            Output.ShouldBe("/* -> 5s */" + Environment.NewLine + Environment.NewLine);
        }

        [Test]
        public void CanSaySql()
        {
            _announcer.Sql("DELETE Blah");
            Output.ShouldBe("DELETE Blah" + Environment.NewLine);
        }
    }
}
