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

using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SQLite;

using Microsoft.Extensions.DependencyInjection;

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
        /// <returns>The migration runner builder</returns>
        // ReSharper disable once InconsistentNaming
        public static IMigrationRunnerBuilder AddSQLite(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<SQLiteDbFactory>()
                .AddScoped<IMigrationProcessor, SQLiteProcessor>()
                .AddScoped<IMigrationGenerator, SQLiteGenerator>();
            return builder;
        }
    }
}
