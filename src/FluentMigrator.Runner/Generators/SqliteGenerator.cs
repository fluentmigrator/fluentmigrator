using System;
using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Generators
{
	public class SqliteGenerator : GeneratorBase
	{
		private string JoinColumns(CreateTableExpression expression)
		{
			string columns = "";
			int total = expression.Columns.Count - 1;
			for (int i = 0; i < expression.Columns.Count; i++)
			{
				columns += expression.Columns[i];

				if (i != total)
					columns += ", ";
			}

			return columns;
		}

		public override string Generate(CreateTableExpression expression)
		{			
			return string.Format("CREATE TABLE {0} ({1})", expression.TableName, JoinColumns(expression));			
		}

		public override string Generate(CreateColumnExpression expression)
		{
			return string.Format("ALTER TABLE {0} ADD COLUMN {1}", expression.TableName, expression.Column.Name);			
		}

		public override string Generate(RenameColumnExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(DeleteTableExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(DeleteColumnExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(CreateForeignKeyExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(DeleteForeignKeyExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(DeleteIndexExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(RenameTableExpression expression)
		{
			throw new NotImplementedException();
		}

		public override string Generate(CreateIndexExpression expression)
		{
			throw new NotImplementedException();
		}
	}
}