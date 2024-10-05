#region License
// Copyright (c) 2019, Fluent Migrator Project
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
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [Category("DependencyInjection")]
    [Category("Postgres")]
    [TestFixture]
    public class PostgresDependencyInjectionTests
    {
        [Test]
        public void CanUseCustomPostgresQuoter()
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddPostgres())
                .AddScoped<PostgresQuoter, CustomPostgresQuoter>()
                .BuildServiceProvider(true);

            using (var scope = services.CreateScope())
            {
                var contextMock = new Mock<IMigrationContext>();
                var expressions = new List<IMigrationExpression>();
                contextMock.SetupGet(context => context.Expressions).Returns(expressions);
                contextMock.SetupGet(context => context.ServiceProvider).Returns(scope.ServiceProvider);
                contextMock.SetupGet(context => context.QuerySchema).Throws<InvalidOperationException>();

                var generator = scope.ServiceProvider.GetRequiredService<IMigrationGenerator>();
                var expression = new AlterTableExpressionBuilder(
                    new AlterTableExpression()
                    {
                        TableName = "a-table",
                    },
                    contextMock.Object);
                expression
                    .AddColumn("a-column").AsDateTimeOffset().WithDefault(SystemMethods.CurrentDateTimeOffset);

                Assert.That(expressions, Is.Not.Empty);
                Assert.That(expressions, Has.Count.EqualTo(1));
                Assert.That(expressions.Single(), Is.TypeOf<CreateColumnExpression>());
                var addColumnExpression = (CreateColumnExpression) expressions.Single();
                var statement = generator.Generate(addColumnExpression);
                Assert.That(statement, Is.EqualTo("ALTER TABLE \"public\".\"a-table\" ADD \"a-column\" timestamptz NOT NULL DEFAULT my_current_timestamp();"));
            }
        }

        /// <summary>
        /// Custom quoter for Postgres.
        /// </summary>
        private class CustomPostgresQuoter : PostgresQuoter
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomPostgresQuoter"/> class.
            /// </summary>
            /// <param name="options">The Postgres specific options.</param>
            public CustomPostgresQuoter(PostgresOptions options)
                : base(options)
            {
            }

            /// <inheritdoc />
            public override string FormatSystemMethods(SystemMethods value)
            {
                switch (value)
                {
                    case SystemMethods.CurrentDateTimeOffset:
                        return "my_current_timestamp()";
                    default:
                        return base.FormatSystemMethods(value);
                }
            }
        }
    }
}
