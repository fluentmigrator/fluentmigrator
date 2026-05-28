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

using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.SqlServer;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    [Category("Generator")]
    [Category("SqlServer2005")]
    public class SqlServer2005AddColumnWithPrimaryKeyTests
    {
        protected IMigrationGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2005Generator();
        }

        [Test]
        public void CanAddColumnWithPrimaryKeyAndIdentity()
        {
            var column = new ColumnDefinition
            {
                Name = "ExampleId",
                Type = DbType.Int64,
                IsNullable = false,
                IsPrimaryKey = true,
                PrimaryKeyName = "PK_Example",
                IsIdentity = true
            };

            var expression = new CreateColumnExpression
            {
                TableName = "Example",
                SchemaName = "dbo",
                Column = column
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[Example] ADD [ExampleId] BIGINT NOT NULL IDENTITY(1,1) CONSTRAINT [PK_Example] PRIMARY KEY;");
        }

        [Test]
        public void CanAddColumnWithPrimaryKeyWithoutIdentity()
        {
            var column = new ColumnDefinition
            {
                Name = "ExampleId",
                Type = DbType.Int32,
                IsNullable = false,
                IsPrimaryKey = true,
                PrimaryKeyName = "PK_Example"
            };

            var expression = new CreateColumnExpression
            {
                TableName = "Example",
                SchemaName = "dbo",
                Column = column
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[Example] ADD [ExampleId] INT NOT NULL CONSTRAINT [PK_Example] PRIMARY KEY;");
        }

        [Test]
        public void CanAddColumnWithPrimaryKeyWithoutName()
        {
            var column = new ColumnDefinition
            {
                Name = "ExampleId",
                Type = DbType.Int32,
                IsNullable = false,
                IsPrimaryKey = true
            };

            var expression = new CreateColumnExpression
            {
                TableName = "Example",
                SchemaName = "dbo",
                Column = column
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[Example] ADD [ExampleId] INT NOT NULL PRIMARY KEY;");
        }
    }
}
