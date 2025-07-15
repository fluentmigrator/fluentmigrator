#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using System.ComponentModel;

using FluentMigrator.Expressions;

namespace FluentMigrator.Builders
{
    /// <summary>
    /// The base class for builders with underlying expressions
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="IMigrationExpression"/></typeparam>
    public abstract class ExpressionBuilderBase<T>
        where T : class, IMigrationExpression
    {
        /// <summary>
        /// Gets the underlying migration expression
        /// </summary>
        public T Expression { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilderBase{T}"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        protected ExpressionBuilderBase(T expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// Make a key/value pair list from an anonymous object, string or RawSql instance
        /// </summary>
        /// <param name="dataAsAnonymousType">The data, can be an anonymous object, string or RawSql instance</param>
        /// <typeparam name="TOut">The output key/value pair list</typeparam>
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The properties of the anonymous type cannot be statically analyzed.")]
#endif
        protected static TOut GetData<TOut>(object dataAsAnonymousType)
            where TOut : IList<KeyValuePair<string, object>>, new()
        {
            var data = new TOut();

            switch (dataAsAnonymousType)
            {
                case RawSql rawSql:
                    data.Add(new KeyValuePair<string, object>("", rawSql));
                    break;

                // Treat string like RawSql
                case string stringValue:
                    data.Add(new KeyValuePair<string, object>("", RawSql.Insert(stringValue)));
                    break;

                default:
                    var properties = TypeDescriptor.GetProperties(dataAsAnonymousType);
                    foreach (PropertyDescriptor property in properties)
                    {
                        data.Add(new KeyValuePair<string, object>(property.Name, property.GetValue(dataAsAnonymousType)));
                    }
                    break;
            }

            return data;
        }
    }
}
