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
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.DB2;
using FluentMigrator.Runner.Generators.DB2.iSeries;

using Microsoft.Extensions.Options;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Db2
{
    [TestFixture]
    [Category("Db2")]
    public class Db2GeneratorTests
    {
        protected Db2Generator Generator;

        [SetUp]
        public void Setup()
        {
            var generatorOptions = new OptionsWrapper<GeneratorOptions>(new GeneratorOptions());
            Generator = new Db2Generator(new Db2ISeriesQuoter(), generatorOptions);
        }

        [Test]
        public void CanCreateAutoIncrementColumnForInt64()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.Columns[0].Type = DbType.Int64;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 BIGINT NOT NULL AS IDENTITY, TestColumn2 INTEGER NOT NULL);");
        }

        [Test]
        public void CanCreateTableWithBinaryColumnWithSize()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].Type = DbType.Binary;
            expression.Columns[0].Size = 10000;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARBINARY(10000) NOT NULL, TestColumn2 INTEGER NOT NULL);");
        }

        [Test]
        public void CanCreateTableWithBoolDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].Type = DbType.Boolean;
            expression.Columns[0].DefaultValue = 'T';

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 CHAR(1) NOT NULL DEFAULT 'T', TestColumn2 INTEGER NOT NULL);");
        }

        [Test]
        public void CanUseSystemMethodCurrentUserAsADefaultValueForAColumn()
        {
            const string tableName = "NewTable";
            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Size = 18, Type = DbType.AnsiString, DefaultValue = SystemMethods.CurrentUser };
            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = tableName };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE NewTable ADD COLUMN NewColumn VARCHAR(18) NOT NULL DEFAULT USER;");
        }

        [Test]
        public void CanUseSystemMethodCurrentUTCDateTimeAsADefaultValueForAColumn()
        {
            const string tableName = "NewTable";
            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Size = 5, Type = DbType.String, DefaultValue = SystemMethods.CurrentUTCDateTime };
            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = tableName };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE NewTable ADD COLUMN NewColumn VARGRAPHIC(5) CCSID 1200 NOT NULL DEFAULT (CURRENT_TIMESTAMP - CURRENT_TIMEZONE);");
        }

        [Test]
        public void NonUnicodeQuotesCorrectly()
        {
            var expression = new InsertDataExpression { TableName = "TestTable" };
            expression.Rows.Add(new InsertionDataDefinition
            {
                new KeyValuePair<string, object>("NormalString", "Just'in"),
                new KeyValuePair<string, object>("UnicodeString", new NonUnicodeString("codethinked'.com"))
            });

            var result = Generator.Generate(expression);
            result.ShouldBe("INSERT INTO TestTable (NormalString, UnicodeString) VALUES ('Just''in', 'codethinked''.com');");
        }

        [Test]
        public void CanAlterColumnAndSetAsNullable()
        {
            var expression = new AlterColumnExpression
            {
                Column = new ColumnDefinition { Type = DbType.String, Name = "TestColumn1", IsNullable = true },
                SchemaName = "TestSchema",
                TableName = "TestTable1"
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 SET DATA TYPE DBCLOB(1048576) CCSID 1200;");
        }

        [Test]
        public void CanAlterColumnAndSetAsNotNullable()
        {
            var expression = new AlterColumnExpression
            {
                Column = new ColumnDefinition { Type = DbType.String, Name = "TestColumn1", IsNullable = false },
                SchemaName = "TestSchema",
                TableName = "TestTable1"
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 SET DATA TYPE DBCLOB(1048576) CCSID 1200 NOT NULL;");
        }

        [Test]
        public void CanDeleteDefaultConstraint()
        {
            var expression = new DeleteDefaultConstraintExpression
            {
                ColumnName = "TestColumn1",
                SchemaName = "TestSchema",
                TableName = "TestTable1"
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 DROP DEFAULT;");
        }

        [Test]
        public void CanAlterDefaultConstraintToCurrentUser()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = SystemMethods.CurrentUser;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 SET DEFAULT USER;");
        }

        [Test]
        public void CanAlterDefaultConstraintToCurrentDate()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = SystemMethods.CurrentDateTime;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 SET DEFAULT CURRENT_TIMESTAMP;");
        }

        //[Test]
        //public void CanAlterDefaultConstraintToCurrentUtcDateTime()
        //{
        //    var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
        //    expression.DefaultValue = SystemMethods.CurrentUTCDateTime;
        //    expression.SchemaName = "TestSchema";

        //    var result = Generator.Generate(expression);
        //    result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 DROP DEFAULT, ALTER TestColumn1 SET DEFAULT (now() at time zone 'UTC')");
        //}

        [Test]
        public void CanAlterColumnAndOnlySetTypeIfIsNullableNotSet()
        {
            var expression = new AlterColumnExpression
            {
                Column = new ColumnDefinition { Type = DbType.String, Name = "TestColumn1", IsNullable = null },
                SchemaName = "TestSchema",
                TableName = "TestTable1"
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 SET DATA TYPE DBCLOB(1048576) CCSID 1200 NOT NULL;");
        }
    }
}
