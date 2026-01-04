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

using FluentMigrator.Builders.Alter.Column;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Alter
{
    /// <summary>
    /// The root expression for alterations
    /// </summary>
    public class AlterExpressionRoot : IAlterExpressionRoot
    {
        private readonly IMigrationContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlterExpressionRoot"/> class.
        /// </summary>
        /// <param name="context">The migration context</param>
        public AlterExpressionRoot(IMigrationContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public IMigrationContext GetMigrationContext() => _context;

        /// <inheritdoc />
        public IAlterTableAddColumnOrAlterColumnOrSchemaOrDescriptionSyntax Table(string tableName)
        {
            var expression = new AlterTableExpression { TableName = tableName };
            _context.Expressions.Add(expression);
            return new AlterTableExpressionBuilder(expression, _context);
        }

        /// <inheritdoc />
        public IAlterColumnOnTableSyntax Column(string columnName)
        {
            var expression = new AlterColumnExpression { Column = { Name = columnName } };
            _context.Expressions.Add(expression);
            return new AlterColumnExpressionBuilder(expression, _context);
        }
    }
}
