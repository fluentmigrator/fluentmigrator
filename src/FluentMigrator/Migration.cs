using System;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Builders.Rename;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Infrastructure;
using FluentMigrator.Builders.Execute;

namespace FluentMigrator
{
	public abstract class Migration : IMigration
	{
		private IMigrationContext _context;
		private readonly object _mutex = new object();

		public abstract void Up();
		public abstract void Down();

		public void ApplyConventions(IMigrationContext context)
		{
			foreach (var expression in context.Expressions)
				expression.ApplyConventions( context.Conventions );
		}

		public virtual void GetUpExpressions(IMigrationContext context)
		{
			lock (_mutex)
			{
				_context = context;
				Up();
				_context = null;
			}
		}

		public virtual void GetDownExpressions(IMigrationContext context)
		{
			lock (_mutex)
			{
				_context = context;
				Down();
				_context = null;
			}
		}

		public ICreateExpressionRoot Create
		{
			get { return new CreateExpressionRoot(_context); }
		}

		public IDeleteExpressionRoot Delete
		{
			get { return new DeleteExpressionRoot(_context); }
		}

		public IRenameExpressionRoot Rename
		{
			get { return new RenameExpressionRoot(_context); }
		}

		public IInsertExpressionRoot Insert
		{
			get { return new InsertExpressionRoot(_context); }
		}

	    public IExecuteExpressionRoot Execute
	    {
	        get { return new ExecuteExpressionRoot(_context);}
	    }

		public ISchemaExpressionRoot Schema
		{
			get { return new SchemaExpressionRoot(_context); }
		}
	}
}
