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
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors
{
    /// <summary>
    /// A migration processor that does not require a database connection.
    /// </summary>
    public class ConnectionlessProcessor: IMigrationProcessor
    {
        [NotNull] private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Processors.ConnectionlessProcessor"/> class.
        /// </summary>
        /// <param name="generatorAccessor">
        /// The accessor providing the migration generator used for generating SQL statements.
        /// </param>
        /// <param name="logger">
        /// The logger used for logging messages and diagnostics.
        /// </param>
        /// <param name="options">
        /// The options snapshot containing configuration for the processor.
        /// </param>
        /// <param name="accessorOptions">
        /// The options specifying the processor accessor configuration, including the processor ID.
        /// </param>
        /// <remarks>
        /// This constructor initializes the processor with the provided generator, logger, and configuration options.
        /// </remarks>
        public ConnectionlessProcessor(
            [NotNull] IGeneratorAccessor generatorAccessor,
            [NotNull] ILogger logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IOptions<SelectingProcessorAccessorOptions> accessorOptions)
        {
            _logger = logger;
            var generator = generatorAccessor.Generator;
            DatabaseType = string.IsNullOrEmpty(accessorOptions.Value.ProcessorId) ? generator.GetName(_logger) : accessorOptions.Value.ProcessorId;
            Generator = generator;
            Options = options.Value;
#pragma warning disable 612
            Announcer = new LoggerAnnouncer(logger, new AnnouncerOptions() { ShowElapsedTime = true, ShowSql = true });
#pragma warning restore 612
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.Processors.ConnectionlessProcessor"/> class.
        /// </summary>
        /// <param name="generatorAccessor">
        /// An accessor to retrieve the migration generator used to generate SQL statements.
        /// </param>
        /// <param name="logger">
        /// The logger instance used for logging migration-related information.
        /// </param>
        /// <param name="options">
        /// The processor options that configure the behavior of the processor.
        /// </param>
        /// <param name="processorIds">
        /// A collection of processor identifiers, where the first identifier represents the database type,
        /// and subsequent identifiers represent aliases for the database type.
        /// </param>
        /// <remarks>
        /// This constructor initializes the processor with the provided generator, logger, options, and processor identifiers.
        /// It also sets up the database type and its aliases based on the provided processor IDs.
        /// </remarks>
        public ConnectionlessProcessor(
            [NotNull] IGeneratorAccessor generatorAccessor,
            [NotNull] ILogger logger,
            [NotNull] IOptionsSnapshot<ProcessorOptions> options,
            [NotNull] IReadOnlyCollection<string> processorIds)
        {
            _logger = logger;
            var generator = generatorAccessor.Generator;
            DatabaseType = processorIds.FirstOrDefault() ?? generator.GetName(_logger);
            DatabaseTypeAliases = processorIds.Count == 0 ? Array.Empty<string>() : processorIds.Skip(1).ToArray();
            Generator = generator;
            Options = options.Value;
#pragma warning disable 612
            Announcer = new LoggerAnnouncer(logger, AnnouncerOptions.AllEnabled);
#pragma warning restore 612
        }

        /// <summary>
        /// Gets or sets the migration generator used to generate SQL statements
        /// for database migration expressions.
        /// </summary>
        /// <value>
        /// An instance of <see cref="IMigrationGenerator"/> that provides methods
        /// to generate SQL for various migration expressions.
        /// </value>
        public IMigrationGenerator Generator { get; set; }

        /// <summary>
        /// Gets or sets the announcer used for logging migration-related messages.
        /// </summary>
        /// <remarks>
        /// This property is marked as <see cref="ObsoleteAttribute"/> and should not be used in new code.
        /// </remarks>
        [Obsolete]
        private IAnnouncer Announcer { get; set; }

        /// <summary>
        /// Gets or sets the processor options that define the behavior and configuration
        /// of the migration processor.
        /// </summary>
        /// <value>
        /// An instance of <see cref="ProcessorOptions"/> containing the processor's settings.
        /// </value>
        private ProcessorOptions Options {get; set;}

        /// <inheritdoc />
        public void Execute(string sql)
        {
            Process(sql);
        }

        /// <inheritdoc />
        public void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        /// <inheritdoc />
        public DataSet ReadTableData(string schemaName, string tableName)
        {
            throw new NotImplementedException($"Method {nameof(ReadTableData)} is not supported by the connectionless processor");
        }

        /// <inheritdoc />
        public DataSet Read(string template, params object[] args)
        {
            throw new NotImplementedException($"Method {nameof(Read)} is not supported by the connectionless processor");
        }

        /// <inheritdoc />
        public bool Exists(string template, params object[] args)
        {
            throw new NotImplementedException($"Method {nameof(Exists)} is not supported by the connectionless processor");
        }

        /// <inheritdoc />
        public void BeginTransaction()
        {

        }

        /// <inheritdoc />
        public void CommitTransaction()
        {

        }

        /// <inheritdoc />
        public void RollbackTransaction()
        {

        }

        /// <summary>
        /// Executes the specified SQL statement using the connectionless processor.
        /// </summary>
        /// <param name="sql">The SQL statement to be executed.</param>
        /// <remarks>
        /// This method logs the provided SQL statement using the associated <see cref="ILogger"/> instance.
        /// </remarks>
        protected void Process(string sql)
        {
            _logger.LogSql(sql);
        }

        /// <inheritdoc />
        public void Process(CreateSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(DeleteSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(AlterTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(AlterColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(CreateTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(CreateColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(DeleteTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(DeleteColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(CreateForeignKeyExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(DeleteForeignKeyExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(CreateIndexExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(DeleteIndexExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(RenameTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(RenameColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(InsertDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(AlterDefaultConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(PerformDBOperationExpression expression)
        {
            _logger.LogSay("Performing DB Operation");
        }

        /// <inheritdoc />
        public void Process(DeleteDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(UpdateDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(AlterSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(CreateSequenceExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(DeleteSequenceExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(CreateConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(DeleteConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public void Process(DeleteDefaultConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        /// <inheritdoc />
        public bool SchemaExists(string schemaName)
        {
            throw new NotImplementedException($"Method {nameof(SchemaExists)} is not supported by the connectionless processor");
        }

        /// <inheritdoc />
        public bool TableExists(string schemaName, string tableName)
        {
            throw new NotImplementedException($"Method {nameof(TableExists)} is not supported by the connectionless processor");
        }

        /// <inheritdoc />
        public bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            throw new NotImplementedException($"Method {nameof(ColumnExists)} is not supported by the connectionless processor");
        }

        /// <inheritdoc />
        public bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            throw new NotImplementedException($"Method {nameof(ConstraintExists)} is not supported by the connectionless processor");
        }

        /// <inheritdoc />
        public bool IndexExists(string schemaName, string tableName, string indexName)
        {
            throw new NotImplementedException($"Method {nameof(IndexExists)} is not supported by the connectionless processor");
        }

        /// <inheritdoc />
        public bool SequenceExists(string schemaName, string sequenceName)
        {
            throw new NotImplementedException($"Method {nameof(SequenceExists)} is not supported by the connectionless processor");
        }

        /// <inheritdoc />
        public bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            throw new NotImplementedException($"Method {nameof(DefaultValueExists)} is not supported by the connectionless processor");
        }

#pragma warning disable 618
        /// <inheritdoc />
        public string DatabaseType { get; }
#pragma warning restore 618

        /// <inheritdoc />
        public IList<string> DatabaseTypeAliases { get; } = new List<string>();

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
