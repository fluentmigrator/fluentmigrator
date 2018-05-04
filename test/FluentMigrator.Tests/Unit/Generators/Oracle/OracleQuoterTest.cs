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

using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Oracle;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    [TestFixture]
    [Category("Generator")]
    [Category("Quoter")]
    [Category("Oracle")]
    public class OracleQuoterTest
    {
        private IQuoter _quoter;

        [SetUp]
        public void SetUp()
        {
            _quoter = new OracleQuoter();
        }

        [Test]
        public void TimeSpanIsFormattedQuotes()
        {
            _quoter.QuoteValue(new TimeSpan(1, 2, 13, 65))
                   .ShouldBe("'1 2:14:5.0'");
        }

        [Test]
        public void GuidIsFormattedAsOracleAndQuoted()
        {
            Guid givenValue = new Guid("CC28B6C7-9260-4800-9C1F-A5243960C087");

            _quoter.QuoteValue(givenValue)
                   .ShouldBe("'C7B628CC609200489C1FA5243960C087'");
        }
    }
}
