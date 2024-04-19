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

using FluentMigrator.Runner.Generators.Jet;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Jet;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IMigrationRunnerBuilder"/>
    /// </summary>
    public static class JetRunnerBuilderExtensions
    {
        /// <summary>
        /// Adds JET engine (Access) support
        /// </summary>
        /// <param name="builder">The builder to add the JET engine (Access)-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddJet(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<JetProcessor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<JetProcessor>())
                .AddScoped<JetQuoter>()
                .AddScoped<IJetTypeMap>(sp => new JetTypeMap())
                .AddScoped<JetGenerator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<JetGenerator>());

            return builder;
        }
    }
}
