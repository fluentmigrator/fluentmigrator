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

using JetBrains.Annotations;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IMigrationGenerator"/>
    /// </summary>
    public static class GeneratorExtensions
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
    }
}
