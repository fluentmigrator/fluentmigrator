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

namespace FluentMigrator.Runner.Conventions
{
    /// <summary>
    /// The class handling the default schema name
    /// </summary>
    /// <remarks>
    /// This class handles all <see cref="ISchemaExpression"/> and additionally
    /// implements other conventions that give access to schema names (e.g.
    /// <see cref="IForeignKeyConvention"/>).
    /// </remarks>
    public sealed class DefaultSchemaConvention : IForeignKeyConvention, IConstraintConvention, IIndexConvention, ISequenceConvention
    {
        private readonly IDefaultSchemaNameConvention _defaultSchemaNameConvention;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSchemaConvention"/> class.
        /// </summary>
        public DefaultSchemaConvention()
            : this(new DefaultSchemaNameConvention(null))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSchemaConvention"/> class.
        /// </summary>
        /// <param name="defaultSchemaName">The default schema name</param>
        public DefaultSchemaConvention(string defaultSchemaName)
            : this(new DefaultSchemaNameConvention(defaultSchemaName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSchemaConvention"/> class.
        /// </summary>
        /// <param name="defaultSchemaNameConvention">The convention used to
        /// return the default schema name for a given original schema name.</param>
        public DefaultSchemaConvention(IDefaultSchemaNameConvention defaultSchemaNameConvention)
        {
            _defaultSchemaNameConvention = defaultSchemaNameConvention;
        }

        /// <summary>
        /// Returns the default schema name depending on the original schema name
        /// </summary>
        /// <param name="originalSchemaName">The original schema name</param>
        /// <returns>Returns the <paramref name="originalSchemaName"/> when the
        /// default schema name is null or empty and returns the new default
        /// schema name when the <paramref name="originalSchemaName"/> is null
        /// or empty</returns>
        string GetSchemaName(string originalSchemaName)
        {
            return _defaultSchemaNameConvention.GetSchemaName(originalSchemaName);
        }

        /// <summary>
        /// Applies a convention to a <see cref="ISchemaExpression"/>
        /// </summary>
        /// <param name="expression">The expression this convention should be applied to</param>
        /// <returns>The same or a new expression. The underlying type must stay the same.</returns>
        public ISchemaExpression Apply(ISchemaExpression expression)
        {
            expression.SchemaName = GetSchemaName(expression.SchemaName);
            return expression;
        }

        /// <inheritdoc />
        public IForeignKeyExpression Apply(IForeignKeyExpression expression)
        {
            expression.ForeignKey.ForeignTableSchema = GetSchemaName(expression.ForeignKey.ForeignTableSchema);
            expression.ForeignKey.PrimaryTableSchema = GetSchemaName(expression.ForeignKey.PrimaryTableSchema);
            return expression;
        }

        /// <inheritdoc />
        public IConstraintExpression Apply(IConstraintExpression expression)
        {
            expression.Constraint.SchemaName = GetSchemaName(expression.Constraint.SchemaName);
            return expression;
        }

        /// <inheritdoc />
        public IIndexExpression Apply(IIndexExpression expression)
        {
            expression.Index.SchemaName = GetSchemaName(expression.Index.SchemaName);
            return expression;
        }

        /// <inheritdoc />
        public ISequenceExpression Apply(ISequenceExpression expression)
        {
            expression.Sequence.SchemaName = GetSchemaName(expression.Sequence.SchemaName);
            return expression;
        }
    }
}
