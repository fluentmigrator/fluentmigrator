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

using System;
using System.Data;

using FluentMigrator.Expressions;

namespace FluentMigrator
{
    /// <summary>
    /// Interface for a migration processor
    /// </summary>
    /// <remarks>
    /// A migration processor generates the SQL statements using a <see cref="IMigrationGenerator"/>
    /// and executes it using the given connection string.
    /// </remarks>
    public interface IMigrationProcessor : IQuerySchema, IDisposable
    {
        /// <summary>
        /// Execute an SQL statement (escaping not needed)
        /// </summary>
        /// <param name="sql">The SQL statement</param>
        void Execute(string sql);

        /// <summary>
        /// Execute an SQL statement
        /// </summary>
        /// <param name="template">The SQL statement</param>
        /// <param name="args">The arguments to replace in the SQL statement</param>
        void Execute(string template, params object[] args);

        /// <summary>
        /// Reads all data from all rows from a table
        /// </summary>
        /// <param name="schemaName">The schema name of the table</param>
        /// <param name="tableName">The table name</param>
        /// <returns>The data from the specified table</returns>
        DataSet ReadTableData(string schemaName, string tableName);

        /// <summary>
        /// Executes and returns the result of an SQL query
        /// </summary>
        /// <param name="template">The SQL query</param>
        /// <param name="args">The arguments of the SQL query</param>
        /// <returns>The data from the specified SQL query</returns>
        DataSet Read(string template, params object[] args);

        /// <summary>
        /// Returns <c>true</c> if data could be found for the given SQL query
        /// </summary>
        /// <param name="template">The SQL query</param>
        /// <param name="args">The arguments of the SQL query</param>
        /// <returns><c>true</c> when the SQL query returned data</returns>
        bool Exists(string template, params object[] args);

        /// <summary>
        /// Begins a transaction
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commits a transaction
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Rollback of a transaction
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Executes a <c>CREATE SCHEMA</c> SQL expression
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(CreateSchemaExpression expression);

        /// <summary>
        /// Executes a <c>DROP SCHEMA</c> SQL expression
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(DeleteSchemaExpression expression);

        /// <summary>
        /// Executes a <c>ALTER TABLE</c> SQL expression
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(AlterTableExpression expression);

        /// <summary>
        /// Executes a <c>ALTER TABLE ALTER COLUMN</c> SQL expression
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(AlterColumnExpression expression);

        /// <summary>
        /// Executes a <c>CREATE TABLE</c> SQL expression
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(CreateTableExpression expression);

        /// <summary>
        /// Executes a <c>ALTER TABLE ADD COLUMN</c> SQL expression
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(CreateColumnExpression expression);

        /// <summary>
        /// Executes a <c>DROP TABLE</c> SQL expression
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(DeleteTableExpression expression);

        /// <summary>
        /// Executes a <c>ALTER TABLE DROP COLUMN</c> SQL expression
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(DeleteColumnExpression expression);

        /// <summary>
        /// Executes an SQL expression to create a foreign key
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(CreateForeignKeyExpression expression);

        /// <summary>
        /// Executes an SQL expression to drop a foreign key
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(DeleteForeignKeyExpression expression);

        /// <summary>
        /// Executes an SQL expression to create an index
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(CreateIndexExpression expression);

        /// <summary>
        /// Executes an SQL expression to drop an index
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(DeleteIndexExpression expression);

        /// <summary>
        /// Executes an SQL expression to rename a table
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(RenameTableExpression expression);

        /// <summary>
        /// Executes an SQL expression to rename a column
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(RenameColumnExpression expression);

        /// <summary>
        /// Executes an SQL expression to INSERT data
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(InsertDataExpression expression);

        /// <summary>
        /// Executes an SQL expression to alter a default constraint
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(AlterDefaultConstraintExpression expression);

        /// <summary>
        /// Executes a DB operation
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(PerformDBOperationExpression expression);

        /// <summary>
        /// Executes an SQL expression to DELETE data
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(DeleteDataExpression expression);

        /// <summary>
        /// Executes an SQL expression to UPDATE data
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(UpdateDataExpression expression);

        /// <summary>
        /// Executes a <c>ALTER SCHEMA</c> SQL expression
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(AlterSchemaExpression expression);

        /// <summary>
        /// Executes a <c>CREATE SEQUENCE</c> SQL expression
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(CreateSequenceExpression expression);

        /// <summary>
        /// Executes a <c>DROP SEQUENCE</c> SQL expression
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(DeleteSequenceExpression expression);

        /// <summary>
        /// Executes an SQL expression to create a constraint
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(CreateConstraintExpression expression);

        /// <summary>
        /// Executes an SQL expression to drop a constraint
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(DeleteConstraintExpression expression);

        /// <summary>
        /// Executes an SQL expression to drop a default constraint
        /// </summary>
        /// <param name="expression">The expression to execute</param>
        void Process(DeleteDefaultConstraintExpression expression);
    }
}
