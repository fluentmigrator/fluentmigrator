#region License
// Copyright (c) 2018, Fluent Migrator Project
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
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Generators.Postgres92;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.Postgres92;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            builder.Services.TryAddScoped<IPostgresTypeMap, PostgresTypeMap>();
            builder.Services
                .AddScoped<PostgresProcessor>()
                .AddScoped<Postgres15_0Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<Postgres15_0Processor>())
                .AddScoped(sp => new PostgresGenerator(
                    sp.GetRequiredService<PostgresQuoter>(),
                    sp.GetRequiredService<IOptions<GeneratorOptions>>(),
                    sp.GetRequiredService<IPostgresTypeMap>()))
                .AddScoped(sp => new Postgres15_0Generator(
                    sp.GetRequiredService<PostgresQuoter>(),
                    sp.GetRequiredService<IOptions<GeneratorOptions>>(),
                    sp.GetRequiredService<IPostgresTypeMap>()))
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<Postgres15_0Generator>());

            return builder.AddCommonPostgresServices();
        }

        /// <summary>
        /// Adds Postgres 9.2 support
        /// </summary>
        /// <param name="builder">The builder to add the Postgres-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddPostgres92(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddScoped<IPostgresTypeMap, Postgres92TypeMap>();
            builder.Services
                .AddScoped<Postgres92Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<Postgres92Processor>())
                .AddScoped(sp => new Postgres92Generator(
                    sp.GetRequiredService<PostgresQuoter>(),
                    sp.GetRequiredService<IOptions<GeneratorOptions>>(),
                    sp.GetRequiredService<IPostgresTypeMap>()))
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<Postgres92Generator>());

            return builder.AddCommonPostgresServices();
        }


        /// <summary>
        /// Adds Postgres 10.0 support
        /// </summary>
        /// <param name="builder">The builder to add the Postgres-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddPostgres10_0(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddScoped<IPostgresTypeMap, Postgres92TypeMap>();
            builder.Services
                .AddScoped<Postgres10_0Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<Postgres10_0Processor>())
                .AddScoped(sp => new Postgres10_0Generator(
                    sp.GetRequiredService<PostgresQuoter>(),
                    sp.GetRequiredService<IOptions<GeneratorOptions>>(),
                    sp.GetRequiredService<IPostgresTypeMap>()))
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<Postgres10_0Generator>());
            return builder.AddCommonPostgresServices();
        }

        /// <summary>
        /// Adds Postgres 11.0 support
        /// </summary>
        /// <param name="builder">The builder to add the Postgres-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddPostgres11_0(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddScoped<IPostgresTypeMap, Postgres92TypeMap>();
            builder.Services
                .AddScoped<Postgres11_0Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<Postgres11_0Processor>())
                .AddScoped(sp => new Postgres11_0Generator(
                    sp.GetRequiredService<PostgresQuoter>(),
                    sp.GetRequiredService<IOptions<GeneratorOptions>>(),
                    sp.GetRequiredService<IPostgresTypeMap>()))
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<Postgres11_0Generator>());
            return builder.AddCommonPostgresServices();
        }

        /// <summary>
        /// Adds Postgres 15.0 support
        /// </summary>
        /// <param name="builder">The builder to add the Postgres-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddPostgres15_0(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddScoped<IPostgresTypeMap, Postgres92TypeMap>();
            builder.Services
                .AddScoped<Postgres15_0Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<Postgres15_0Processor>())
                .AddScoped(sp => new Postgres15_0Generator(
                    sp.GetRequiredService<PostgresQuoter>(),
                    sp.GetRequiredService<IOptions<GeneratorOptions>>(),
                    sp.GetRequiredService<IPostgresTypeMap>()))
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<Postgres15_0Generator>());
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
