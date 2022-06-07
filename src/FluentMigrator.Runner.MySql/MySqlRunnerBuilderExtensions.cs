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

using FluentMigrator.Runner.Generators.MySql;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.MySql;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IMigrationRunnerBuilder"/>
    /// </summary>
    public static class MySqlRunnerBuilderExtensions
    {
        /// <summary>
        /// Adds MySQL 4 support
        /// </summary>
        /// <param name="builder">The builder to add the MySQL 4-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddMySql4(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddScoped<MySqlDbFactory>();
            builder.Services.TryAddScoped<MySqlQuoter>();
            builder.Services
                .AddScoped<MySql4Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<MySql4Processor>())
                .AddScoped<MySql4Generator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<MySql4Generator>());

            MigrationProcessorFactoryProvider.Register(new MySql4ProcessorFactory());

            return builder;
        }

        /// <summary>
        /// Adds MySQL 5 support
        /// </summary>
        /// <param name="builder">The builder to add the MySQL 5-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddMySql5(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddScoped<MySqlDbFactory>();
            builder.Services.TryAddScoped<MySqlQuoter>();
            builder.Services
                .AddScoped<MySql5Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<MySql5Processor>())
                .AddScoped<MySql5Generator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<MySql5Generator>());

            MigrationProcessorFactoryProvider.Register(new MySql5ProcessorFactory());

            return builder;
        }
    }
}
