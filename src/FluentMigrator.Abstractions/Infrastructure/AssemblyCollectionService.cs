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
using System.Linq;

using JetBrains.Annotations;

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// A compatibility service to get the assembly collection from the found migrations
    /// </summary>
    [Obsolete]
    public class AssemblyCollectionService : AssemblyCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyCollectionService"/> class.
        /// </summary>
        /// <param name="migrations">The migrations to get the assemblies for</param>
        public AssemblyCollectionService([NotNull, ItemNotNull] IEnumerable<IMigration> migrations)
            : base(migrations.Select(m => m.GetType().Assembly).Distinct())
        {
        }
    }
}
