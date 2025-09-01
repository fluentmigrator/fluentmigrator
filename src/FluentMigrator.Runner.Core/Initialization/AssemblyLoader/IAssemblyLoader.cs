#region License
// 
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System.Reflection;

namespace FluentMigrator.Runner.Initialization.AssemblyLoader
{
    /// <summary>
    /// Defines the contract for loading assemblies.
    /// </summary>
    /// <remarks>
    /// This interface provides a method to load assemblies, which can be implemented
    /// to support different strategies for locating and loading assemblies, such as
    /// by name or from a file path.
    /// </remarks>
    public interface IAssemblyLoader
    {
        /// <summary>
        /// Loads an assembly based on the implementation of the loader.
        /// </summary>
        /// <returns>
        /// The loaded <see cref="System.Reflection.Assembly"/> instance.
        /// </returns>
        /// <remarks>
        /// The specific strategy for loading the assembly depends on the implementation of the loader.
        /// For example, it could load an assembly by its name or from a file path.
        /// </remarks>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown if the assembly cannot be found.
        /// </exception>
        /// <exception cref="System.BadImageFormatException">
        /// Thrown if the assembly is not a valid assembly or was compiled with a later version of the CLR than the current process.
        /// </exception>
        /// <exception cref="System.IO.FileLoadException">
        /// Thrown if the assembly is found but cannot be loaded.
        /// </exception>
        Assembly Load();
    }
}
