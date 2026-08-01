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

using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators.MySql;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.MySql4
{
    [TestFixture]
    [Category("Generator")]
    [Category("Quoter")]
    [Category("MySql4")]
    public class MySql4QuoterTest
    {
        private IQuoter _quoter = default(MySqlQuoter);

        [SetUp]
        public void SetUp()
        {
            _quoter = new MySqlQuoter();
        }

        [Test]
        public void CurrentUTCDateTimeIsFormattedParentheses()
        {
            _quoter.QuoteValue(SystemMethods.CurrentUTCDateTime)
                .ShouldBe("(UTC_TIMESTAMP)");
        }

        [Test]
        public void TimeSpanIsFormattedQuotes()
        {
            _quoter.QuoteValue(new TimeSpan(1,2, 13, 65))
                .ShouldBe("'26:14:05'");
        }

        /// <summary>
        /// MySQL needs backslashes doubled inside a string literal, and that escaping is what
        /// <see cref="MySqlQuoter.QuoteValue"/> adds on top of the generic quoter.
        /// </summary>
        [Test]
        public void BackslashInStringIsEscaped()
        {
            _quoter.QuoteValue(@"a\b")
                .ShouldBe(@"'a\\b'");
        }

        /// <summary>
        /// Regression test for #2335. <see cref="RawSql"/> is an opt-in escape hatch: the caller has
        /// taken responsibility for the SQL, so it must reach the server byte-for-byte. The
        /// string-literal backslash escaping was being applied to it as well, which silently
        /// corrupted regex predicates, LIKE patterns and JSON paths.
        /// </summary>
        [TestCase("CURRENT_TIMESTAMP")]
        [TestCase(@"c REGEXP '\d+'")]
        [TestCase(@"path LIKE 'C:\Temp\%'")]
        [TestCase(@"json_col->'$.a\b'")]
        [TestCase("name = 'O''Brien'")]
        public void RawSqlIsPassedThroughVerbatim(string sql)
        {
            _quoter.QuoteValue(RawSql.Insert(sql))
                .ShouldBe(sql);
        }
    }
}
