#region License
// Copyright (c) 2018, FluentMigrator Project
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
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner.Generators
{
    /// <summary>
    /// The default <see cref="IMigrationGeneratorAccessor"/> implementation
    /// </summary>
    public class DefaultMigrationGeneratorAccessor : IMigrationGeneratorAccessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMigrationGeneratorAccessor"/> class.
        /// </summary>
        /// <param name="generators">The list of available generators</param>
        /// <param name="context">The runner context</param>
        public DefaultMigrationGeneratorAccessor(IEnumerable<IMigrationGenerator> generators, IRunnerContext context)
        {
            var generatorsList = generators.ToList();
            if (generatorsList.Count == 1)
            {
                ActiveGenerator = generatorsList[0];
            }
            else
            {
                var generator = generatorsList
                    .FirstOrDefault(f => string.Equals(f.GetName(), context.Database, StringComparison.OrdinalIgnoreCase));
                if (generator == null)
                {
                    var choices = string.Join(", ", generatorsList.Select(x => x.GetName()));
                    throw new ProcessorFactoryNotFoundException(
                        $"The provider or dbtype parameter is incorrect. Available choices are: {choices}");
                }

                ActiveGenerator = generator;
            }
        }

        /// <inheritdoc />
        public IMigrationGenerator ActiveGenerator { get; }
    }
}
