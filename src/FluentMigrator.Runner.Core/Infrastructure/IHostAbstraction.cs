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
using System.Collections.Generic;
using System.Reflection;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Infrastructure
{
    /// <summary>
    /// Defines an abstraction for interacting with the host environment, providing functionality
    /// for retrieving the base directory, creating instances of types, and accessing loaded assemblies.
    /// </summary>
    /// <remarks>
    /// This interface serves as a contract for host-specific operations, enabling interaction with
    /// the underlying runtime environment. Implementations of this interface are responsible for
    /// providing runtime-specific behavior, such as resolving assemblies and creating object instances.
    /// </remarks>
    public interface IHostAbstraction
    {
        /// <summary>
        /// Gets the base directory of the host environment.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the absolute path to the base directory
        /// where the application is running. This directory is typically used as the
        /// root location for resolving relative paths.
        /// </value>
        /// <remarks>
        /// The base directory is determined by the specific implementation of the 
        /// <see cref="IHostAbstraction"/> interface. For example, in a .NET Framework 
        /// environment, it corresponds to the <see cref="AppDomain.BaseDirectory"/>.
        /// </remarks>
        string BaseDirectory { get; }

        /// <summary>
        /// Creates an instance of a specified type from a given assembly, optionally using a service provider for dependency resolution.
        /// </summary>
        /// <param name="serviceProvider">
        /// An optional <see cref="IServiceProvider"/> used to resolve dependencies for the created instance.
        /// If <c>null</c>, the instance will be created without dependency injection.
        /// </param>
        /// <param name="assemblyName">
        /// The name of the assembly containing the type to be instantiated. This parameter cannot be <c>null</c>.
        /// </param>
        /// <param name="typeName">
        /// The fully qualified name of the type to be instantiated. This parameter cannot be <c>null</c>.
        /// </param>
        /// <returns>
        /// An instance of the specified type.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="assemblyName"/> or <paramref name="typeName"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="TypeLoadException">
        /// Thrown if the specified type cannot be found in the given assembly.
        /// </exception>
        /// <exception cref="MissingMethodException">
        /// Thrown if the specified type does not have a parameterless constructor.
        /// </exception>
        /// <remarks>
        /// This method provides a mechanism to dynamically create instances of types at runtime, which can be useful
        /// for scenarios such as plugin loading or dependency injection in loosely coupled architectures.
        /// </remarks>
        [NotNull]
        object CreateInstance([CanBeNull] IServiceProvider serviceProvider, [NotNull] string assemblyName, [NotNull] string typeName);

        /// <summary>
        /// Retrieves the assemblies currently loaded in the application domain.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Assembly"/> objects representing the loaded assemblies.
        /// </returns>
        /// <remarks>
        /// This method provides access to the assemblies that have been loaded into the current application domain.
        /// It can be used for tasks such as reflection, type discovery, or plugin management.
        /// </remarks>
        [NotNull, ItemNotNull]
        IEnumerable<Assembly> GetLoadedAssemblies();
    }
}
