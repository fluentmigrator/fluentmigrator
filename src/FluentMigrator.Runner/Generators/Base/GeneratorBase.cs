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

using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Generators.Base
{
    public abstract class GeneratorBase : IMigrationGenerator
    {
        /// <summary>Whether to throw a <see cref="DatabaseOperationNotSupportedException"/> when a SQL command is not supported by the underlying database type.</summary>
        public bool StrictCompatibility { get; set; }

        /// <summary>Whether to imitate database support for some SQL commands that are not supported by the underlying database type.</summary>
        /// <remarks>For example, schema support can be emulated by prefixing the schema name to the table name (<c>`schema`.`table`</c> => <c>`schema_table`</c>).</remarks>
        public bool EmulateCompatibility { get; set; }

        private readonly IColumn _column;
        private readonly IQuoter _quoter;

        public GeneratorBase(IColumn column, IQuoter quoter)
        {
            _column = column;
            _quoter = quoter;
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
        public abstract string Generate(UpdateDataExpression expression);
        public abstract string Generate(AlterSchemaExpression expression);
        public abstract string Generate(CreateSequenceExpression expression);
        public abstract string Generate(DeleteSequenceExpression expression);
        public abstract string Generate(CreateConstraintExpression expression);
        public abstract string Generate(DeleteConstraintExpression expression);
        public abstract string Generate(DeleteDefaultConstraintExpression expression);

        public virtual bool IsAdditionalFeatureSupported(string feature)
        {
            return false;
        }

        public string Generate(AlterTableExpression expression)
        {
            // returns nothing because the individual AddColumn and AlterColumn calls
            //  create CreateColumnExpression and AlterColumnExpression respectively
            return string.Empty;
        }

        protected IColumn Column
        {
            get { return _column; }
        }

        protected IQuoter Quoter
        {
            get { return _quoter; }
        }

        /// <summary>Generate a blank string for an unsupported SQL command, or throw an exception if the generator is in strict compatibility mode.</summary>
        /// <param name="message">The exception message describing the incompatibility.</param>
        /// <exception cref="DatabaseOperationNotSupportedException">The SQL command is not supported by the underlying database, and the generator is in strict compatibility mode.</exception>
        protected string UnsupportedCommand(string message)
        {
            if (this.StrictCompatibility)
                throw new DatabaseOperationNotSupportedException(message);
            return string.Empty;
        }
    }
}
