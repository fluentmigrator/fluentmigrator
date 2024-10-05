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

using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="ProcessorOptions"/>
    /// </summary>
    internal static class ProcessorOptionsExtensions
    {
        /// <summary>
        /// Returns a value indicating whether quoted identifiers are enforced
        /// </summary>
        /// <param name="options">The processor options</param>
        /// <returns><c>true</c> when quoted identifiers are enforced</returns>
        public static bool IsQuotingForced(this ProcessorOptions options)
        {
            return Quoted(options.ProviderSwitches);
        }

        /// <summary>
        /// Returns a value indicating whether quoted identifiers are enforced
        /// </summary>
        /// <param name="options">The provider switches</param>
        /// <returns><c>true</c> when quoted identifiers are enforced</returns>
        internal static bool Quoted(string options)
        {
            return !string.IsNullOrEmpty(options)
             && options.IndexOf("QUOTEDIDENTIFIERS=TRUE", StringComparison.InvariantCultureIgnoreCase) != -1;
        }
    }
}
