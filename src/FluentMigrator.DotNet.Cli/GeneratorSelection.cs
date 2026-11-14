#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

namespace FluentMigrator.DotNet.Cli
{
    /// <summary>
    /// Decides which migration generator the CLI selects for the given processor.
    /// </summary>
    /// <remarks>
    /// A generator describes a database engine version. A processor also encodes the database driver.
    /// The <c>--processor</c> option is a shorthand that selects both, so a processor whose id names a
    /// driver is mapped here to the generator for its engine instead of teaching the generator about
    /// the driver.
    /// </remarks>
    internal static class GeneratorSelection
    {
        private static readonly Dictionary<string, string> _generatorByProcessor = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ProcessorIdConstants.Oracle12cManaged] = GeneratorIdConstants.Oracle12c,
        };

        /// <summary>
        /// Gets the generator id to select.
        /// </summary>
        /// <param name="generatorType">The value of <c>--generator</c>, if given</param>
        /// <param name="processorType">The value of <c>--processor</c></param>
        /// <returns>The generator id, or <c>null</c> to select the generator with the same id as the processor</returns>
        internal static string Resolve(string generatorType, string processorType)
        {
            if (!string.IsNullOrEmpty(generatorType))
            {
                return generatorType;
            }

            return !string.IsNullOrEmpty(processorType) && _generatorByProcessor.TryGetValue(processorType, out var generatorId)
                ? generatorId
                : null;
        }
    }
}
