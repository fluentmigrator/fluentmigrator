#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;

using FluentMigrator.Runner.Processors;

using JetBrains.Annotations;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods
    /// </summary>
    [Obsolete]
    internal static class LegacyExtensions
    {
        /// <summary>
        /// Gets the name for a given migration generator instance
        /// </summary>
        /// <param name="generator">The migration generator instance to get its name for</param>
        /// <returns>The name of the migration generator</returns>
        [NotNull]
        public static string GetName([NotNull] this IMigrationGenerator generator)
        {
            return generator.GetType().Name.Replace("Generator", string.Empty);
        }

        /// <summary>
        /// Gets a <see cref="ProcessorOptions"/> instance for a given <see cref="IMigrationProcessorOptions"/> implementation
        /// </summary>
        /// <param name="options">The instance to get the <see cref="ProcessorOptions"/> for</param>
        /// <param name="connectionString">The connection string</param>
        /// <returns>The found/created <see cref="ProcessorOptions"/></returns>
        internal static ProcessorOptions GetProcessorOptions(this IMigrationProcessorOptions options, string connectionString)
        {
            if (options == null)
                return null;

            return options as ProcessorOptions ?? new ProcessorOptions()
            {
                ConnectionString = connectionString,
                PreviewOnly = options.PreviewOnly,
                ProviderSwitches = options.ProviderSwitches,
                Timeout = options.Timeout == null ? null : (TimeSpan?) TimeSpan.FromSeconds(options.Timeout.Value),
            };
        }
    }
}
