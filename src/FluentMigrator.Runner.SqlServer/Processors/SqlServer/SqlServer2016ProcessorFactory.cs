#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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

using FluentMigrator.Runner.Generators.SqlServer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    public class SqlServer2016ProcessorFactory : MigrationProcessorFactory
    {
        private static readonly string[] _dbTypes = {"SqlServer2016", "SqlServer"};
        private readonly IServiceProvider _serviceProvider;

        [Obsolete]
        public SqlServer2016ProcessorFactory()
            : this(serviceProvider: null)
        {
        }

        public SqlServer2016ProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [Obsolete]
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new SqlServerDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new SqlServerProcessor(_dbTypes, connection, new SqlServer2016Generator(), announcer, options, factory);
        }

        /// <inheritdoc />
        public override IMigrationProcessor Create()
        {
            if (_serviceProvider == null)
                return null;
            var factory = new SqlServerDbFactory().Factory;
            var options = _serviceProvider.GetRequiredService<IOptions<ProcessorOptions>>();
            var announcer = _serviceProvider.GetRequiredService<IAnnouncer>();
            var generator = new SqlServer2016Generator();
            return new SqlServerProcessor(_dbTypes, factory, generator, announcer, options);
        }
    }
}
