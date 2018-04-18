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
using System.Collections.Generic;

using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.VersionTableInfo;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using Scrutor;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for the <see cref="IMigrationRunnerBuilder"/> interface
    /// </summary>
    [CLSCompliant(false)]
    public static class MigrationRunnerBuilderExtensions
    {
        public static IMigrationRunnerBuilder WithAnnouncer(this IMigrationRunnerBuilder builder, IAnnouncer announcer)
        {
            builder.Services.AddSingleton(_ => announcer);
            return builder;
        }

        public static IMigrationRunnerBuilder WithProcessorOptions(this IMigrationRunnerBuilder builder,
            IMigrationProcessorOptions options)
        {
            builder.Services.AddSingleton(_ => options);
            return builder;
        }

        public static IMigrationRunnerBuilder ConfigureVersionTable(
            this IMigrationRunnerBuilder builder,
            IVersionTableMetaData versionTableMetaData)
        {
            builder.Services.AddSingleton(_ => versionTableMetaData);
            return builder;
        }

        public static IMigrationRunnerBuilder WithRunnerContext(
            this IMigrationRunnerBuilder builder,
            IRunnerContext context)
        {
            builder.Services.AddScoped(_ => context);
            return builder;
        }

        public static IMigrationRunnerBuilder WithRunnerConventions(
            this IMigrationRunnerBuilder builder,
            IMigrationRunnerConventions conventions)
        {
            builder.Services.AddScoped(_ => conventions);
            return builder;
        }

        public static IMigrationRunnerBuilder AddMigrations(
            this IMigrationRunnerBuilder builder,
            IEnumerable<string> targets,
            [CanBeNull] string @namespace = null,
            bool withNestedNamespaces = false)
        {
            using (var sp = builder.Services.BuildServiceProvider())
            {
                var loaderFactory = sp.GetRequiredService<AssemblyLoaderFactory>();
                var assemblies = loaderFactory.GetTargetAssemblies(targets);
                builder.Services.Scan(
                    selector => selector
                        .FromAssemblies(assemblies)
                        .AddClasses(filter =>
                        {
                            if (string.IsNullOrEmpty(@namespace))
                                return;

                            if (!withNestedNamespaces)
                            {
                                filter.InNamespaces(@namespace);
                                return;
                            }

                            var namespaceWithDot = $"{@namespace}.";
                            filter.Where(t =>
                            {
                                if (t.Namespace == null)
                                    return false;
                                return t.Namespace == @namespace
                                 || t.Namespace.StartsWith(namespaceWithDot,
                                        StringComparison.InvariantCulture);
                            });
                        })
                        .As<IMigration>()
                        .WithScopedLifetime());
            }

            return builder;
        }

        public static IMigrationRunnerBuilder AddMigrations(
            this IMigrationRunnerBuilder builder,
            Action<ITypeSourceSelector> typeSelectorAction)
        {
            builder.Services.Scan(typeSelectorAction);
            return builder;
        }
    }
}
