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

using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IMigrationRunnerBuilder"/>
    /// </summary>
    public static class SnowflakeRunnerBuilderExtensions
    {
        /// <summary>
        /// Adds Snowflake support
        /// </summary>
        /// <param name="builder">The builder to add the Snowflake-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddSnowflake(this IMigrationRunnerBuilder builder)
        {
            return builder.AddSnowflake(SnowflakeOptions.QuotingDisabled());
        }


        /// <summary>
        /// Adds Snowflake support
        /// </summary>
        /// <param name="builder">The builder to add the Snowflake-specific services to</param>
        /// <param name="sfOptions">Snowflake options</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddSnowflake(this IMigrationRunnerBuilder builder, SnowflakeOptions sfOptions)
        {
            builder.Services
                .AddScoped<SnowflakeOptions>()
                .AddTransient<SnowflakeBatchParser>()
                .AddScoped<SnowflakeDbFactory>()
                .AddScoped<SnowflakeProcessor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<SnowflakeProcessor>())
                .AddScoped<SnowflakeQuoter>()
                .AddScoped<ISnowflakeTypeMap>(sp => new SnowflakeTypeMap())
                .AddScoped<SnowflakeGenerator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<SnowflakeGenerator>());
            return builder;
        }
    }
}
