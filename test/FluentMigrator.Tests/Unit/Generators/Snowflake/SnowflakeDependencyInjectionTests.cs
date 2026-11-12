#region License
// Copyright (c) 2026, Fluent Migrator Project
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

using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Snowflake
{
    [Category("DependencyInjection")]
    [Category("Snowflake")]
    [TestFixture]
    public class SnowflakeDependencyInjectionTests
    {
        [Test]
        public void CustomQuoterIsUsedForColumnNames()
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSnowflake())
                .AddScoped<SnowflakeQuoter, LowercaseSnowflakeQuoter>()
                .BuildServiceProvider(true);

            using (var scope = services.CreateScope())
            {
                var generator = scope.ServiceProvider.GetRequiredService<IMigrationGenerator>();
                var expression = new CreateTableExpression { TableName = "MyTable" };
                expression.Columns.Add(new ColumnDefinition { Name = "MyColumn", Type = DbType.Int32, TableName = "MyTable" });

                var statement = generator.Generate(expression);

                Assert.That(statement, Is.EqualTo("CREATE TABLE public.mytable (mycolumn NUMBER NOT NULL);"));
            }
        }

        [Test]
        public void SnowflakeOptionsArePassedToTheGenerator()
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSnowflake(SnowflakeOptions.QuotingEnabled()))
                .BuildServiceProvider(true);

            using (var scope = services.CreateScope())
            {
                var generator = scope.ServiceProvider.GetRequiredService<IMigrationGenerator>();
                var expression = new CreateTableExpression { TableName = "MyTable" };
                expression.Columns.Add(new ColumnDefinition { Name = "MyColumn", Type = DbType.Int32, TableName = "MyTable" });

                var statement = generator.Generate(expression);

                Assert.That(scope.ServiceProvider.GetRequiredService<SnowflakeOptions>().ForceQuote, Is.True);
                Assert.That(statement, Is.EqualTo("CREATE TABLE \"PUBLIC\".\"MyTable\" (\"MyColumn\" NUMBER NOT NULL);"));
            }
        }

        [Test]
        public void ObsoleteQuoteIdentifiersSetsForceQuote()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var options = new SnowflakeOptions { QuoteIdentifiers = true };
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.That(options.ForceQuote, Is.True);
        }

        /// <summary>
        /// Quoter that lowercases unquoted names instead of quoting them.
        /// </summary>
        private class LowercaseSnowflakeQuoter : SnowflakeQuoter
        {
            public LowercaseSnowflakeQuoter(SnowflakeOptions options)
                : base(options)
            {
            }

            /// <inheritdoc />
            public override string Quote(string name) => IsQuoted(name) ? name : name?.ToLowerInvariant();
        }
    }
}
