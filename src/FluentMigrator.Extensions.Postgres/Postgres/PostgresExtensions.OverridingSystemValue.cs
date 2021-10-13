#region License
// Copyright (c) 2021, FluentMigrator Project
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

using FluentMigrator.Builders.Insert;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;

namespace FluentMigrator.Postgres
{
    public static partial class PostgresExtensions
    {
        public const string OverridingSystemValue = "PostgresOverridingSystemValue";

        /// <summary>
        /// Enables the current insert expression to set explicit values (other than <c>DEFAULT</c>)
        /// for identity columns defined as <c>GENERATED ALWAYS</c>
        /// </summary>
        /// <param name="expression">The current <see cref="IInsertDataSyntax"/> expression</param>
        /// <returns>The current <see cref="IInsertDataSyntax"/> expression</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInsertDataSyntax WithOverridingSystemValue(this IInsertDataSyntax expression)
        {
            if (!(expression is ISupportAdditionalFeatures castExpression))
            {
                throw new InvalidOperationException(
                    string.Format(
                        ErrorMessages.MethodXMustBeCalledOnObjectImplementingY,
                        "WithOverridingSystemValue",
                        nameof(ISupportAdditionalFeatures)));
            }

            castExpression.SetAdditionalFeature(OverridingSystemValue, value: true);

            return expression;
        }
    }
}
