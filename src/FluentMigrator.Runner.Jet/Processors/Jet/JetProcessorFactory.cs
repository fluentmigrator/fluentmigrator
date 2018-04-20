#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Data.OleDb;

using FluentMigrator.Runner.Generators.Jet;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.Jet
{
    public class JetProcessorFactory : MigrationProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        [Obsolete]
        public JetProcessorFactory()
        {
        }

        public JetProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [Obsolete]
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var connection = new OleDbConnection(connectionString);
            return new JetProcessor(connection, new JetGenerator(), announcer, options);
        }

        /// <inheritdoc />
        public override IMigrationProcessor Create()
        {
            if (_serviceProvider == null)
                return null;
            var factory = OleDbFactory.Instance;
            var options = _serviceProvider.GetRequiredService<IOptions<ProcessorOptions>>();
            var announcer = _serviceProvider.GetRequiredService<IAnnouncer>();
            var generator = new JetGenerator();
            return new JetProcessor(factory, generator, announcer, options);
        }
    }
}
