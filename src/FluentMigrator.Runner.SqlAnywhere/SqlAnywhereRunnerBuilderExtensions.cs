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

using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.Generators.SqlAnywhere;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlAnywhere;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IMigrationRunnerBuilder"/>
    /// </summary>
    public static class SqlAnywhereRunnerBuilderExtensions
    {
        /// <summary>
        /// Adds SAP SQL Anywhere support
        /// </summary>
        /// <param name="builder">The builder to add the SAP SQL Anywhere-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddSqlAnywhere16(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddTransient<SqlAnywhereBatchParser>()
                .AddScoped<SqlAnywhereDbFactory>()
                .AddScoped<SqlAnywhere16Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<SqlAnywhere16Processor>())
                .AddScoped<SqlAnywhereQuoter>()
                .AddScoped<SqlAnywhere16Generator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<SqlAnywhere16Generator>());

            MigrationProcessorFactoryProvider.Register(new SqlAnywhere16ProcessorFactory());

            return builder;
        }

        /// <summary>
        /// Adds SAP SQL Anywhere support for the latest version
        /// </summary>
        /// <param name="builder">The builder to add the SAP SQL Anywhere-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddSqlAnywhere(this IMigrationRunnerBuilder builder)
        {
            return builder.AddSqlAnywhere16();
        }
    }
}
