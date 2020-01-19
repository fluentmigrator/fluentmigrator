#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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
    public static class PostgresExtensions
    {
        public static string IdentityGeneration => "PostgresIdentityGeneration";

        public static TNext Identity<TNext, TNextFk>(this IColumnOptionSyntax<TNext, TNextFk> expression, PostgresGenerationType generation)
            where TNext : IFluentSyntax
            where TNextFk : IFluentSyntax
            => SetIdentity(expression, generation, GetColumn(expression));

        private static ISupportAdditionalFeatures GetColumn<TNext, TNextFk>(IColumnOptionSyntax<TNext, TNextFk> expression)
            where TNext : IFluentSyntax
            where TNextFk : IFluentSyntax
        {
            if (expression is IColumnExpressionBuilder cast)
            {
                return (ISupportAdditionalFeatures)(object)cast.Column;
            }
            throw new InvalidOperationException(UnsupportedMethodMessage("IdentityGeneration", nameof(IColumnExpressionBuilder)));
        }

        private static TNext SetIdentity<TNext, TNextFk>(IColumnOptionSyntax<TNext, TNextFk> expression, PostgresGenerationType generation, ISupportAdditionalFeatures castColumn)
            where TNext : IFluentSyntax
            where TNextFk : IFluentSyntax
        {
            castColumn.AdditionalFeatures[IdentityGeneration] = generation;
            return expression.Identity();
        }

        private static string UnsupportedMethodMessage(string methodName, string interfaceName)
            => string.Format(ErrorMessages.MethodXMustBeCalledOnObjectImplementingY, methodName, interfaceName);

    }
}
