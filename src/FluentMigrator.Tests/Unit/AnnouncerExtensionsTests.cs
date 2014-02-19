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

using FluentMigrator.Runner;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class AnnouncerExtensionsTests
    {
        
        [SetUp]
        public void Setup()
        {
            announcer = new Mock<IAnnouncer>(MockBehavior.Strict).Object;
        }

        private IAnnouncer announcer;

        [Test]
        public void Error_Should_Error_string_formatted_message()
        {
            Mock.Get(announcer).Setup(a => a.Error("Hello Error"));

            announcer.Error("Hello {0}", "Error");
        }

        [Test]
        public void Heading_Should_Heading_string_formatted_message()
        {
            Mock.Get(announcer).Setup(a => a.Heading("Hello Heading"));

            announcer.Heading("Hello {0}", "Heading");
        }

        [Test]
        public void Say_Should_Say_string_formatted_message()
        {
            Mock.Get(announcer).Setup(a => a.Say("Hello Say"));

            announcer.Say("Hello {0}", "Say");
        }
    }
}