#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Helpers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Models;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.Firebird
{
    /// <summary>
    /// The Firebird processor for FluentMigrator.
    /// </summary>
    public class FirebirdProcessor : GenericProcessorBase
    {
        /// <summary>
        /// An instance of <see cref="FluentMigrator.Runner.Generators.Firebird.FirebirdTruncator"/> used to handle truncation of long names
        /// in Firebird database objects, such as tables, columns, and constraints.
        /// </summary>
        /// <remarks>
        /// This field is marked as <see cref="System.ObsoleteAttribute"/>. Use the <see cref="Truncator"/> property instead.
        /// </remarks>
        [Obsolete("Use the Truncator property")]
        protected readonly FirebirdTruncator truncator;

        private readonly Lazy<Version> _firebirdVersionFunc;
        private readonly FirebirdQuoter _quoter;

        /// <summary>
        /// A list of table names that have been created during the execution of DDL (Data Definition Language) operations.
        /// </summary>
        protected List<string> DDLCreatedTables;
        /// <summary>
        /// Tracks the columns created during the execution of DDL (Data Definition Language) operations.
        /// The dictionary maps table names to a list of column names that have been created for each table.
        /// </summary>
        protected Dictionary<string, List<string>> DDLCreatedColumns;
        /// <summary>
        /// A list of table names that have been affected by Data Definition Language (DDL) operations.
        /// </summary>
        /// <remarks>
        /// This field is used to track tables that have been modified, created, or otherwise touched
        /// during the execution of DDL operations. It is primarily utilized for managing and verifying
        /// table locks and ensuring consistency during database schema changes.
        /// </remarks>
        protected List<string> DDLTouchedTables;
        /// <summary>
        /// A dictionary that tracks columns affected by DDL (Data Definition Language) operations,
        /// organized by table name. Each key represents a table name, and the associated value is a list
        /// of column names that have been modified or touched during the operation.
        /// </summary>
        protected Dictionary<string, List<string>> DDLTouchedColumns;

        /// <inheritdoc />
        public FirebirdProcessor(
            [NotNull] FirebirdDbFactory factory,
            [NotNull] FirebirdGenerator generator,
            [NotNull] FirebirdQuoter quoter,
            [NotNull] ILogger<FirebirdProcessor> logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IConnectionStringAccessor connectionStringAccessor,
            [NotNull] FirebirdOptions fbOptions)
            : base(() => factory.Factory, generator, logger, options.Value, connectionStringAccessor)
        {
            FBOptions = fbOptions ?? throw new ArgumentNullException(nameof(fbOptions));
            _firebirdVersionFunc = new Lazy<Version>(GetFirebirdVersion);
            _quoter = quoter;
#pragma warning disable 618
            truncator = new FirebirdTruncator(FBOptions.TruncateLongNames, FBOptions.PackKeyNames);
#pragma warning restore 618
            ClearLocks();
            ClearDDLFollowers();
        }

        /// <inheritdoc />
        public override string DatabaseType => ProcessorIdConstants.Firebird;

        /// <inheritdoc />
        public override IList<string> DatabaseTypeAliases { get; } = new List<string>();

        /// <summary>
        /// Gets the options specific to the Firebird database processor.
        /// </summary>
        /// <remarks>
        /// This property provides access to configuration settings for the Firebird processor,
        /// such as transaction models, name truncation, and other Firebird-specific behaviors.
        /// </remarks>
        public FirebirdOptions FBOptions { get; }
        /// <summary>
        /// Gets a value indicating whether the connected Firebird database is version 3.0 or higher.
        /// </summary>
        /// <value>
        /// <c>true</c> if the Firebird database version is 3.0 or higher; otherwise, <c>false</c>.
        /// </value>
        public bool IsFirebird3 => _firebirdVersionFunc.Value >= new Version(3, 0);
        /// <summary>
        /// Gets the migration generator used to create SQL statements for database migrations.
        /// </summary>
        /// <remarks>
        /// This property overrides the <see cref="ProcessorBase.Generator"/> property to provide
        /// an instance of <see cref="IMigrationGenerator"/> specific to the Firebird database.
        /// </remarks>
        public new IMigrationGenerator Generator => base.Generator;

        /// <summary>
        /// Gets the announcer used for logging migration-related messages.
        /// </summary>
        /// <remarks>
        /// This property is marked as <see cref="ObsoleteAttribute"/> and should not be used. 
        /// Consider using alternative logging mechanisms or properties provided by the processor.
        /// </remarks>
        /// <returns>
        /// An instance of <see cref="IAnnouncer"/> used for logging.
        /// </returns>
        [Obsolete]
        public new IAnnouncer Announcer => base.Announcer;

#pragma warning disable 618
        /// <summary>
        /// Gets the <see cref="FluentMigrator.Runner.Generators.Firebird.FirebirdTruncator"/> instance used to handle truncation of long names
        /// in Firebird database objects, such as tables, columns, and constraints.
        /// </summary>
        /// <remarks>
        /// This property replaces the obsolete <c>truncator</c> field and should be used for truncation operations
        /// in Firebird database processing.
        /// </remarks>
        public FirebirdTruncator Truncator => truncator;
#pragma warning restore 618

        /// <summary>
        /// Retrieves the version of the Firebird database engine currently in use.
        /// </summary>
        /// <remarks>
        /// This method queries the Firebird database to determine its version by executing
        /// a SQL statement that retrieves the engine version from the system context.
        /// If the version cannot be determined, it defaults to version 2.0 or 2.1 based on the error handling logic.
        /// </remarks>
        /// <returns>
        /// A <see cref="Version"/> object representing the Firebird database engine version.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the database connection is not open or cannot be established.
        /// </exception>
        private Version GetFirebirdVersion()
        {
            EnsureConnectionIsOpen();
            try
            {
                var result = Read("SELECT rdb$get_context('SYSTEM', 'ENGINE_VERSION') from rdb$database");
                var version = (string)result.Tables[0].Rows[0].ItemArray[0];
                var versionRegEx = new Regex(@"\d+\.\d+(\.\d+)?");
                var match = versionRegEx.Match(version);
                if (match.Success)
                {
                    return new Version(match.Value);
                }

                return new Version(2, 1);
            }
            catch
            {
                // Ignore error - Older than version 2.1
                return new Version(2, 0);
            }
        }

        /// <inheritdoc />
        public override bool SchemaExists(string schemaName)
        {
            //No schema support in firebird
            return true;
        }

        /// <inheritdoc />
        public override bool TableExists(string schemaName, string tableName)
        {
            CheckTable(schemaName);
            return Exists("select rdb$relation_name from rdb$relations where (rdb$flags IS NOT NULL) and (lower(rdb$relation_name) = lower('{0}'))", FormatToSafeName(tableName));
        }

        /// <inheritdoc />
        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            CheckTable(tableName);
            CheckColumn(tableName, columnName);
            return Exists("select rdb$field_name from rdb$relation_fields where (lower(rdb$relation_name) = lower('{0}')) and (lower(rdb$field_name) = lower('{1}'))", FormatToSafeName(tableName), FormatToSafeName(columnName));
        }

        /// <inheritdoc />
        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            CheckTable(tableName);
            return Exists("select rdb$constraint_name from rdb$relation_constraints where (lower(rdb$relation_name) = lower('{0}')) and (lower(rdb$constraint_name) = lower('{1}'))", FormatToSafeName(tableName), FormatToSafeName(constraintName));
        }

        /// <inheritdoc />
        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            CheckTable(tableName);
            return Exists("select rdb$index_name from rdb$indices where (lower(rdb$relation_name) = lower('{0}')) and (lower(rdb$index_name) = lower('{1}')) and (rdb$system_flag <> 1 OR rdb$system_flag IS NULL) and (rdb$foreign_key IS NULL)", FormatToSafeName(tableName), FormatToSafeName(indexName));
        }

        /// <inheritdoc />
        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return Exists("select rdb$generator_name from rdb$generators where lower(rdb$generator_name) = lower('{0}')", FormatToSafeName(sequenceName));
        }

        /// <summary>
        /// Determines whether a trigger with the specified name exists in the given schema and table.
        /// </summary>
        /// <param name="schemaName">The name of the schema containing the table. Can be empty if the schema is not required.</param>
        /// <param name="tableName">The name of the table to check for the trigger.</param>
        /// <param name="triggerName">The name of the trigger to check for existence.</param>
        /// <returns><c>true</c> if the trigger exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tableName"/> is <c>null</c> or empty.</exception>
        /// <remarks>
        /// This method queries the Firebird database system tables to verify the existence of the specified trigger.
        /// </remarks>
        public virtual bool TriggerExists(string schemaName, string tableName, string triggerName)
        {
            CheckTable(tableName);
            return Exists("select rdb$trigger_name from rdb$triggers where (lower(rdb$relation_name) = lower('{0}')) and (lower(rdb$trigger_name) = lower('{1}'))", FormatToSafeName(tableName), FormatToSafeName(triggerName));
        }

        /// <inheritdoc />
        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            CheckTable(tableName);
            return Read("SELECT * FROM {0}", _quoter.QuoteTableName(tableName, schemaName));
        }

        /// <inheritdoc />
        public override DataSet Read(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = CreateCommand(string.Format(template, args)))
            using (var reader = command.ExecuteReader())
            {
                return reader.ReadDataSet();
            }
        }

        /// <inheritdoc />
        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            return false;
        }

        /// <inheritdoc />
        public override bool Exists(string template, params object[] args)
        {
            EnsureConnectionIsOpen();

            using (var command = CreateCommand(string.Format(template, args)))
            using (var reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }

        /// <inheritdoc />
        public override void BeginTransaction()
        {
            base.BeginTransaction();
            ClearLocks();
            ClearDDLFollowers();
        }

        /// <inheritdoc />
        public override void CommitTransaction()
        {
            base.CommitTransaction();
            EnsureConnectionIsClosed();
            ClearLocks();
        }

        /// <inheritdoc />
        public override void RollbackTransaction()
        {
            base.RollbackTransaction();
            EnsureConnectionIsClosed();
            ClearLocks();
        }

        /// <summary>
        /// Commits the current transaction and immediately begins a new one, retaining the connection.
        /// </summary>
        /// <remarks>
        /// This method is useful for scenarios where a transaction needs to be committed, but the connection 
        /// should remain open and ready for further operations. It is typically used to ensure that changes 
        /// are saved while maintaining the transactional context.
        /// </remarks>
        /// <seealso cref="AutoCommit"/>
        /// <seealso cref="CommitTransaction"/>
        /// <seealso cref="BeginTransaction"/>
        public virtual void CommitRetaining()
        {
            if (IsRunningOutOfMigrationScope())
            {
                EnsureConnectionIsClosed();
                return;
            }

            Logger.LogSay("Committing and Retaining Transaction");

            CommitTransaction();
            BeginTransaction();
        }

        /// <summary>
        /// Automatically commits the current transaction if the transaction model is set to <see cref="FirebirdTransactionModel.AutoCommit"/>.
        /// </summary>
        /// <remarks>
        /// This method ensures that changes are committed to the database when the transaction model is configured for automatic commits.
        /// </remarks>
        /// <seealso cref="CommitRetaining"/>
        public virtual void AutoCommit()
        {
            if (FBOptions.TransactionModel == FirebirdTransactionModel.AutoCommit)
                CommitRetaining();
        }

        /// <summary>
        /// Determines whether the current operation is being executed outside the scope of a migration.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current operation is running outside a migration scope (i.e., when no active transaction exists); 
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method checks the <see cref="Transaction"/> property to determine if a transaction is active.
        /// </remarks>
        public bool IsRunningOutOfMigrationScope()
        {
            return Transaction == null;
        }

        /// <summary>
        /// Resets the internal tracking of Data Definition Language (DDL) operations.
        /// </summary>
        /// <remarks>
        /// This method clears the lists and dictionaries used to track created tables and columns
        /// during the migration process. It is typically called to ensure a clean state before
        /// starting a new set of DDL operations.
        /// </remarks>
        protected void ClearDDLFollowers()
        {
            DDLCreatedTables = new List<string>();
            DDLCreatedColumns = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Registers the creation of a table in the internal tracking list.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table to be registered as created.
        /// </param>
        /// <remarks>
        /// This method ensures that the specified table is added to the list of created tables
        /// if it has not already been registered. It is used internally to track DDL operations.
        /// </remarks>
        protected void RegisterTableCreation(string tableName)
        {
            if (!DDLCreatedTables.Contains(tableName))
                DDLCreatedTables.Add(tableName);
        }

        /// <summary>
        /// Registers the creation of a column in a specified table.
        /// </summary>
        /// <param name="tableName">The name of the table where the column is being created.</param>
        /// <param name="columnName">The name of the column being created.</param>
        /// <remarks>
        /// This method ensures that the column creation is tracked in the internal dictionary of created columns.
        /// If the table does not exist in the dictionary, it is added along with the column.
        /// If the column is not already registered for the table, it is added to the list of created columns for that table.
        /// </remarks>
        protected void RegisterColumnCreation(string tableName, string columnName)
        {
            if (!DDLCreatedColumns.ContainsKey(tableName))
            {
                DDLCreatedColumns.Add(tableName, new List<string>() { columnName });
            }
            else if (!DDLCreatedColumns[tableName].Contains(columnName))
            {
                DDLCreatedColumns[tableName].Add(columnName);
            }
        }

        /// <summary>
        /// Determines whether a table with the specified name has been created during the current migration process.
        /// </summary>
        /// <param name="tableName">The name of the table to check.</param>
        /// <returns>
        /// <c>true</c> if the table has been created; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method checks the internal list of tables that have been registered as created during the migration process.
        /// </remarks>
        protected bool IsTableCreated(string tableName)
        {
            return DDLCreatedTables.Contains(tableName);
        }

        /// <summary>
        /// Determines whether a specific column has been created in the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table to check.</param>
        /// <param name="columnName">The name of the column to check.</param>
        /// <returns>
        /// <c>true</c> if the column has been created in the specified table; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsColumnCreated(string tableName, string columnName)
        {
            return DDLCreatedColumns.ContainsKey(tableName) && DDLCreatedColumns[tableName].Contains(columnName);
        }

        /// <summary>
        /// Clears the locks by resetting the lists of touched tables and columns.
        /// </summary>
        /// <remarks>
        /// This method initializes the <see cref="DDLTouchedTables"/> and <see cref="DDLTouchedColumns"/> 
        /// to their default empty states, ensuring that no lingering locks remain from previous operations.
        /// </remarks>
        protected void ClearLocks()
        {
            DDLTouchedTables = new List<string>();
            DDLTouchedColumns = new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// Locks the specified table to ensure it is tracked as being touched during the migration process.
        /// </summary>
        /// <param name="tableName">The name of the table to lock.</param>
        /// <remarks>
        /// This method adds the table name to the internal list of touched tables if it is not already present.
        /// It is used to track tables that have been modified or accessed during the migration process.
        /// </remarks>
        public void LockTable(string tableName)
        {
            if (!DDLTouchedTables.Contains(tableName))
                DDLTouchedTables.Add(tableName);
        }

        /// <summary>
        /// Locks the specified columns in the given table to prevent modifications during migration operations.
        /// </summary>
        /// <param name="tableName">The name of the table containing the columns to lock.</param>
        /// <param name="columns">A collection of column names to lock within the specified table.</param>
        /// <remarks>
        /// This method ensures that the specified columns are locked by iterating through the collection
        /// and invoking the <see cref="LockColumn(string, string)"/> method for each column.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="tableName"/> or <paramref name="columns"/> is <c>null</c>.
        /// </exception>
        public void LockColumn(string tableName, IEnumerable<string> columns)
        {
            columns.ToList().ForEach(x => LockColumn(tableName, x));
        }

        /// <summary>
        /// Locks the specified column in the given table to prevent modifications during migration operations.
        /// </summary>
        /// <param name="tableName">The name of the table containing the column to lock.</param>
        /// <param name="columnName">The name of the column to lock within the specified table.</param>
        /// <remarks>
        /// This method ensures that the specified column is locked by adding it to the internal tracking structure
        /// if it has not already been marked as locked.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="tableName"/> or <paramref name="columnName"/> is <c>null</c>.
        /// </exception>
        public void LockColumn(string tableName, string columnName)
        {
            if (!DDLTouchedColumns.ContainsKey(tableName))
            {
                DDLTouchedColumns.Add(tableName, new List<string>() { columnName });
            }
            else if (!DDLTouchedColumns[tableName].Contains(columnName))
            {
                DDLTouchedColumns[tableName].Add(columnName);
            }
        }

        /// <summary>
        /// Verifies the existence and state of a specified table in the Firebird database.
        /// </summary>
        /// <param name="tableName">The name of the table to check.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the table is locked and the <see cref="FirebirdOptions.VirtualLock"/> property is set to <c>true</c>.
        /// </exception>
        /// <remarks>
        /// If the table has been previously touched and the <see cref="FirebirdOptions.TransactionModel"/> is set to 
        /// <see cref="FirebirdTransactionModel.AutoCommitOnCheckFail"/>, the method will commit the transaction 
        /// retaining its state.
        /// </remarks>
        public void CheckTable(string tableName)
        {
            if (DDLTouchedTables.Contains(tableName))
            {
                if (FBOptions.TransactionModel == FirebirdTransactionModel.AutoCommitOnCheckFail)
                {
                    CommitRetaining();
                    return;
                }

                if (FBOptions.VirtualLock)
                    throw new InvalidOperationException($"Table {tableName} is locked");
            }
        }

        /// <summary>
        /// Verifies the existence of the specified columns in the given table.
        /// </summary>
        /// <param name="tableName">The name of the table to check.</param>
        /// <param name="columns">A collection of column names to verify in the specified table.</param>
        /// <remarks>
        /// This method iterates through the provided column names and checks each column's existence in the specified table.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="tableName"/> or <paramref name="columns"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a specified column does not exist in the table.
        /// </exception>
        public void CheckColumn(string tableName, IEnumerable<string> columns)
        {
            columns.ToList().ForEach(x => CheckColumn(tableName, x));
        }

        /// <summary>
        /// Validates the existence and state of a specific column in a given table.
        /// </summary>
        /// <param name="tableName">The name of the table containing the column to check.</param>
        /// <param name="columnName">The name of the column to validate.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the column is locked and the <see cref="FirebirdOptions.VirtualLock"/> option is enabled.
        /// </exception>
        /// <remarks>
        /// This method ensures that the specified column is properly checked and handles scenarios
        /// where the column has been previously touched. Depending on the <see cref="FirebirdOptions.TransactionModel"/>,
        /// it may commit the transaction if a check fails.
        /// </remarks>
        public void CheckColumn(string tableName, string columnName)
        {
            CheckTable(tableName);
            if (DDLTouchedColumns.Any(x => x.Key == tableName && x.Value.Contains(columnName)))
            {
                if (FBOptions.TransactionModel == FirebirdTransactionModel.AutoCommitOnCheckFail)
                {
                    CommitRetaining();
                    return;
                }

                if (FBOptions.VirtualLock)
                    throw new InvalidOperationException(string.Format("Column {0} in table {1} is locked", columnName, tableName));
            }
        }

        /// <inheritdoc />
        public override void Process(CreateColumnExpression expression)
        {
            Truncator.Truncate(expression);
            CheckColumn(expression.TableName, expression.Column.Name);
            LockColumn(expression.TableName, expression.Column.Name);
            RegisterColumnCreation(expression.TableName, expression.Column.Name);
            InternalProcess(Generator.Generate(expression));

            if (expression.Column.IsIdentity)
            {
                CreateSequenceForIdentity(expression.TableName, expression.Column.Name);
            }

            if (FBOptions.TransactionModel != FirebirdTransactionModel.None)
                CommitRetaining();
        }

        /// <inheritdoc />
        public override void Process(AlterColumnExpression expression)
        {
            Truncator.Truncate(expression);
            CheckColumn(expression.TableName, expression.Column.Name);
            FirebirdSchemaProvider schema = new FirebirdSchemaProvider(this, _quoter);
            FirebirdTableSchema table = schema.GetTableSchema(expression.TableName);
            ColumnDefinition colDef = table.Definition.Columns.FirstOrDefault(x => x.Name == _quoter.ToFbObjectName(expression.Column.Name));

            var generator = (FirebirdGenerator) Generator;

            var tableName = expression.Column.TableName ?? expression.TableName;

            //Change nullable constraint
            if (colDef == null || colDef.IsNullable != expression.Column.IsNullable)
            {
                string nullConstraintCommand;
                if (IsFirebird3)
                {
                    nullConstraintCommand = generator.GenerateSetNull3(tableName, expression.Column);
                }
                else
                {
                    nullConstraintCommand = generator.GenerateSetNullPre3(tableName, expression.Column);
                }

                InternalProcess(nullConstraintCommand);
            }

            //Change default value
            if (colDef == null || !FirebirdGenerator.DefaultValuesMatch(colDef, expression.Column))
            {
                IMigrationExpression defaultConstraint;
                if (expression.Column.DefaultValue is ColumnDefinition.UndefinedDefaultValue)
                {
                    defaultConstraint = new DeleteDefaultConstraintExpression()
                    {
                        SchemaName = expression.SchemaName,
                        TableName = expression.TableName,
                        ColumnName = expression.Column.Name
                    };
                }
                else
                {
                    defaultConstraint = new AlterDefaultConstraintExpression()
                    {
                        ColumnName = expression.Column.Name,
                        DefaultValue = expression.Column.DefaultValue,
                        TableName = expression.TableName,
                        SchemaName = expression.SchemaName
                    };
                }

                if (defaultConstraint is DeleteDefaultConstraintExpression deleteDefaultConstraintExpression)
                {
                    InternalProcess(Generator.Generate(deleteDefaultConstraintExpression));
                }
                else
                {
                    InternalProcess(Generator.Generate((AlterDefaultConstraintExpression) defaultConstraint));
                }
            }

            //Change type
            if (colDef == null || !FirebirdGenerator.ColumnTypesMatch(colDef, expression.Column))
            {
                InternalProcess(generator.GenerateSetType(tableName, expression.Column));
            }

            bool identitySequenceExists;
            try
            {
                identitySequenceExists = SequenceExists(string.Empty, GetSequenceName(expression.TableName, expression.Column.Name));
            }
            catch (ArgumentException)
            {
                identitySequenceExists = false;
            }

            //Adjust identity generators
            if (expression.Column.IsIdentity)
            {
                if (!identitySequenceExists)
                    CreateSequenceForIdentity(expression.TableName, expression.Column.Name);
            }
            else
            {
                if (identitySequenceExists)
                    DeleteSequenceForIdentity(expression.TableName, expression.Column.Name);
            }
        }

        /// <inheritdoc />
        public override void Process(RenameColumnExpression expression)
        {
            Truncator.Truncate(expression);
            CheckColumn(expression.TableName, expression.OldName);
            CheckColumn(expression.TableName, expression.NewName);
            LockColumn(expression.TableName, expression.OldName);
            LockColumn(expression.TableName, expression.NewName);
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(DeleteColumnExpression expression)
        {
            Truncator.Truncate(expression);
            CheckColumn(expression.TableName, expression.ColumnNames);
            LockColumn(expression.TableName, expression.ColumnNames);
            foreach (string columnName in expression.ColumnNames)
            {
                try
                {
                    if (SequenceExists(string.Empty, GetSequenceName(expression.TableName, columnName)))
                    {
                        DeleteSequenceForIdentity(expression.TableName, columnName);
                    }
                }
                catch (ArgumentException)
                {
                    // Ignore argument exception
                }
            }

            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(CreateTableExpression expression)
        {
            Truncator.Truncate(expression);
            LockTable(expression.TableName);
            RegisterTableCreation(expression.TableName);
            InternalProcess(Generator.Generate(expression));
            foreach (ColumnDefinition colDef in expression.Columns)
            {
                if (colDef.IsIdentity)
                    CreateSequenceForIdentity(expression.TableName, colDef.Name);
            }

            if (FBOptions.TransactionModel != FirebirdTransactionModel.None)
                CommitRetaining();
        }

        /// <inheritdoc />
        public override void Process(AlterTableExpression expression)
        {
            Truncator.Truncate(expression);
            CheckTable(expression.TableName);
            LockTable(expression.TableName);
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(RenameTableExpression expression)
        {
            Truncator.Truncate(expression);
            FirebirdSchemaProvider schema = new FirebirdSchemaProvider(this, _quoter);
            FirebirdTableDefinition firebirdTableDef = schema.GetTableDefinition(expression.OldName);
            firebirdTableDef.Name = expression.NewName;
            CreateTableExpression createNew = new CreateTableExpression()
            {
                TableName = expression.NewName,
                SchemaName = string.Empty
            };

            //copy column definitions (nb: avoid to copy key names, because in firebird they must be globally unique, so let it rename)
            firebirdTableDef.Columns.ToList().ForEach(x => createNew.Columns.Add(new ColumnDefinition()
            {
                Name = x.Name,
                DefaultValue = x.DefaultValue,
                IsForeignKey = x.IsForeignKey,
                IsIdentity = x.IsIdentity,
                IsIndexed = x.IsIndexed,
                IsNullable = x.IsNullable,
                IsPrimaryKey = x.IsPrimaryKey,
                IsUnique = x.IsUnique,
                ModificationType = x.ModificationType,
                Precision = x.Precision,
                Size = x.Size,
                Type = x.Type,
                CustomType = x.CustomType,
                Expression = x.Expression,
                ExpressionStored = x.ExpressionStored,
            }));

            Process(createNew);

            int columnCount = firebirdTableDef.Columns.Count;
            string[] columns = firebirdTableDef.Columns.Select(x => x.Name).ToArray();
            InsertDataExpression data = new InsertDataExpression();
            data.TableName = firebirdTableDef.Name;
            data.SchemaName = firebirdTableDef.SchemaName;
            using (DataSet ds = ReadTableData(string.Empty, expression.OldName))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    InsertionDataDefinition insert = new InsertionDataDefinition();
                    for (int i = 0; i < columnCount; i++)
                    {
                        insert.Add(new KeyValuePair<string, object>(columns[i], dr.ItemArray[i]));
                    }
                    data.Rows.Add(insert);
                }
            }
            Process(data);

            DeleteTableExpression delTable = new DeleteTableExpression()
            {
                TableName = expression.OldName,
                SchemaName = string.Empty
            };
            Process(delTable);
        }

        /// <inheritdoc />
        public override void Process(DeleteTableExpression expression)
        {
            Truncator.Truncate(expression);
            CheckTable(expression.TableName);
            LockTable(expression.TableName);
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(AlterDefaultConstraintExpression expression)
        {
            Truncator.Truncate(expression);
            CheckColumn(expression.TableName, expression.ColumnName);
            LockColumn(expression.TableName, expression.ColumnName);
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(DeleteDefaultConstraintExpression expression)
        {
            Truncator.Truncate(expression);
            CheckColumn(expression.TableName, expression.ColumnName);
            LockColumn(expression.TableName, expression.ColumnName);
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(CreateIndexExpression expression)
        {
            Truncator.Truncate(expression);
            CheckTable(expression.Index.TableName);
            CheckColumn(expression.Index.TableName, expression.Index.Columns.Select(x => x.Name));
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(DeleteIndexExpression expression)
        {
            Truncator.Truncate(expression);
            CheckTable(expression.Index.TableName);
            CheckColumn(expression.Index.TableName, expression.Index.Columns.Select(x => x.Name));
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(CreateSchemaExpression expression)
        {
            Truncator.Truncate(expression);
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(AlterSchemaExpression expression)
        {
            Truncator.Truncate(expression);
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(DeleteSchemaExpression expression)
        {
            Truncator.Truncate(expression);
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(CreateConstraintExpression expression)
        {
            Truncator.Truncate(expression);
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(DeleteConstraintExpression expression)
        {
            Truncator.Truncate(expression);
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(CreateForeignKeyExpression expression)
        {
            Truncator.Truncate(expression);
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(DeleteForeignKeyExpression expression)
        {
            Truncator.Truncate(expression);
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(CreateSequenceExpression expression)
        {
            Truncator.Truncate(expression);
            InternalProcess(Generator.Generate(expression));

            if (expression.Sequence.StartWith != null)
                InternalProcess(((FirebirdGenerator) Generator).GenerateAlterSequence(expression.Sequence));
        }

        /// <inheritdoc />
        public override void Process(DeleteSequenceExpression expression)
        {
            Truncator.Truncate(expression);
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Process(InsertDataExpression expression)
        {
            Truncator.Truncate(expression);
            CheckTable(expression.TableName);
            expression.Rows.ForEach(x => x.ForEach(y => CheckColumn(expression.TableName, y.Key)));
            var subExpression = new InsertDataExpression() { SchemaName = expression.SchemaName, TableName = expression.TableName };
            foreach (var row in expression.Rows)
            {
                subExpression.Rows.Clear();
                subExpression.Rows.Add(row);
                InternalProcess(Generator.Generate(subExpression));
            }
        }

        /// <inheritdoc />
        public override void Process(DeleteDataExpression expression)
        {
            Truncator.Truncate(expression);
            CheckTable(expression.TableName);
            var subExpression = new DeleteDataExpression()
            {
                SchemaName = expression.SchemaName,
                TableName = expression.TableName,
                IsAllRows = expression.IsAllRows
            };
            if (expression.IsAllRows)
            {
                InternalProcess(Generator.Generate(expression));
            }
            else
            {
                foreach (var row in expression.Rows)
                {
                    subExpression.Rows.Clear();
                    subExpression.Rows.Add(row);
                    InternalProcess(Generator.Generate(subExpression));
                }
            }
        }

        /// <inheritdoc />
        public override void Process(UpdateDataExpression expression)
        {
            Truncator.Truncate(expression);
            CheckColumn(expression.TableName, expression.Set.Select(x => x.Key));
            InternalProcess(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public override void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        /// <inheritdoc />
        public override void Process(PerformDBOperationExpression expression)
        {
            var message = string.IsNullOrEmpty(expression.Description) 
                ? "Performing DB Operation" 
                : $"Performing DB Operation: {expression.Description}";
            Logger.LogSay(message);

            if (Options.PreviewOnly)
                return;

            EnsureConnectionIsOpen();

            if (expression.Operation != null)
            {
                expression.Operation(Connection, Transaction);

                if (FBOptions.TransactionModel == FirebirdTransactionModel.AutoCommit)
                    CommitRetaining();

            }
        }

        /// <summary>
        /// Executes the provided SQL command against the Firebird database.
        /// </summary>
        /// <param name="sql">The SQL command to execute. If the command is empty or the operation is in preview mode, it will not be executed.</param>
        /// <exception cref="System.Exception">
        /// Thrown when an error occurs during the execution of the SQL command. The exception is rethrown with the SQL command included in the message.
        /// </exception>
        /// <remarks>
        /// If the <see cref="FirebirdOptions.TransactionModel"/> is set to <see cref="FirebirdTransactionModel.AutoCommit"/>, 
        /// the transaction will be committed after the command is executed.
        /// </remarks>
        protected void InternalProcess(string sql)
        {
            Logger.LogSql(sql);

            if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
                return;

            EnsureConnectionIsOpen();

            using (var command = CreateCommand(sql))
            {
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    ReThrowWithSql(ex, sql);
                }
            }

            if (FBOptions.TransactionModel == FirebirdTransactionModel.AutoCommit)
                CommitRetaining();

        }

        /// <inheritdoc />
        protected override void Process(string sql)
        {
            InternalProcess(sql);
        }

        /// <summary>
        /// Formats the provided SQL name into a safe format for use in Firebird SQL queries.
        /// </summary>
        /// <param name="sqlName">The SQL name to format.</param>
        /// <returns>
        /// A safely formatted SQL name. If the name is quoted, it is unquoted and escaped. 
        /// Otherwise, it is escaped and converted to uppercase.
        /// </returns>
        /// <remarks>
        /// This method utilizes the <see cref="FluentMigrator.Runner.Generators.Generic.GenericQuoter.IsQuoted"/> 
        /// and <see cref="FluentMigrator.Runner.Generators.Generic.GenericQuoter.UnQuote"/> methods 
        /// to handle quoted names, and the <see cref="FluentMigrator.Runner.Helpers.FormatHelper.FormatSqlEscape"/> 
        /// method to escape the SQL name.
        /// </remarks>
        private string FormatToSafeName(string sqlName)
        {
            if (_quoter.IsQuoted(sqlName))
                return FormatHelper.FormatSqlEscape(_quoter.UnQuote(sqlName));
            else
                return FormatHelper.FormatSqlEscape(sqlName).ToUpper();
        }

        /// <summary>
        /// Generates the name of a sequence for a specified table and column in the Firebird database.
        /// </summary>
        /// <param name="tableName">The name of the table for which the sequence is being generated.</param>
        /// <param name="columnName">The name of the column for which the sequence is being generated.</param>
        /// <returns>The truncated sequence name in the format <c>gen_{tableName}_{columnName}</c>.</returns>
        /// <remarks>
        /// This method uses the <see cref="FirebirdTruncator"/> to ensure the sequence name adheres to Firebird's naming constraints.
        /// </remarks>
        private string GetSequenceName(string tableName, string columnName)
        {
            return Truncator.Truncate($"gen_{tableName}_{columnName}");
        }

        /// <summary>
        /// Generates the name of the identity trigger for a specified table and column.
        /// </summary>
        /// <param name="tableName">The name of the table for which the identity trigger is being generated.</param>
        /// <param name="columnName">The name of the column for which the identity trigger is being generated.</param>
        /// <returns>The truncated name of the identity trigger.</returns>
        /// <remarks>
        /// The method uses the <see cref="FirebirdTruncator"/> to ensure the generated trigger name
        /// adheres to the database's naming constraints.
        /// </remarks>
        private string GetIdentityTriggerName(string tableName, string columnName)
        {
            return Truncator.Truncate($"gen_id_{tableName}_{columnName}");
        }

        /// <summary>
        /// Creates a sequence and an associated trigger for an identity column in the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table containing the identity column.</param>
        /// <param name="columnName">The name of the identity column for which the sequence and trigger are created.</param>
        /// <remarks>
        /// This method ensures that the table is checked and locked before creating the sequence and trigger.
        /// If the sequence does not already exist, it is created along with a trigger to auto-generate values
        /// for the identity column during insert operations.
        /// </remarks>
        private void CreateSequenceForIdentity(string tableName, string columnName)
        {
            CheckTable(tableName);
            LockTable(tableName);
            string sequenceName = GetSequenceName(tableName, columnName);
            if (!SequenceExists(string.Empty, sequenceName))
            {
                CreateSequenceExpression sequence = new CreateSequenceExpression()
                {
                    Sequence = new SequenceDefinition() { Name = sequenceName }
                };
                Process(sequence);
            }
            string triggerName = GetIdentityTriggerName(tableName, columnName);
            string quotedColumn = _quoter.Quote(columnName);
            string trigger =
                $"as begin if (NEW.{quotedColumn} is NULL) then NEW.{quotedColumn} = GEN_ID({_quoter.QuoteSequenceName(sequenceName, string.Empty)}, 1); end";

            PerformDBOperationExpression createTrigger = CreateTriggerExpression(tableName, triggerName, true, TriggerEvent.Insert, trigger);
            Process(createTrigger);
        }

        /// <summary>
        /// Deletes the sequence associated with an identity column in the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table containing the identity column.</param>
        /// <param name="columnName">The name of the identity column for which the sequence should be deleted.</param>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="number">
        /// <item>Validates the existence of the specified table by calling <see cref="CheckTable"/>.</item>
        /// <item>Locks the table to ensure safe operations by calling <see cref="LockTable"/>.</item>
        /// <item>Attempts to retrieve the sequence name associated with the identity column using <see cref="GetSequenceName"/>.</item>
        /// <item>If the sequence exists, creates a <see cref="DeleteSequenceExpression"/> to delete it.</item>
        /// <item>Generates a trigger deletion operation using <see cref="DeleteTriggerExpression"/> and processes it.</item>
        /// <item>If a sequence deletion expression was created, processes it as well.</item>
        /// </list>
        /// If the sequence name cannot be determined, the method exits without performing any operations.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if the sequence name cannot be determined.</exception>
        private void DeleteSequenceForIdentity(string tableName, string columnName)
        {
            CheckTable(tableName);
            LockTable(tableName);

            string sequenceName;
            try{
                sequenceName = GetSequenceName(tableName, columnName);
            }
            catch (ArgumentException)
            {
                return;
            }

            DeleteSequenceExpression deleteSequence = null;
            if (SequenceExists(string.Empty, sequenceName))
            {
                deleteSequence = new DeleteSequenceExpression() { SchemaName = string.Empty, SequenceName = sequenceName };
            }
            string triggerName = GetIdentityTriggerName(tableName, columnName);
            PerformDBOperationExpression deleteTrigger = DeleteTriggerExpression(tableName, triggerName);
            Process(deleteTrigger);

            if (deleteSequence != null)
                Process(deleteSequence);

        }

        /// <summary>
        /// Creates a <see cref="PerformDBOperationExpression"/> to define a trigger for the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table where the trigger will be created.</param>
        /// <param name="trigger">An instance of <see cref="TriggerInfo"/> containing details about the trigger, such as its name, type, event, and body.</param>
        /// <returns>A <see cref="PerformDBOperationExpression"/> representing the trigger creation operation.</returns>
        /// <remarks>
        /// This method simplifies the creation of a trigger by using the properties of the <see cref="TriggerInfo"/> object.
        /// </remarks>
        public PerformDBOperationExpression CreateTriggerExpression(string tableName, TriggerInfo trigger)
        {
            return CreateTriggerExpression(tableName, trigger.Name, trigger.Before, trigger.Event, trigger.Body);
        }

        /// <summary>
        /// Creates a database trigger for the specified table with the given parameters.
        /// </summary>
        /// <param name="tableName">The name of the table for which the trigger is created.</param>
        /// <param name="triggerName">The name of the trigger to be created.</param>
        /// <param name="onBefore">
        /// A value indicating whether the trigger should be executed before the specified event.
        /// If <c>true</c>, the trigger is executed before the event; otherwise, it is executed after.
        /// </param>
        /// <param name="onEvent">The event that activates the trigger (e.g., <see cref="TriggerEvent.Insert"/>).</param>
        /// <param name="triggerBody">The body of the trigger, containing the SQL logic to execute.</param>
        /// <returns>
        /// A <see cref="PerformDBOperationExpression"/> that represents the operation to create the trigger.
        /// </returns>
        /// <remarks>
        /// This method ensures that the table and trigger names are truncated if necessary, 
        /// and locks the table before creating the trigger. The trigger is created with the specified
        /// event and execution timing.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="tableName"/>, <paramref name="triggerName"/>, or <paramref name="triggerBody"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the trigger creation operation fails.
        /// </exception>
        public PerformDBOperationExpression CreateTriggerExpression(string tableName, string triggerName, bool onBefore, TriggerEvent onEvent, string triggerBody)
        {
            tableName = Truncator.Truncate(tableName);
            triggerName = Truncator.Truncate(triggerName);
            CheckTable(tableName);
            LockTable(tableName);
            PerformDBOperationExpression createTrigger = new PerformDBOperationExpression();
            createTrigger.Operation = (connection, transaction) =>
            {
                string triggerSql = string.Format(@"CREATE TRIGGER {0} FOR {1} ACTIVE {2} {3} POSITION 0
                    {4}
                    ", _quoter.Quote(triggerName), _quoter.Quote(tableName),
                     onBefore ? "before" : "after",
                     onEvent.ToString().ToLower(),
                     triggerBody
                     );
                Logger.LogSql(triggerSql);
                using (var cmd = CreateCommand(triggerSql, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }
            };
            return createTrigger;
        }

        /// <summary>
        /// Creates an expression to delete a trigger from the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table containing the trigger to be deleted.</param>
        /// <param name="triggerName">The name of the trigger to be deleted.</param>
        /// <returns>
        /// A <see cref="PerformDBOperationExpression"/> that represents the operation to delete the specified trigger.
        /// </returns>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="number">
        /// <item>Truncates the table and trigger names using <see cref="FirebirdTruncator.Truncate(string)"/> to ensure they meet database constraints.</item>
        /// <item>Validates the existence of the specified table by calling <see cref="CheckTable"/>.</item>
        /// <item>Locks the table to ensure safe operations by calling <see cref="LockTable"/>.</item>
        /// <item>Generates a SQL command to drop the trigger and logs the SQL statement.</item>
        /// <item>Executes the SQL command within the provided database connection and transaction.</item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="tableName"/> or <paramref name="triggerName"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the trigger cannot be deleted due to database constraints or other issues.
        /// </exception>
        public PerformDBOperationExpression DeleteTriggerExpression(string tableName, string triggerName)
        {
            tableName = Truncator.Truncate(tableName);
            triggerName = Truncator.Truncate(triggerName);
            CheckTable(tableName);
            LockTable(tableName);
            PerformDBOperationExpression deleteTrigger = new PerformDBOperationExpression();
            deleteTrigger.Operation = (connection, transaction) =>
            {
                string triggerSql = string.Format("DROP TRIGGER {0}", _quoter.Quote(triggerName));
                Logger.LogSql(triggerSql);
                using (var cmd = CreateCommand(triggerSql, connection, transaction))
                {
                    cmd.ExecuteNonQuery();
                }
            };
            return deleteTrigger;
        }
    }
}
