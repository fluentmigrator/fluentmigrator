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

using System;
using System.Reflection;

using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.VersionTableInfo;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for the <see cref="IMigrationRunnerBuilder"/> interface
    /// </summary>
    public static class MigrationRunnerBuilderExtensions
    {
        /// <summary>
        /// Sets configuration action for global processor options
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="configureAction">The configuration action</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder ConfigureGlobalProcessorOptions(
            this IMigrationRunnerBuilder builder,
            Action<ProcessorOptions> configureAction)
        {
            builder.Services.Configure(configureAction);
            return builder;
        }

        /// <summary>
        /// Sets the global connection string
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="connectionStringOrName">The connection string or name to use</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithGlobalConnectionString(
            this IMigrationRunnerBuilder builder,
            string connectionStringOrName)
        {
            builder.Services.Configure<ProcessorOptions>(opt => opt.ConnectionString = connectionStringOrName);
            return builder;
        }

        /// <summary>
        /// Sets the global command timeout
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="commandTimeout">The global command timeout</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithGlobalCommandTimeout(
            this IMigrationRunnerBuilder builder,
            TimeSpan commandTimeout)
        {
            builder.Services.Configure<ProcessorOptions>(opt => opt.Timeout = commandTimeout);
            return builder;
        }

        /// <summary>
        /// Sets the global preview mode
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="preview">The global preview mode</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder AsGlobalPreview(
            this IMigrationRunnerBuilder builder,
            bool preview = true)
        {
            builder.Services.Configure<ProcessorOptions>(opt => opt.PreviewOnly = preview);
            return builder;
        }

        /// <summary>
        /// Sets the version table meta data
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="versionTableMetaData">The version table meta data</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithVersionTable(
            this IMigrationRunnerBuilder builder,
            IVersionTableMetaData versionTableMetaData)
        {
            builder.Services
                .AddScoped<IVersionTableMetaDataAccessor>(
                    _ => new PassThroughVersionTableMetaDataAccessor(versionTableMetaData));
            return builder;
        }

        /// <summary>
        /// Sets the migration runner conventions
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="conventions">The migration runner conventions</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithRunnerConventions(
            this IMigrationRunnerBuilder builder,
            IMigrationRunnerConventions conventions)
        {
            builder.Services
                .AddSingleton<IMigrationRunnerConventionsAccessor>(
                    new PassThroughMigrationRunnerConventionsAccessor(conventions));
            return builder;
        }

        /// <summary>
        /// Adds the migrations
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="assemblies">The target assemblies</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithMigrationsIn(
            this IMigrationRunnerBuilder builder,
            [NotNull, ItemNotNull] params Assembly[] assemblies)
        {
            builder.Services
                .AddSingleton<IMigrationSourceItem>(new AssemblyMigrationSourceItem(assemblies));
            return builder;
        }
    }
}
