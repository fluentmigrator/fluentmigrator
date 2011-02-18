

namespace FluentMigrator.Tests.Unit.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using System.Data;
    using NUnit.Framework;

    [TestFixture]
    public class GeneratorTestBase
    {

        protected virtual string TestTableName1 { get; set; }
        protected virtual string TestTableName2 { get; set; }
        protected virtual string TestColumnName1 { get; set; }
        protected virtual string TestColumnName2 { get; set; }
        protected virtual string TestIndexName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:GeneratorTestBase"/> class.
        /// </summary>
        public GeneratorTestBase()
        {
            TestTableName1 = "TestTable1";
            TestTableName2 = "TestTable2";
            TestColumnName1 = "TestColumn1";
            TestColumnName2 = "TestColumn2";
            TestIndexName = "TestIndex";
        }

        protected DeleteColumnExpression GetDeleteColumnExpression(){
            return new DeleteColumnExpression { TableName = this.TestTableName1, ColumnName = this.TestColumnName1 };
        }
        protected DeleteTableExpression GetDeleteTableExpression()
        {
            return new DeleteTableExpression { TableName = TestTableName1 };
        }

        protected CreateIndexExpression GetCreateIndexExpression()
        {
            IndexColumnDefinition indexColumnDefinition = new IndexColumnDefinition { Name = TestIndexName };
            IndexDefinition indexDefinition = new IndexDefinition { TableName = TestTableName1, Name = TestIndexName, Columns = new List<IndexColumnDefinition> { indexColumnDefinition } };
            return new CreateIndexExpression { Index = indexDefinition };
        }

        protected DeleteIndexExpression GetDeleteIndexExpression()
        {
            IndexDefinition indexDefinition = new IndexDefinition { Name = TestIndexName };
            return new DeleteIndexExpression { Index = indexDefinition };
        }

        protected RenameColumnExpression GetRenameColumnExpression()
        {
            return new RenameColumnExpression { OldName = TestColumnName1, NewName = TestColumnName2, TableName = TestTableName1 };
        }

        protected CreateColumnExpression GetCreateColumnExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, Type = DbType.String };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        protected CreateColumnExpression GetCreateAutoIncrementColumnExpression()
        {
            ColumnDefinition column = new ColumnDefinition { Name = TestColumnName1, IsIdentity = true, IsPrimaryKey = true, Type = DbType.String };
            return new CreateColumnExpression { TableName = TestTableName1, Column = column };
        }

        protected CreateTableExpression GetCreateTableWithPrimaryKeyIdentityExpression()
        {
            var expression = new CreateTableExpression { TableName = TestTableName1 };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, IsIdentity = true, IsPrimaryKey = true, Type = DbType.String });
            return expression;
        }

        protected CreateTableExpression GetCreateTableExpression()
        {
            CreateTableExpression expression = new CreateTableExpression() { TableName = TestTableName1, };
            expression.Columns.Add(new ColumnDefinition { Name = TestColumnName1, Type = DbType.String });
            return expression;
        }
    }

    
}
