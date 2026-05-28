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
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Generators.SqlServer;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    [Category("Generator")]
    [Category("SqlServer2005")]
    public class SqlServer2005AlterTableFluentApiTests
    {
        protected IMigrationGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2005Generator();
        }

        [Test]
        public void AlterTable_AddColumn_WithPrimaryKey_WithoutIdentity_UsingFluentApi()
        {
            var migration = new TestMigration_PrimaryKeyNoIdentity();
            var context = new TestMigrationContext();
            migration.GetUpExpressions(context);

            var createColumnExpression = context.Expressions.OfType<CreateColumnExpression>().First();
            var result = Generator.Generate(createColumnExpression);
            
            result.ShouldBe("ALTER TABLE [dbo].[Example] ADD [ExampleId] BIGINT NOT NULL CONSTRAINT [PK_Example] PRIMARY KEY;");
        }

        [Test]
        public void AlterTable_AddColumn_WithPrimaryKey_WithIdentity_UsingFluentApi()
        {
            var migration = new TestMigration_PrimaryKeyWithIdentity();
            var context = new TestMigrationContext();
            migration.GetUpExpressions(context);

            var createColumnExpression = context.Expressions.OfType<CreateColumnExpression>().First();
            var result = Generator.Generate(createColumnExpression);
            
            result.ShouldBe("ALTER TABLE [dbo].[Example] ADD [ExampleId] BIGINT NOT NULL IDENTITY(1,1) CONSTRAINT [PK_Example] PRIMARY KEY;");
        }

        private class TestMigration_PrimaryKeyNoIdentity : Migration
        {
            public override void Up()
            {
                Alter.Table("Example")
                    .InSchema("dbo")
                    .AddColumn("ExampleId")
                    .AsInt64()
                    .NotNullable()
                    .PrimaryKey("PK_Example");
            }

            public override void Down()
            {
            }
        }

        private class TestMigration_PrimaryKeyWithIdentity : Migration
        {
            public override void Up()
            {
                Alter.Table("Example")
                    .InSchema("dbo")
                    .AddColumn("ExampleId")
                    .AsInt64()
                    .NotNullable()
                    .PrimaryKey("PK_Example")
                    .Identity();
            }

            public override void Down()
            {
            }
        }

        private class TestMigrationContext : IMigrationContext
        {
            public ICollection<IMigrationExpression> Expressions { get; set; } = new List<IMigrationExpression>();

            public IServiceProvider ServiceProvider => null;

            public IQuerySchema QuerySchema => null;

            public string Connection { get; set; } = "";
        }
    }
}
