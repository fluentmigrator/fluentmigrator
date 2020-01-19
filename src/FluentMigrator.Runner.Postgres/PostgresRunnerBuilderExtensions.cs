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

using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Generators.Postgres92;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.Postgres92;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IMigrationRunnerBuilder"/>
    /// </summary>
    public static class PostgresRunnerBuilderExtensions
    {
        /// <summary>
        /// Adds Postgres support
        /// </summary>
        /// <param name="builder">The builder to add the Postgres-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddPostgres(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<PostgresProcessor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<PostgresProcessor>())
                .AddScoped<PostgresGenerator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<PostgresGenerator>());
            return builder.AddCommonPostgresServices();
        }

        /// <summary>
        /// Adds Postgres 9.2 support
        /// </summary>
        /// <param name="builder">The builder to add the Postgres-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddPostgres92(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<Postgres92Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<Postgres92Processor>())
                .AddScoped<Postgres92Generator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<Postgres92Generator>());
            return builder.AddCommonPostgresServices();
        }


        /// <summary>
        /// Adds Postgres 10.0 support
        /// </summary>
        /// <param name="builder">The builder to add the Postgres-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddPostgres100(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<Postgres100Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<Postgres100Processor>())
                .AddScoped<Postgres100Generator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<Postgres100Generator>());
            return builder.AddCommonPostgresServices();
        }

        /// <summary>
        /// Add common Postgres services.
        /// </summary>
        /// <param name="builder">The builder to add the Postgres-specific services to</param>
        /// <returns>The migration runner builder</returns>
        private static IMigrationRunnerBuilder AddCommonPostgresServices(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped(
                    sp =>
                    {
                        var processorOptions = sp.GetRequiredService<IOptionsSnapshot<ProcessorOptions>>();
                        return PostgresOptions.ParseProviderSwitches(processorOptions.Value.ProviderSwitches);
                    })
                .AddScoped<PostgresDbFactory>()
                .AddScoped<PostgresQuoter>();
            return builder;
        }
    }
}
