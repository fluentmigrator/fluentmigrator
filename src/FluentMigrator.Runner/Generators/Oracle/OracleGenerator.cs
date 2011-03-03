

namespace FluentMigrator.Runner.Generators.Oracle
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.Generic;
    using FluentMigrator.Runner.Generators.Base;

	public class OracleGenerator : GenericGenerator
	{
        public OracleGenerator()
            : base(new OracleColumn(), new OracleQuoter())
		{
		}

        public override string AddColumn { get  { return "ALTER TABLE {0} ADD {1}"; } }

        public override string RenameTable { get { return "ALTER TABLE {0} RENAME TO {1}";  } }

        public override string InsertData { get { return "INTO {0} ({1}) VALUES ({2})"; } }

        public override string IfNotExistsString(CreateTableExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("If not exists statments are note supported in Oracle");
        }

        //public override string Generate(AlterColumnExpression expression)
        //{
        //    return String.Format("ALTER TABLE {0} MODIFY {1}", expression.TableName, Column.Generate(expression.Column));
        //}

        //public override string Generate(CreateTableExpression expression)
        //{
        //    return String.Format("CREATE TABLE {0} ({1})", expression.TableName, Column.Generate(expression));
        //}

        //public override string Generate(CreateColumnExpression expression)
        //{
        //    return String.Format("ALTER TABLE {0} ADD {1}", expression.TableName, Column.Generate(expression.Column));
        //}

        //public override string Generate(DeleteTableExpression expression)
        //{
        //    return String.Format("DROP TABLE {0}", expression.TableName);
        //}

        //public override string Generate(DeleteColumnExpression expression)
        //{

        //    return String.Format("ALTER TABLE {0} DROP COLUMN {1}", expression.TableName, expression.ColumnName);
        //}

        //public override string Generate(CreateForeignKeyExpression expression)
        //{
        //    //string primaryColumns = GetColumnList(expression.ForeignKey.PrimaryColumns);
        //    //string foreignColumns = GetColumnList(expression.ForeignKey.ForeignColumns);

        //    string sql = "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}";

        //    return String.Format(sql,
        //                    expression.ForeignKey.ForeignTable,
        //                    expression.ForeignKey.Name,
        //                    //foreignColumns,
        //                    expression.ForeignKey.PrimaryTable,
        //                    //primaryColumns,
        //                    FormatCascade("DELETE", expression.ForeignKey.OnDelete)
        //                    );
        //}

        //public override string Generate(DeleteForeignKeyExpression expression)
        //{
        //    string sql = "ALTER TABLE {0} DROP CONSTRAINT {1}";
        //    return String.Format(sql, expression.ForeignKey.ForeignTable, expression.ForeignKey.Name);
        //}

        //public override string Generate(CreateIndexExpression expression)
        //{
        //    var result = new StringBuilder("CREATE");
        //    if (expression.Index.IsUnique)
        //        result.Append(" UNIQUE");

        //    //if (expression.Index.IsClustered)
        //    //    result.Append(" CLUSTERED");
        //    //else
        //    //    result.Append(" NONCLUSTERED");

        //    result.Append(" INDEX {0} ON {1} (");

        //    bool first = true;
        //    foreach (IndexColumnDefinition column in expression.Index.Columns)
        //    {
        //        if (first)
        //            first = false;
        //        else
        //            result.Append(",");

        //        result.Append(column.Name);
        //        if (column.Direction == Direction.Ascending)
        //        {
        //            result.Append(" ASC");
        //        }
        //        else
        //        {
        //            result.Append(" DESC");
        //        }
        //    }
        //    result.Append(")");

        //    return String.Format(result.ToString(), expression.Index.Name, expression.Index.TableName);
        //}

        //public override string Generate(DeleteIndexExpression expression)
        //{
        //    return String.Format("DROP INDEX {0}", expression.Index.Name, expression.Index.TableName);
        //}

        //public override string Generate(RenameTableExpression expression)
        //{
        //    return String.Format("ALTER TABLE {0} RENAME TO {1}", expression.OldName, expression.NewName);
        //}

        //public override string Generate(RenameColumnExpression expression)
        //{
        //    return String.Format("ALTER TABLE {0} RENAME COLUMN {1} TO {2}", expression.TableName, expression.OldName, expression.NewName);
        //}

		public override string Generate(InsertDataExpression expression)
		{
            List<string> columnNames = new List<string>();
            List<string> columnValues = new List<string>();
            List<string> insertStrings = new List<string>();

            foreach (InsertionDataDefinition row in expression.Rows)
            {
                columnNames.Clear();
                columnValues.Clear();
                foreach (KeyValuePair<string, object> item in row)
                {
                    columnNames.Add(Quoter.QuoteColumnName(item.Key));
                    columnValues.Add(Quoter.QuoteValue(item.Value));
                }

                string columns = String.Join(", ", columnNames.ToArray());
                string values = String.Join(", ", columnValues.ToArray());
                insertStrings.Add(String.Format(InsertData, Quoter.QuoteTableName(expression.TableName), columns, values));
            }
            return "INSERT ALL " + String.Join(" ", insertStrings.ToArray()) + " SELECT 1 FROM DUAL";

		}


        //public override string Generate(UpdateDataExpression expression)
        //{
        //    var result = new StringBuilder();

        //    var set = String.Empty;
        //    var i = 0;
        //    foreach (var item in expression.Set)
        //    {
        //        if (i != 0)
        //        {
        //            set += ", ";
        //        }

        //        set += String.Format("[{0}] = {1}", item.Key, Constant.Format(item.Value));
        //        i++;
        //    }

        //    var where = String.Empty;
        //    i = 0;
        //    foreach (var item in expression.Where)
        //    {
        //        if (i != 0)
        //        {
        //            where += " AND ";
        //        }

        //        where += String.Format("[{0}] {1} {2}", item.Key, item.Value == null ? "IS" : "=", Constant.Format(item.Value));
        //        i++;
        //    }

        //    result.Append(String.Format("UPDATE [{0}] SET {1} WHERE {2};", expression.TableName, set, where));

        //    return result.ToString();
        //}

        //public override string Generate(DeleteDataExpression expression)
        //{
        //    var result = new StringBuilder();
        //    foreach (var row in expression.Rows)
        //    {
        //        var where = String.Empty;
        //        var i = 0;

        //        foreach (var item in row)
        //        {
        //            if (i != 0)
        //            {
        //                where += " AND ";
        //            }

        //            where += String.Format("{0} {1} {2}", item.Key, item.Value == null ? "IS" : "=", Constant.Format(item.Value));
        //            i++;
        //        }

        //        result.Append(String.Format("DELETE FROM {0} WHERE {1}", expression.TableName, where));
        //    }
        //    return result.ToString();
        //}

		public override string Generate(AlterDefaultConstraintExpression expression)
		{
			throw new NotImplementedException();
		}

        //private string GetColumnList(IEnumerable<string> columns)
        //{
        //    string result = "";
        //    foreach (string column in columns)
        //    {
        //        result += column + ",";
        //    }
        //    return result.TrimEnd(',');
        //}

        //private string GetDataList(List<object> data)
        //{
        //    string result = "";
        //    foreach (object column in data)
        //    {
        //        result += Constant.Format(column) + ",";
        //    }
        //    return result.TrimEnd(',');
        //}

        //protected string FormatCascade(string onWhat, Rule rule)
        //{
        //    string action = "NO ACTION";
        //    switch (rule)
        //    {
        //        case Rule.None:
        //        case Rule.SetDefault:
        //            return "";
        //        case Rule.Cascade:
        //            action = "CASCADE";
        //            break;
        //        case Rule.SetNull:
        //            action = "SET NULL";
        //            break;
        //    }

        //    return string.Format(" ON {0} {1}", onWhat, action);
        //}
	}
}
