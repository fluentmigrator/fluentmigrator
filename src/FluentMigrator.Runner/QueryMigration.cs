#region License
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

using System;
using System.Linq;
using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Builders.IfDatabase;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Builders.Rename;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Builders.Update;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Query object for using fluent syntax directly, without recording migrations history.
    /// Use <see cref="Migrator.GetQuery"/> to get a new query instance.
    /// To apply changes, either call <see cref="Process"/> or <see cref="IDisposable.Dispose"/> (via <c>using</c> statement).
    /// </summary>
    public class QueryMigration
    {
        private readonly Migrator _migrator;
        private readonly IMigrationContext _context;

        internal QueryMigration(Migrator migrator, IQuerySchema querySchema)
        {
            _migrator = migrator;
            _context = new MigrationContext(_migrator.Conventions, querySchema, _migrator.MigrationAssembly, _migrator.ApplicationContext);
        }

        /// <summary>Apply current expressions. Can be called multiple times.</summary>
        /// <param name="reverse">Reverse current expressions before applying them.</param>
        /// <remarks><see cref="Delete"/>, <see cref="Execute"/> and <see cref="Update"/> expressions cannot be reversed.</remarks>
        /// <exception cref="NotSupportedException">Expressions cannot be reversed.</exception>
        public void Process(bool reverse = false)
        {
            if (reverse)
                _context.Expressions = _context.Expressions.Select(e => e.Reverse()).Reverse().ToList();
            _migrator.ProcessQuery(this, _context);
            _context.Expressions.Clear();
        }

        /// <summary>Query database schema: check wether schema, table, column, index exist.</summary>
        public ISchemaExpressionRoot Schema
        {
            get { return new SchemaExpressionRoot(_context); }
        }

        /// <summary>Alter database objects: add or alter columns.</summary>
        public IAlterExpressionRoot Alter
        {
            get { return new AlterExpressionRoot(_context); }
        }

        /// <summary>Create database objects: tables, columns, indexes, primary and foreign keys, contraints.</summary>
        public ICreateExpressionRoot Create
        {
            get { return new CreateExpressionRoot(_context); }
        }

        /// <summary>Rename database objects: tables, columns.</summary>
        public IRenameExpressionRoot Rename
        {
            get { return new RenameExpressionRoot(_context); }
        }

        /// <summary>Delete database objects: tables, columns, indexes, primary and foreign keys, contraints. Delete rows from database tables.</summary>
        /// <remarks>Cannot be automatically reversed with <c>reverse</c> argument of <see cref="Process"/>.</remarks>
        public IDeleteExpressionRoot Delete
        {
            get { return new DeleteExpressionRoot(_context); }
        }

        /// <summary>Execute SQL scripts on database.</summary>
        /// <remarks>Cannot be automatically reversed with <c>reverse</c> argument of <see cref="Process"/>.</remarks>
        public IExecuteExpressionRoot Execute
        {
            get { return new ExecuteExpressionRoot(_context); }
        }

        /// <summary>Insert new rows into database tables.</summary>
        public IInsertExpressionRoot Insert
        {
            get { return new InsertExpressionRoot(_context); }
        }

        /// <summary>Update existing rows in database tables.</summary>
        /// <remarks>Cannot be automatically reversed with <c>reverse</c> argument of <see cref="Process"/>.</remarks>
        public IUpdateExpressionRoot Update
        {
            get { return new UpdateExpressionRoot(_context); }
        }

        /// <summary>Apply conditions only on specific database engines. See <see cref="Migrator.AvailableEngines"/> for a list of available engines.</summary>
        /// <param name="databaseType">List of database engines to apply expressions on.</param>
        public IIfDatabaseExpressionRoot IfDatabase(params string[] databaseType)
        {
            return new IfDatabaseExpressionRoot(_context, databaseType);
        }
    }
}