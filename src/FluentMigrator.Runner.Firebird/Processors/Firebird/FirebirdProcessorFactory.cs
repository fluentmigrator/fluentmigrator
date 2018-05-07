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

using FluentMigrator.Runner.Generators.Firebird;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Processors.Firebird
{
    [Obsolete]
    public class FirebirdProcessorFactory : MigrationProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        [Obsolete]
        public FirebirdProcessorFactory() : this(FirebirdOptions.AutoCommitBehaviour()) { }

        [Obsolete]
        public FirebirdProcessorFactory(FirebirdOptions fbOptions)
            : this(serviceProvider: null, new OptionsWrapper<FirebirdOptions>(fbOptions))
        {
        }

        public FirebirdProcessorFactory(IServiceProvider serviceProvider, IOptions<FirebirdOptions> fbOptions = null)
        {
            _serviceProvider = serviceProvider;
            FbOptions = fbOptions?.Value ?? FirebirdOptions.AutoCommitBehaviour();
        }

        public FirebirdOptions FbOptions { get; set; }

        [Obsolete]
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var fbOpt = ((FirebirdOptions) FbOptions.Clone())
                .ApplyProviderSwitches(options.ProviderSwitches);
            var factory = new FirebirdDbFactory(_serviceProvider);
            var connection = factory.CreateConnection(connectionString);
            return new FirebirdProcessor(connection, new FirebirdGenerator(FbOptions), announcer, options, factory, fbOpt);
        }
    }
}
