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
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IMigrationRunnerBuilder"/>
    /// </summary>
    public static class SqlServerCeRunnerBuilderExtensions
    {
        /// <summary>
        /// Adds SQL Server Compact Edition support
        /// </summary>
        /// <param name="builder">The builder to add the SQL Server CE-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddSqlServerCe(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddScoped<SqlServer2000Quoter>();
            builder.Services.TryAddTransient<SqlServerBatchParser>();
            builder.Services
                .AddScoped<SqlServerCeDbFactory>()
                .AddScoped<SqlServerCeProcessor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<SqlServerCeProcessor>())
                .AddScoped<SqlServer2000Generator>()
                .AddScoped<SqlServerCeGenerator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<SqlServerCeGenerator>());

            MigrationProcessorFactoryProvider.Register(new SqlServerCeProcessorFactory());

            return builder;
        }
    }
}
