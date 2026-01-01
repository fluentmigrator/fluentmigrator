#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Postgres.Builder.SecurityLabel
{
    /// <summary>
    /// Builds an expression to delete a security label from a PostgreSQL object.
    /// </summary>
    public class DeleteSecurityLabelExpressionBuilder :
        IDeleteSecurityLabelFromObjectSyntax,
        IDeleteSecurityLabelFromTableSyntax,
        IDeleteSecurityLabelFromColumnSyntax,
        IDeleteSecurityLabelFromColumnTableSyntax,
        IDeleteSecurityLabelFromViewSyntax,
        IDeleteSecurityLabelWithProviderSyntax
    {
        private readonly IMigrationContext _context;
        private readonly PostgresSecurityLabelDefinition _definition;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteSecurityLabelExpressionBuilder"/> class.
        /// </summary>
        /// <param name="context">The migration context.</param>
        public DeleteSecurityLabelExpressionBuilder(IMigrationContext context)
        {
            _context = context;
            _definition = new PostgresSecurityLabelDefinition();
        }

        /// <inheritdoc />
        public IDeleteSecurityLabelFromTableSyntax FromTable(string tableName)
        {
            _definition.ObjectType = PostgresSecurityLabelObjectType.Table;
            _definition.ObjectName = tableName;
            return this;
        }

        /// <inheritdoc />
        public IDeleteSecurityLabelFromColumnSyntax FromColumn(string columnName)
        {
            _definition.ObjectType = PostgresSecurityLabelObjectType.Column;
            _definition.ColumnName = columnName;
            return this;
        }

        /// <inheritdoc />
        public IDeleteSecurityLabelWithProviderSyntax FromSchema(string schemaName)
        {
            _definition.ObjectType = PostgresSecurityLabelObjectType.Schema;
            _definition.ObjectName = schemaName;
            return this;
        }

        /// <inheritdoc />
        public IDeleteSecurityLabelWithProviderSyntax FromRole(string roleName)
        {
            _definition.ObjectType = PostgresSecurityLabelObjectType.Role;
            _definition.ObjectName = roleName;
            return this;
        }

        /// <inheritdoc />
        public IDeleteSecurityLabelFromViewSyntax FromView(string viewName)
        {
            _definition.ObjectType = PostgresSecurityLabelObjectType.View;
            _definition.ObjectName = viewName;
            return this;
        }

        /// <inheritdoc />
        IDeleteSecurityLabelWithProviderSyntax IDeleteSecurityLabelFromTableSyntax.InSchema(string schemaName)
        {
            _definition.SchemaName = schemaName;
            return this;
        }

        /// <inheritdoc />
        IDeleteSecurityLabelFromColumnTableSyntax IDeleteSecurityLabelFromColumnSyntax.OnTable(string tableName)
        {
            _definition.ObjectName = tableName;
            return this;
        }

        /// <inheritdoc />
        IDeleteSecurityLabelWithProviderSyntax IDeleteSecurityLabelFromColumnTableSyntax.InSchema(string schemaName)
        {
            _definition.SchemaName = schemaName;
            return this;
        }

        /// <inheritdoc />
        IDeleteSecurityLabelWithProviderSyntax IDeleteSecurityLabelFromViewSyntax.InSchema(string schemaName)
        {
            _definition.SchemaName = schemaName;
            return this;
        }

        /// <inheritdoc />
        public void WithProvider(string provider)
        {
            _definition.Provider = provider;
            AddExpression();
        }

        /// <inheritdoc />
        public void Delete()
        {
            AddExpression();
        }

        private void AddExpression()
        {
            var sql = PostgresSecurityLabelSqlGenerator.GenerateDeleteSecurityLabelSql(_definition);
            var expression = new ExecuteSqlStatementExpression { SqlStatement = sql };
            _context.Expressions.Add(expression);
        }
    }
}
