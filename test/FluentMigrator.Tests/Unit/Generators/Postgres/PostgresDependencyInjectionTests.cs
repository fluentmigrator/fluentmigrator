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
using System.Data;
using System.Linq;

using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Base;
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

        [Test]
        public void CanUseCustomPostgresTypeMap()
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .AddScoped<IPostgresTypeMap, CustomPostgresTypeMap>()
                .ConfigureRunner(rb => rb.AddPostgres())
                .BuildServiceProvider(true);

            using (var scope = services.CreateScope())
            {
                var generator = scope.ServiceProvider.GetRequiredService<IMigrationGenerator>();
                var createTableExpression = new CreateTableExpression { TableName = "TestTable" };
                createTableExpression.Columns.Add(new Model.ColumnDefinition { Name = "Name", Type = DbType.String, Size = 255 });

                var statement = generator.Generate(createTableExpression);
                Assert.That(statement, Is.EqualTo("CREATE TABLE \"public\".\"TestTable\" (\"Name\" citext NOT NULL);"));
            }
        }

        [Test]
        public void CanUseCustomPostgresTypeMapWithPostgres15()
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .AddScoped<IPostgresTypeMap, CustomPostgresTypeMap>()
                .ConfigureRunner(rb => rb.AddPostgres15_0())
                .BuildServiceProvider(true);

            using (var scope = services.CreateScope())
            {
                var generator = scope.ServiceProvider.GetRequiredService<IMigrationGenerator>();
                var createTableExpression = new CreateTableExpression { TableName = "TestTable" };
                createTableExpression.Columns.Add(new Model.ColumnDefinition { Name = "Name", Type = DbType.String, Size = 255 });

                var statement = generator.Generate(createTableExpression);
                Assert.That(statement, Is.EqualTo("CREATE TABLE \"public\".\"TestTable\" (\"Name\" citext NOT NULL);"));
            }
        }

        [Test]
        public void CanUseCustomPostgresTypeMapWithPostgres92()
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .AddScoped<IPostgresTypeMap, CustomPostgresTypeMap>()
                .ConfigureRunner(rb => rb.AddPostgres92())
                .BuildServiceProvider(true);

            using (var scope = services.CreateScope())
            {
                var generator = scope.ServiceProvider.GetRequiredService<IMigrationGenerator>();
                var createTableExpression = new CreateTableExpression { TableName = "TestTable" };
                createTableExpression.Columns.Add(new Model.ColumnDefinition { Name = "Name", Type = DbType.String, Size = 255 });

                var statement = generator.Generate(createTableExpression);
                Assert.That(statement, Is.EqualTo("CREATE TABLE \"public\".\"TestTable\" (\"Name\" citext NOT NULL);"));
            }
        }

        [Test]
        public void CanUseCustomPostgresTypeMapRegisteredAfterAddPostgres()
        {
            var services = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddPostgres())
                .AddScoped<IPostgresTypeMap, CustomPostgresTypeMap>()
                .BuildServiceProvider(true);

            using (var scope = services.CreateScope())
            {
                var generator = scope.ServiceProvider.GetRequiredService<IMigrationGenerator>();
                var createTableExpression = new CreateTableExpression { TableName = "TestTable" };
                createTableExpression.Columns.Add(new Model.ColumnDefinition { Name = "Name", Type = DbType.String, Size = 255 });

                var statement = generator.Generate(createTableExpression);
                Assert.That(statement, Is.EqualTo("CREATE TABLE \"public\".\"TestTable\" (\"Name\" citext NOT NULL);"));
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

        /// <summary>
        /// Custom type map for Postgres that maps strings to citext.
        /// </summary>
        private class CustomPostgresTypeMap : TypeMapBase, IPostgresTypeMap
        {
            private const int DecimalCapacity = 1000;
            private const int PostgresMaxVarcharSize = 10485760;

            /// <summary>
            /// Initializes a new instance of the <see cref="CustomPostgresTypeMap"/> class.
            /// </summary>
            public CustomPostgresTypeMap()
            {
                SetupTypeMaps();
            }

            /// <inheritdoc />
            protected override void SetupTypeMaps()
            {
                SetTypeMap(DbType.AnsiStringFixedLength, "char(255)");
                SetTypeMap(DbType.AnsiStringFixedLength, "char($size)", int.MaxValue);
                SetTypeMap(DbType.AnsiString, "text");
                SetTypeMap(DbType.AnsiString, "varchar($size)", PostgresMaxVarcharSize);
                SetTypeMap(DbType.AnsiString, "text", int.MaxValue);
                SetTypeMap(DbType.Binary, "bytea");
                SetTypeMap(DbType.Binary, "bytea", int.MaxValue);
                SetTypeMap(DbType.Boolean, "boolean");
                SetTypeMap(DbType.Byte, "smallint");
                SetTypeMap(DbType.Currency, "money");
                SetTypeMap(DbType.Date, "date");
                SetTypeMap(DbType.DateTime, "timestamp");
                SetTypeMap(DbType.DateTime2, "timestamp");
                SetTypeMap(DbType.DateTimeOffset, "timestamptz");
                SetTypeMap(DbType.Decimal, "decimal(19,5)");
                SetTypeMap(DbType.Decimal, "decimal($size,$precision)", DecimalCapacity);
                SetTypeMap(DbType.Double, "float8");
                SetTypeMap(DbType.Guid, "uuid");
                SetTypeMap(DbType.Int16, "smallint");
                SetTypeMap(DbType.Int32, "integer");
                SetTypeMap(DbType.Int64, "bigint");
                SetTypeMap(DbType.Single, "float4");
                SetTypeMap(DbType.StringFixedLength, "char(255)");
                SetTypeMap(DbType.StringFixedLength, "char($size)", int.MaxValue);
                SetTypeMap(DbType.String, "citext"); // Custom: Map String to citext
                SetTypeMap(DbType.String, "citext", PostgresMaxVarcharSize); // Custom: Map String to citext
                SetTypeMap(DbType.String, "citext", int.MaxValue); // Custom: Map String to citext
                SetTypeMap(DbType.Time, "time");
                SetTypeMap(DbType.Xml, "xml");
                SetTypeMap(DbType.Object, "json");
            }
        }
    }
}
