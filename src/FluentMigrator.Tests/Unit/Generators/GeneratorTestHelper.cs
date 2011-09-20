using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
        public static string TestFunctionName = "TestFunction";

        public static CreateTableExpression GetCreateTableExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1, };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32 });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithDefaultValue()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1, };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String, DefaultValue = "Default", TableName = TestTableName1 });
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName2, Type = DbType.Int32, DefaultValue = 0, TableName = TestTableName1 });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithDefaultFunctionValue()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition
                                       {
                                           Name = TestColumnName1,
                                           Type = DbType.String,
                                           Size = 50,
                                           DefaultValue = new FunctionValue(TestFunctionName)
                                       });
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithDefaultGuidValue()
        {
            var expression = GetCreateTableWithDefaultFunctionValue();
            expression.Columns.First().DefaultValue = SystemMethods.NewGuid;
            return expression;
        }

        public static CreateTableExpression GetCreateTableWithDefaultCurrentDateTimeValue()
        {
            var expression = GetCreateTableWithDefaultFunctionValue();
            expression.Columns.First().DefaultValue = SystemMethods.CurrentDateTime;
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
            var expression = new InsertDataExpression {TableName = TestTableName1};
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
            var expression = new UpdateDataExpression
                                 {
                                     TableName = TestTableName1,
                                     Set = new List<KeyValuePair<string, object>>
                                               {
                                                   new KeyValuePair<string, object>("Name", "Just'in"),
                                                   new KeyValuePair<string, object>("Age", 25)
                                               },
                                     Where = new List<KeyValuePair<string, object>>
                                                 {
                                                     new KeyValuePair<string, object>("Id", 9),
                                                     new KeyValuePair<string, object>("Homepage", null)
                                                 }
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
            var expression = new DeleteDataExpression {TableName = TestTableName1};
            expression.Rows.Add(new DeletionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Name", "Just'in"),
                                        new KeyValuePair<string, object>("Website", null)
                                    });
            return expression;
        }

        public static DeleteDataExpression GetDeleteDataMultipleRowsExpression()
        {
            var expression = new DeleteDataExpression {TableName = TestTableName1};
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
            var expression = new DeleteDataExpression
                                 {
                                     TableName = TestTableName1,
                                     IsAllRows = true
                                 };
            return expression;
        }

        public static RenameColumnExpression GetRenameColumnExpression()
        {
            var expression = new RenameColumnExpression
                                 {
                                     OldName = TestColumnName1,
                                     NewName = TestColumnName2,
                                     TableName = TestTableName1
                                 };
            return expression;
        }

        public static CreateColumnExpression GetCreateDecimalColumnExpression()
        {
            var column = new ColumnDefinition { Name = TestColumnName1, Type = DbType.Decimal, Size = 19, Precision = 2 };
            var expression = new CreateColumnExpression { TableName = TestTableName1, Column = column };
            return expression;
        }

        public static CreateColumnExpression GetCreateColumnExpression()
        {
            var column = new ColumnDefinition { Name = TestColumnName1, Type = DbType.String, Size = 5 };
            var expression = new CreateColumnExpression { TableName = TestTableName1, Column = column };
            return expression;
        }

        public static CreateColumnExpression GetAlterTableAutoIncrementColumnExpression()
        {
            var column = new ColumnDefinition { Name = TestColumnName1, IsIdentity = true, Type = DbType.Int32 };
            var expression = new CreateColumnExpression { TableName = TestTableName1, Column = column };
            return expression;
        }

        public static AlterColumnExpression GetAlterColumnAddAutoIncrementExpression()
        {
            var column = new ColumnDefinition { Name = TestColumnName1, IsIdentity = true, IsPrimaryKey = true, Type = DbType.Int32 };
            var expression = new AlterColumnExpression { TableName = TestTableName1, Column = column };
            return expression;
        }

        public static RenameTableExpression GetRenameTableExpression()
        {
            var expression = new RenameTableExpression
                                 {
                                     OldName = TestTableName1,
                                     NewName = TestTableName2
                                 };
            return expression;
        }

        public static AlterColumnExpression GetAlterColumnExpression()
        {
            var column = new ColumnDefinition
                             {
                                 Name = TestColumnName1,
                                 Type = DbType.String,
                                 Size = 20,
                                 IsNullable = false
                             };
            var expression = new AlterColumnExpression
                                 {
                                     TableName = TestTableName1,
                                     Column = column
                                 };
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
            var expression =  new DeleteTableExpression { TableName = TestTableName1 };
            return expression;
        }

        public static DeleteColumnExpression GetDeleteColumnExpression()
        {
            var expression = new DeleteColumnExpression { TableName = TestTableName1, ColumnName = TestColumnName1 };
            return expression;
        }

        public static DeleteIndexExpression GetDeleteIndexExpression()
        {
            var indexDefinition = new IndexDefinition
                                      {
                                          Name = TestIndexName,
                                          TableName = TestTableName1
                                      };
            var expression = new DeleteIndexExpression { Index = indexDefinition };
            return expression;
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