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

using System.Collections.Generic;
using System.Reflection;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Represents an item for the <see cref="AssemblySource"/>
    /// </summary>
    public interface IAssemblySourceItem
    {
        /// <summary>
        /// Gets all assemblies covered by this item
        /// </summary>
        IEnumerable<Assembly> Assemblies { get; }
    }
}
