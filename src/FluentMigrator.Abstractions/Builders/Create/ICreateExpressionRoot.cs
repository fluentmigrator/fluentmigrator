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

using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Builders.Create.Constraint;
using FluentMigrator.Builders.Create.ForeignKey;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Builders.Create.Schema;
using FluentMigrator.Builders.Create.Sequence;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create
{
    /// <summary>
    /// The root expression for a CREATE operation
    /// </summary>
    public interface ICreateExpressionRoot : IFluentSyntax
    {
        /// <summary>
        /// Creates a schema
        /// </summary>
        /// <param name="schemaName">The schema name</param>
        /// <returns>The options for the schema creation</returns>
        ICreateSchemaOptionsSyntax Schema(string schemaName);

        /// <summary>
        /// Creates a table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <returns>Additional information about the table creation</returns>
        ICreateTableWithColumnOrSchemaOrDescriptionSyntax Table(string tableName);

        /// <summary>
        /// Creates a column
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns>Additional information about the column creation</returns>
        ICreateColumnOnTableSyntax Column(string columnName);

        /// <summary>
        /// Creates a foreign key with a default name
        /// </summary>
        /// <returns>Additional information about the foreign key creation</returns>
        ICreateForeignKeyFromTableSyntax ForeignKey();

        /// <summary>
        /// Creates a foreign key with the given name
        /// </summary>
        /// <param name="foreignKeyName">The foreign key name</param>
        /// <returns>Additional information about the foreign key creation</returns>
        ICreateForeignKeyFromTableSyntax ForeignKey(string foreignKeyName);

        /// <summary>
        /// Creates an index with a default name
        /// </summary>
        /// <returns>Additional information about the index creation</returns>
        ICreateIndexForTableSyntax Index();

        /// <summary>
        /// Creates an index with the given name
        /// </summary>
        /// <param name="indexName">The index name</param>
        /// <returns>Additional information about the index creation</returns>
        ICreateIndexForTableSyntax Index(string indexName);

        /// <summary>
        /// Creates a sequence
        /// </summary>
        /// <param name="sequenceName">The sequence name</param>
        /// <returns>Additional information about the sequence creation</returns>
        ICreateSequenceInSchemaSyntax Sequence(string sequenceName);

        /// <summary>
        /// Creates a primary key with a default name
        /// </summary>
        /// <returns>Additional information about the primary key creation</returns>
        ICreateConstraintOnTableSyntax PrimaryKey();

        /// <summary>
        /// Creates a primary key with the given name
        /// </summary>
        /// <param name="primaryKeyName">The primary key name</param>
        /// <returns>Additional information about the primary key creation</returns>
        ICreateConstraintOnTableSyntax PrimaryKey(string primaryKeyName);

        /// <summary>
        /// Creates an unique constraint with a default name
        /// </summary>
        /// <returns>Additional information about the unique constraint creation</returns>
        ICreateConstraintOnTableSyntax UniqueConstraint();

        /// <summary>
        /// Creates an unique constraint with the given name
        /// </summary>
        /// <param name="constraintName">The unique constraint name</param>
        /// <returns>Additional information about the unique constraint creation</returns>
        ICreateConstraintOnTableSyntax UniqueConstraint(string constraintName);
    }
}
