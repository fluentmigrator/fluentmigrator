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
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Delete
{
    /// <summary>
    /// An expression builder for a <see cref="DeleteDataExpression"/>
    /// </summary>
    public class DeleteDataExpressionBuilder : ExpressionBuilderBase<DeleteDataExpression>, IDeleteDataOrInSchemaSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteDataExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        public DeleteDataExpressionBuilder(DeleteDataExpression expression) : base(expression)
        {
        }

        /// <inheritdoc />
        public void IsNull(string columnName)
        {
            Expression.Rows.Add(
                new DeletionDataDefinition
                {
                    new KeyValuePair<string, object>(columnName, null)
                });
        }

        /// <inheritdoc />
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The properties of the anonymous type cannot be statically analyzed.")]
#endif
        public IDeleteDataSyntax Row(object dataAsAnonymousType)
        {
            Expression.Rows.Add(GetData<DeletionDataDefinition>(dataAsAnonymousType));
            return this;
        }

        /// <inheritdoc />
        public IDeleteDataSyntax InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
            return this;
        }

        /// <inheritdoc />
        public void AllRows()
        {
            Expression.IsAllRows = true;
        }
    }
}
