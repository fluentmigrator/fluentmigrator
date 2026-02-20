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
using System.Diagnostics;
using System.IO;
using System.Reflection;

using FluentMigrator.Runner.Infrastructure.Hosts;

namespace FluentMigrator.Runner.Infrastructure
{
    /// <summary>
    /// Provides functionality to interact with the runtime environment, including locating and retrieving assemblies.
    /// </summary>
    /// <remarks>
    /// This static class serves as an abstraction layer for runtime-specific operations, such as finding assemblies
    /// in the Global Assembly Cache (GAC) or other runtime-specific directories. It is designed to support
    /// different runtime environments by utilizing an internal host abstraction.
    /// </remarks>
#if NET
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This type uses the AppDomain to load assemblies, which may not be preserved in trimmed applications.")]
#endif
    public static class RuntimeHost
    {
        private static readonly string[] _noNames = new string[0];

#if NETFRAMEWORK
        private static readonly IHostAbstraction _currentHost = new NetFrameworkHost();
#else
        private static readonly IHostAbstraction _currentHost = new NetCoreHost();
#endif

        /// <summary>
        /// Gets the current host abstraction used by the runtime environment.
        /// </summary>
        /// <remarks>
        /// This property provides access to the runtime-specific implementation of <see cref="IHostAbstraction"/>.
        /// It is used to perform operations such as retrieving the base directory, creating instances of types,
        /// and accessing loaded assemblies in the current runtime environment.
        /// </remarks>
        /// <value>
        /// An instance of <see cref="IHostAbstraction"/> representing the current runtime host.
        /// </value>
        public static IHostAbstraction Current => _currentHost;

        /// <summary>
        /// Finds and retrieves all assemblies available in the runtime environment.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="AssemblyName"/> objects representing the assemblies found.
        /// </returns>
        /// <remarks>
        /// This method searches for assemblies in runtime-specific directories, such as the Global Assembly Cache (GAC)
        /// on Windows or equivalent locations in other environments. It iterates through directories and retrieves
        /// assembly metadata for each discovered assembly.
        /// </remarks>
        public static IEnumerable<AssemblyName> FindAssemblies()
        {
            foreach (var fullGacDirectory in GetFullGacDirectories())
            {
                foreach (var directory in Directory.EnumerateDirectories(fullGacDirectory))
                {
                    var asmBaseName = Path.GetFileName(directory);
                    foreach (var asmName in GetAssemblyNames(fullGacDirectory, asmBaseName))
                    {
                        yield return asmName;
                    }
                }
            }
        }

        /// <summary>
        /// Finds and retrieves assembly names from the runtime environment that match the specified name.
        /// </summary>
        /// <param name="name">The name of the assembly to search for.</param>
        /// <returns>
        /// An enumerable collection of <see cref="AssemblyName"/> objects representing the assemblies
        /// that match the specified name.
        /// </returns>
        /// <remarks>
        /// This method filters the assemblies found by <see cref="FindAssemblies()"/> to include only those
        /// whose names match the specified <paramref name="name"/> (case-insensitive).
        /// </remarks>
        public static IEnumerable<AssemblyName> FindAssemblies(string name)
        {
            foreach (var assemblyName in FindAssemblies())
            {
                if (string.Equals(assemblyName.Name, name, StringComparison.OrdinalIgnoreCase))
                    yield return assemblyName;
            }
        }

        /// <summary>
        /// Retrieves the assembly names from a specified directory within the Global Assembly Cache (GAC).
        /// </summary>
        /// <param name="fullGacDirectory">
        /// The full path to the GAC directory where the assemblies are located.
        /// </param>
        /// <param name="assemblyName">
        /// The base name of the assembly to search for within the specified GAC directory.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="AssemblyName"/> objects representing the assemblies found
        /// in the specified directory.
        /// </returns>
        /// <remarks>
        /// This method iterates through subdirectories of the specified GAC directory, parsing metadata such as
        /// version, culture, and public key token to construct <see cref="AssemblyName"/> instances for each assembly.
        /// Invalid or incomplete metadata is ignored.
        /// </remarks>
        private static IEnumerable<AssemblyName> GetAssemblyNames(string fullGacDirectory, string assemblyName)
        {
            foreach (var fullPath in Directory.EnumerateDirectories(Path.Combine(fullGacDirectory, assemblyName)))
            {
                var versionInfo = Path.GetFileName(fullPath);
                // Console.WriteLine(versionInfo);

                Version asmVersion;
                string culture;
                string pkToken;

                Debug.Assert(versionInfo != null, nameof(versionInfo) + " != null");
                var parts = versionInfo.Split('_');
                if (parts.Length < 3)
                    continue;

                try
                {
                    if (!parts[0].StartsWith("v"))
                    {
                        asmVersion = Version.Parse(parts[0]);
                        culture = parts[1];
                        pkToken = parts[2];
                    }
                    else
                    {
                        asmVersion = Version.Parse(parts[1]);
                        culture = parts[2];
                        pkToken = parts[3];
                    }
                }
                catch
                {
                    // Ignore errors
                    continue;
                }

                if (string.IsNullOrEmpty(culture))
                {
                    culture = "neutral";
                }

                var asmName = $"{assemblyName}, Version={asmVersion}, Culture={culture}, PublicKeyToken={pkToken}";
                yield return new AssemblyName(asmName);
            }
        }

