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

namespace FluentMigrator.Builders.Create.Index
{
    public class CreateIndexExpressionBuilder : ExpressionBuilderBase<CreateIndexExpression>,
        ICreateIndexForTableSyntax,
        ICreateIndexOnColumnOrInSchemaSyntax,
        ICreateIndexColumnOptionsSyntax,
        ICreateIndexOptionsSyntax
    {
        public IndexColumnDefinition CurrentColumn { get; set; }

        public CreateIndexExpressionBuilder(CreateIndexExpression expression)
            : base(expression)
        {
        }

        public ICreateIndexOnColumnOrInSchemaSyntax OnTable(string tableName)
        {
            Expression.Index.TableName = tableName;
            return this;
        }

        public ICreateIndexColumnOptionsSyntax OnColumn(string columnName)
        {
            CurrentColumn = new IndexColumnDefinition { Name = columnName };
            Expression.Index.Columns.Add(CurrentColumn);
            return this;
        }

        public ICreateIndexOptionsSyntax WithOptions()
        {
            return this;
        }

        public ICreateIndexOnColumnSyntax InSchema(string schemaName)
        {
            Expression.Index.SchemaName = schemaName;
            return this;
        }

        public ICreateIndexOnColumnSyntax Ascending()
        {
            CurrentColumn.Direction = Direction.Ascending;
            return this;
        }

        public ICreateIndexOnColumnSyntax Descending()
        {
            CurrentColumn.Direction = Direction.Descending;
            return this;
        }

        ICreateIndexOnColumnSyntax ICreateIndexColumnOptionsSyntax.Unique()
        {
            Expression.Index.IsUnique = true;
            return this;
        }

        public ICreateIndexOnColumnSyntax Unique()
        {
            Expression.Index.IsUnique = true;
            return this;
        }

        public ICreateIndexOnColumnSyntax NonClustered()
        {
            Expression.Index.IsClustered = false;
            return this;
        }

        public ICreateIndexOnColumnSyntax Clustered()
        {
            Expression.Index.IsClustered = true;
            return this;
        }
    }
}