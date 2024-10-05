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

using FluentMigrator.Runner.Generators.Redshift;
using FluentMigrator.Runner.Processors.Redshift;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IMigrationRunnerBuilder"/>
    /// </summary>
    public static class RedshiftRunnerBuilderExtensions
    {
        /// <summary>
        /// Adds Redshift support
        /// </summary>
        /// <param name="builder">The builder to add the Redshift-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddRedshift(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<RedshiftDbFactory>()
                .AddScoped<RedshiftProcessor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<RedshiftProcessor>())
                .AddScoped<RedshiftQuoter>()
                .AddScoped<IRedshiftTypeMap>(sp => new RedshiftTypeMap())
                .AddScoped<RedshiftGenerator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<RedshiftGenerator>());

            return builder;
        }
    }
}
