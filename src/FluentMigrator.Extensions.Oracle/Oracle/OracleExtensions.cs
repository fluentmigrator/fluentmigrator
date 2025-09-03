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

namespace FluentMigrator.Oracle
{
    /// <summary>
    /// Provides extension methods for Oracle-specific column identity configuration.
    /// </summary>
    public static class OracleExtensions
    {
        /// <summary>
        /// The additional feature key for Oracle identity generation type.
        /// </summary>
        public static string IdentityGeneration => "OracleIdentityGeneration";
        /// <summary>
        /// The additional feature key for Oracle identity start value.
        /// </summary>
        public static string IdentityStartWith => "OracleIdentityStartWith";
        /// <summary>
        /// The additional feature key for Oracle identity increment value.
        /// </summary>
        public static string IdentityIncrementBy => "OracleIdentityIncrementBy";
        /// <summary>
        /// The additional feature key for Oracle identity minimum value.
        /// </summary>
        public static string IdentityMinValue => "OracleIdentityMinValue";
        /// <summary>
        /// The additional feature key for Oracle identity maximum value.
        /// </summary>
        public static string IdentityMaxValue => "OracleIdentityMaxValue";

        /// <summary>
        /// Generates an error message indicating that a specific method must be called on an object implementing a specified interface.
        /// </summary>
        /// <param name="methodName">The name of the method that was attempted to be called.</param>
        /// <param name="interfaceName">The name of the required interface that the object must implement.</param>
        /// <returns>A formatted error message string.</returns>
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

        /// <summary>
        /// Configures the identity settings for an Oracle column.
        /// </summary>
        /// <typeparam name="TNext">The next fluent syntax type in the chain.</typeparam>
        /// <typeparam name="TNextFk">The next fluent syntax type in the chain for foreign keys.</typeparam>
        /// <param name="expression">The column option syntax to configure.</param>
        /// <param name="generation">The identity generation type for the column.</param>
        /// <param name="startWith">The starting value for the identity column (optional).</param>
        /// <param name="incrementBy">The increment value for the identity column (optional).</param>
        /// <param name="minValue">The minimum value for the identity column (optional).</param>
        /// <param name="maxValue">The maximum value for the identity column (optional).</param>
        /// <param name="castColumn">The column supporting additional features.</param>
        /// <returns>The next fluent syntax in the chain.</returns>
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

        /// <summary>
        /// Retrieves the column definition associated with the specified column option syntax.
        /// </summary>
        /// <typeparam name="TNext">The type of the next fluent syntax in the chain.</typeparam>
        /// <typeparam name="TNextFk">The type of the next fluent syntax in the chain for foreign keys.</typeparam>
        /// <param name="expression">The column option syntax from which to retrieve the column definition.</param>
        /// <returns>The column definition that supports additional features.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the provided <paramref name="expression"/> does not implement <see cref="IColumnExpressionBuilder"/>.
        /// </exception>
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
