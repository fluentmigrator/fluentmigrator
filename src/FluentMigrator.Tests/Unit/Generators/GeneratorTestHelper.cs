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
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithDefaultValue()
        {
            CreateTableExpression expression = new CreateTableExpression() { TableName = TestTableName1, };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String, DefaultValue = "POO" });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithPrimaryKeyExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsIdentity = false, IsPrimaryKey = true, Type = DbType.String });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithGetAutoIncrementExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsIdentity = true, IsPrimaryKey = true, Type = DbType.String });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithMultipartKeyExpression()
        {
            CreateTableExpression expression = GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].IsPrimaryKey = true;
            return expression;

        }

        public static CreateIndexExpression GetCreateIndexExpression()
        {
            IndexColumnDefinition indexColumnDefinition = new IndexColumnDefinition { Name = TestIndexName };
            IndexDefinition indexDefinition = new IndexDefinition { TableName = TestTableName1, Name = TestIndexName, Columns = new List<IndexColumnDefinition> { indexColumnDefinition } };
            return new CreateIndexExpression { Index = indexDefinition };
        }

        public static CreateIndexExpression GetMultiColumnCreateIndexExpression()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = "IX_TEST";
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

        public static RenameColumnExpression GetRenameColumnExpression()
        {
            return new RenameColumnExpression { OldName = TestColumnName1, NewName = TestColumnName2, TableName = TestTableName1 };
        }

        public static CreateColumnExpression GetCreateDecimalColumnExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, Type = DbType.Decimal, Size = 5, Precision = 19 };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static CreateColumnExpression GetCreateColumnExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, Type = DbType.String, Size = 5 };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        public static CreateColumnExpression GetCreateAutoIncrementColumnExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, IsIdentity = true, IsPrimaryKey = true, Type = DbType.String };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
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
            expression.ForeignKey.PrimaryTable = TestTableName1;
            expression.ForeignKey.ForeignTable = TestTableName2;
            expression.ForeignKey.PrimaryColumns = new[] { TestColumnName1 };
            expression.ForeignKey.ForeignColumns = new[] { TestColumnName2 };

            return expression;
        }

        public static CreateForeignKeyExpression GetCreateMultiColumnForeignKeyExpression()
        {
            var expression = new CreateForeignKeyExpression();
            expression.ForeignKey.Name = "FK_Test";
            expression.ForeignKey.PrimaryTable = TestTableName1;
            expression.ForeignKey.ForeignTable = TestTableName2;
            expression.ForeignKey.PrimaryColumns = new[] { TestColumnName1, "Column2" };
            expression.ForeignKey.ForeignColumns = new[] { TestColumnName2, "Column4" };

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
            IndexDefinition indexDefinition = new IndexDefinition { Name = TestIndexName };
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
