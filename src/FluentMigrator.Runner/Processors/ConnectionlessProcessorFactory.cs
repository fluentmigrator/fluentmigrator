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

using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

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
        private readonly IRunnerContext _runnerContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionlessProcessorFactory"/> class.
        /// </summary>
        /// <param name="generatorAccessor">The accessor to get the migration generator to use</param>
        /// <param name="runnerContext">The runner context</param>
        public ConnectionlessProcessorFactory([NotNull] IMigrationGeneratorAccessor generatorAccessor, [NotNull] IRunnerContext runnerContext)
        {
            _generator = generatorAccessor.ActiveGenerator;
            _runnerContext = runnerContext;
            Name = _generator.GetName();
        }

        /// <inheritdoc />
        public IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            return new ConnectionlessProcessor(_generator, _runnerContext, options);
        }

        /// <inheritdoc />
        public bool IsForProvider(string provider) => true;

        /// <inheritdoc />
        public string Name { get; }
    }
}
