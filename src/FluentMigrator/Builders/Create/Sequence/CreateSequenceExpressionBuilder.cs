#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Create.Sequence
{
    /// <summary>
    /// An expression builder for a <see cref="CreateSchemaExpression"/>
    /// </summary>
    public class CreateSequenceExpressionBuilder : ExpressionBuilderBase<CreateSequenceExpression>, ICreateSequenceInSchemaSyntax
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSequenceExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        public CreateSequenceExpressionBuilder(CreateSequenceExpression expression)
            : base(expression)
        {
        }

        /// <inheritdoc />
        public ICreateSequenceSyntax IncrementBy(long increment)
        {
            Expression.Sequence.Increment = increment;
            return this;
        }

        /// <inheritdoc />
        public ICreateSequenceSyntax MinValue(long minValue)
        {
            Expression.Sequence.MinValue = minValue;
            return this;
        }

        /// <inheritdoc />
        public ICreateSequenceSyntax MaxValue(long maxValue)
        {
            Expression.Sequence.MaxValue = maxValue;
            return this;
        }

        /// <inheritdoc />
        public ICreateSequenceSyntax StartWith(long startwith)
        {
            Expression.Sequence.StartWith = startwith;
            return this;
        }

        /// <inheritdoc />
        public ICreateSequenceSyntax Cache(long value)
        {
            Expression.Sequence.Cache = value;
            return this;
        }

        /// <inheritdoc />
        public ICreateSequenceSyntax Cycle()
        {
            Expression.Sequence.Cycle = true;
            return this;
        }

        /// <inheritdoc />
        public ICreateSequenceSyntax InSchema(string schemaName)
        {
            Expression.Sequence.SchemaName = schemaName;
            return this;
        }
    }
}
