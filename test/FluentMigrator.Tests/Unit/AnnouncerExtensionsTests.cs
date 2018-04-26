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

using FluentMigrator.Runner;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    [Obsolete]
    public class AnnouncerExtensionsTests
    {
        [SetUp]
        public void Setup()
        {
            _announcer = new Mock<IAnnouncer>(MockBehavior.Strict).Object;
        }

        private IAnnouncer _announcer;

        [Test]
        public void ErrorShouldErrorStringFormattedMessage()
        {
            Mock.Get(_announcer).Setup(a => a.Error("Hello Error"));

            _announcer.Error("Hello {0}", "Error");
        }

        [Test]
        public void HeadingShouldHeadingStringFormattedMessage()
        {
            Mock.Get(_announcer).Setup(a => a.Heading("Hello Heading"));

            _announcer.Heading("Hello {0}", "Heading");
        }

        [Test]
        public void SayShouldSayStringFormattedMessage()
        {
            Mock.Get(_announcer).Setup(a => a.Say("Hello Say"));

            _announcer.Say("Hello {0}", "Say");
        }
    }
}
