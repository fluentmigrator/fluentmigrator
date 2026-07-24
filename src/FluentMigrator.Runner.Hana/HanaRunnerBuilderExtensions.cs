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

using FluentMigrator.Runner.Generators.Hana;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Hana;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IMigrationRunnerBuilder"/>
    /// </summary>
    public static class HanaRunnerBuilderExtensions
    {
        /// <summary>
        /// Adds SAP Hana support using the <c>Sap.Data.Hana.Net.v8.0</c> NuGet package
        /// </summary>
        /// <param name="builder">The builder to add the SAP Hana-specific services to</param>
        /// <returns>The migration runner builder</returns>
        /// <remarks>
        /// Requires the <c>Sap.Data.Hana.Net.v8.0</c> NuGet package to be installed.
        /// </remarks>
        public static IMigrationRunnerBuilder AddHana8(this IMigrationRunnerBuilder builder)
        {
            return builder.RegisterHanaServices();
        }

        /// <summary>
        /// Adds SAP Hana support using the <c>Sap.Data.Hana.Net.v10.0</c> NuGet package
        /// </summary>
        /// <param name="builder">The builder to add the SAP Hana-specific services to</param>
        /// <returns>The migration runner builder</returns>
        /// <remarks>
        /// Requires the <c>Sap.Data.Hana.Net.v10.0</c> NuGet package to be installed.
        /// </remarks>
        public static IMigrationRunnerBuilder AddHana10(this IMigrationRunnerBuilder builder)
        {
            return builder.RegisterHanaServices();
        }

        /// <summary>
        /// Adds SAP Hana support
        /// </summary>
        /// <param name="builder">The builder to add the SAP Hana-specific services to</param>
        /// <returns>The migration runner builder</returns>
        [Obsolete("Use AddHana8() for .NET 8 with the Sap.Data.Hana.Net.v8.0 NuGet package, or AddHana10() for .NET 10 with the Sap.Data.Hana.Net.v10.0 NuGet package.")]
        public static IMigrationRunnerBuilder AddHana(this IMigrationRunnerBuilder builder)
        {
            return builder.RegisterHanaServices();
        }

        private static IMigrationRunnerBuilder RegisterHanaServices(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<HanaDbFactory>()
                .AddScoped<HanaProcessor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<HanaProcessor>())
                .AddScoped<HanaQuoter>()
                .AddScoped<IHanaTypeMap>(sp => new HanaTypeMap())
                .AddScoped<HanaGenerator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<HanaGenerator>());

            return builder;
        }
    }
}
