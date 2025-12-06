#region License
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors
{
    /// <summary>
    /// An <see cref="IProcessorAccessor"/> implementation that selects one processor by name
    /// </summary>
    public class SelectingProcessorAccessor : IProcessorAccessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectingProcessorAccessor"/> class.
        /// </summary>
        /// <param name="processors">The processors to select from</param>
        /// <param name="options">The options used to determine the processor to be returned</param>
        /// <param name="generatorSelectorOptions">The generator selector options</param>
        /// <param name="serviceProvider">The service provider</param>
        public SelectingProcessorAccessor(
            [NotNull, ItemNotNull] IEnumerable<IMigrationProcessor> processors,
            [NotNull] IOptionsSnapshot<SelectingProcessorAccessorOptions> options,
            [NotNull] IOptionsSnapshot<SelectingGeneratorAccessorOptions> generatorSelectorOptions,
            [NotNull] IServiceProvider serviceProvider)
        {
            var procs = processors.ToList();

            var processorId = string.IsNullOrEmpty(options.Value.ProcessorId)
                ? generatorSelectorOptions.Value.GeneratorId
                : options.Value.ProcessorId;

            IMigrationProcessor foundProcessor;
            if (string.IsNullOrEmpty(processorId))
            {
                // No generator selected
                if (procs.Count == 0)
                {
                    throw new ProcessorFactoryNotFoundException("No migration processor registered.");
                }

                if (procs.Count > 1)
                {
                    throw new ProcessorFactoryNotFoundException("More than one processor registered, but no processor id given. Specify the processor id by configuring SelectingProcessorAccessorOptions.");
                }
                foundProcessor = procs.Single();
            }
            else
            {
                // One of multiple generators
                foundProcessor = FindProcessor(procs, processorId);
            }

            // Special handling when no connection string could be found
            var connectionStringAccessor = serviceProvider.GetRequiredService<IConnectionStringAccessor>();
            var connectionString = connectionStringAccessor.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                if (foundProcessor is ProcessorBase processorBase)
                {
                    var databaseIds = new List<string> { processorBase.DatabaseType };
                    databaseIds.AddRange(processorBase.DatabaseTypeAliases);

                    var processorOptions = serviceProvider.GetRequiredService<IOptionsSnapshot<ProcessorOptions>>();
                    foundProcessor = new ConnectionlessProcessor(
                        new PassThroughGeneratorAccessor(processorBase.Generator),
                        processorBase.Logger,
                        processorOptions,
                        databaseIds);
                }
            }

            Processor = foundProcessor;
        }

        /// <inheritdoc />
        public IMigrationProcessor Processor { get; }

        [NotNull]
        private IMigrationProcessor FindProcessor(
            [NotNull, ItemNotNull] IReadOnlyCollection<IMigrationProcessor> processors,
            [NotNull] string processorId)
        {
            foreach (var processor in processors)
            {
                if (string.Equals(processor.DatabaseType, processorId, StringComparison.OrdinalIgnoreCase))
                    return processor;
            }

            foreach (var processor in processors)
            {
                foreach (var databaseTypeAlias in processor.DatabaseTypeAliases)
                {
                    if (string.Equals(databaseTypeAlias, processorId, StringComparison.OrdinalIgnoreCase))
                        return processor;
                }
            }

            var processorNames = string.Join(", ", processors.Select(p => p.DatabaseType).Union(processors.SelectMany(p => p.DatabaseTypeAliases)));
            throw new ProcessorFactoryNotFoundException($@"A migration processor with the ID {processorId} couldn't be found. Available processors are: {processorNames}");
        }

        private class PassThroughGeneratorAccessor : IGeneratorAccessor
        {
            public PassThroughGeneratorAccessor(IMigrationGenerator generator)
            {
                Generator = generator;
            }

            /// <inheritdoc />
            public IMigrationGenerator Generator { get; }
        }
    }
}
