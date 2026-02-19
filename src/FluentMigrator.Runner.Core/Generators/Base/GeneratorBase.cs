#region License
// 
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System.Collections.Generic;

using FluentMigrator.Expressions;
using FluentMigrator.Generation;

namespace FluentMigrator.Runner.Generators.Base
{
    /// <summary>
    /// Base class for migration SQL generators.
    /// </summary>
    public abstract class GeneratorBase : IMigrationGenerator
    {
        private readonly IColumn _column;
        private readonly IQuoter _quoter;
        private readonly IDescriptionGenerator _descriptionGenerator;

        /// <inheritdoc />
        public GeneratorBase(IColumn column, IQuoter quoter, IDescriptionGenerator descriptionGenerator)
        {
            _column = column;
            _quoter = quoter;
            _descriptionGenerator = descriptionGenerator;
        }

        /// <inheritdoc />
        public abstract string GeneratorId { get; }

        /// <inheritdoc />
        public abstract List<string> GeneratorIdAliases { get; }

        /// <inheritdoc />
        public abstract string Generate(CreateSchemaExpression expression);
        /// <inheritdoc />
        public abstract string Generate(DeleteSchemaExpression expression);
        /// <inheritdoc />
        public abstract string Generate(CreateTableExpression expression);
        /// <inheritdoc />
        public abstract string Generate(AlterColumnExpression expression);
        /// <inheritdoc />
        public abstract string Generate(CreateColumnExpression expression);
        /// <inheritdoc />
        public abstract string Generate(DeleteTableExpression expression);
        /// <inheritdoc />
        public abstract string Generate(DeleteColumnExpression expression);
        /// <inheritdoc />
        public abstract string Generate(CreateForeignKeyExpression expression);
        /// <inheritdoc />
        public abstract string Generate(DeleteForeignKeyExpression expression);
        /// <inheritdoc />
        public abstract string Generate(CreateIndexExpression expression);
        /// <inheritdoc />
        public abstract string Generate(DeleteIndexExpression expression);
        /// <inheritdoc />
        public abstract string Generate(RenameTableExpression expression);
        /// <inheritdoc />
        public abstract string Generate(RenameColumnExpression expression);
        /// <inheritdoc />
        public abstract string Generate(InsertDataExpression expression);
        /// <inheritdoc />
        public abstract string Generate(AlterDefaultConstraintExpression expression);
        /// <inheritdoc />
        public abstract string Generate(DeleteDataExpression expression);
        /// <inheritdoc />
        public abstract string Generate(UpdateDataExpression expression);
        /// <inheritdoc />
        public abstract string Generate(AlterSchemaExpression expression);
        /// <inheritdoc />
        public abstract string Generate(CreateSequenceExpression expression);
        /// <inheritdoc />
        public abstract string Generate(DeleteSequenceExpression expression);
        /// <inheritdoc />
        public abstract string Generate(CreateConstraintExpression expression);
        /// <inheritdoc />
        public abstract string Generate(DeleteConstraintExpression expression);
        /// <inheritdoc />
        public abstract string Generate(DeleteDefaultConstraintExpression expression);

        /// <inheritdoc />
        public virtual bool IsAdditionalFeatureSupported(string feature)
        {
            return false;
        }

        /// <inheritdoc />
        public virtual string Generate(AlterTableExpression expression)
        {
            // returns nothing because the individual AddColumn and AlterColumn calls
            //  create CreateColumnExpression and AlterColumnExpression respectively
            return string.Empty;
        }

        /// <inheritdoc />
        protected IColumn Column => _column;

        /// <inheritdoc />
        public IQuoter Quoter => _quoter;

        /// <inheritdoc />
        protected IDescriptionGenerator DescriptionGenerator => _descriptionGenerator;
    }
}
