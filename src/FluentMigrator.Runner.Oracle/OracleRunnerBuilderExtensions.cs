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

using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.DotConnectOracle;
using FluentMigrator.Runner.Processors.Oracle;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IMigrationRunnerBuilder"/>
    /// </summary>
    public static class OracleRunnerBuilderExtensions
    {
        /// <summary>
        /// Register Oracle quoter
        /// </summary>
        /// <param name="builder">The builder to add the Oracle-specific services to</param>
        private static void RegisterOracleQuoter(IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddScoped<OracleQuoterBase>(
                sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<ProcessorOptions>>();
                    return opt.Value.IsQuotingForced() ?
                        new OracleQuoterQuotedIdentifier() :
                        new OracleQuoter();
                });
        }

        /// <summary>
        /// Register Oracle generator
        /// </summary>
        /// <param name="builder">The builder to add the Oracle-specific services to</param>
        private static void RegisterOracleGenerator(IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<IOracleTypeMap>(sp => new OracleTypeMap())
                .TryAddScoped<IOracleGenerator, OracleGenerator>();
        }

        /// <summary>
        /// Register Oracle 12c generator
        /// </summary>
        /// <param name="builder">The builder to add the Oracle-specific services to</param>
        private static void RegisterOracle12CGenerator(IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddScoped<IOracle12CGenerator, Oracle12CGenerator>();
        }

        /// <summary>
        /// Register Oracle processor dependencies
        /// </summary>
        /// <param name="builder">The builder to add the Oracle-specific services to</param>
        private static void RegisterOracleProcessor<
#if NET
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            T>(IMigrationRunnerBuilder builder)
            where T : OracleProcessor
        {
            RegisterOracleQuoter(builder);

            builder.Services
                .AddScoped<OracleDbFactory>()
                .AddScoped<T>()
                .AddScoped<OracleProcessorBase>(sp => sp.GetRequiredService<T>())
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<T>());
        }

        /// <summary>
        /// Register Oracle managed processor dependencies
        /// </summary>
        /// <param name="builder">The builder to add the Oracle-specific services to</param>
        private static void RegisterOracleManagedProcessor<
#if NET
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            T>(IMigrationRunnerBuilder builder)
            where T : OracleManagedProcessor
        {
            RegisterOracleQuoter(builder);

            builder.Services
                .AddScoped<OracleManagedDbFactory>()
                .AddScoped<T>()
                .AddScoped<OracleProcessorBase>(sp => sp.GetRequiredService<T>())
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<T>());

        }

        /// <summary>
        /// Register dotConnection Oracle processor dependencies
        /// </summary>
        /// <param name="builder">The builder to add the Oracle-specific services to</param>
        private static void RegisterDotConnectOracleProcessor<
#if NET
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            T>(IMigrationRunnerBuilder builder)
            where T : DotConnectOracleProcessor
        {
            RegisterOracleQuoter(builder);

            builder.Services
                .AddScoped<DotConnectOracleDbFactory>()
                .AddScoped<T>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<T>());
        }

        /// <summary>
        /// Adds Oracle support
        /// </summary>
        /// <param name="builder">The builder to add the Oracle-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddOracle(this IMigrationRunnerBuilder builder)
        {
            RegisterOracleGenerator(builder);

            RegisterOracleProcessor<OracleProcessor>(builder);

            builder.Services.AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<IOracleGenerator>());

            return builder;
        }

        /// <summary>
        /// Adds managed Oracle support
        /// </summary>
        /// <param name="builder">The builder to add the managed Oracle-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddOracleManaged(this IMigrationRunnerBuilder builder)
        {
            RegisterOracleGenerator(builder);

            RegisterOracleManagedProcessor<OracleManagedProcessor>(builder);

            builder.Services.AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<IOracleGenerator>());

            return builder;
        }

        /// <summary>
        /// Adds .Connect Oracle support
        /// </summary>
        /// <param name="builder">The builder to add the .Connect Oracle-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddDotConnectOracle(this IMigrationRunnerBuilder builder)
        {
            RegisterOracleGenerator(builder);

            RegisterDotConnectOracleProcessor<DotConnectOracleProcessor>(builder);

            builder.Services.AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<IOracleGenerator>());

            return builder;
        }

        /// <summary>
        /// Adds Oracle 12c support
        /// </summary>
        /// <param name="builder">The builder to add the Oracle-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddOracle12C(this IMigrationRunnerBuilder builder)
        {
            RegisterOracle12CGenerator(builder);

            RegisterOracleProcessor<Oracle12CProcessor>(builder);

            builder.Services.AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<IOracle12CGenerator>());

            return builder;
        }

        /// <summary>
        /// Adds managed Oracle 12c support
        /// </summary>
        /// <param name="builder">The builder to add the managed Oracle-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddOracle12CManaged(this IMigrationRunnerBuilder builder)
        {
            RegisterOracle12CGenerator(builder);

            RegisterOracleManagedProcessor<Oracle12CManagedProcessor>(builder);

            builder.Services.AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<IOracle12CGenerator>());

            return builder;
        }

        /// <summary>
        /// Adds .Connect Oracle 12c support
        /// </summary>
        /// <param name="builder">The builder to add the .Connect Oracle-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddDotConnectOracle12C(this IMigrationRunnerBuilder builder)
        {
            RegisterOracle12CGenerator(builder);

            RegisterDotConnectOracleProcessor<DotConnectOracle12CProcessor>(builder);

            builder.Services.AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<IOracle12CGenerator>());

            return builder;
        }
    }
}
