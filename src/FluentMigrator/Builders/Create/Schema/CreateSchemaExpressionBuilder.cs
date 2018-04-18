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

using System.Collections.Generic;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.Schema
{
    /// <summary>
    /// An expression builder for a <see cref="CreateSchemaExpression"/>
    /// </summary>
    public class CreateSchemaExpressionBuilder : ExpressionBuilderBase<CreateSchemaExpression>, ICreateSchemaOptionsSyntax, ISupportAdditionalFeatures
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSchemaExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        public CreateSchemaExpressionBuilder(CreateSchemaExpression expression)
            : base(expression)
        {
        }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures => Expression.AdditionalFeatures;
    }
}
