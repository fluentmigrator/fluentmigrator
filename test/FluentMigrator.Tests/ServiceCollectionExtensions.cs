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
using System.Runtime.CompilerServices;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentMigrator.Tests
{
    public static class ServiceCollectionExtensions
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IServiceCollection CreateServices()
        {
            return new ServiceCollection()
                .AddFluentMigratorCore()
                .AddSingleton<IAssemblySourceItem>(new AssemblySourceItem(Assembly.GetExecutingAssembly()))
                .ConfigureRunner(builder => builder
                    .WithAnnouncer(new TextWriterAnnouncer(TestContext.Out) { ShowSql = true }))
                .Configure<RunnerOptions>(opt => opt.AllowBreakingChange = true);
        }

        [Obsolete]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IServiceCollection CreateServices(this IMigrationProcessor processor)
        {
#pragma warning disable 612
            var sourceProcOpt = processor.Options;
            var connectionString = processor.ConnectionString;
#pragma warning restore 612
            var procOpt = sourceProcOpt.GetProcessorOptions(connectionString);
            return CreateServices()
                .Configure<ProcessorOptions>(
                    opt =>
                    {
                        opt.ConnectionString = procOpt.ConnectionString;
                        opt.PreviewOnly = procOpt.PreviewOnly;
                        opt.ProviderSwitches = procOpt.ProviderSwitches;
                        opt.Timeout = procOpt.Timeout;
                    })
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader(connectionString))
                .AddScoped(sp => new PassThroughProcessorAccessor(processor));
        }

        public static IServiceCollection WithAllTestMigrations(
            [NotNull] this IServiceCollection services)
        {
            return services
                .WithMigrationsIn(@namespace: null);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IServiceCollection WithMigrationsIn(
            [NotNull] this IServiceCollection services,
            [CanBeNull] string @namespace,
            bool recursive = false)
        {
            return services
                .Configure<TypeFilterOptions>(
                    opt =>
                    {
                        opt.Namespace = @namespace;
                        opt.NestedNamespaces = recursive;
                    })
                .ConfigureRunner(builder => builder.WithMigrationsIn(Assembly.GetExecutingAssembly()));
        }

        private class PassThroughProcessorAccessor : IProcessorAccessor
        {
            public PassThroughProcessorAccessor(IMigrationProcessor processor)
            {
                Processor = processor;
            }

            /// <inheritdoc />
            public IMigrationProcessor Processor { get; }
        }
    }
}
