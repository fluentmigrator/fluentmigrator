#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2011, Grant Archibald
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
using System.Linq;
using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Builders.Rename;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Builders.Update;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.IfDatabase
{
    /// <summary>
    /// Allows for conditional inclusion of expresions based on the migration context
    /// </summary>
    public class IfDatabaseExpressionRoot : IIfDatabaseExpressionRoot
    {
        /// <summary>
        /// The context to add expressions into
        /// </summary>
        /// <remarks>If the database type doe snot apply then this will be a new context that is not used by the caller</remarks>
        private readonly IMigrationContext _context;

        /// <summary>
        /// Constricts a new instance of a <see cref="IfDatabaseExpressionRoot"/> that will only add expressions to the provided <see cref="context"/> if <see cref="databaseType"/> matches the migration processor
        /// </summary>
        /// <remarks>If the database type does not apply then a <seealso cref="NullIfDatabaseProcessor"/> will be used as a container to void any fluent expressions that would have been executed</remarks>
        /// <param name="context">The context to add expressions to if the database type applies</param>
        /// <param name="databaseType">The database type that the expressions relate to</param>
        public IfDatabaseExpressionRoot(IMigrationContext context, params string[] databaseType)
        {
            if (databaseType == null) throw new ArgumentNullException("databaseType");

            _context = DatabaseTypeApplies(context, databaseType) ? context : new MigrationContext(new MigrationConventions(), new NullIfDatabaseProcessor(), context.MigrationAssemblies, context.ApplicationContext, "");
        }

        /// <summary>
        /// Alter the schema of an existing object
        /// </summary>
        public IAlterExpressionRoot Alter
        {
            get { return new AlterExpressionRoot(_context); }
        }

        /// <summary>
        /// Create a new database object
        /// </summary>
        public ICreateExpressionRoot Create
        {
            get { return new CreateExpressionRoot(_context); }
        }

        /// <summary>
        /// Delete a database object, table, or row
        /// </summary>
        public IDeleteExpressionRoot Delete
        {
            get { return new DeleteExpressionRoot(_context); }
        }

        /// <summary>
        /// Rename tables / columns
        /// </summary>
        public IRenameExpressionRoot Rename
        {
            get { return new RenameExpressionRoot(_context); }
        }

        /// <summary>
        /// Insert data into a table
        /// </summary>
        public IInsertExpressionRoot Insert
        {
            get { return new InsertExpressionRoot(_context); }
        }

        /// <summary>
        /// Execute SQL statements
        /// </summary>
        public IExecuteExpressionRoot Execute
        {
            get { return new ExecuteExpressionRoot(_context); }
        }

        public ISchemaExpressionRoot Schema
        {
            get { return new SchemaExpressionRoot(_context); }
        }

        /// <summary>
        /// Update an existing row
        /// </summary>
        public IUpdateExpressionRoot Update
        {
            get { return new UpdateExpressionRoot(_context); }
        }

        /// <summary>
        /// Checks if the database type matches the name of the context migration processor
        /// </summary>
        /// <param name="context">The context to evaluate</param>
        /// <param name="databaseType">The type to be checked</param>
        /// <returns><c>True</c> if the database type applies, <c>False</c> if not</returns>
        private static bool DatabaseTypeApplies(IMigrationContext context, params string[] databaseType)
        {
            if (context.QuerySchema is IMigrationProcessor)
            {
                string currentDatabaseType = context.QuerySchema.DatabaseType;

                return (from db in databaseType
                        where currentDatabaseType.StartsWith(db, StringComparison.OrdinalIgnoreCase)
                        select db).Any();
            }

            return false;
        }
    }
}
