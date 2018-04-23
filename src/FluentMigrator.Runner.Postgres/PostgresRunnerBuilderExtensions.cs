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
using FluentMigrator.Runner.Processors.Postgres;

using Microsoft.Extensions.DependencyInjection;

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
                .AddScoped<PostgresDbFactory>()
                .AddScoped<PostgresProcessor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<PostgresProcessor>())
                .AddScoped<PostgresQuoter>()
                .AddScoped<PostgresGenerator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<PostgresGenerator>());
            return builder;
        }
    }
}
