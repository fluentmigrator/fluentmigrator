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
using FluentMigrator.Runner.Announcers;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Announcers
{
    [TestFixture]
    public class AnnouncerTests
    {
        [SetUp]
        public void Setup()
        {
            var announcerMock = new Mock<Announcer> {CallBase = true};
            announcer = announcerMock.Object;
        }

        private Announcer announcer;

        [Test]
        public void ElapsedTime_Should_Not_Write_When_ShowElapsedTime_Is_False()
        {
            var time = new TimeSpan(0, 1, 40);

            announcer.ElapsedTime(time);

            Mock.Get(announcer).Verify(a => a.Write(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void ElapsedTime_Should_Write_When_ShowElapsedTime_Is_True()
        {
            var time = new TimeSpan(0, 1, 40);
            Mock.Get(announcer).Setup(a => a.Write("=> 100s", true)).Verifiable();
            announcer.ShowElapsedTime = true;

            announcer.ElapsedTime(time);

            Mock.Get(announcer).VerifyAll();
        }

        [Test]
        public void Error_Should_Write()
        {
            var message = "TheMessage";
            Mock.Get(announcer).Setup(a => a.Write(string.Format("!!! {0}", message), true)).Verifiable();

            announcer.Error(message);

            Mock.Get(announcer).VerifyAll();
        }

        [Test]
        public void Heading_Should_Write()
        {
            var message = "TheMessage";
            Mock.Get(announcer).Setup(a => a.Write(message, true)).Verifiable();

            announcer.Heading(message);

            Mock.Get(announcer).VerifyAll();
        }

        [Test]
        public void Say_Should_Write()
        {
            var message = "TheMessage";
            Mock.Get(announcer).Setup(a => a.Write(message, true)).Verifiable();

            announcer.Say(message);

            Mock.Get(announcer).VerifyAll();
        }

        [Test]
        public void Sql_Should_Not_Write_When_Show_Sql_Is_False()
        {
            var sql = "INSERT INTO table(Id,Name) VALUES (1, 'Test');";

            announcer.Sql(sql);

            Mock.Get(announcer).Verify(a => a.Write(It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void Sql_Should_Write_When_Show_Sql_Is_True()
        {
            var sql = "INSERT INTO table(Id,Name) VALUES (1, 'Test');";
            Mock.Get(announcer).Setup(a => a.Write(sql, false)).Verifiable();
            announcer.ShowSql = true;

            announcer.Sql(sql);

            Mock.Get(announcer).VerifyAll();
        }

        [Test]
        public void Sql_Should_Write_When_Show_Sql_Is_True_And_Sql_Is_Empty()
        {
            var sql = "";
            Mock.Get(announcer).Setup(a => a.Write("No SQL statement executed.", true)).Verifiable();
            announcer.ShowSql = true;

            announcer.Sql(sql);

            Mock.Get(announcer).VerifyAll();
        }
    }
}