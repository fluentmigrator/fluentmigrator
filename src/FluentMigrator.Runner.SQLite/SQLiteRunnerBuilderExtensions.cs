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

using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SQLite;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IMigrationRunnerBuilder"/>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class SQLiteRunnerBuilderExtensions
    {
        /// <summary>
        /// Adds SQLite support
        /// </summary>
        /// <param name="builder">The builder to add the SQLite-specific services to</param>
        /// <param name="binaryGuid">True if guids are stored as binary, false if guids are stored as string</param>
        /// <param name="useStrictTables">True if SQLite strict table mode is used. All tables in database should have the same strict table setting.</param>
        /// <param name="compatibilityMode">The compatibility mode for the migration generator</param>
        /// <returns>The migration runner builder</returns>
        // ReSharper disable once InconsistentNaming
        public static IMigrationRunnerBuilder AddSQLite(this IMigrationRunnerBuilder builder, bool binaryGuid = false, bool useStrictTables = false, CompatibilityMode? compatibilityMode = null)
        {
            builder.Services
                .AddTransient<SQLiteBatchParser>()
                .AddScoped<SQLiteDbFactory>()
                .AddScoped<SQLiteProcessor>(sp =>
                {
                    var factory = sp.GetService<SQLiteDbFactory>();
                    var logger = sp.GetService<ILogger<SQLiteProcessor>>();
                    var options = sp.GetService<IOptionsSnapshot<ProcessorOptions>>();
                    var connectionStringAccessor = sp.GetService<IConnectionStringAccessor>();
                    var sqliteQuoter = new SQLiteQuoter(false);
                    return new SQLiteProcessor(factory, sp.GetService<SQLiteGenerator>(), logger, options, connectionStringAccessor, sp, sqliteQuoter);
                })
                .AddScoped<ISQLiteTypeMap>(sp => new SQLiteTypeMap(useStrictTables))
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<SQLiteProcessor>())
                
                .AddScoped(
                    sp =>
                    {
                        var typeMap = sp.GetRequiredService<ISQLiteTypeMap>();
                        return new SQLiteGenerator(
                            new SQLiteQuoter(binaryGuid),
                            typeMap,
                            new OptionsWrapper<GeneratorOptions>(new GeneratorOptions { CompatibilityMode = compatibilityMode }));
                    })
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<SQLiteGenerator>());

            return builder;
        }
    }
}
