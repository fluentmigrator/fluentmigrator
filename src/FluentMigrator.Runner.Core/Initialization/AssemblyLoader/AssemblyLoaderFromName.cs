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
    /// Provides functionality to load an assembly by its name.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="IAssemblyLoader"/> interface and is used to load assemblies
    /// based on their names. It is typically used when the assembly name is known but not its file path.
    /// </remarks>
    public class AssemblyLoaderFromName : IAssemblyLoader
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyLoaderFromName"/> class with the specified assembly name.
        /// </summary>
        /// <param name="name">The name of the assembly to be loaded.</param>
        /// <remarks>
        /// This constructor sets the assembly name that will be used to load the assembly
        /// when the <see cref="Load"/> method is called.
        /// </remarks>
        public AssemblyLoaderFromName(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Loads the assembly specified by the name provided during the initialization of this instance.
        /// </summary>
        /// <returns>
        /// The loaded <see cref="System.Reflection.Assembly"/> corresponding to the specified name.
        /// </returns>
        /// <remarks>
        /// This method uses the <see cref="System.Reflection.Assembly.Load(string)"/> method to load the assembly
        /// by its name. Ensure that the assembly name is valid and resolvable in the current application context.
        /// </remarks>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown if the assembly with the specified name cannot be found.
        /// </exception>
        /// <exception cref="System.BadImageFormatException">
        /// Thrown if the assembly is not a valid assembly or was compiled with a later version of the CLR than the current process.
        /// </exception>
        /// <exception cref="System.IO.FileLoadException">
        /// Thrown if the assembly is found but cannot be loaded.
        /// </exception>
        public Assembly Load()
        {
            Assembly assembly = Assembly.Load(_name);
            return assembly;
        }
    }
}
