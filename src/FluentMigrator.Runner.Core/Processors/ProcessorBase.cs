#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

using FluentMigrator.Expressions;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FluentMigrator.Runner.Processors
{
    /// <summary>
    /// Minimalist base class for a processor.
    /// </summary>
    public abstract class ProcessorBase : IMigrationProcessor
    {
        protected internal readonly IMigrationGenerator Generator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorBase"/> class.
        /// </summary>
        /// <param name="generator">The migration generator.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The options.</param>
        protected ProcessorBase(
            [NotNull] IMigrationGenerator generator,
            [NotNull] ILogger logger,
            [NotNull] ProcessorOptions options)
        {
            Generator = generator;
            Options = options;
            Logger = logger;
        }

        /// <summary>
        /// Gets the default database type identifier.
        /// </summary>
        public abstract string DatabaseType { get; }

        /// <summary>
        /// Gets the database type aliases.
        /// </summary>
        public abstract IList<string> DatabaseTypeAliases { get; }

        /// <summary>
        /// Gets or sets a value indicating whether a transaction was committed or rolled back.
        /// </summary>
        public bool WasCommitted { get; protected set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected internal ILogger Logger { get; }

        /// <summary>
        /// Gets the processor options.
        /// </summary>
        [NotNull]
        protected ProcessorOptions Options { get; }

        public virtual void Process(CreateSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(DeleteSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(CreateTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(AlterTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(AlterColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(CreateColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(DeleteTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(DeleteColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(CreateForeignKeyExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(DeleteForeignKeyExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(CreateIndexExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(DeleteIndexExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(RenameTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(RenameColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(InsertDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(DeleteDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(AlterDefaultConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(UpdateDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public abstract void Process(PerformDBOperationExpression expression);

        public virtual void Process(AlterSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(CreateSequenceExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(DeleteSequenceExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(CreateConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(DeleteConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public virtual void Process(DeleteDefaultConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        protected abstract void Process(string sql);

        public virtual void BeginTransaction()
        {
        }

        public virtual void CommitTransaction()
        {
        }

        public virtual void RollbackTransaction()
        {
        }

        public abstract DataSet ReadTableData(string schemaName, string tableName);

        public abstract DataSet Read(string template, params object[] args);

        public abstract bool Exists(string template, params object[] args);

        /// <inheritdoc />
        public virtual void Execute(string sql)
        {
            Execute(sql.Replace("{", "{{").Replace("}", "}}"), Array.Empty<object>());
        }

        public abstract void Execute(string template, params object[] args);

        public abstract bool SchemaExists(string schemaName);

        public abstract bool TableExists(string schemaName, string tableName);

        public abstract bool ColumnExists(string schemaName, string tableName, string columnName);

        public abstract bool ConstraintExists(string schemaName, string tableName, string constraintName);

        public abstract bool IndexExists(string schemaName, string tableName, string indexName);

        public abstract bool SequenceExists(string schemaName, string sequenceName);

        public abstract bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue);

        public void Dispose()
        {
            Dispose(true);
        }

        protected abstract void Dispose(bool isDisposing);
    }
}
