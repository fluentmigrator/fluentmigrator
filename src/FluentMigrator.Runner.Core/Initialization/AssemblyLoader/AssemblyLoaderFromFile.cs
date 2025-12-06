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

using System;
using System.IO;
using System.Reflection;

using FluentMigrator.Runner.Infrastructure;

namespace FluentMigrator.Runner.Initialization.AssemblyLoader
{
    /// <summary>
    /// Provides functionality to load an assembly from a specified file path.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="IAssemblyLoader"/> interface and is used to load assemblies
    /// based on their file paths. It resolves the file path if it is not rooted and attempts to locate
    /// the assembly in the current runtime's base directory or other possible locations.
    /// </remarks>
    public class AssemblyLoaderFromFile : IAssemblyLoader
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyLoaderFromFile"/> class with the specified file name.
        /// </summary>
        /// <param name="name">The file name of the assembly to be loaded. This can be a relative or absolute path.</param>
        /// <remarks>
        /// The constructor sets the file name that will be used to locate and load the assembly
        /// when the <see cref="Load"/> method is invoked. If the file name is not rooted, it attempts to resolve
        /// the path relative to the runtime's base directory.
        /// </remarks>
        public AssemblyLoaderFromFile(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Loads an assembly from the file specified during the construction of the <see cref="AssemblyLoaderFromFile"/> instance.
        /// </summary>
        /// <returns>
        /// The loaded <see cref="Assembly"/> instance.
        /// </returns>
        /// <remarks>
        /// This method attempts to resolve the file path of the assembly if it is not rooted. It first checks the runtime's base directory
        /// and then attempts to resolve the full path. If the file cannot be found, it uses the original file name.
        /// </remarks>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the specified assembly file does not exist.
        /// </exception>
        /// <exception cref="BadImageFormatException">
        /// Thrown if the file is not a valid assembly or was compiled with a later version of the runtime than is currently loaded.
        /// </exception>
        /// <exception cref="FileLoadException">
        /// Thrown if the assembly is found but cannot be loaded.
        /// </exception>
        public Assembly Load()
        {
            string fileName = _name;
            if (!Path.IsPathRooted(fileName))
            {
                fileName = Path.Combine(RuntimeHost.Current.BaseDirectory, _name);
                if (!File.Exists(fileName))
                {
                    fileName = Path.GetFullPath(_name);
                    if (!File.Exists(fileName))
                    {
                        fileName = _name;
                    }
                }
            }

            Assembly assembly = Assembly.LoadFrom(fileName);
            return assembly;
        }
    }
}
