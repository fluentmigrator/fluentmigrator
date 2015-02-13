using System;
using System.Globalization;
using System.Threading;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Generators.Jet;
using FluentMigrator.Runner.Generators.MySql;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    [TestFixture]
    public class OracleQuoterTest
    {
        private IQuoter quoter = default(OracleQuoter);
        private readonly CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

        [SetUp]
        public void SetUp()
        {
            quoter = new OracleQuoter();
        }

        [Test]
        public void TimeSpanIsFormattedQuotes()
        {
            quoter.QuoteValue(new TimeSpan(1,2, 13, 65))
                .ShouldBe("'1 2:14:5.0'");
        }
    }
}
