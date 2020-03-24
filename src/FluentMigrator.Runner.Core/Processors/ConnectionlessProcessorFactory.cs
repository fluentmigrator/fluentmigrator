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

using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Logging;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors
{
    /// <summary>
    /// A processor factory to create SQL statements only (without executing them)
    /// </summary>
    [Obsolete]
    public class ConnectionlessProcessorFactory : IMigrationProcessorFactory
    {
        [NotNull]
        private readonly IMigrationGenerator _generator;

        [NotNull]
        private readonly string _databaseId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionlessProcessorFactory"/> class.
        /// </summary>
        /// <param name="generatorAccessor">The accessor to get the migration generator to use</param>
        /// <param name="runnerContext">The runner context</param>
        [Obsolete]
        public ConnectionlessProcessorFactory(
            [NotNull] IGeneratorAccessor generatorAccessor,
            [NotNull] IRunnerContext runnerContext)
        {
            _generator = generatorAccessor.Generator;
            _databaseId = runnerContext.Database;
            Name = _generator.GetName();
        }

        /// <inheritdoc />
        [Obsolete]
        public IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var processorOptions = options.GetProcessorOptions(connectionString);
            return new ConnectionlessProcessor(
                new PassThroughGeneratorAccessor(_generator),
                new AnnouncerFluentMigratorLogger(announcer),
                new ProcessorOptionsSnapshot(processorOptions),
                new OptionsWrapper<SelectingProcessorAccessorOptions>(
                    new SelectingProcessorAccessorOptions()
                    {
                        ProcessorId = _databaseId,
                    }));
        }

        /// <inheritdoc />
        public bool IsForProvider(string provider) => true;

        /// <inheritdoc />
        public string Name { get; }

        private class PassThroughGeneratorAccessor : IGeneratorAccessor
        {
            public PassThroughGeneratorAccessor(IMigrationGenerator generator)
            {
                Generator = generator;
            }

            /// <inheritdoc />
            public IMigrationGenerator Generator { get; }
        }

        private class ProcessorOptionsSnapshot : IOptionsSnapshot<ProcessorOptions>
        {
            public ProcessorOptionsSnapshot(ProcessorOptions options)
            {
                Value = options;
            }

            public ProcessorOptions Value { get; }

            public ProcessorOptions Get(string name)
            {
                return Value;
            }
        }
    }
}
