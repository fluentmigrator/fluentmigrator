#region License
// Copyright (c) 2007-2019, FluentMigrator Project
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

namespace FluentMigrator.Postgres
{
    /// <summary>
    /// Feature extension for PostgreSQL
    /// </summary>
    public static class PostgresExtensions
    {
        /// <summary>
        /// Column identity generation ability for PostgreSQL 10 and/or above
        /// </summary>
        public static string IdentityGeneration => "PostgresIdentityGeneration";

        private static string UnsupportedMethodMessage(object methodName, string interfaceName)
        {
            var msg = string.Format(ErrorMessages.MethodXMustBeCalledOnObjectImplementingY, methodName, interfaceName);
            return msg;
        }

        /// <summary>
        /// Makes a column an Identity column using the specified generation type.
        /// </summary>
        /// <param name="expression">Column on which to apply the identity.</param>
        /// <param name="generation">The generation type</param>
        /// <returns></returns>
        public static TNext Identity<TNext, TNextFk>(
            this IColumnOptionSyntax<TNext, TNextFk> expression,
            PostgresGenerationType generation)
            where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            var castColumn = GetColumn(expression);
            return SetIdentity(expression, generation, castColumn);
        }

        private static TNext SetIdentity<TNext, TNextFk>(
            IColumnOptionSyntax<TNext, TNextFk> expression,
            PostgresGenerationType generation,
            ISupportAdditionalFeatures castColumn)
            where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            castColumn.AdditionalFeatures[IdentityGeneration] = generation;
            return expression.Identity();
        }

        private static ISupportAdditionalFeatures GetColumn<TNext, TNextFk>(IColumnOptionSyntax<TNext, TNextFk> expression) where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            if (expression is IColumnExpressionBuilder cast1)
            {
                return cast1.Column;
            }

            throw new InvalidOperationException(UnsupportedMethodMessage(nameof(IdentityGeneration), nameof(IColumnExpressionBuilder)));
        }
    }
}
