using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Tests.Unit.Generators
{
    public static class GeneratorTestHelper
    {

        public static string TestTableName1 = "TestTable1";
        public static string TestTableName2 = "TestTable2";
        public static string TestColumnName1 = "TestColumn1";
        public static string TestColumnName2 = "TestColumn2";
        public static string TestIndexName = "TestIndex";
        public static Guid TestGuid = Guid.NewGuid();

        public static CreateTableExpression GetCreateTableExpression()
        {
            CreateTableExpression expression = new CreateTableExpression() { TableName = TestTableName1, };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32 });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithDefaultValue()
        {
            CreateTableExpression expression = new CreateTableExpression() { TableName = TestTableName1, };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String, DefaultValue = "Default", TableName = TestTableName1 });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32, DefaultValue = 0, TableName = TestTableName1 });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithPrimaryKeyExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsPrimaryKey = true, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32 });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithNamedPrimaryKeyExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsPrimaryKey = true, PrimaryKeyName = "TestKey", Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32 });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithMultiColumNamedPrimaryKeyExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsPrimaryKey = true, PrimaryKeyName = "TestKey", Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32, IsPrimaryKey = true });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithAutoIncrementExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsIdentity = true, Type = DbType.Int32 });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32 });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithMultiColumnPrimaryKeyExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsPrimaryKey = true, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, IsPrimaryKey = true, Type = DbType.Int32 });
            return expression;

        }

        public static CreateTableExpression GetCreateTableWithNullableColumn()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsNullable = true, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32 });
            return expression;
        }

        public static CreateIndexExpression GetCreateIndexExpression()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = TestIndexName;
            expression.Index.TableName = TestTableName1;
            expression.Index.IsUnique = false;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = TestColumnName1 });
            return expression;
        }

        public static CreateIndexExpression GetCreateMultiColumnCreateIndexExpression()
        {

            var expression = new CreateIndexExpression();
            expression.Index.Name = TestIndexName;
            expression.Index.TableName = TestTableName1;
            expression.Index.IsUnique = false;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = TestColumnName1 });
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = TestColumnName2 });
            return expression;
        }

        public static CreateIndexExpression GetCreateUniqueIndexExpression()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = TestIndexName;
            expression.Index.TableName = TestTableName1;
            expression.Index.IsUnique = true;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = TestColumnName1 });
            return expression;
        }

        public static CreateIndexExpression GetCreateUniqueMultiColumnIndexExpression()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = TestIndexName;
            expression.Index.TableName = TestTableName1;
            expression.Index.IsUnique = true;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = TestColumnName1 });
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = TestColumnName2 });
            return expression;
        }

        public static InsertDataExpression GetInsertDataExpression()
        {
            var expression = new InsertDataExpression();
            expression.TableName = TestTableName1;
            expression.Rows.Add(new ExplicitDataDefinition(
                                        new DataValue("Id", 1),
                                        new DataValue("Name", "Just'in"),
                                        new DataValue("Website", "codethinked.com")
                                ));
            expression.Rows.Add(new ExplicitDataDefinition(
                                        new DataValue("Id", 2),
                                        new DataValue("Name", @"Na\te"),
                                        new DataValue("Website", "kohari.org")
                                ));

            return expression;
        }

        public static UpdateDataExpression GetUpdateDataExpression()
        {
            var expression = new UpdateDataExpression();
            expression.TableName = TestTableName1;

            expression.Set.AddRange(new []  { new ExplicitDataDefinition(new DataValue("Name", "Just'in"), new DataValue("Age", 25)) } );
            expression.Where.AddRange(new [] { new ExplicitDataDefinition(new DataValue("Id", 9), new DataValue("Homepage", null)) } );

            return expression;
        }

        public static UpdateDataExpression GetUpdateDataExpressionWithAllRows()
        {
            var expression = new UpdateDataExpression();
            expression.TableName = TestTableName1;

            expression.Set.AddRange(new[] { new ExplicitDataDefinition(new DataValue("Name", "Just'in"), new DataValue("Age", 25)) });
            expression.IsAllRows = true;

            return expression;
        }

        public static InsertDataExpression GetInsertGUIDExpression()
        {
            var expression = new InsertDataExpression { TableName = TestTableName1 };
            expression.Rows.Add(new ExplicitDataDefinition(new DataValue("guid", TestGuid)));

            return expression;
        }

        public static DeleteDataExpression GetDeleteDataExpression()
        {
            var expression = new DeleteDataExpression();
            expression.TableName = TestTableName1;
            expression.Rows.Add(new ExplicitDataDefinition(new DataValue("Name", "Just'in"), new DataValue("Website", null)));

            return expression;
        }

        public static DeleteDataExpression GetDeleteDataMultipleRowsExpression()
        {
            var expression = new DeleteDataExpression();
            expression.TableName = TestTableName1;
            expression.Rows.Add(new ExplicitDataDefinition(new DataValue("Name", "Just'in"), new DataValue("Website", null)));
            expression.Rows.Add(new ExplicitDataDefinition(new DataValue("Website", "github.com")));

            return expression;
        }

        public static DeleteDataExpression GetDeleteDataAllRowsExpression()
        {
            var expression = new DeleteDataExpression();
            expression.TableName = TestTableName1;
            expression.IsAllRows = true;
            return expression;
        }

        public static RenameColumnExpression GetRenameColumnExpression()
        {
            return new RenameColumnExpression { OldName = TestColumnName1, NewName = TestColumnName2, TableName = TestTableName1 };
        }

        public static CreateColumnExpression GetCreateDecimalColumnExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, Type = DbType.Decimal, Size = 19, Precision = 2 };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static CreateColumnExpression GetCreateColumnExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, Type = DbType.String, Size = 5 };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static CreateColumnExpression GetAlterTableAutoIncrementColumnExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, IsIdentity = true, Type = DbType.Int32 };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static AlterColumnExpression GetAlterColumnAddAutoIncrementExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, IsIdentity = true, IsPrimaryKey = true, Type = DbType.Int32 };
            return new AlterColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static RenameTableExpression GetRenameTableExpression()
        {
            var expression = new RenameTableExpression();
            expression.OldName = TestTableName1;
            expression.NewName = TestTableName2;
            return expression;
        }

        public static AlterColumnExpression GetAlterColumnExpression()
        {
            var expression = new AlterColumnExpression();
            expression.TableName = TestTableName1;

            expression.Column = new ColumnDefinition();
            expression.Column.Name = TestColumnName1;
            expression.Column.Type = DbType.String;
            expression.Column.Size = 20;
            expression.Column.IsNullable = false;
            expression.Column.ModificationType = ColumnModificationType.Alter;

            return expression;
        }

        public static CreateForeignKeyExpression GetCreateForeignKeyExpression()
        {
            var expression = new CreateForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.PrimaryTable = TestTableName2;
            expression.ForeignKey.ForeignTable = TestTableName1;
            expression.ForeignKey.PrimaryColumns = new[] { TestColumnName2 };
            expression.ForeignKey.ForeignColumns = new[] { TestColumnName1 };

            return expression;
        }

        public static CreateForeignKeyExpression GetCreateMultiColumnForeignKeyExpression()
        {
            var expression = new CreateForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.PrimaryTable = TestTableName2;
            expression.ForeignKey.ForeignTable = TestTableName1;
            expression.ForeignKey.PrimaryColumns = new[] { TestColumnName2, "TestColumn4" };
            expression.ForeignKey.ForeignColumns = new[] { TestColumnName1, "TestColumn3" };

            return expression;
        }

        public static DeleteTableExpression GetDeleteTableExpression()
        {
            return new DeleteTableExpression { TableName = TestTableName1 };
        }

        public static DeleteColumnExpression GetDeleteColumnExpression()
        {
            return GetDeleteColumnExpression(new [] {TestColumnName1});
        }

        public static DeleteColumnExpression GetDeleteColumnExpression(string[] columns)
        {
            return new DeleteColumnExpression { TableName = TestTableName1, ColumnNames = columns };
        }

        public static DeleteIndexExpression GetDeleteIndexExpression()
        {
            IndexDefinition indexDefinition = new IndexDefinition { Name = TestIndexName, TableName = TestTableName1 };
            return new DeleteIndexExpression { Index = indexDefinition };
        }

        public static DeleteForeignKeyExpression GetDeleteForeignKeyExpression()
        {
            var expression = new DeleteForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.ForeignTable = TestTableName1;
            return expression;
        }

        public static DeleteConstraintExpression GetDeletePrimaryKeyExpression()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.ConstraintName = "TESTPRIMARYKEY";
            return expression;
        }

        public static DeleteConstraintExpression GetDeleteUniqueConstraintExpression()
        {
            var expression = new DeleteConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.ConstraintName = "TESTUNIQUECONSTRAINT";
            return expression;
        }


        public static CreateConstraintExpression GetCreatePrimaryKeyExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.ApplyConventions(new MigrationConventions());
            return expression;
        }

        public static CreateConstraintExpression GetCreateNamedPrimaryKeyExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.Constraint.ConstraintName = "TESTPRIMARYKEY";
            return expression;
        }

        public static CreateConstraintExpression GetCreateMultiColumnPrimaryKeyExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.Constraint.Columns.Add(TestColumnName2);
            expression.ApplyConventions(new MigrationConventions());
            return expression;
        }

        public static CreateConstraintExpression GetCreateMultiColumnNamedPrimaryKeyExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.PrimaryKey);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.Constraint.Columns.Add(TestColumnName2);
            expression.Constraint.ConstraintName = "TESTPRIMARYKEY";
            return expression;
        }

        public static CreateConstraintExpression GetCreateUniqueConstraintExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.ApplyConventions(new MigrationConventions());
            return expression;
        }

        public static CreateConstraintExpression GetCreateNamedUniqueConstraintExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.Constraint.ConstraintName = "TESTUNIQUECONSTRAINT";
            return expression;
        }

        public static CreateConstraintExpression GetCreateMultiColumnUniqueConstraintExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.Constraint.Columns.Add(TestColumnName2);
            expression.ApplyConventions(new MigrationConventions());
            return expression;
        }

        public static CreateConstraintExpression GetCreateMultiColumnNamedUniqueConstraintExpression()
        {
            var expression = new CreateConstraintExpression(ConstraintType.Unique);
            expression.Constraint.TableName = TestTableName1;
            expression.Constraint.Columns.Add(TestColumnName1);
            expression.Constraint.Columns.Add(TestColumnName2);
            expression.Constraint.ConstraintName = "TESTUNIQUECONSTRAINT";
            return expression;
        }

        public static AlterDefaultConstraintExpression GetAlterDefaultConstraintExpression()
        {
            var expression = new AlterDefaultConstraintExpression
                                 {
                                     ColumnName = TestColumnName1,
                                     DefaultValue = 1,
                                     TableName = TestTableName1
                                 };
            return expression;
        }
    }
}
