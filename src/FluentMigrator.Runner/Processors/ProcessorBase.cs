using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Processors
{
	public abstract class ProcessorBase : IMigrationProcessor
	{
		protected IMigrationGenerator generator;
        
		public virtual void Process(CreateTableExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public virtual void Process(CreateColumnExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public virtual void Process(DeleteTableExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public virtual void Process(DeleteColumnExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public virtual void Process(CreateForeignKeyExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public virtual void Process(DeleteForeignKeyExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public virtual void Process(CreateIndexExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public virtual void Process(DeleteIndexExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public virtual void Process(RenameTableExpression expression)
		{
			Process(generator.Generate(expression));
		}

		public virtual void Process(RenameColumnExpression expression)
		{
			Process(generator.Generate(expression));
		}

		protected abstract void Process(string sql);		
	}
}