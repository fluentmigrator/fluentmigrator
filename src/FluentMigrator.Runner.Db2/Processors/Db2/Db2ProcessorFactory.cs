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

using FluentMigrator.Runner.Generators.DB2;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.DB2
{
    public class Db2ProcessorFactory : MigrationProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        [Obsolete]
        public Db2ProcessorFactory()
        {
        }

        public Db2ProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [Obsolete]
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new Db2DbFactory(_serviceProvider);
            var connection = factory.CreateConnection(connectionString);
            return new Db2Processor(connection, new Db2Generator(new Db2Quoter()), announcer, options, factory);
        }

        /// <inheritdoc />
        public override IMigrationProcessor Create()
        {
            var factory = new Db2DbFactory(_serviceProvider).Factory;
            var options = _serviceProvider.GetRequiredService<IOptions<ProcessorOptions>>();
            var announcer = _serviceProvider.GetRequiredService<IAnnouncer>();
            var generator = new Db2Generator(new Db2Quoter());
            return new Db2Processor(factory, generator, announcer, options);
        }
    }
}
