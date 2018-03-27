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
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Delete.Index
{
    public class DeleteIndexExpressionBuilder : ExpressionBuilderBase<DeleteIndexExpression>,
        IDeleteIndexForTableSyntax,
        IDeleteIndexOnColumnOrInSchemaSyntax
    {
        public IndexColumnDefinition CurrentColumn { get; set; }

        public DeleteIndexExpressionBuilder(DeleteIndexExpression expression)
            : base(expression)
        {
        }

        public IDeleteIndexOnColumnOrInSchemaSyntax OnTable(string tableName)
        {
            Expression.Index.TableName = tableName;
            return this;
        }

        public IDeleteIndexOnColumnSyntax InSchema(string schemaName)
        {
            Expression.Index.SchemaName = schemaName;
            return this;
        }

        public void OnColumn(string columnName)
        {
            var column = new IndexColumnDefinition { Name = columnName };
            Expression.Index.Columns.Add(column);
        }

        public void OnColumns(params string[] columnNames)
        {
            foreach (string columnName in columnNames)
                Expression.Index.Columns.Add(new IndexColumnDefinition { Name = columnName });
        }
    }
}