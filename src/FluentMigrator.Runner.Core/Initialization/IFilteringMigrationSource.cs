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

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// This is a specialization of <see cref="IMigrationSource"/> that allows filtering the types beforehand
    /// </summary>
    public interface IFilteringMigrationSource
    {
        /// <summary>
        /// Returns the instances for all found types implementing <see cref="IMigration"/>
        /// </summary>
        /// <param name="predicate">The predicate used to select the types to instantiate</param>
        /// <returns>the instances for all found types implementing <see cref="IMigration"/></returns>
        [NotNull, ItemNotNull]
        IEnumerable<IMigration> GetMigrations(Func<Type, bool> predicate);
    }
}
