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
using FluentMigrator.Builders.Create.Constraint;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Oracle
{
    public static class OracleExtensions
    {
        public static string IdentityGeneration => "OracleIdentityGeneration";
        public static string IdentityStartWith => "OracleIdentityStartWith";
        public static string IdentityIncrementBy => "OracleIdentityIncrementBy";
        public static string IdentityMinValue => "OracleIdentityMinValue";
        public static string IdentityMaxValue => "OracleIdentityMaxValue";

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
            OracleGenerationType generation)
            where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            var castColumn = GetColumn(expression);
            return SetIdentity(expression, generation, startWith: null, incrementBy: null, minValue: null, maxValue: null, castColumn);
        }

        /// <summary>
        /// Makes a column an Identity column using the specified generation type, seed and increment values.
        /// </summary>
        /// <param name="expression">Column on which to apply the identity.</param>
        /// <param name="generation">The generation type</param>
        /// <param name="startWith">Starting value of the identity.</param>
        /// <param name="incrementBy">Increment value of the identity.</param>
        /// <returns></returns>
        public static TNext Identity<TNext, TNextFk>(
            this IColumnOptionSyntax<TNext, TNextFk> expression,
            OracleGenerationType generation,
            int startWith,
            int incrementBy)
            where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            var castColumn = GetColumn(expression);
            return SetIdentity(expression, generation, startWith, incrementBy, minValue: null, maxValue: null, castColumn);
        }

        /// <summary>
        /// Makes a column an Identity column using the specified generation type, seed and increment values with bigint support.
        /// </summary>
        /// <param name="expression">Column on which to apply the identity.</param>
        /// <param name="generation">The generation type</param>
        /// <param name="startWith">Starting value of the identity.</param>
        /// <param name="incrementBy">Increment value of the identity.</param>
        /// <returns></returns>
        public static TNext Identity<TNext, TNextFk>(
            this IColumnOptionSyntax<TNext, TNextFk> expression,
            OracleGenerationType generation,
            long startWith,
            int incrementBy)
            where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            var castColumn = GetColumn(expression);
            return SetIdentity(expression, generation, startWith, incrementBy, minValue: null, maxValue: null, castColumn);
        }

        /// <summary>
        /// Makes a column an Identity column using the specified generation type, startWith, increment, minValue and maxValue with bigint support.
        /// </summary>
        /// <param name="expression">Column on which to apply the identity.</param>
        /// <param name="generation">The generation type</param>
        /// <param name="startWith">Starting value of the identity.</param>
        /// <param name="incrementBy">Increment value of the identity.</param>
        /// <param name="minValue">Min value of the identity.</param>
        /// <param name="maxValue">Max value of the identity.</param>
        /// <returns></returns>
        public static TNext Identity<TNext, TNextFk>(
            this IColumnOptionSyntax<TNext, TNextFk> expression,
            OracleGenerationType generation,
            long startWith,
            int incrementBy,
            long minValue,
            long maxValue)
            where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            var castColumn = GetColumn(expression);
            return SetIdentity(expression, generation, startWith, incrementBy, minValue, maxValue, castColumn);
        }

        private static TNext SetIdentity<TNext, TNextFk>(
            IColumnOptionSyntax<TNext, TNextFk> expression,
            OracleGenerationType generation,
            long? startWith,
            int? incrementBy,
            long? minValue,
            long? maxValue,
            ISupportAdditionalFeatures castColumn)
            where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            castColumn.AdditionalFeatures[IdentityGeneration] = generation;
            castColumn.AdditionalFeatures[IdentityStartWith] = startWith;
            castColumn.AdditionalFeatures[IdentityIncrementBy] = incrementBy;
            castColumn.AdditionalFeatures[IdentityMinValue] = minValue;
            castColumn.AdditionalFeatures[IdentityMaxValue] = maxValue;
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
