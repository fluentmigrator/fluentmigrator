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

using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Generators
{
    /// <summary>
    /// A generator whose SQL for <c>Create.Table(...).IfNotExists()</c> also creates some or all of the table's
    /// <see cref="CreateTableExpression.ExpressionsForNewTable"/>, inside the same existence check, so that they are
    /// created only along with the table.
    /// </summary>
    /// <remarks>
    /// The processor runs the expressions the generator does not include as separate statements after the table, as for
    /// any generator that does not implement this interface.
    /// </remarks>
    public interface ICreateTableIfNotExistsGenerator
    {
        /// <summary>
        /// Whether the SQL generated for <paramref name="expression"/> includes <paramref name="expressionForNewTable"/>.
        /// </summary>
        /// <param name="expression">A <see cref="CreateTableExpression"/> with <see cref="CreateTableExpression.IfNotExists"/></param>
        /// <param name="expressionForNewTable">One of its <see cref="CreateTableExpression.ExpressionsForNewTable"/></param>
        /// <remarks>
        /// This is the one decision: the generator writes exactly the expressions this selects, and the processor runs the
        /// rest. A generator that chose its statements another way would disagree with an override of this method, and
        /// an expression would then be created twice, or not at all.
        /// </remarks>
        bool GeneratesWithTable(CreateTableExpression expression, IMigrationExpression expressionForNewTable);
    }

    /// <summary>
    /// Extension methods for <see cref="CreateTableExpression.ExpressionsForNewTable"/>.
    /// </summary>
    public static class CreateTableIfNotExistsExtensions
    {
        /// <summary>
        /// The <see cref="CreateTableExpression.ExpressionsForNewTable"/> that <paramref name="generator"/> does not
        /// include in the SQL for the table, and so must be processed after it.
        /// </summary>
        public static IEnumerable<IMigrationExpression> GetExpressionsProcessedSeparately(
            this CreateTableExpression expression,
            IMigrationGenerator generator)
        {
            if (expression.ExpressionsForNewTable == null)
            {
                return Enumerable.Empty<IMigrationExpression>();
            }

            if (!expression.IfNotExists || !(generator is ICreateTableIfNotExistsGenerator conditionalGenerator))
            {
                return expression.ExpressionsForNewTable;
            }

            return expression.ExpressionsForNewTable.Where(e => !conditionalGenerator.GeneratesWithTable(expression, e));
        }
    }
}
