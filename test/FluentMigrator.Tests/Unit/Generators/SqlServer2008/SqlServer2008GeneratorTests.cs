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
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2008
{
    [TestFixture]
    [Category("Generator")]
    [Category("SqlServer2008")]
    public class SqlServer2008GeneratorTests
    {
        protected SqlServer2008Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2008Generator();
        }

        [Test]
        public void CanCreateTableWithDateTimeOffsetColumn()
        {
            var expression = new CreateTableExpression {TableName = "TestTable1"};
            expression.Columns.Add(new ColumnDefinition {TableName = "TestTable1", Name = "TestColumn1", Type = DbType.DateTimeOffset});
            expression.Columns.Add(new ColumnDefinition {TableName = "TestTable1", Name = "TestColumn2", Type = DbType.DateTime2});
            expression.Columns.Add(new ColumnDefinition {TableName = "TestTable1", Name = "TestColumn3", Type = DbType.Date});
            expression.Columns.Add(new ColumnDefinition { TableName = "TestTable1", Name = "TestColumn4", Type = DbType.Time });

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE [dbo].[TestTable1] ([TestColumn1] DATETIMEOFFSET NOT NULL, [TestColumn2] DATETIME2 NOT NULL, [TestColumn3] DATE NOT NULL, [TestColumn4] TIME NOT NULL);");
        }

        [Test]
        public void CanUseSystemMethodCurrentDateTimeOffsetAsADefaultValueForAColumn()
        {
            var expression = new CreateColumnExpression
            {
                Column = new ColumnDefinition
                {
                    Name = "NewColumn",
                    Type = DbType.DateTime,
                    DefaultValue = SystemMethods.CurrentDateTimeOffset
                },
                TableName = "NewTable"
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[NewTable] ADD [NewColumn] DATETIME NOT NULL CONSTRAINT [DF__NewColumn] DEFAULT SYSDATETIMEOFFSET();");
        }

        [Test]
        public void CanInsertScopeIdentity()
        {
            var expression = new InsertDataExpression {TableName = "TestTable"};
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Id", 1),
                                        new KeyValuePair<string, object>("Name", RawSql.Insert("SCOPE_IDENTITY()")),
                                        new KeyValuePair<string, object>("Website", "codethinked.com")
                                    });

            var result = Generator.Generate(expression);
            result.ShouldBe("INSERT INTO [dbo].[TestTable] ([Id], [Name], [Website]) VALUES (1, SCOPE_IDENTITY(), N'codethinked.com');");
        }

        [Test]
        public void CanInsertAtAtIdentity()
        {
            var expression = new InsertDataExpression {TableName = "TestTable"};
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Id", 1),
                                        new KeyValuePair<string, object>("Name", RawSql.Insert("@@IDENTITY")),
                                        new KeyValuePair<string, object>("Website", "codethinked.com")
                                    });

            var result = Generator.Generate(expression);
            result.ShouldBe("INSERT INTO [dbo].[TestTable] ([Id], [Name], [Website]) VALUES (1, @@IDENTITY, N'codethinked.com');");
        }

        [Test]
        public void NonUnicodeQuotesCorrectly()
        {
            var expression = new InsertDataExpression { TableName = "TestTable" };
            expression.Rows.Add(new InsertionDataDefinition
            {
                new KeyValuePair<string, object>("NonUnicodeStringValue", new NonUnicodeString("NonUnicodeString")),
            });

            var result = Generator.Generate(expression);
            result.ShouldBe("INSERT INTO [dbo].[TestTable] ([NonUnicodeStringValue]) VALUES ('NonUnicodeString');");
        }
    }
}
