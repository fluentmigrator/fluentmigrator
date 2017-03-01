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
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.MySql
{
    public class MySqlQuoterTest
    {
        private IQuoter quoter = default(MySqlQuoter);
        private readonly CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

        public MySqlQuoterTest()
        {
            quoter = new MySqlQuoter();
        }

        [Fact]
        public void TimeSpanIsFormattedQuotes()
        {
            quoter.QuoteValue(new TimeSpan(1,2, 13, 65))
                .ShouldBe("'26:14:05'");
        }
    }
}

