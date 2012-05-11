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

using FluentMigrator.Builders.Delete.Column;
using FluentMigrator.Builders.Delete.DefaultConstraint;
using FluentMigrator.Builders.Delete.ForeignKey;
using FluentMigrator.Infrastructure;
using FluentMigrator.Builders.Delete.Index;
using FluentMigrator.Builders.Delete.Constraint;

namespace FluentMigrator.Builders.Delete
{
    public interface IDeleteExpressionRoot : IFluentSyntax
    {
        void Schema(string schemaName);
        IInSchemaSyntax Table(string tableName);
        IDeleteColumnFromTableSyntax Column(string columnName);
        IDeleteForeignKeyFromTableSyntax ForeignKey();
        IDeleteForeignKeyOnTableSyntax ForeignKey(string foreignKeyName);
        IDeleteDataOrInSchemaSyntax FromTable(string tableName);

        /// <summary>
        /// Deletes an index
        /// </summary>
        /// <param name="indexName">the name of the index</param>
        /// <returns></returns>
        IDeleteIndexForTableSyntax Index(string indexName);
        /// <summary>
        /// Deletes an index, based on the naming convention in effect
        /// </summary>
        /// <returns></returns>
        IDeleteIndexForTableSyntax Index();

        IInSchemaSyntax Sequence(string sequenceName);

        /// <summary>
        /// Deletes a named Primary Key from a table
        /// </summary>
        /// <param name="primaryKeyName"></param>
        /// <returns></returns>
        IDeleteConstraintOnTableSyntax PrimaryKey(string primaryKeyName);

        /// <summary>
        /// Deletes a named Unique Constraint From a table
        /// </summary>
        /// <param name="constraintName"></param>
        /// <returns></returns>
        IDeleteConstraintOnTableSyntax UniqueConstraint(string constraintName);

        IDeleteDefaultConstraintOnTableSyntax DefaultConstraint();
    }
}
