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

using FluentMigrator.Builders.Delete.Column;
using FluentMigrator.Builders.Delete.DefaultConstraint;
using FluentMigrator.Builders.Delete.ForeignKey;
using FluentMigrator.Builders.Delete.Index;
using FluentMigrator.Builders.Delete.Constraint;

namespace FluentMigrator.Builders.Delete
{
    /// <summary>
    /// The root expression for a DELETE operation
    /// </summary>
    public interface IDeleteExpressionRoot : IFluentSyntaxRoot
    {
        /// <summary>
        /// Specify the schema to delete
        /// </summary>
        /// <param name="schemaName">The name of the schema</param>
        void Schema(string schemaName);

        /// <summary>
        /// Specify the table to delete
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <returns>The next step</returns>
        IIfExistsOrInSchemaSyntax Table(string tableName);

        /// <summary>
        /// Specify the column to delete
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The next step</returns>
        IDeleteColumnFromTableSyntax Column(string columnName);

        /// <summary>
        /// Specify that a foreign key with a default name should be deleted
        /// </summary>
        /// <returns>The next step</returns>
        IDeleteForeignKeyFromTableSyntax ForeignKey();

        /// <summary>
        /// Specify that a foreign key with the given name should be deleted
        /// </summary>
        /// <param name="foreignKeyName">The foreign key name</param>
        /// <returns>The next step</returns>
        IDeleteForeignKeyOnTableSyntax ForeignKey(string foreignKeyName);

        /// <summary>
        /// Deletes data from a table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <returns>The next step</returns>
        IDeleteDataOrInSchemaSyntax FromTable(string tableName);

        /// <summary>
        /// Deletes an index
        /// </summary>
        /// <param name="indexName">the name of the index</param>
        /// <returns>The next step</returns>
        IDeleteIndexForTableSyntax Index(string indexName);

        /// <summary>
        /// Deletes an index, based on the naming convention in effect
        /// </summary>
        /// <returns>The next step</returns>
        IDeleteIndexForTableSyntax Index();

        /// <summary>
        /// Delete a sequence with the given name
        /// </summary>
        /// <param name="sequenceName">The sequence name</param>
        /// <returns>The next step</returns>
        IInSchemaSyntax Sequence(string sequenceName);

        /// <summary>
        /// Deletes a named Primary Key from a table
        /// </summary>
        /// <param name="primaryKeyName">The name of the primary key</param>
        /// <returns>The next step</returns>
        IDeleteConstraintOnTableSyntax PrimaryKey(string primaryKeyName);

        /// <summary>
        /// Deletes a named Unique Constraint From a table
        /// </summary>
        /// <param name="constraintName">The constraint name</param>
        /// <returns>The next step</returns>
        IDeleteConstraintOnTableSyntax UniqueConstraint(string constraintName);

        /// <summary>
        /// Deletes a named Unique Constraint from a table based on the naming convention in effect
        /// </summary>
        /// <returns>The next step</returns>
        IDeleteConstraintOnTableSyntax UniqueConstraint();

        /// <summary>
        /// Deletes a default constraint from a column
        /// </summary>
        /// <returns>The next step</returns>
        IDeleteDefaultConstraintOnTableSyntax DefaultConstraint();
    }
}
