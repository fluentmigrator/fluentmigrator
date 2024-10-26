#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using FluentMigrator.Runner;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    [Category("StopWatch")]
    public class StopWatchTests
    {
        [Test]
        public void CanGetTheElapsedTime()
        {
            var watch = new StopWatch();

            StopWatch.TimeNow = () => new DateTime(2009, 9, 6, 15, 53, 0, 0);

            watch.Start();

            StopWatch.TimeNow = () => new DateTime(2009, 9, 6, 15, 53, 0, 5);

            watch.Stop();

            watch.ElapsedTime().Milliseconds.ShouldBe(5);
        }
    }
}
