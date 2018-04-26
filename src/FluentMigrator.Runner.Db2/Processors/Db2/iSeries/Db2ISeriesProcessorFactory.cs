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

using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.DB2.iSeries;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.DB2.iSeries
{
    [Obsolete]
    public class Db2ISeriesProcessorFactory : MigrationProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        [Obsolete]
        public Db2ISeriesProcessorFactory()
        {
        }

        public Db2ISeriesProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [Obsolete]
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new Db2ISeriesDbFactory(_serviceProvider);
            var quoter = new Db2ISeriesQuoter();
            var connection = factory.CreateConnection(connectionString);
            var generatorOptions = new OptionsWrapper<GeneratorOptions>(new GeneratorOptions());
            return new Db2ISeriesProcessor(connection, new Db2ISeriesGenerator(quoter, generatorOptions), announcer, options, factory);
        }
    }
}
