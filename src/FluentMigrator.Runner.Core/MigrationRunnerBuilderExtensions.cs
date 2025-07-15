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

using System;
using System.Linq;
using System.Reflection;

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.VersionTableInfo;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        /// Sets the global connection string
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="configureConnectionString">The function that creates the connection string.</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithGlobalConnectionString(
            this IMigrationRunnerBuilder builder, Func<IServiceProvider, string> configureConnectionString)
        {
            builder.Services
                .AddSingleton<IConfigureOptions<ProcessorOptions>>(
                    s =>
                    {
                        return new ConfigureNamedOptions<ProcessorOptions>(
                            Options.DefaultName,
                            opt => opt.ConnectionString = configureConnectionString(s));
                    });

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
        /// Sets the global strip comment
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="stripComments">The global strip comments</param>
        /// <returns>The runner builder</returns>
        public static IMigrationRunnerBuilder WithGlobalStripComments(
            this IMigrationRunnerBuilder builder,
            bool stripComments)
        {
            builder.Services.Configure<ProcessorOptions>(opt => opt.StripComments = stripComments);
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
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses gets the exported types from assemblies, which may not be preserved in trimmed applications.")]
#endif
        public static IMigrationRunnerBuilder WithMigrationsIn(
            this IMigrationRunnerBuilder builder,
            [NotNull, ItemNotNull] params Assembly[] assemblies)
        {
            builder.Services
                .AddSingleton<IMigrationSourceItem>(new AssemblyMigrationSourceItem(assemblies));
            return builder;
        }

        public static IMigrationRunnerBuilder WithTypes(
            this IMigrationRunnerBuilder builder,
#if NET
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            [NotNull] Type[] types)
        {
            builder.Services
                .AddSingleton(new ArrayTypeSource(types));
            return builder;
        }

        /// <summary>
        /// Scans for types in the given assemblies
        /// </summary>
        /// <param name="builder">The runner builder</param>
        /// <param name="assemblies">The assemblies to scan</param>
        /// <returns>The next step</returns>
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to load types, which may not be preserved in trimmed applications.")]
#endif
        public static IScanInBuilder ScanIn(
            this IMigrationRunnerBuilder builder,
            [NotNull, ItemNotNull] params Assembly[] assemblies)
        {
            var sourceItem = new AssemblySourceItem(assemblies);
            return new ScanInBuilder(builder, sourceItem);
        }

#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This type uses reflection to load types, which may not be preserved in trimmed applications.")]
#endif
        private class ScanInBuilder : IScanInBuilder, IScanInForBuilder
        {
            private readonly IMigrationRunnerBuilder _builder;

            public ScanInBuilder(IMigrationRunnerBuilder builder, IAssemblySourceItem currentSourceItem)
            {
                if (builder.DanglingAssemblySourceItem != null)
                {
                    builder.Services
                        .AddSingleton(builder.DanglingAssemblySourceItem);
                }

                _builder = builder;
                _builder.DanglingAssemblySourceItem = currentSourceItem;
                SourceItem = currentSourceItem;
            }

            private ScanInBuilder(
                IMigrationRunnerBuilder builder,
                IAssemblySourceItem currentSourceItem,
                IMigrationSourceItem sourceItem)
            {
                _builder = builder;
                SourceItem = currentSourceItem;

                _builder.DanglingAssemblySourceItem = null;
                Services.AddSingleton(sourceItem);
            }

            private ScanInBuilder(
                IMigrationRunnerBuilder builder,
                IAssemblySourceItem currentSourceItem,
                IVersionTableMetaDataSourceItem sourceItem)
            {
                _builder = builder;
                SourceItem = currentSourceItem;

                _builder.DanglingAssemblySourceItem = null;
                Services.AddSingleton(sourceItem);
            }

            private ScanInBuilder(
                IMigrationRunnerBuilder builder,
                IAssemblySourceItem currentSourceItem,
                ITypeSourceItem<IConventionSet> sourceItem)
            {
                _builder = builder;
                SourceItem = currentSourceItem;

                _builder.DanglingAssemblySourceItem = null;
                Services.AddSingleton(sourceItem);
            }

            private ScanInBuilder(
                IMigrationRunnerBuilder builder,
                IAssemblySourceItem currentSourceItem,
                IEmbeddedResourceProvider sourceItem)
            {
                _builder = builder;
                SourceItem = currentSourceItem;

                _builder.DanglingAssemblySourceItem = null;
                Services.AddSingleton(sourceItem);
            }

            /// <inheritdoc />
            public IServiceCollection Services => _builder.Services;

            /// <inheritdoc />
            public IAssemblySourceItem DanglingAssemblySourceItem
            {
                get => _builder.DanglingAssemblySourceItem;
                set => _builder.DanglingAssemblySourceItem = value;
            }

            /// <inheritdoc />
            public IAssemblySourceItem SourceItem { get; }

            /// <inheritdoc />
            public IScanInForBuilder For => this;

            /// <inheritdoc />
            public IScanInBuilder Migrations()
            {
                var sourceItem = new AssemblyMigrationSourceItem(SourceItem.Assemblies.ToList());
                return new ScanInBuilder(_builder, SourceItem, sourceItem);
            }

            /// <inheritdoc />
            public IScanInBuilder VersionTableMetaData()
            {
                var sourceItem = new AssemblyVersionTableMetaDataSourceItem(SourceItem.Assemblies.ToArray());
                return new ScanInBuilder(_builder, SourceItem, sourceItem);
            }

            /// <inheritdoc />
            public IScanInBuilder ConventionSet()
            {
                var sourceItem = new AssemblySourceItem<IConventionSet>(SourceItem.Assemblies.ToArray());
                return new ScanInBuilder(_builder, SourceItem, sourceItem);
            }

            /// <inheritdoc />
            public IScanInBuilder EmbeddedResources()
            {
                var sourceItem = new DefaultEmbeddedResourceProvider(SourceItem.Assemblies.ToArray());
                return new ScanInBuilder(_builder, SourceItem, sourceItem);
            }

            /// <inheritdoc />
            public IMigrationRunnerBuilder All()
            {
                Services.AddSingleton(SourceItem);
                _builder.DanglingAssemblySourceItem = null;
                return _builder;
            }
        }
    }
}
