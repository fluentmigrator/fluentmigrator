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
using System.Collections.Generic;
using System.Data;
using System.IO;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Logging;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FluentMigrator.Runner.Processors
{
    /// <summary>
    /// Base class for migration processors.
    /// </summary>
    public abstract class ProcessorBase : IMigrationProcessor
    {
        /// <inheritdoc />
        protected internal readonly IMigrationGenerator Generator;

        /// <inheritdoc />
#pragma warning disable 612
        [Obsolete]
        protected readonly IAnnouncer Announcer;
#pragma warning restore 612

        /// <inheritdoc />
        [Obsolete]
        protected ProcessorBase(
            IMigrationGenerator generator,
            IAnnouncer announcer,
            [NotNull] IMigrationProcessorOptions options)
        {
            Generator = generator;
            Announcer = announcer;
            Logger = new AnnouncerFluentMigratorLogger(announcer);
            Options = options as ProcessorOptions ?? new ProcessorOptions()
            {
                PreviewOnly = options.PreviewOnly,
                ProviderSwitches = options.ProviderSwitches,
                Timeout = options.Timeout == null ? null : (TimeSpan?) TimeSpan.FromSeconds(options.Timeout.Value),
            };
        }

        /// <inheritdoc />
        [Obsolete]
        protected ProcessorBase(
            [NotNull] IMigrationGenerator generator,
            [NotNull] IAnnouncer announcer,
            [NotNull] ProcessorOptions options)
        {
            Generator = generator;
            Announcer = announcer;
            Options = options;
            Logger = new AnnouncerFluentMigratorLogger(announcer);
        }

        /// <inheritdoc />
        protected ProcessorBase(
            [NotNull] IMigrationGenerator generator,
            [NotNull] ILogger logger,
            [NotNull] ProcessorOptions options)
        {
            Generator = generator;
            Options = options;
            Logger = logger;
#pragma warning disable 612
            Announcer = new LoggerAnnouncer(
                logger,
                new AnnouncerOptions()
                {
                    ShowSql = true,
                    ShowElapsedTime = true,
                });
#pragma warning restore 612
        }

        /// <inheritdoc />
        public abstract string DatabaseType { get; }

        /// <inheritdoc />
        public abstract IList<string> DatabaseTypeAliases { get; }

        /// <inheritdoc />
        public bool WasCommitted { get; protected set; }

        /// <inheritdoc />
        protected internal ILogger Logger { get; }

        /// <inheritdoc />
        [NotNull]
        protected ProcessorOptions Options { get; }

        /// <inheritdoc />
        public virtual void Process(CreateSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(DeleteSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(CreateTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(AlterTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(AlterColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(CreateColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(DeleteTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(DeleteColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(CreateForeignKeyExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(DeleteForeignKeyExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(CreateIndexExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(DeleteIndexExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(RenameTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(RenameColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(InsertDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(DeleteDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(AlterDefaultConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(UpdateDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public abstract void Process(PerformDBOperationExpression expression);

        /// <inheritdoc />
        public virtual void Process(AlterSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(CreateSequenceExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(DeleteSequenceExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(CreateConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(DeleteConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public virtual void Process(DeleteDefaultConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        protected abstract void Process(string sql);

        /// <inheritdoc />
        public virtual void BeginTransaction()
        {
        }

        /// <inheritdoc />
        public virtual void CommitTransaction()
        {
        }

        /// <inheritdoc />
        public virtual void RollbackTransaction()
        {
        }

        /// <inheritdoc />
        public abstract DataSet ReadTableData(string schemaName, string tableName);

        /// <inheritdoc />
        public abstract DataSet Read(string template, params object[] args);

        /// <inheritdoc />
        public abstract bool Exists(string template, params object[] args);

        /// <inheritdoc />
        public virtual void Execute(string sql)
        {
            Execute(sql.Replace("{", "{{").Replace("}", "}}"), Array.Empty<object>());
        }

        /// <inheritdoc />
        public abstract void Execute(string template, params object[] args);

        /// <inheritdoc />
        public abstract bool SchemaExists(string schemaName);

        /// <inheritdoc />
        public abstract bool TableExists(string schemaName, string tableName);

        /// <inheritdoc />
        public abstract bool ColumnExists(string schemaName, string tableName, string columnName);

        /// <inheritdoc />
        public abstract bool ConstraintExists(string schemaName, string tableName, string constraintName);

        /// <inheritdoc />
        public abstract bool IndexExists(string schemaName, string tableName, string indexName);

        /// <inheritdoc />
        public abstract bool SequenceExists(string schemaName, string sequenceName);

        /// <inheritdoc />
        public abstract bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue);

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        protected abstract void Dispose(bool isDisposing);

        /// <inheritdoc />
        protected virtual void ReThrowWithSql(Exception ex, string sql)
        {
            using (var message = new StringWriter())
            {
                message.WriteLine("An error occurred executing the following sql:");
                message.WriteLine(sql);
                message.WriteLine("The error was {0}", ex.Message);

                throw new Exception(message.ToString(), ex);
            }
        }
    }
}
