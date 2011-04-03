using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using System.Data;

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
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String, DefaultValue = "Default", TableName=TestTableName1 });
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
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsPrimaryKey = true, PrimaryKeyName="TestKey", Type = DbType.String });
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
            expression.Index.Name =  TestIndexName;
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
            expression.Rows.Add(new InsertionDataDefinition
									{
										new KeyValuePair<string, object>("Id", 1),
										new KeyValuePair<string, object>("Name", "Just'in"),
										new KeyValuePair<string, object>("Website", "codethinked.com")
									});
            expression.Rows.Add(new InsertionDataDefinition
									{
										new KeyValuePair<string, object>("Id", 2),
										new KeyValuePair<string, object>("Name", @"Na\te"),
										new KeyValuePair<string, object>("Website", "kohari.org")
									});

            return expression;
        }

        public static UpdateDataExpression GetUpdateDataExpression()
        {
            var expression = new UpdateDataExpression();
            expression.TableName = TestTableName1;

            expression.Set = new List<KeyValuePair<string, object>>
								 {
									 new KeyValuePair<string, object>("Name", "Just'in"),
									 new KeyValuePair<string, object>("Age", 25)
								 };

            expression.Where = new List<KeyValuePair<string, object>>
								   {
									   new KeyValuePair<string, object>("Id", 9),
									   new KeyValuePair<string, object>("Homepage", null)
								   };
            return expression;
        }

        public static InsertDataExpression GetInsertGUIDExpression()
        {
            var expression = new InsertDataExpression { TableName = TestTableName1 };
            expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("guid", TestGuid) });

            return expression;
        }

        public static DeleteDataExpression GetDeleteDataExpression()
        {
            var expression = new DeleteDataExpression();
            expression.TableName = TestTableName1;
            expression.Rows.Add(new DeletionDataDefinition
									{
										new KeyValuePair<string, object>("Name", "Just'in"),
										new KeyValuePair<string, object>("Website", null)
									});

            return expression;
        }

        public static DeleteDataExpression GetDeleteDataMultipleRowsExpression()
        {
            var expression = new DeleteDataExpression();
            expression.TableName = TestTableName1;
            expression.Rows.Add(new DeletionDataDefinition
									{
										new KeyValuePair<string, object>("Name", "Just'in"),
										new KeyValuePair<string, object>("Website", null)
									});
            expression.Rows.Add(new DeletionDataDefinition
									{
										new KeyValuePair<string, object>("Website", "github.com")
									});

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

        public static AlterColumnExpression GetAlterTableExpression()
        {
            var expression = new AlterColumnExpression();
            expression.TableName = TestTableName1;

            expression.Column = new ColumnDefinition();
            expression.Column.Name = TestColumnName1;
            expression.Column.Type = DbType.String;
            expression.Column.Size = 20;
            expression.Column.IsNullable = false;

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
            return new DeleteColumnExpression { TableName = TestTableName1, ColumnName = TestColumnName1 };
        }

        public static DeleteIndexExpression GetDeleteIndexExpression()
        {
            IndexDefinition indexDefinition = new IndexDefinition { Name = TestIndexName, TableName=TestTableName1 };
            return new DeleteIndexExpression { Index = indexDefinition };
        }

        public static DeleteForeignKeyExpression GetDeleteForeignKeyExpression()
        {
            var expression = new DeleteForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.ForeignTable = TestTableName1;
            return expression;
        }


    }
}
