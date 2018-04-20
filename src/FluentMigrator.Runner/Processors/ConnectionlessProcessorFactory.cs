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

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors
{
    /// <summary>
    /// A processor factory to create SQL statements only (without executing them)
    /// </summary>
    public class ConnectionlessProcessorFactory : IMigrationProcessorFactory
    {
        [NotNull]
        private readonly IMigrationGenerator _generator;

        [NotNull]
        private readonly string _databaseId;

        [NotNull]
        private readonly ProcessorOptions _options;

        [NotNull]
        private readonly IAnnouncer _announcer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionlessProcessorFactory"/> class.
        /// </summary>
        /// <param name="generatorAccessor">The accessor to get the migration generator to use</param>
        /// <param name="runnerContext">The runner context</param>
        [Obsolete]
        public ConnectionlessProcessorFactory(
            [NotNull] IMigrationGeneratorAccessor generatorAccessor,
            [NotNull] IRunnerContext runnerContext)
        {
            _generator = generatorAccessor.ActiveGenerator;
            _databaseId = runnerContext.Database;
            _announcer = runnerContext.Announcer;
            _options = new ProcessorOptions(runnerContext);
            Name = _generator.GetName();
        }

        public ConnectionlessProcessorFactory(
            [NotNull] IMigrationGeneratorAccessor generatorAccessor,
            [NotNull] IOptions<ProcessorOptions> options,
            [NotNull] IAnnouncer announcer)
        {
            _generator = generatorAccessor.ActiveGenerator;
            _options = options.Value;
            _announcer = announcer;
            Name = _databaseId = _generator.GetName();
        }

        /// <inheritdoc />
        [Obsolete]
        public IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var processorOptions = options.GetProcessorOptions(connectionString);
            return new ConnectionlessProcessor(_databaseId, _generator, _announcer, processorOptions);
        }

        /// <inheritdoc />
        public IMigrationProcessor Create()
        {
            return new ConnectionlessProcessor(_databaseId, _generator, _announcer, _options);
        }

        /// <inheritdoc />
        public bool IsForProvider(string provider) => true;

        /// <inheritdoc />
        public string Name { get; }
    }
}
