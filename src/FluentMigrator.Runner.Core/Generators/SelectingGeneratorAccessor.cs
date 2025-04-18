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

using FluentMigrator.Runner.Processors;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators
{
    /// <summary>
    /// An <see cref="IGeneratorAccessor"/> implementation that selects one generator by name
    /// </summary>
    public class SelectingGeneratorAccessor : IGeneratorAccessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectingGeneratorAccessor"/> class.
        /// </summary>
        /// <param name="generators">The generators to select from</param>
        /// <param name="options">The options used to determine the generator to be returned</param>
        /// <param name="processorSelectorOptions">The processor selector options</param>
        public SelectingGeneratorAccessor(
            [NotNull, ItemNotNull] IEnumerable<IMigrationGenerator> generators,
            [NotNull] IOptionsSnapshot<SelectingGeneratorAccessorOptions> options,
            [NotNull] IOptionsSnapshot<SelectingProcessorAccessorOptions> processorSelectorOptions)
        {
            var gens = generators.ToList();

            var generatorId = string.IsNullOrEmpty(options.Value.GeneratorId)
                ? processorSelectorOptions.Value.ProcessorId
                : options.Value.GeneratorId;

            if (string.IsNullOrEmpty(generatorId))
            {
                // No generator selected
                if (gens.Count == 0)
                    throw new InvalidOperationException("No migration generator registered.");
                if (gens.Count > 1)
                    throw new InvalidOperationException("More than one generator registered, but no generator id given. Specify the generator id by configuring SelectingGeneratorAccessorOptions.");
                Generator = gens.Single();
            }
            else
            {
                // One of multiple generators
                Generator = FindGenerator(gens, generatorId);
            }
        }

        /// <inheritdoc />
        public IMigrationGenerator Generator { get; }

        [NotNull]
        private IMigrationGenerator FindGenerator(
            [NotNull, ItemNotNull] IReadOnlyCollection<IMigrationGenerator> generators,
            [NotNull] string generatorId)
        {
            foreach (var generator in generators)
            {
                if (string.Equals(generator.GeneratorId, generatorId, StringComparison.OrdinalIgnoreCase))
                    return generator;
                if (generator.GeneratorIdAliases.Any(g => string.Equals(g, generatorId, StringComparison.OrdinalIgnoreCase)))
                    return generator;
                if (string.Equals(GetName(generator), generatorId, StringComparison.OrdinalIgnoreCase))
                    return generator;
            }

            var generatorNames = string.Join(", ", generators.Select(GetName));
            throw new InvalidOperationException($@"A migration generator with the ID {generatorId} couldn't be found. Available generators are: {generatorNames}");
        }

        /// <summary>
        /// Gets the name for a given migration generator instance
        /// </summary>
        /// <param name="generator">The migration generator instance to get its name for</param>
        /// <returns>The name of the migration generator</returns>
        [NotNull]
        private static string GetName([NotNull] IMigrationGenerator generator)
        {
            return generator.GetType().Name.Replace("Generator", string.Empty);
        }
    }
}
