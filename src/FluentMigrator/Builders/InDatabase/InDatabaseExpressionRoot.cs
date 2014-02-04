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
using FluentMigrator.Builders.IfDatabase;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Builders.Rename;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Builders.Update;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.InDatabase
{
    /// <summary>
    /// Allows to apply expressions to a different (non-default) database
    /// </summary>
    public class InDatabaseExpressionRoot : IInDatabaseExpressionRoot
    {
        /// <summary>
        /// The context to add expressions into (this context will use provided connection name).
        /// </summary>
        private readonly IMigrationContext _context;

        /// <summary>Constructs a new instance of a <see cref="InDatabaseExpressionRoot"/> that will be using specified <paramref name="databaseKey" /></summary>
        /// <param name="context">The parent context</param>
        /// <param name="databaseKey">Database key to use (based on keys provided to <see cref="IMultiDatabaseMigrationProcessor" />)</param>
        public InDatabaseExpressionRoot(IMigrationContext context, string databaseKey)
        {
            if (databaseKey == null)
                throw new ArgumentNullException("databaseKey");

            var processor = context.QuerySchema as IMultiDatabaseMigrationProcessor;
            if (processor == null)
                throw new ArgumentException("Multiple databases can only be used with IMultiDatabaseMigrationProcessor.");

            if (!processor.HasDatabaseKey(databaseKey))
            {
                var message = string.Format("Database key '{0}' was not found. Available databases: '{1}'.", databaseKey, string.Join("', '", processor.GetDatabaseKeys().ToArray()));
                throw new ArgumentException("context", message);
            }

            _context = new AlternateDatabaseMigrationContext(context, processor, databaseKey);
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
        
        public IIfDatabaseExpressionRoot IfDatabase(params string[] databaseType)
        {
            return new IfDatabaseExpressionRoot(_context, databaseType);
        }
    }
}