        /// <summary>
        /// Retrieves the full paths of directories containing the Global Assembly Cache (GAC) or equivalent locations
        /// based on the current runtime environment.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="string"/> objects representing the full paths of GAC directories
        /// or equivalent runtime-specific directories.
        /// </returns>
        /// <remarks>
        /// This method determines the appropriate directories to search for assemblies based on the operating system
        /// and runtime environment. On Windows, it locates GAC directories using the <c>WINDIR</c> environment variable.
        /// On Mono, it identifies the GAC directory relative to the framework's base directory.
        /// If no suitable directories are found, an empty collection is returned.
        /// </remarks>
        private static IEnumerable<string> GetFullGacDirectories()
        {
            var winDir = Environment.GetEnvironmentVariable("WINDIR");
            if (!string.IsNullOrEmpty(winDir))
            {
                return GetFullGacDirectoriesOnWindows(winDir);
            }

            var asmPath = typeof(int).Assembly.Location;
            var isMono = asmPath.Contains("/mono/");
            if (!isMono)
                return _noNames;

            var frameworkDir = Path.GetDirectoryName(asmPath);
            var frameworkBaseDir = Path.GetDirectoryName(frameworkDir);
            if (frameworkBaseDir == null)
                return _noNames;

            var gacDir = Path.Combine(frameworkBaseDir, "gac");
            if (!Directory.Exists(gacDir))
                return _noNames;

            return new[] { gacDir };
        }

        /// <summary>
        /// Retrieves the full paths of Global Assembly Cache (GAC) directories on Windows.
        /// </summary>
        /// <param name="winDir">The Windows directory path, typically obtained from the <c>WINDIR</c> environment variable.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="string"/> containing the full paths to the GAC directories.
        /// </returns>
        /// <remarks>
        /// This method constructs the full paths to the GAC directories based on the provided Windows directory path.
        /// It iterates through predefined GAC directory names and combines them with the base .NET assembly paths.
        /// Only directories that exist on the file system are included in the result.
        /// </remarks>
        private static IEnumerable<string> GetFullGacDirectoriesOnWindows(string winDir)
        {
            var netAssemblyPaths = new []
            {
                Path.Combine(winDir, "Microsoft.NET", "assembly"),
                Path.Combine(winDir, "assembly"),
            };

            var gacDirs = GetGacDirectories();
            foreach (var netAssemblyPath in netAssemblyPaths)
            {
                foreach (var gacDir in gacDirs)
                {
                    var fullGacDir = Path.Combine(netAssemblyPath, gacDir);
                    if (!Directory.Exists(fullGacDir))
                        continue;
                    yield return fullGacDir;
                }
            }
        }

        /// <summary>
        /// Retrieves the names of directories used by the Global Assembly Cache (GAC) in the current runtime environment.
        /// </summary>
        /// <returns>
        /// An array of strings representing the names of the GAC directories specific to the current runtime environment.
        /// </returns>
        /// <remarks>
        /// This method determines the appropriate GAC directories based on whether the process is running in a 64-bit or 32-bit environment.
        /// For 64-bit processes, it includes directories such as "GAC_MSIL", "GAC_64", and others.
        /// For 32-bit processes, it includes directories such as "GAC_MSIL", "GAC_32", and others.
        /// </remarks>
        private static string[] GetGacDirectories()
        {
            if (Environment.Is64BitProcess)
                return new[] { "GAC_MSIL", "GAC_64", "NativeImages_v2.0.50727_64", "GAC" };
            return new[] { "GAC_MSIL", "GAC_32", "NativeImages_v2.0.50727_32", "GAC" };
        }
    }
}
