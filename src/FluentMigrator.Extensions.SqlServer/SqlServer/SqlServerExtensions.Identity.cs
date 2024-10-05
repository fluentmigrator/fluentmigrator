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

using System;

using FluentMigrator.Builders;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.SqlServer
{
    public static partial class SqlServerExtensions
    {
        /// <summary>
        /// Makes a column an Identity column using the specified seed and increment values.
        /// </summary>
        /// <param name="expression">Column on which to apply the identity.</param>
        /// <param name="seed">Starting value of the identity.</param>
        /// <param name="increment">Increment value of the identity.</param>
        /// <returns></returns>
        public static TNext Identity<TNext, TNextFk>(
            this IColumnOptionSyntax<TNext, TNextFk> expression,
            int seed,
            int increment)
            where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            ISupportAdditionalFeatures castColumn = GetColumn(expression);
            return SetIdentity(expression, seed, increment, castColumn);
        }

        /// <summary>
        /// Makes a column an Identity column using the specified seed and increment values with bigint support.
        /// </summary>
        /// <param name="expression">Column on which to apply the identity.</param>
        /// <param name="seed">Starting value of the identity.</param>
        /// <param name="increment">Increment value of the identity.</param>
        /// <returns></returns>
        public static TNext Identity<TNext, TNextFk>(
            this IColumnOptionSyntax<TNext, TNextFk> expression,
            long seed,
            int increment)
            where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            ISupportAdditionalFeatures castColumn = GetColumn(expression);
            return SetIdentity(expression, seed, increment, castColumn);
        }

        private static TNext SetIdentity<TNext, TNextFk>(
            IColumnOptionSyntax<TNext, TNextFk> expression,
            object seed,
            int increment,
            ISupportAdditionalFeatures castColumn)
            where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            castColumn.AdditionalFeatures[IdentitySeed] = seed;
            castColumn.AdditionalFeatures[IdentityIncrement] = increment;
            return expression.Identity();
        }

        private static ISupportAdditionalFeatures GetColumn<TNext, TNextFk>(IColumnOptionSyntax<TNext, TNextFk> expression) where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            if (expression is IColumnExpressionBuilder cast1)
                return cast1.Column;

            throw new InvalidOperationException(UnsupportedMethodMessage(nameof(IdentityIncrement), nameof(IColumnExpressionBuilder)));
        }
    }
}
