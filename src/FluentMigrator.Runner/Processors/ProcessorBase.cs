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

using FluentMigrator.Builders.Execute;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Processors
{
	public abstract class ProcessorBase : IMigrationProcessor
	{
		protected readonly IMigrationGenerator Generator;
		protected readonly IAnnouncer Announcer;
		public IMigrationProcessorOptions Options { get; private set; }

		protected ProcessorBase(IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options)
		{
			Generator = generator;
			Announcer = announcer;
			Options = options;
		}

		public virtual void Process(CreateSchemaExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public virtual void Process(DeleteSchemaExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public virtual void Process(CreateTableExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public virtual void Process(AlterColumnExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public virtual void Process(CreateColumnExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public virtual void Process(DeleteTableExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public virtual void Process(DeleteColumnExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public virtual void Process(CreateForeignKeyExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public virtual void Process(DeleteForeignKeyExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public virtual void Process(CreateIndexExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public virtual void Process(DeleteIndexExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public virtual void Process(RenameTableExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public virtual void Process(RenameColumnExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public void Process(InsertDataExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public void Process(AlterDefaultConstraintExpression expression)
		{
			Process(Generator.Generate(expression));
		}

		public abstract void Process(PerformDBOperationExpression expression);

		protected abstract void Process(string sql);

		public virtual void BeginTransaction()
		{
		}

		public virtual void CommitTransaction()
		{
		}

		public virtual void RollbackTransaction()
		{
		}

		public abstract System.Data.DataSet ReadTableData(string tableName);
		public abstract System.Data.DataSet Read(string template, params object[] args);
		public abstract bool Exists(string template, params object[] args);
		public abstract void Execute(string template, params object[] args);
		public abstract bool SchemaExists(string tableName);
		public abstract bool TableExists(string tableName);
		public abstract bool ColumnExists(string tableName, string columnName);
		public abstract bool ConstraintExists(string tableName, string constraintName);
	}
}
