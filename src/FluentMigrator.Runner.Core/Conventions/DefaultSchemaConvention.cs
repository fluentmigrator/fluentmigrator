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

using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Conventions
{
    public sealed class DefaultSchemaConvention : IForeignKeyConvention, IConstraintConvention, IIndexConvention, ISequenceConvention
    {
        private readonly IDefaultSchemaNameConvention _defaultSchemaNameConvention;

        public DefaultSchemaConvention()
            : this(new DefaultSchemaNameConvention(null))
        {
        }

        public DefaultSchemaConvention(string defaultSchemaName)
            : this(new DefaultSchemaNameConvention(defaultSchemaName))
        {
        }

        public DefaultSchemaConvention(IDefaultSchemaNameConvention defaultSchemaNameConvention)
        {
            _defaultSchemaNameConvention = defaultSchemaNameConvention;
        }

        string GetSchemaName(string originalSchemaName)
        {
            return _defaultSchemaNameConvention.GetSchemaName(originalSchemaName);
        }

        public ISchemaExpression Apply(ISchemaExpression expression)
        {
            expression.SchemaName = GetSchemaName(expression.SchemaName);
            return expression;
        }

        public IForeignKeyExpression Apply(IForeignKeyExpression expression)
        {
            expression.ForeignKey.ForeignTableSchema = GetSchemaName(expression.ForeignKey.ForeignTableSchema);
            expression.ForeignKey.PrimaryTableSchema = GetSchemaName(expression.ForeignKey.PrimaryTableSchema);
            return expression;
        }

        public IConstraintExpression Apply(IConstraintExpression expression)
        {
            expression.Constraint.SchemaName = GetSchemaName(expression.Constraint.SchemaName);
            return expression;
        }

        public IIndexExpression Apply(IIndexExpression expression)
        {
            expression.Index.SchemaName = GetSchemaName(expression.Index.SchemaName);
            return expression;
        }

        public ISequenceExpression Apply(ISequenceExpression expression)
        {
            expression.Sequence.SchemaName = GetSchemaName(expression.Sequence.SchemaName);
            return expression;
        }
    }
}
