using System.Collections.Generic;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Redshift;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Redshift
{
    [TestFixture]
    public class RedshiftGeneratorTests
    {
        protected RedshiftGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new RedshiftGenerator();
        }
            
        [Test]
        public void CanCreateTableWithBoolDefaultValue()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].DefaultValue = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" text NOT NULL DEFAULT true, \"TestColumn2\" integer NOT NULL)");
        }

        [Test]
        public void CanUseSystemMethodCurrentUserAsADefaultValueForAColumn()
        {
            const string tableName = "NewTable";
            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Size = 15, Type = DbType.String, DefaultValue = SystemMethods.CurrentUser };
            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = tableName };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"NewColumn\" varchar(15) NOT NULL DEFAULT current_user");
        }

        [Test]
        public void CanUseSystemMethodCurrentUTCDateTimeAsADefaultValueForAColumn()
        {
            const string tableName = "NewTable";
            var columnDefinition = new ColumnDefinition { Name = "NewColumn", Size = 5, Type = DbType.String, DefaultValue = SystemMethods.CurrentUTCDateTime };
            var expression = new CreateColumnExpression { Column = columnDefinition, TableName = tableName };

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"NewTable\" ADD \"NewColumn\" varchar(5) NOT NULL DEFAULT (now() at time zone 'UTC')");
        }

        [Test]
        public void ExplicitUnicodeStringIgnoredForNonSqlServer()
        {
            var expression = new InsertDataExpression {TableName = "TestTable"};
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("NormalString", "Just'in"),
                                        new KeyValuePair<string, object>("UnicodeString", new ExplicitUnicodeString("codethinked'.com"))
                                    });

            var result = Generator.Generate(expression);
            result.ShouldBe("INSERT INTO \"public\".\"TestTable\" (\"NormalString\",\"UnicodeString\") VALUES ('Just''in','codethinked''.com');");
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
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ALTER \"TestColumn1\" TYPE text, ALTER \"TestColumn1\" DROP NOT NULL");
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
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ALTER \"TestColumn1\" TYPE text, ALTER \"TestColumn1\" SET NOT NULL");
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
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ALTER \"TestColumn1\" DROP DEFAULT");
        }

        [Test]
        public void CanAlterDefaultConstraintToCurrentUser()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = SystemMethods.CurrentUser;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ALTER \"TestColumn1\" DROP DEFAULT, ALTER \"TestColumn1\" SET DEFAULT current_user");
        }

        [Test]
        public void CanAlterDefaultConstraintToCurrentDate()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = SystemMethods.CurrentDateTime;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ALTER \"TestColumn1\" DROP DEFAULT, ALTER \"TestColumn1\" SET DEFAULT now()");
        }

        [Test]
        public void CanAlterDefaultConstraintToCurrentUtcDateTime()
        {
            var expression = GeneratorTestHelper.GetAlterDefaultConstraintExpression();
            expression.DefaultValue = SystemMethods.CurrentUTCDateTime;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ALTER \"TestColumn1\" DROP DEFAULT, ALTER \"TestColumn1\" SET DEFAULT (now() at time zone 'UTC')");
        }

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
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ALTER \"TestColumn1\" TYPE text");
        }
    }
}