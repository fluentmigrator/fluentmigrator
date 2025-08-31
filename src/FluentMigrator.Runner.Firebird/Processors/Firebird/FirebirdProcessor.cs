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
        /// <inheritdoc />
        [Obsolete("Use the Truncator property")]
        protected readonly FirebirdTruncator truncator;

        private readonly Lazy<Version> _firebirdVersionFunc;
        private readonly FirebirdQuoter _quoter;

        protected List<string> DDLCreatedTables;
        protected Dictionary<string, List<string>> DDLCreatedColumns;
        protected List<string> DDLTouchedTables;
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

        /// <inheritdoc />
        public FirebirdOptions FBOptions { get; }
        /// <inheritdoc />
        public bool IsFirebird3 => _firebirdVersionFunc.Value >= new Version(3, 0);
        /// <inheritdoc />
        public new IMigrationGenerator Generator => base.Generator;

        /// <inheritdoc />
        [Obsolete]
        public new IAnnouncer Announcer => base.Announcer;

#pragma warning disable 618
        /// <inheritdoc />
        public FirebirdTruncator Truncator => truncator;
#pragma warning restore 618

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
                // Fehler ignorieren - Älter als Version 2.1
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public virtual void AutoCommit()
        {
            if (FBOptions.TransactionModel == FirebirdTransactionModel.AutoCommit)
                CommitRetaining();
        }

        /// <inheritdoc />
        public bool IsRunningOutOfMigrationScope()
        {
            return Transaction == null;
        }

        protected void ClearDDLFollowers()
        {
            DDLCreatedTables = new List<string>();
            DDLCreatedColumns = new Dictionary<string, List<string>>();
        }

        protected void RegisterTableCreation(string tableName)
        {
            if (!DDLCreatedTables.Contains(tableName))
                DDLCreatedTables.Add(tableName);
        }

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

        protected bool IsTableCreated(string tableName)
        {
            return DDLCreatedTables.Contains(tableName);
        }

        protected bool IsColumnCreated(string tableName, string columnName)
        {
            return DDLCreatedColumns.ContainsKey(tableName) && DDLCreatedColumns[tableName].Contains(columnName);
        }

        protected void ClearLocks()
        {
            DDLTouchedTables = new List<string>();
            DDLTouchedColumns = new Dictionary<string, List<string>>();
        }

        /// <inheritdoc />
        public void LockTable(string tableName)
        {
            if (!DDLTouchedTables.Contains(tableName))
                DDLTouchedTables.Add(tableName);
        }

        /// <inheritdoc />
        public void LockColumn(string tableName, IEnumerable<string> columns)
        {
            columns.ToList().ForEach(x => LockColumn(tableName, x));
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
                    throw new InvalidOperationException(string.Format("Table {0} is locked", tableName));
            }
        }

        /// <inheritdoc />
        public void CheckColumn(string tableName, IEnumerable<string> columns)
        {
            columns.ToList().ForEach(x => CheckColumn(tableName, x));
        }

        /// <inheritdoc />
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
                CustomType = x.CustomType
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
            Logger.LogSay("Performing DB Operation");

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

        private string FormatToSafeName(string sqlName)
        {
            if (_quoter.IsQuoted(sqlName))
                return FormatHelper.FormatSqlEscape(_quoter.UnQuote(sqlName));
            else
                return FormatHelper.FormatSqlEscape(sqlName).ToUpper();
        }

        private string GetSequenceName(string tableName, string columnName)
        {
            return Truncator.Truncate($"gen_{tableName}_{columnName}");
        }

        private string GetIdentityTriggerName(string tableName, string columnName)
        {
            return Truncator.Truncate($"gen_id_{tableName}_{columnName}");
        }

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
            string trigger = string.Format("as begin if (NEW.{0} is NULL) then NEW.{1} = GEN_ID({2}, 1); end", quotedColumn, quotedColumn, _quoter.QuoteSequenceName(sequenceName, string.Empty));

            PerformDBOperationExpression createTrigger = CreateTriggerExpression(tableName, triggerName, true, TriggerEvent.Insert, trigger);
            Process(createTrigger);
        }

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

        /// <inheritdoc />
        public PerformDBOperationExpression CreateTriggerExpression(string tableName, TriggerInfo trigger)
        {
            return CreateTriggerExpression(tableName, trigger.Name, trigger.Before, trigger.Event, trigger.Body);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
