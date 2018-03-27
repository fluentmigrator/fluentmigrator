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

namespace FluentMigrator.Builders.Delete.ForeignKey
{
    public class DeleteForeignKeyExpressionBuilder : ExpressionBuilderBase<DeleteForeignKeyExpression>,
        IDeleteForeignKeyFromTableSyntax,
        IDeleteForeignKeyForeignColumnOrInSchemaSyntax,
        IDeleteForeignKeyToTableSyntax,
        IDeleteForeignKeyPrimaryColumnSyntax,
        IDeleteForeignKeyOnTableSyntax,
        IInSchemaSyntax
    {
        public DeleteForeignKeyExpressionBuilder(DeleteForeignKeyExpression expression)
            : base(expression)
        {
        }

        public IDeleteForeignKeyForeignColumnOrInSchemaSyntax FromTable(string foreignTableName)
        {
            Expression.ForeignKey.ForeignTable = foreignTableName;
            return this;
        }

        public IDeleteForeignKeyForeignColumnSyntax InSchema(string foreignSchemaName)
        {
            Expression.ForeignKey.ForeignTableSchema = foreignSchemaName;
            return this;
        }

        public IDeleteForeignKeyToTableSyntax ForeignColumn(string column)
        {
            Expression.ForeignKey.ForeignColumns.Add(column);
            return this;
        }

        public IDeleteForeignKeyToTableSyntax ForeignColumns(params string[] columns)
        {
            foreach (var column in columns)
                Expression.ForeignKey.ForeignColumns.Add(column);

            return this;
        }

        public IDeleteForeignKeyPrimaryColumnSyntax ToTable(string table)
        {
            Expression.ForeignKey.PrimaryTable = table;
            return this;
        }

        public void PrimaryColumn(string column)
        {
            Expression.ForeignKey.PrimaryColumns.Add(column);
        }

        public void PrimaryColumns(params string[] columns)
        {
            foreach (var column in columns)
                Expression.ForeignKey.PrimaryColumns.Add(column);
        }

        IInSchemaSyntax IDeleteForeignKeyOnTableSyntax.OnTable(string foreignTableName)
        {
            Expression.ForeignKey.ForeignTable = foreignTableName;
            return this;
        }

        void IInSchemaSyntax.InSchema(string schemaName)
        {
            Expression.ForeignKey.ForeignTableSchema = schemaName;
        }
    }
}