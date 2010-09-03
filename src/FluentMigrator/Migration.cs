#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Builders.Rename;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Expressions;
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
				if(context.Conventions.GetAutoReverse())
				{
					Up();
					ICollection<IMigrationExpression> upExpressions = _context.Expressions.Reverse().ToList();
					_context.Expressions = new List<IMigrationExpression>();
					foreach (IMigrationExpression expression in upExpressions)
					{
						_context.Expressions.Add(expression.Reverse());
					}
					
				} 
				else
					Down();

				_context = null;
			}
		}

	    public IAlterExpressionRoot Alter
	    {
	        get { return new AlterExpressionRoot(_context);}
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
