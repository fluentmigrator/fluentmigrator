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
using System.Reflection;

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// A bundle of one or more Assembly instances
    /// </summary>
    [Obsolete]
    public interface IAssemblyCollection
    {
        /// <summary>
        /// Gets the Assemblies contained in this collection
        /// </summary>
        [Obsolete]
        Assembly[] Assemblies { get; }

        /// <summary>
        /// The result of this method is equivalent to calling GetExportedTypes
        /// on each Assembly in Assemblies.
        /// </summary>
        /// <returns>The array of exported types</returns>
        [Obsolete]
        Type[] GetExportedTypes();

        /// <summary>
        /// Gets a array of resources defined in each of the assemblies that are
        /// contained in this collection, plus which assembly it is defined in.
        /// </summary>
        /// <returns>An array of value pairs of resource name plus assembly.</returns>
        [Obsolete]
        ManifestResourceNameWithAssembly[] GetManifestResourceNames();
    }
}
