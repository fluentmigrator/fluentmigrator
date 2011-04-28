using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;
using System.Linq;
using System.Data;

namespace FluentMigrator.Runner.Generators.Generic
{
	public abstract class GenericGenerator : GeneratorBase
	{
		public CompatabilityMode compatabilityMode;

		public GenericGenerator(IColumn column, IQuoter quoter) : base(column, quoter)
		{
			compatabilityMode = CompatabilityMode.LOOSE;
		}

        public virtual string CreateTable { get { return "CREATE TABLE {2}{0} ({1})"; } }
		public virtual string DropTable { get { return "DROP TABLE {0}"; } }

		public virtual string AddColumn { get { return "ALTER TABLE {0} ADD COLUMN {1}"; } }
		public virtual string DropColumn { get { return "ALTER TABLE {0} DROP COLUMN {1}"; } }
		public virtual string AlterColumn { get { return "ALTER TABLE {0} ALTER COLUMN {1}"; } }
		public virtual string RenameColumn { get { return "ALTER TABLE {0} RENAME COLUMN {1} TO {2}"; } }

		public virtual string RenameTable { get { return "RENAME TABLE {0} TO {1}"; } }

		public virtual string CreateSchema { get { return "CREATE SCHEMA {0}"; } }
		public virtual string AlterSchema { get { return "ALTER SCHEMA {0} TRANSFER {1}.{2}"; } }
		public virtual string DropSchema { get { return "DROP SCHEMA {0}"; } }

		public virtual string CreateIndex { get { return "CREATE {0}{1}INDEX {2} ON {3} ({4})"; } }
		public virtual string DropIndex { get { return "DROP INDEX {0}"; } }

		public virtual string InsertData { get { return "INSERT INTO {0} ({1}) VALUES ({2})"; } }
		public virtual string UpdateData { get { return "UPDATE {0} SET {1} WHERE {2}"; } }
		public virtual string DeleteData { get { return "DELETE FROM {0} WHERE {1}"; } }

        public virtual string CreateConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} {2} ({3})"; } }
       

