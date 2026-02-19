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

using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    [Category("Generator")]
    [Category("Quoter")]
    [Category("Postgres")]
    public class PostgresQuotesTests
    {
        [SetUp]
        public void SetUp()
        {
            _quoter = new PostgresQuoter(new PostgresOptions());
        }

        private IQuoter _quoter = default(PostgresQuoter);

        [Test]
        public void ByteArrayIsFormattedWithQuotes()
        {
            _quoter.QuoteValue(new byte[] { 0, 254, 13, 18, 125, 17 })
                .ShouldBe(@"E'\\x00FE0D127D11'");
        }

        [Test]
        public void DisableForceQuoteRemovesQuotes()
        {
            _quoter = new PostgresQuoter(new PostgresOptions() { ForceQuote = false });
            _quoter.Quote("TableName").ShouldBe("TableName");
        }

        [Test]
        public void DisableForceQuoteQuotesReservedKeyword()
        {
            _quoter = new PostgresQuoter(new PostgresOptions() { ForceQuote = false });

            _quoter.Quote("between").ShouldBe(@"""between""");
            _quoter.Quote("BETWEEN").ShouldBe(@"""BETWEEN""");
        }
    }
}
