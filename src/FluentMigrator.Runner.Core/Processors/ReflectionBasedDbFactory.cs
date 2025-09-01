#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using System.IO;
using System.Linq;
using System.Reflection;

using FluentMigrator.Runner.Infrastructure;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Processors
{
    using System.Data.Common;

    /// <summary>
    /// Provides a database factory implementation that uses reflection to dynamically load database provider factories.
    /// </summary>
    /// <remarks>
    /// This class serves as a base for creating database provider factories by attempting to load assemblies and types
    /// dynamically at runtime. It supports scenarios where database drivers are not directly referenced in the project
    /// but need to be loaded from external assemblies.
    /// </remarks>
    public class ReflectionBasedDbFactory : DbFactoryBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TestEntry[] _testEntries;

        private DbProviderFactory _instance;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="testEntries"></param>
        /// <exception cref="ArgumentException"></exception>
        protected ReflectionBasedDbFactory(IServiceProvider serviceProvider, params TestEntry[] testEntries)
        {
            if (testEntries.Length == 0)
            {
                throw new ArgumentException(@"At least one test entry must be specified", nameof(testEntries));
            }

            _serviceProvider = serviceProvider;
            _testEntries = testEntries;
        }

        /// <summary>
        /// Creates and returns an instance of <see cref="DbProviderFactory"/>.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="DbProviderFactory"/>.
        /// </returns>
        /// <exception cref="AggregateException">
        /// Thrown when the factory cannot be created. This exception contains details about all 
        /// the errors encountered during the factory creation process.
        /// </exception>
        protected override DbProviderFactory CreateFactory()
        {
            if (_instance != null)
            {
                return _instance;
            }

            var exceptions = new List<Exception>();
            if (TryCreateFactory(_serviceProvider, _testEntries, exceptions, out var factory))
            {
                _instance = factory;
                return factory;
            }

            var assemblyNames = string.Join(", ", _testEntries.Select(x => x.AssemblyName));
            var fullExceptionOutput = string.Join(Environment.NewLine, exceptions.Select(x => x.ToString()));

            throw new AggregateException($"Unable to load the driver. Attempted to load: {assemblyNames}, with {fullExceptionOutput}", exceptions);
        }

        [Obsolete]
        protected static bool TryCreateFactory(
            [NotNull, ItemNotNull] IEnumerable<TestEntry> entries,
            [NotNull, ItemNotNull] ICollection<Exception> exceptions,
            out DbProviderFactory factory)
        {
            return TryCreateFactory(serviceProvider: null, entries, exceptions, out factory);
        }

        /// <summary>
        /// Attempts to create a <see cref="DbProviderFactory"/> instance by dynamically loading assemblies and types.
        /// </summary>
        /// <param name="serviceProvider">
        /// An optional <see cref="IServiceProvider"/> instance that can be used to resolve dependencies during the factory creation process.
        /// </param>
        /// <param name="entries">
        /// A collection of <see cref="TestEntry"/> objects representing the assemblies and types to be tested for creating the factory.
        /// </param>
        /// <param name="exceptions">
        /// A collection to store any exceptions encountered during the factory creation process.
        /// </param>
        /// <param name="factory">
        /// When this method returns, contains the created <see cref="DbProviderFactory"/> instance if the creation was successful;
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if a <see cref="DbProviderFactory"/> instance was successfully created; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method iterates through the provided <paramref name="entries"/> and attempts to create a factory using various strategies,
        /// such as loading from the current application domain, runtime host, or GAC. If none of the strategies succeed, the method
        /// returns <c>false</c>.
        /// </remarks>
        protected static bool TryCreateFactory(
            [CanBeNull] IServiceProvider serviceProvider,
            [NotNull, ItemNotNull] IEnumerable<TestEntry> entries,
            [NotNull, ItemNotNull] ICollection<Exception> exceptions,
            out DbProviderFactory factory)
        {
            var entriesCollection = entries.ToList();

            foreach (var entry in entriesCollection)
            {
                if (TryCreateFromCurrentDomain(entry, exceptions, out factory))
                {
                    return true;
                }
            }

            foreach (var entry in entriesCollection)
            {
                if (TryCreateFactoryFromRuntimeHost(entry, exceptions, serviceProvider, out factory))
                {
                    return true;
                }
            }

            foreach (var entry in entriesCollection)
            {
                if (TryCreateFromAppDomainPaths(entry, exceptions, out factory))
                {
                    return true;
                }
            }

            foreach (var entry in entriesCollection)
            {
                if (TryCreateFromGac(entry, exceptions, out factory))
                {
                    return true;
                }
            }

            factory = null;
            return false;
        }

        /// <summary>
        /// Attempts to create a <see cref="DbProviderFactory"/> instance by searching for the specified assembly and type
        /// within the application domain's paths.
        /// </summary>
        /// <param name="entry">
        /// The <see cref="TestEntry"/> containing the assembly name and the database provider factory type name to locate.
        /// </param>
        /// <param name="exceptions">
        /// A collection to which any exceptions encountered during the process will be added.
        /// </param>
        /// <param name="factory">
        /// When this method returns, contains the created <see cref="DbProviderFactory"/> instance if the operation succeeded;
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="DbProviderFactory"/> was successfully created; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method dynamically attempts to load the specified assembly and type from the application domain's directories.
        /// If the assembly or type cannot be loaded, or if an exception occurs during instantiation, the method will return
        /// <c>false</c> and add the encountered exceptions to the <paramref name="exceptions"/> collection.
        /// </remarks>
        protected static bool TryCreateFromAppDomainPaths(
            [NotNull] TestEntry entry,
            [NotNull, ItemNotNull] ICollection<Exception> exceptions,
            out DbProviderFactory factory)
        {
            if (TryLoadAssemblyFromAppDomainDirectories(entry.AssemblyName, exceptions, out var assembly))
            {
                try
                {
                    var type = assembly.GetType(entry.DBProviderFactoryTypeName, true);
                    if (TryGetInstance(type, out factory))
                    {
                        return true;
                    }

                    factory = (DbProviderFactory) Activator.CreateInstance(type);
                    return true;
                }
                catch (Exception ex)
                {
                    // Ignore
                    exceptions.Add(ex);
                }
            }

            factory = null;
            return false;
        }

        [Obsolete]
        protected static bool TryCreateFactoryFromRuntimeHost(
            [NotNull] TestEntry entry,
            [NotNull, ItemNotNull] ICollection<Exception> exceptions,
            out DbProviderFactory factory)
        {
            return TryCreateFactoryFromRuntimeHost(entry, exceptions, serviceProvider: null, out factory);
        }

        /// <summary>
        /// Attempts to create a <see cref="DbProviderFactory"/> instance using the runtime host.
        /// </summary>
        /// <param name="entry">
        /// The <see cref="TestEntry"/> containing the assembly name and database provider factory type name.
        /// </param>
        /// <param name="exceptions">
        /// A collection to store any exceptions encountered during the creation process.
        /// </param>
        /// <param name="serviceProvider">
        /// An optional <see cref="IServiceProvider"/> used to resolve dependencies during factory creation.
        /// </param>
        /// <param name="factory">
        /// When this method returns, contains the created <see cref="DbProviderFactory"/> instance if the operation succeeded;
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the factory was successfully created; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method uses the runtime host to dynamically load the specified assembly and create an instance of the
        /// database provider factory. If the operation fails, it attempts to create the factory from the current application
        /// domain. Any exceptions encountered during the process are added to the <paramref name="exceptions"/> collection.
        /// </remarks>
        protected static bool TryCreateFactoryFromRuntimeHost(
            [NotNull] TestEntry entry,
            [NotNull, ItemNotNull] ICollection<Exception> exceptions,
            [CanBeNull] IServiceProvider serviceProvider,
            out DbProviderFactory factory)
        {
            try
            {
                factory = (DbProviderFactory)RuntimeHost.Current.CreateInstance(
                    serviceProvider,
                    entry.AssemblyName,
                    entry.DBProviderFactoryTypeName);
                return true;
            }
            catch (Exception ex)
            {
                // Ignore, check if we could load the assembly
                exceptions.Add(ex);
            }

            // Try to create from current domain in case of a successfully loaded assembly
            return TryCreateFromCurrentDomain(entry, exceptions, out factory);
        }

        /// <summary>
        /// Attempts to load an assembly with the specified name from the directories associated with the current application domain.
        /// </summary>
        /// <param name="assemblyName">
        /// The name of the assembly to load. This parameter must not be <c>null</c>.
        /// </param>
        /// <param name="exceptions">
        /// A collection to which any exceptions encountered during the loading process will be added. This parameter must not be <c>null</c>.
        /// </param>
        /// <param name="assembly">
        /// When this method returns, contains the loaded <see cref="Assembly"/> if the operation was successful; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the assembly was successfully loaded; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method searches the directories associated with the current application domain for the specified assembly.
        /// If the assembly cannot be found or loaded, any exceptions encountered will be added to the <paramref name="exceptions"/> collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="assemblyName"/> or <paramref name="exceptions"/> is <c>null</c>.
        /// </exception>
        protected static bool TryLoadAssemblyFromAppDomainDirectories(
            [NotNull] string assemblyName,
            [NotNull, ItemNotNull] ICollection<Exception> exceptions,
            out Assembly assembly)
        {
            return TryLoadAssemblyFromDirectories(
                GetPathsFromAppDomain(),
                assemblyName,
                exceptions,
                out assembly);
        }

        /// <summary>
        /// Attempts to load an assembly with the specified name from a collection of directories.
        /// </summary>
        /// <param name="directories">
        /// A collection of directory paths to search for the assembly.
        /// </param>
        /// <param name="assemblyName">
        /// The name of the assembly to load, without the file extension.
        /// </param>
        /// <param name="exceptions">
        /// A collection to which any exceptions encountered during the loading process will be added.
        /// </param>
        /// <param name="assembly">
        /// When this method returns, contains the loaded <see cref="System.Reflection.Assembly"/> if the operation was successful;
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the assembly was successfully loaded; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method iterates through the provided directories, attempting to locate and load the specified assembly.
        /// If the assembly cannot be loaded from any of the directories, the method returns <c>false</c> and populates
        /// the <paramref name="exceptions"/> collection with details of the encountered errors.
        /// </remarks>
        protected static bool TryLoadAssemblyFromDirectories(
            [NotNull, ItemNotNull] IEnumerable<string> directories,
            [NotNull] string assemblyName,
            [NotNull, ItemNotNull] ICollection<Exception> exceptions,
            out Assembly assembly)
        {
            var alreadyTested = new HashSet<string>(StringComparer.InvariantCulture);
            var assemblyFileName = $"{assemblyName}.dll";
            foreach (var directory in directories)
            {
                var path = Path.Combine(directory, assemblyFileName);
                if (!alreadyTested.Add(path))
                {
                    continue;
                }

                try
                {
                    assembly = Assembly.LoadFile(path);
                    return true;
                }
                catch (Exception ex)
                {
                    exceptions.Add(new Exception($"Failed to load file {path}", ex));
                }
            }

            assembly = null;
            return false;
        }

        /// <summary>
        /// Attempts to create a <see cref="DbProviderFactory"/> instance by loading the specified assembly from the Global Assembly Cache (GAC).
        /// </summary>
        /// <param name="entry">
        /// The <see cref="TestEntry"/> containing the assembly name and the type name of the database provider factory to load.
        /// </param>
        /// <param name="exceptions">
        /// A collection to which any exceptions encountered during the process will be added.
        /// </param>
        /// <param name="factory">
        /// When this method returns, contains the created <see cref="DbProviderFactory"/> instance if the operation succeeded;
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="DbProviderFactory"/> was successfully created; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method searches the GAC for the specified assembly, loads it, and attempts to create an instance of the
        /// specified database provider factory type. If the operation fails, any exceptions encountered are added to the
        /// <paramref name="exceptions"/> collection.
        /// </remarks>
        private static bool TryCreateFromGac(
            [NotNull] TestEntry entry,
            [NotNull, ItemNotNull] ICollection<Exception> exceptions,
            out DbProviderFactory factory)
        {
            var asmNames = FindAssembliesInGac(entry.AssemblyName);
            var asmName = asmNames.OrderByDescending(n => n.Version).FirstOrDefault();

            if (asmName == null)
            {
                factory = null;
                return false;
            }

            try
            {
                var assembly = Assembly.Load(asmName);
                var type = assembly.GetType(entry.DBProviderFactoryTypeName, true);
                if (TryGetInstance(type, out factory))
                {
                    return true;
                }

                factory = (DbProviderFactory) Activator.CreateInstance(type);
                return true;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                factory = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to create a <see cref="DbProviderFactory"/> instance from the currently loaded assemblies in the application domain.
        /// </summary>
        /// <param name="entry">
        /// The <see cref="TestEntry"/> containing the assembly name and the type name of the database provider factory to load.
        /// </param>
        /// <param name="exceptions">
        /// A collection to which any exceptions encountered during the process will be added.
        /// </param>
        /// <param name="factory">
        /// When this method returns, contains the created <see cref="DbProviderFactory"/> instance if the operation was successful; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="DbProviderFactory"/> was successfully created; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method attempts to load the specified assembly and type from the currently loaded assemblies in the application domain.
        /// If successful, it creates an instance of the specified type as a <see cref="DbProviderFactory"/>.
        /// Any exceptions encountered during the process are added to the <paramref name="exceptions"/> collection.
        /// </remarks>
        private static bool TryCreateFromCurrentDomain(
            [NotNull] TestEntry entry,
            [NotNull, ItemNotNull] ICollection<Exception> exceptions,
            out DbProviderFactory factory)
        {
            if (TryLoadAssemblyFromCurrentDomain(entry.AssemblyName, exceptions, out var assembly))
            {
                try
                {
                    var type = assembly.GetType(entry.DBProviderFactoryTypeName, true);
                    if (TryGetInstance(type, out factory))
                    {
                        return true;
                    }

                    factory = (DbProviderFactory) Activator.CreateInstance(type);
                    return true;
                }
                catch (Exception ex)
                {
                    // Ignore
                    exceptions.Add(ex);
                }
            }

            factory = null;
            return false;
        }

        /// <summary>
        /// Attempts to load an assembly with the specified name from the current application domain.
        /// </summary>
        /// <param name="assemblyName">
        /// The name of the assembly to load. This parameter must not be <c>null</c>.
        /// </param>
        /// <param name="exceptions">
        /// A collection to which any exceptions encountered during the loading process will be added. 
        /// This parameter must not be <c>null</c>.
        /// </param>
        /// <param name="assembly">
        /// When this method returns, contains the loaded <see cref="Assembly"/> if the operation was successful; 
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the assembly was successfully loaded; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method attempts to load an assembly by its name from the current application domain. 
        /// If the assembly cannot be loaded, any exceptions encountered during the process are added 
        /// to the <paramref name="exceptions"/> collection, and the method returns <c>false</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="assemblyName"/> or <paramref name="exceptions"/> is <c>null</c>.
        /// </exception>
        private static bool TryLoadAssemblyFromCurrentDomain(
            [NotNull] string assemblyName,
            [NotNull, ItemNotNull] ICollection<Exception> exceptions,
            out Assembly assembly)
        {
            try
            {
                assembly = AppDomain.CurrentDomain.Load(new AssemblyName(assemblyName));
                return true;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                assembly = null;
                return false;
            }
        }

        /// <summary>
        /// Finds and retrieves assembly names from the Global Assembly Cache (GAC) that match the specified names.
        /// </summary>
        /// <param name="names">An array of assembly names to search for in the GAC.</param>
        /// <returns>
        /// An enumerable collection of <see cref="AssemblyName"/> objects representing the assemblies found in the GAC
        /// that match the specified names.
        /// </returns>
        /// <remarks>
        /// This method utilizes the <see cref="RuntimeHost.FindAssemblies(string)"/> method to locate assemblies in the GAC.
        /// It iterates through the provided assembly names and yields matching assembly names found in the GAC.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="names"/> parameter is <c>null</c>.</exception>
        [NotNull, ItemNotNull]
        private static IEnumerable<AssemblyName> FindAssembliesInGac([NotNull, ItemNotNull] params string[] names)
        {
            foreach (var name in names)
            {
                foreach (var assemblyName in RuntimeHost.FindAssemblies(name))
                {
                    yield return assemblyName;
                }
            }
        }

        /// <summary>
        /// Attempts to retrieve an instance of a <see cref="System.Data.Common.DbProviderFactory"/> from the specified factory type.
        /// </summary>
        /// <param name="factoryType">
        /// The <see cref="System.Type"/> representing the factory type from which to retrieve the instance.
        /// </param>
        /// <param name="factory">
        /// When this method returns, contains the <see cref="System.Data.Common.DbProviderFactory"/> instance if the operation succeeds; otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if an instance of <see cref="System.Data.Common.DbProviderFactory"/> was successfully retrieved; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method attempts to retrieve the factory instance by inspecting the static "Instance" field or property
        /// of the specified factory type. If neither is found or accessible, the method returns <c>false</c>.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="factoryType"/> is <c>null</c>.
        /// </exception>
        private static bool TryGetInstance(
            [NotNull] Type factoryType,
            out DbProviderFactory factory)
        {
            var instanceField = factoryType.GetField(
                "Instance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);

            if (instanceField != null && TryCastInstance(instanceField.GetValue(null), out factory))
            {
                return true;
            }

            var instanceProperty = factoryType.GetProperty(
                "Instance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);
            if (instanceProperty != null && TryCastInstance(instanceProperty.GetValue(null, null), out factory))
            {
                return true;
            }

            factory = null;
            return false;
        }

        /// <summary>
        /// Attempts to cast the provided object to a <see cref="DbProviderFactory"/> instance.
        /// </summary>
        /// <param name="value">The object to be cast.</param>
        /// <param name="factory">
        /// When this method returns, contains the <see cref="DbProviderFactory"/> instance if the cast was successful;
        /// otherwise, <c>null</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the object was successfully cast to a <see cref="DbProviderFactory"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="value"/> is <c>null</c>.</exception>
        private static bool TryCastInstance(
            [NotNull] object value,
            out DbProviderFactory factory)
        {
            factory = value as DbProviderFactory;
            return factory != null;
        }

        /// <summary>
        /// Retrieves a collection of directory paths from the assemblies loaded in the current application domain.
        /// </summary>
        /// <remarks>
        /// This method iterates through all assemblies currently loaded in the <see cref="AppDomain.CurrentDomain"/> 
        /// and extracts the directory paths of their locations. Assemblies without a valid location or dynamically 
        /// generated assemblies are skipped.
        /// </remarks>
        /// <returns>
        /// An enumerable collection of directory paths where the assemblies in the current application domain are located.
        /// </returns>
        /// <exception cref="AppDomainUnloadedException">
        /// Thrown if the application domain has been unloaded during the execution of this method.
        /// </exception>
        [NotNull, ItemNotNull]
        private static IEnumerable<string> GetPathsFromAppDomain()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyDirectory;
                try
                {
                    assemblyDirectory = Path.GetDirectoryName(assembly.Location);
                    if (assemblyDirectory == null)
                    {
                        continue;
                    }
                }
                catch
                {
                    // Ignore error caused by dynamic assembly
                    continue;
                }

                yield return assemblyDirectory;
            }
        }

        /// <summary>
        /// Represents an entry used for testing the creation of database provider factories via reflection.
        /// </summary>
        /// <remarks>
        /// This class encapsulates the necessary information, such as the assembly name and the database provider factory type name, 
        /// to dynamically load and create database provider factories at runtime. It is primarily used internally by 
        /// <see cref="ReflectionBasedDbFactory"/> and its derived classes.
        /// </remarks>
        protected class TestEntry
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReflectionBasedDbFactory.TestEntry"/> class.
            /// </summary>
            /// <param name="assemblyName">
            /// The name of the assembly containing the database provider factory type.
            /// </param>
            /// <param name="dbProviderFactoryTypeName">
            /// The fully qualified name of the database provider factory type.
            /// </param>
            /// <remarks>
            /// This constructor is used to create a test entry that encapsulates the information required to 
            /// dynamically load and create a database provider factory via reflection.
            /// </remarks>
            public TestEntry(
                [NotNull] string assemblyName,
                [NotNull] string dbProviderFactoryTypeName)
            {
                AssemblyName = assemblyName;
                DBProviderFactoryTypeName = dbProviderFactoryTypeName;
            }

            /// <summary>
            /// Gets the name of the assembly that contains the database provider factory.
            /// </summary>
            /// <remarks>
            /// This property provides the assembly name required to dynamically load the database provider factory 
            /// during runtime. It is used internally by the <see cref="ReflectionBasedDbFactory"/> to locate and 
            /// instantiate the appropriate factory.
            /// </remarks>
            [NotNull]
            public string AssemblyName { get; }

            /// <summary>
            /// Gets the fully qualified name of the database provider factory type.
            /// </summary>
            /// <remarks>
            /// This property specifies the type name of the database provider factory that is used to create 
            /// database connections via reflection. It is primarily utilized internally by the 
            /// <see cref="ReflectionBasedDbFactory"/> class to dynamically load and instantiate the appropriate 
            /// database provider factory at runtime.
            /// </remarks>
            [NotNull]
            public string DBProviderFactoryTypeName { get; }
        }
    }
}