		public virtual string CreateForeignKeyConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6}"; } }
		
        public virtual string DeleteConstraint { get { return "ALTER TABLE {0} DROP CONSTRAINT {1}"; } }


        public override string Generate(CreateConstraintExpression expression)
        {

            var constraintType = (expression.Constraint.IsPrimaryKeyConstraint) ? "PRIMARY KEY" : "UNIQUE";

            string[] columns = new string[expression.Constraint.Columns.Count];

            for(int i=0;i<expression.Constraint.Columns.Count;i++){
                columns[i] = Quoter.QuoteColumnName(expression.Constraint.Columns.ElementAt(i));
            }

            return string.Format(CreateConstraint, Quoter.QuoteTableName(expression.Constraint.TableName),
                Quoter.Quote(expression.Constraint.ConstraintName),
                constraintType,
                String.Join(", ", columns));
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            return string.Format(DeleteConstraint,Quoter.QuoteTableName(expression.Constraint.TableName),Quoter.Quote(expression.Constraint.ConstraintName));
        }

		public virtual string GetUniqueString(CreateIndexExpression column)
		{
			return column.Index.IsUnique ? "UNIQUE " : string.Empty;
		}

		public virtual string GetClusterTypeString(CreateIndexExpression column)
		{
			return string.Empty;
		}

        public virtual string IfNotExistsString(CreateTableExpression expression)
        {
            return expression.IfNotExists ? "IF NOT EXISTS " : "";
        }

		/// <summary>
		/// Outputs a create table string
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		public override string Generate(CreateTableExpression expression)
		{
			if (string.IsNullOrEmpty(expression.TableName)) throw new ArgumentNullException("expression", "expression.TableName cannot be empty");
			if (expression.Columns.Count == 0) throw new ArgumentException("You must specifiy at least one column");

			string quotedTableName = Quoter.QuoteTableName(expression.TableName);

            string ifNotExists = IfNotExistsString(expression);

			return string.Format(CreateTable, quotedTableName, Column.Generate(expression.Columns, quotedTableName),ifNotExists);
		}

		public override string Generate(DeleteTableExpression expression)
		{
			return String.Format(DropTable, Quoter.QuoteTableName(expression.TableName));
		}

		public override string Generate(RenameTableExpression expression)
		{
			return String.Format(RenameTable, Quoter.QuoteTableName(expression.OldName), Quoter.QuoteTableName(expression.NewName));
		}

		public override string Generate(CreateColumnExpression expression)
		{
			return String.Format(AddColumn, Quoter.QuoteTableName(expression.TableName), Column.Generate(expression.Column));
		}


		public override string Generate(AlterColumnExpression expression)
		{
			return String.Format(AlterColumn, Quoter.QuoteTableName(expression.TableName), Column.Generate(expression.Column));
		}

		public override string Generate(DeleteColumnExpression expression)
		{
			return String.Format(DropColumn, Quoter.QuoteTableName(expression.TableName), Quoter.QuoteColumnName(expression.ColumnName));
		}


		public override string Generate(RenameColumnExpression expression)
		{
			return String.Format(RenameColumn, expression.TableName, expression.OldName, expression.NewName);
		}

		public override string Generate(CreateIndexExpression expression)
		{

			string[] indexColumns = new string[expression.Index.Columns.Count];
			IndexColumnDefinition columnDef;


			for (int i = 0; i < expression.Index.Columns.Count; i++)
			{
				columnDef = expression.Index.Columns.ElementAt(i);
				if (columnDef.Direction == Direction.Ascending)
				{
					indexColumns[i] = Quoter.QuoteColumnName(columnDef.Name) + " ASC";
				}
				else
				{
					indexColumns[i] = Quoter.QuoteColumnName(columnDef.Name) + " DESC";
				}
			}

			return String.Format(CreateIndex
                , GetUniqueString(expression)
				, GetClusterTypeString(expression)
				, Quoter.QuoteIndexName(expression.Index.Name)
				, Quoter.QuoteTableName(expression.Index.TableName)
				, String.Join(", ", indexColumns));
		}

		public override string Generate(DeleteIndexExpression expression)
		{
			return String.Format(DropIndex, Quoter.QuoteIndexName(expression.Index.Name), Quoter.QuoteTableName(expression.Index.TableName));
		}

		public override string Generate(CreateForeignKeyExpression expression)
		{
			if (expression.ForeignKey.PrimaryColumns.Count != expression.ForeignKey.ForeignColumns.Count)
			{
				throw new ArgumentException("Number of primary columns and secondary columns must be equal");
			}

			string keyName = string.IsNullOrEmpty(expression.ForeignKey.Name)
				? GenerateForeignKeyName(expression)
				: expression.ForeignKey.Name;

			List<string> primaryColumns = new List<string>();
			List<string> foreignColumns = new List<string>();
			foreach (var column in expression.ForeignKey.PrimaryColumns)
			{
				primaryColumns.Add(Quoter.QuoteColumnName(column));
			}

			foreach (var column in expression.ForeignKey.ForeignColumns)
			{
				foreignColumns.Add(Quoter.QuoteColumnName(column));
			}
			return string.Format(
				CreateForeignKeyConstraint,
				Quoter.QuoteTableName(expression.ForeignKey.ForeignTable),
				Quoter.QuoteColumnName(keyName),
				String.Join(", ", foreignColumns.ToArray()),
				Quoter.QuoteTableName(expression.ForeignKey.PrimaryTable),
				String.Join(", ", primaryColumns.ToArray()),
				FormatCascade("DELETE", expression.ForeignKey.OnDelete),
				FormatCascade("UPDATE", expression.ForeignKey.OnUpdate)
				);
		}

		public virtual string GenerateForeignKeyName(CreateForeignKeyExpression expression)
		{
			return string.Format("FK_{0}_{1}", expression.ForeignKey.PrimaryTable.Substring(0, 5), expression.ForeignKey.ForeignTable.Substring(0, 5));
		}

		public override string Generate(DeleteForeignKeyExpression expression)
		{
			return string.Format(DeleteConstraint, Quoter.QuoteTableName(expression.ForeignKey.ForeignTable), Quoter.QuoteColumnName(expression.ForeignKey.Name));
		}


		protected string FormatCascade(string onWhat, Rule rule)
		{
			string action = "NO ACTION";
			switch (rule)
			{
				case Rule.None:
					return "";
				case Rule.Cascade:
					action = "CASCADE";
					break;
				case Rule.SetNull:
					action = "SET NULL";
					break;
				case Rule.SetDefault:
					action = "SET DEFAULT";
					break;
			}

			return string.Format(" ON {0} {1}", onWhat, action);
		}


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
			return String.Join("; ", insertStrings.ToArray());
		}

		public override string Generate(UpdateDataExpression expression)
		{

			List<string> updateItems = new List<string>();
			List<string> whereClauses = new List<string>();

			foreach (var item in expression.Set)
			{
				updateItems.Add(string.Format("{0} = {1}", Quoter.QuoteColumnName(item.Key), Quoter.QuoteValue(item.Value)));
			}

			foreach (var item in expression.Where)
			{
				whereClauses.Add(string.Format("{0} {1} {2}", Quoter.QuoteColumnName(item.Key), item.Value == null ? "IS" : "=", Quoter.QuoteValue(item.Value)));
			}

			return String.Format(UpdateData, Quoter.QuoteTableName(expression.TableName), String.Join(", ", updateItems.ToArray()), String.Join(" AND ", whereClauses.ToArray()));
		}

		public override string Generate(DeleteDataExpression expression)
		{
			var deleteItems = new List<string>();
			

            if (expression.IsAllRows)
            {
                deleteItems.Add(string.Format(DeleteData, Quoter.QuoteTableName(expression.TableName), "1 = 1"));
            }
            else
            {
                foreach (var row in expression.Rows)
                {
                    var whereClauses = new List<string>();
                    foreach (KeyValuePair<string, object> item in row)
                    {
                        whereClauses.Add(string.Format("{0} {1} {2}", Quoter.QuoteColumnName(item.Key), item.Value == null ? "IS" : "=", Quoter.QuoteValue(item.Value)));
                    }

                    deleteItems.Add(string.Format(DeleteData, Quoter.QuoteTableName(expression.TableName), String.Join(" AND ", whereClauses.ToArray())));
                }
            }
			
			return String.Join("; ", deleteItems.ToArray());
		}


		//All Schema method throw by default as only Sql server 2005 and up supports them.
		public override string Generate(CreateSchemaExpression expression)
		{
			return compatabilityMode.HandleCompatabilty("Schemas are not supported");

		}

		public override string Generate(DeleteSchemaExpression expression)
		{
			return compatabilityMode.HandleCompatabilty("Schemas are not supported");
		}

		public override string Generate(AlterSchemaExpression expression)
		{
			return compatabilityMode.HandleCompatabilty("Schemas are not supported");
		}

	}
}
