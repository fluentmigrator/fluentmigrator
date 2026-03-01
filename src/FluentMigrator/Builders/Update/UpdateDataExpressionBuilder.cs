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
using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Update
{
    /// <summary>
    /// An expression builder for a <see cref="UpdateDataExpression"/>
    /// </summary>
    public class UpdateDataExpressionBuilder : ExpressionBuilderBase<UpdateDataExpression>, IUpdateSetOrInSchemaSyntax,
        IUpdateWhereSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDataExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        public UpdateDataExpressionBuilder(UpdateDataExpression expression) : base(expression)
        {
        }

        /// <inheritdoc />
        public IUpdateSetSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }

        /// <inheritdoc />
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The properties of the anonymous type cannot be statically analyzed.")]
#endif
        public IUpdateWhereSyntax Set(object dataAsAnonymousType)
        {
            Expression.Set = GetData<List<KeyValuePair<string, object>>>(dataAsAnonymousType);
            return this;
        }

        /// <inheritdoc />
        public IUpdateWhereSyntax Set(IDictionary<string, object> data)
        {
#pragma warning disable IL2026 // GetData handles IDictionary<string, object> without reflection
            Expression.Set = GetData<List<KeyValuePair<string, object>>>(data);
#pragma warning restore IL2026
            return this;
        }

        /// <inheritdoc />
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The properties of the anonymous type cannot be statically analyzed.")]
#endif
        public void Where(object dataAsAnonymousType)
        {
            Expression.Where = GetData<List<KeyValuePair<string, object>>>(dataAsAnonymousType);
        }

        /// <inheritdoc />
        public void Where(IDictionary<string, object> data)
        {
#pragma warning disable IL2026 // GetData handles IDictionary<string, object> without reflection
            Expression.Where = GetData<List<KeyValuePair<string, object>>>(data);
#pragma warning restore IL2026
        }

        /// <inheritdoc />
        public void AllRows()
        {
            Expression.IsAllRows = true;
        }
    }
}
