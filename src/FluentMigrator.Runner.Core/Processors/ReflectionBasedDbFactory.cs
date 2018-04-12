#region License
// Copyright (c) 2007-2018, Fluent Migrator Project
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

namespace FluentMigrator.Runner.Processors
{
    using System.Data.Common;

    public class ReflectionBasedDbFactory : DbFactoryBase
    {
        private readonly TestEntry[] _testEntries;

        private DbProviderFactory _instance;

        public ReflectionBasedDbFactory(string assemblyName, string dbProviderFactoryTypeName)
            : this(new TestEntry(assemblyName, dbProviderFactoryTypeName))
        {
        }

        protected ReflectionBasedDbFactory(params TestEntry[] testEntries)
        {
            if (testEntries.Length == 0)
                throw new ArgumentException(nameof(testEntries), "At least one test entry must be specified");
            _testEntries = testEntries;
        }

        protected override DbProviderFactory CreateFactory()
        {
            if (_instance != null)
                return _instance;

            var exceptions = new List<Exception>();
            if (TryCreateFactory(_testEntries, exceptions, out var factory))
            {
                _instance = factory;
                return factory;
            }

            var assemblyNames = string.Join(", ", _testEntries.Select(x => x.AssemblyName));
            throw new AggregateException($"Unable to load the driver. Attempted to load: {assemblyNames}", exceptions);
        }

        protected static bool TryCreateFactory(IEnumerable<TestEntry> entries, ICollection<Exception> exceptions, out DbProviderFactory factory)
        {
            foreach (var entry in entries)
            {
                if (TryCreateFromCurrentDomain(entry, exceptions, out factory))
                    return true;
                if (TryCreateFactoryFromFile(entry, exceptions, out factory))
                    return true;
                if (TryCreateFromGac(entry, exceptions, out factory))
                    return true;
            }

            factory = null;
            return false;
        }

        protected static bool TryCreateFactoryFromFile(TestEntry entry, ICollection<Exception> exceptions, out DbProviderFactory factory)
        {
            try
            {
                factory = (DbProviderFactory) RuntimeHost.Current.CreateInstance(
                    serviceProvider: null,
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

        private static bool TryCreateFromGac(TestEntry entry, ICollection<Exception> exceptions, out DbProviderFactory factory)
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
                    return true;

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

        private static bool TryCreateFromCurrentDomain(TestEntry entry, ICollection<Exception> exceptions, out DbProviderFactory factory)
        {
            if (TryLoadAssemblyFromCurrentDomain(entry.AssemblyName, exceptions, out var assembly))
            {
                try
                {
                    var type = assembly.GetType(entry.DBProviderFactoryTypeName, true);
                    if (TryGetInstance(type, out factory))
                        return true;

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

        private static bool TryLoadAssemblyFromCurrentDomain(string assemblyName, ICollection<Exception> exceptions, out Assembly assembly)
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

        private static IEnumerable<AssemblyName> FindAssembliesInGac(params string[] names)
        {
            foreach (var name in names)
            {
                foreach (var assemblyName in RuntimeHost.FindAssemblies(name))
                {
                    yield return assemblyName;
                }
            }
        }

        private static bool TryGetInstance(Type factoryType, out DbProviderFactory factory)
        {
            var instanceField = factoryType.GetField(
                "Instance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField);

            if (instanceField != null && TryCastInstance(instanceField.GetValue(null), out factory))
                return true;

            var instanceProperty = factoryType.GetProperty(
                "Instance",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty);
            if (instanceProperty != null && TryCastInstance(instanceProperty.GetValue(null, null), out factory))
                return true;

            factory = null;
            return false;
        }

        private static bool TryCastInstance(object value, out DbProviderFactory factory)
        {
            factory = value as DbProviderFactory;
            return factory != null;
        }

        protected class TestEntry
        {
            public TestEntry(string assemblyName, string dbProviderFactoryTypeName)
            {
                AssemblyName = assemblyName;
                DBProviderFactoryTypeName = dbProviderFactoryTypeName;
            }

            public string AssemblyName { get; }
            public string DBProviderFactoryTypeName { get; }
        }
    }
}
