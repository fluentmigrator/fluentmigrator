#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

namespace FluentMigrator.Builders.Delete.Column
{
    public class DeleteColumnExpressionBuilder : ExpressionBuilderBase<DeleteColumnExpression>,
        IDeleteColumnFromTableSyntax, IInSchemaSyntax
    {
        public DeleteColumnExpressionBuilder(DeleteColumnExpression expression)
            : base(expression)
        {
        }

        public IInSchemaSyntax FromTable(string tableName)
        {
            Expression.TableName = tableName;
            return this;
        }

        public IDeleteColumnFromTableSyntax Column(string columnName) 
        {
            Expression.ColumnNames.Add(columnName);
            return this;
        }

        public void InSchema(string schemaName)
        {
            Expression.SchemaName = schemaName;
        }
    }
}