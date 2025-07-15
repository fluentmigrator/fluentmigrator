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

    public class ReflectionBasedDbFactory : DbFactoryBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TestEntry[] _testEntries;

        private DbProviderFactory _instance;

        [Obsolete]
        public ReflectionBasedDbFactory(string assemblyName, string dbProviderFactoryTypeName)
            : this(new TestEntry(assemblyName, dbProviderFactoryTypeName, () => Type.GetType($"{dbProviderFactoryTypeName}, {assemblyName}")))
        {
        }

        [Obsolete]
        protected ReflectionBasedDbFactory(params TestEntry[] testEntries)
        {
            if (testEntries.Length == 0)
            {
                throw new ArgumentException(@"At least one test entry must be specified", nameof(testEntries));
            }

            _testEntries = testEntries;
        }

        protected ReflectionBasedDbFactory(IServiceProvider serviceProvider, params TestEntry[] testEntries)
        {
            if (testEntries.Length == 0)
            {
                throw new ArgumentException(@"At least one test entry must be specified", nameof(testEntries));
            }

            _serviceProvider = serviceProvider;
            _testEntries = testEntries;
        }

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

        protected static bool TryCreateFactory(
            [CanBeNull] IServiceProvider serviceProvider,
            [NotNull, ItemNotNull] IEnumerable<TestEntry> entries,
            [NotNull, ItemNotNull] ICollection<Exception> exceptions,
            out DbProviderFactory factory)
        {
            var entriesCollection = entries.ToList();

            foreach (var item in entriesCollection)
            {
                if (TryCreateFromPreloadedType(exceptions, item, out factory))
                {
                    return true;
                }
            }

#if NET
            if (!System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
            {
                // Dynamic code is not supported, so we cannot use reflection to load types.
                // This is a limitation of the current environment (e.g., AOT compilation).
                factory = null;
                return false;
            }
#endif

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

        protected static bool TryCreateFromPreloadedType(
            ICollection<Exception> exceptions,
            TestEntry item,
            out DbProviderFactory factory)
        {
            var type = item.TypeFactory?.Invoke();

            if (type == null)
            {
                factory = null;
                return false;
            }

            try
            {
#pragma warning disable IL2072 // The constructor has the attribute DynamicallyAccessedMembersAttribute
                if (TryGetInstance(type, out factory))
                {
                    return true;
                }

                factory = (DbProviderFactory) Activator.CreateInstance(type);
                return true;
#pragma warning restore IL2072
            }
            catch (Exception ex)
            {
                // Ignore, check if we could load the assembly
                exceptions.Add(new Exception($"Failed to create instance of {item.DBProviderFactoryTypeName} from {item.AssemblyName}", ex));
            }

            factory = null;
            return false;
        }

#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to load DbProviderFactory types, which may not be preserved in trimmed applications.")]
#endif
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

#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to load DbProviderFactory types, which may not be preserved in trimmed applications.")]
#endif
        [Obsolete]
        protected static bool TryCreateFactoryFromRuntimeHost(
            [NotNull] TestEntry entry,
            [NotNull, ItemNotNull] ICollection<Exception> exceptions,
            out DbProviderFactory factory)
        {
            return TryCreateFactoryFromRuntimeHost(entry, exceptions, serviceProvider: null, out factory);
        }

#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to load DbProviderFactory types, which may not be preserved in trimmed applications.")]
#endif
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

#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to load DbProviderFactory types, which may not be preserved in trimmed applications.")]
#endif
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

#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to load DbProviderFactory types, which may not be preserved in trimmed applications.")]
#endif
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

#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to load DbProviderFactory types, which may not be preserved in trimmed applications.")]
#endif
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

#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This method uses reflection to load DbProviderFactory types, which may not be preserved in trimmed applications.")]
#endif
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

        private static bool TryGetInstance(
#if NET
            [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
            [CanBeNull] Type factoryType,
            out DbProviderFactory factory)
        {
            if (factoryType == null)
            {
                factory = null;
                return false;
            }

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

        private static bool TryCastInstance(
            [CanBeNull] object value,
            out DbProviderFactory factory)
        {
            factory = value as DbProviderFactory;
            return factory != null;
        }

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
#if NET
        [return: System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        protected delegate Type GetTypeDelegate();

        protected class TestEntry
        {
            public TestEntry(
                [NotNull] string assemblyName,
                [NotNull] string dbProviderFactoryTypeName,
                [CanBeNull] GetTypeDelegate typeFactory)
            {
                AssemblyName = assemblyName;
                DBProviderFactoryTypeName = dbProviderFactoryTypeName;
                TypeFactory = typeFactory;
            }

            [NotNull]
            public string AssemblyName { get; }

            [NotNull]
            public string DBProviderFactoryTypeName { get; }

            [CanBeNull]
            public GetTypeDelegate TypeFactory { get; }
        }
    }
}
