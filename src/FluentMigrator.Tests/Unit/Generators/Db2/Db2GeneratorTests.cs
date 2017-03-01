﻿using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.DB2;
using Xunit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FluentMigrator.Tests.Unit.Generators.Db2
{
    public class Db2GeneratorTests
    {
        protected Db2Generator Generator;

        public Db2GeneratorTests()
        {
            Generator = new Db2Generator();
        }

        [Fact]
        public void CanCreateAutoIncrementColumnForInt64()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.Columns[0].Type = DbType.Int64;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 BIGINT NOT NULL AS IDENTITY, TestColumn2 INTEGER NOT NULL)");
        }

        [Fact]
        public void CanCreateTableWithBinaryColumnWithSize()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].Type = DbType.Binary;
            expression.Columns[0].Size = 10000;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 VARBINARY(10000) NOT NULL, TestColumn2 INTEGER NOT NULL)");
        }

        [Fact]
        public void CanCreateTableWithBoolDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].Type = DbType.Boolean;
            expression.Columns[0].DefaultValue = 'T';

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE TestTable1 (TestColumn1 CHAR(1) NOT NULL DEFAULT 'T', TestColumn2 INTEGER NOT NULL)");
        }

        [Fact]
        public void CanUseSystemMethodCurrentUserAsADefaultValueForAColumn()
        {
            const string tableName = "NewTable";
            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Size = 18, Type = DbType.AnsiString, DefaultValue = SystemMethods.CurrentUser };
            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = tableName };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE NewTable ADD COLUMN NewColumn VARCHAR(18) NOT NULL DEFAULT USER");
        }

        [Fact]
        public void CanUseSystemMethodCurrentUTCDateTimeAsADefaultValueForAColumn()
        {
            const string tableName = "NewTable";
            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Size = 5, Type = DbType.String, DefaultValue = SystemMethods.CurrentUTCDateTime };
            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = tableName };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE NewTable ADD COLUMN NewColumn VARGRAPHIC(5) CCSID 1200 NOT NULL DEFAULT (CURRENT_TIMESTAMP - CURRENT_TIMEZONE)");
        }

        [Fact]
        public void ExplicitUnicodeStringIgnoredForNonSqlServer()
        {
            var expression = new InsertDataExpression { TableName = "TestTable" };
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("NormalString", "Just'in"),
                                        new KeyValuePair<string, object>("UnicodeString", new ExplicitUnicodeString("codethinked'.com"))
                                    });

            var result = Generator.Generate(expression);
            result.ShouldBe("INSERT INTO TestTable (NormalString, UnicodeString) VALUES ('Just''in', 'codethinked''.com')");
        }

        [Fact]
        public void CanAlterColumnAndSetAsNullable()
        {
            var expression = new AlterColumnExpression
            {
                Column = new ColumnDefinition { Type = DbType.String, Name = "TestColumn1", IsNullable = true },
                SchemaName = "TestSchema",
                TableName = "TestTable1"
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 SET DATA TYPE DBCLOB(1048576) CCSID 1200");
        }

        [Fact]
        public void CanAlterColumnAndSetAsNotNullable()
        {
            var expression = new AlterColumnExpression
            {
                Column = new ColumnDefinition { Type = DbType.String, Name = "TestColumn1", IsNullable = false },
                SchemaName = "TestSchema",
                TableName = "TestTable1"
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 SET DATA TYPE DBCLOB(1048576) CCSID 1200 NOT NULL");
        }

        [Fact]
        public void CanDeleteDefaultConstraint()
        {
            var expression = new DeleteDefaultConstraintExpression
            {
                ColumnName = "TestColumn1",
                SchemaName = "TestSchema",
                TableName = "TestTable1"
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 DROP DEFAULT");
        }

        [Fact]
        public void CanAlterDefaultConstraintToCurrentUser()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = SystemMethods.CurrentUser;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 SET DEFAULT USER");
        }

        [Fact]
        public void CanAlterDefaultConstraintToCurrentDate()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = SystemMethods.CurrentDateTime;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 SET DEFAULT CURRENT_TIMESTAMP");
        }

        //[Fact]
        //public void CanAlterDefaultConstraintToCurrentUtcDateTime()
        //{
        //    var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
        //    expression.DefaultValue = SystemMethods.CurrentUTCDateTime;
        //    expression.SchemaName = "TestSchema";

        //    var result = Generator.Generate(expression);
        //    result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 DROP DEFAULT, ALTER TestColumn1 SET DEFAULT (now() at time zone 'UTC')");
        //}

        [Fact]
        public void CanAlterColumnAndOnlySetTypeIfIsNullableNotSet()
        {
            var expression = new AlterColumnExpression
            {
                Column = new ColumnDefinition { Type = DbType.String, Name = "TestColumn1", IsNullable = null },
                SchemaName = "TestSchema",
                TableName = "TestTable1"
            };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE TestSchema.TestTable1 ALTER COLUMN TestColumn1 SET DATA TYPE DBCLOB(1048576) CCSID 1200 NOT NULL");
        }
    }
}
