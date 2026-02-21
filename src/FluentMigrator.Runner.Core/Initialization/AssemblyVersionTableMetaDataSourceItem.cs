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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentMigrator.Runner.VersionTableInfo;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Implementation of <see cref="IVersionTableMetaDataSourceItem"/> that uses assemblies as source
    /// </summary>
#if NET
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This type uses the AppDomain to load assemblies, which may not be preserved in trimmed applications.")]
#endif
    public class AssemblyVersionTableMetaDataSourceItem : IVersionTableMetaDataSourceItem
    {
        [NotNull, ItemNotNull]
        private readonly Assembly[] _assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyVersionTableMetaDataSourceItem"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to load the type from</param>
        public AssemblyVersionTableMetaDataSourceItem([NotNull, ItemNotNull] params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        /// <inheritdoc />
        public IEnumerable<Type> GetCandidates(Predicate<Type> predicate)
        {
            return _assemblies.SelectMany(a => a.GetExportedTypes())
                .Where(t => !t.IsAbstract && t.IsClass)
                .Where(t => typeof(IVersionTableMetaData).IsAssignableFrom(t))
                .Where(t => predicate(t));
        }
    }
}
