#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

using FluentMigrator.Builders.Create.CheckConstraint;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Alter.CheckConstraint
{
    /// <summary>
    /// Builds a <see cref="AlterCheckConstraintExpression"/>.
    /// </summary>
    public class AlterCheckConstraintExpressionBuilder : ExpressionBuilderBase<AlterCheckConstraintExpression>,
        IAlterCheckOnTableSyntax,
        IAlterCheckInSchemaOrActionSyntax,
        IAlterCheckEnableOptionsSyntax,
        ICreateCheckOptionsSyntax
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="AlterCheckConstraintExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression.</param>
        public AlterCheckConstraintExpressionBuilder(AlterCheckConstraintExpression expression)
            : base(expression)
        {
        }

        /// <inheritdoc />
        public IAlterCheckInSchemaOrActionSyntax OnTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
            return this;
        }

        /// <inheritdoc />
        public IAlterCheckActionSyntax InSchema(string schemaName)
        {
            Expression.Constraint.SchemaName = schemaName;
            return this;
        }

        /// <inheritdoc />
        public ICreateCheckOptionsSyntax Condition(string sqlCondition)
        {
            Expression.Action = CheckConstraintAction.ChangeCondition;
            Expression.Constraint.Condition = sqlCondition;
            return this;
        }

        /// <inheritdoc />
        public IAlterCheckEnableOptionsSyntax Enabled()
        {
            Expression.Action = CheckConstraintAction.Enable;
            return this;
        }

        /// <inheritdoc />
        public void Disabled()
        {
            Expression.Action = CheckConstraintAction.Disable;
            Expression.Constraint.Enabled = false;
        }

        /// <inheritdoc />
        void ICreateCheckOptionsSyntax.Disabled()
        {
            Expression.Constraint.Enabled = false;
        }

        /// <inheritdoc />
        public void WithNoCheck()
        {
            Expression.Constraint.NoCheck = true;
        }
    }
}
