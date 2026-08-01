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

using Riok.Mapperly.Abstractions;

namespace FluentMigrator.DotNet.Cli
{
    /// <summary>
    /// Copies the values of one <see cref="MigratorOptions"/> instance onto another.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mapperly generates the body of <see cref="Apply"/> at compile time, so the copy is plain
    /// property assignment: no reflection, no configuration to validate at startup, and nothing
    /// left in the shipped tool at runtime.
    /// </para>
    /// <para>
    /// <see cref="MigratorOptions.Task"/> is deliberately not copied. It is assigned by the
    /// factory methods on <see cref="MigratorOptions"/> and has no setter, so it was never
    /// carried across by the mapper this type replaced either.
    /// </para>
    /// </remarks>
    [Mapper]
    internal static partial class MigratorOptionsMapper
    {
        /// <summary>
        /// Copies every settable property from <paramref name="source"/> onto <paramref name="target"/>.
        /// </summary>
        /// <param name="source">The options to read from.</param>
        /// <param name="target">The options instance to populate.</param>
        /// <example>
        /// <code>
        /// var target = new MigratorOptions();
        /// MigratorOptionsMapper.Apply(source, target);
        /// </code>
        /// </example>
        [MapperIgnoreSource(nameof(MigratorOptions.Task))]
        public static partial void Apply(MigratorOptions source, [MappingTarget] MigratorOptions target);
    }
}
