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

using System.Collections.Generic;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Conventions;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods that work in <see cref="IMigrationExpression"/> and are using <see cref="IConventionSet"/> implementations.
    /// </summary>
    public static class ConventionSetExtensions
    {
        /// <summary>
        /// Applies all conventions in a <paramref name="conventionSet"/> to the given <paramref name="expressions"/>.
        /// </summary>
        /// <param name="expressions">The expression to apply the convention set to</param>
        /// <param name="conventionSet">The convention set to apply to the expressions</param>
        /// <returns>A sequence of expressions where the conventions have been applied to</returns>
        public static IEnumerable<IMigrationExpression> Apply(this IEnumerable<IMigrationExpression> expressions,
            IConventionSet conventionSet)
        {
            foreach (var expression in expressions)
            {
                var expr = expression.Apply(conventionSet);
                yield return expr;
            }
        }

        /// <summary>
        /// Apply all conventions in a <paramref name="conventionSet"/> to the given <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="T">The expression type, derived from <see cref="IMigrationExpression"/></typeparam>
        /// <param name="expression">The expression to apply the <paramref name="conventionSet"/> to</param>
        /// <param name="conventionSet">The convention set to apply to the <paramref name="expression"/></param>
        /// <returns>The same or a new instance of the expression. The underlying type of the expression must
        /// not be changed.</returns>
        public static T Apply<T>(this T expression, IConventionSet conventionSet)
            where T : IMigrationExpression
        {
            if (conventionSet.SchemaConvention != null && expression is ISchemaExpression schemaExpression)
            {
                expression = (T) conventionSet.SchemaConvention.Apply(schemaExpression);
            }

            if (conventionSet.RootPathConvention != null && expression is IFileSystemExpression fileSystemExpression)
            {
                expression = (T) conventionSet.RootPathConvention.Apply(fileSystemExpression);
            }

            if (expression is IColumnsExpression columnsExpression)
            {
                foreach (var convention in conventionSet.ColumnsConventions)
                {
                    columnsExpression = convention.Apply(columnsExpression);
                }

                expression = (T) columnsExpression;
            }

            if (expression is IConstraintExpression constraintExpression)
            {
                foreach (var convention in conventionSet.ConstraintConventions)
                {
                    constraintExpression = convention.Apply(constraintExpression);
                }

                expression = (T) constraintExpression;
            }

            if (expression is IForeignKeyExpression foreignKeyExpression)
            {
                foreach (var convention in conventionSet.ForeignKeyConventions)
                {
                    foreignKeyExpression = convention.Apply(foreignKeyExpression);
                }

                expression = (T) foreignKeyExpression;
            }

            if (expression is IIndexExpression indexExpression)
            {
                foreach (var convention in conventionSet.IndexConventions)
                {
                    indexExpression = convention.Apply(indexExpression);
                }

                expression = (T) indexExpression;
            }

            if (expression is ISequenceExpression sequenceExpression)
            {
                foreach (var convention in conventionSet.SequenceConventions)
                {
                    sequenceExpression = convention.Apply(sequenceExpression);
                }

                expression = (T) sequenceExpression;
            }

            if (expression is IAutoNameExpression autoNameExpression)
            {
                foreach (var convention in conventionSet.AutoNameConventions)
                {
                    autoNameExpression = convention.Apply(autoNameExpression);
                }

                expression = (T) autoNameExpression;
            }

            return expression;
        }
    }
}
