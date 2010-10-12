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
using System.Data;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	public abstract class GeneratorBase : IMigrationGenerator
	{
		private readonly IColumn _column;
		private readonly IConstantFormatter _constantFormatter;

		public GeneratorBase(IColumn column, IConstantFormatter constantFormatter)
		{
			_column = column;
			_constantFormatter = constantFormatter;
		}

		public abstract string Generate(CreateSchemaExpression expression);
		public abstract string Generate(DeleteSchemaExpression expression);
		public abstract string Generate(CreateTableExpression expression);
		public abstract string Generate(AlterColumnExpression expression);
		public abstract string Generate(CreateColumnExpression expression);
		public abstract string Generate(DeleteTableExpression expression);
		public abstract string Generate(DeleteColumnExpression expression);
		public abstract string Generate(CreateForeignKeyExpression expression);
		public abstract string Generate(DeleteForeignKeyExpression expression);
		public abstract string Generate(CreateIndexExpression expression);
		public abstract string Generate(DeleteIndexExpression expression);
		public abstract string Generate(RenameTableExpression expression);
		public abstract string Generate(RenameColumnExpression expression);
		public abstract string Generate(InsertDataExpression expression);
		public abstract string Generate(AlterDefaultConstraintExpression expression);
	    public abstract string Generate(DeleteDataExpression expression);

		protected IColumn Column
		{
			get { return _column; }
		}

		protected IConstantFormatter Constant
		{
			get { return _constantFormatter; }
		}
	}
}
