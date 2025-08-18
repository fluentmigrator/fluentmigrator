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
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Logging;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors
{
    public class ConnectionlessProcessor: IMigrationProcessor
    {
        [NotNull] private readonly ILogger _logger;

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

        public IMigrationGenerator Generator { get; set; }

        [Obsolete]
        public IAnnouncer Announcer { get; set; }
        public ProcessorOptions Options {get; set;}

        /// <inheritdoc />
        public void Execute(string sql)
        {
            Process(sql);
        }

        public void Execute(string template, params object[] args)
        {
            Process(string.Format(template, args));
        }

        public DataSet ReadTableData(string schemaName, string tableName)
        {
            throw new NotImplementedException($"Method {nameof(ReadTableData)} is not supported by the connectionless processor");
        }

        public DataSet Read(string template, params object[] args)
        {
            throw new NotImplementedException($"Method {nameof(Read)} is not supported by the connectionless processor");
        }

        public bool Exists(string template, params object[] args)
        {
            throw new NotImplementedException($"Method {nameof(Exists)} is not supported by the connectionless processor");
        }

        public void BeginTransaction()
        {

        }

        public void CommitTransaction()
        {

        }

        public void RollbackTransaction()
        {

        }

        protected void Process(string sql)
        {
            _logger.LogSql(sql);
        }

        public void Process(CreateSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(DeleteSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(AlterTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(AlterColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(CreateTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(CreateColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(DeleteTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(DeleteColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(CreateForeignKeyExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(DeleteForeignKeyExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(CreateIndexExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(DeleteIndexExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(RenameTableExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(RenameColumnExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(InsertDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(AlterDefaultConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(PerformDBOperationExpression expression)
        {
            _logger.LogSay("Performing DB Operation");
        }

        public void Process(DeleteDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(UpdateDataExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(AlterSchemaExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(CreateSequenceExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(DeleteSequenceExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(CreateConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(DeleteConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public void Process(DeleteDefaultConstraintExpression expression)
        {
            Process(Generator.Generate(expression));
        }

        public bool SchemaExists(string schemaName)
        {
            throw new NotImplementedException($"Method {nameof(SchemaExists)} is not supported by the connectionless processor");
        }

        public bool TableExists(string schemaName, string tableName)
        {
            throw new NotImplementedException($"Method {nameof(TableExists)} is not supported by the connectionless processor");
        }

        public bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            throw new NotImplementedException($"Method {nameof(ColumnExists)} is not supported by the connectionless processor");
        }

        public bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            throw new NotImplementedException($"Method {nameof(ConstraintExists)} is not supported by the connectionless processor");
        }

        public bool IndexExists(string schemaName, string tableName, string indexName)
        {
            throw new NotImplementedException($"Method {nameof(IndexExists)} is not supported by the connectionless processor");
        }

        public bool SequenceExists(string schemaName, string sequenceName)
        {
            throw new NotImplementedException($"Method {nameof(SequenceExists)} is not supported by the connectionless processor");
        }

        public bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            throw new NotImplementedException($"Method {nameof(DefaultValueExists)} is not supported by the connectionless processor");
        }

#pragma warning disable 618
        public string DatabaseType { get; }
#pragma warning restore 618

        public IList<string> DatabaseTypeAliases { get; } = new List<string>();

        public void Dispose()
        {
        }
    }
}
