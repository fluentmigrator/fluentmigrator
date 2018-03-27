#region License
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.Linq;
using System.Reflection;

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Processors.DB2;
using FluentMigrator.Runner.Processors.DotConnectOracle;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Runner.Processors.SQLite;

namespace FluentMigrator.Runner.Processors
{
    public class MigrationProcessorFactoryProvider
    {
        private static readonly object _lock = new object();
        private static IDictionary<string, IMigrationProcessorFactory> _migrationProcessorFactories;

        static MigrationProcessorFactoryProvider()
        {
            // Register all available processor factories. The library usually tries
            // to find all provider factories by scanning all referenced assemblies,
            // but this fails if we don't have any reference. Adding the package
            // isn't enough. We MUST have a reference to a type, otherwise the
            // assembly reference gets removed by the C# compiler!
            Register(new Db2ProcessorFactory());
            Register(new DotConnectOracleProcessorFactory());
            Register(new FirebirdProcessorFactory());
            Register(new MySql4ProcessorFactory());
            Register(new MySql5ProcessorFactory());
            Register(new OracleManagedProcessorFactory());
            Register(new OracleProcessorFactory());
            Register(new PostgresProcessorFactory());
            Register(new SQLiteProcessorFactory());
            Register(new SqlServer2000ProcessorFactory());
            Register(new SqlServer2005ProcessorFactory());
            Register(new SqlServer2008ProcessorFactory());
            Register(new SqlServer2012ProcessorFactory());
            Register(new SqlServer2014ProcessorFactory());
            Register(new SqlServerProcessorFactory());
            Register(new SqlServerCeProcessorFactory());

#if NET40 || NET45
            Register(new Hana.HanaProcessorFactory());
            Register(new Jet.JetProcessorFactory());
#endif
        }

        private static IDictionary<string, IMigrationProcessorFactory> MigrationProcessorFactories
        {
            get
            {
                lock (_lock)
                {
                    return _migrationProcessorFactories ?? (_migrationProcessorFactories = FindProcessorFactories());
                }
            }
        }

        public static void Register(IMigrationProcessorFactory factory)
        {
            lock (_lock)
            {
                if (_migrationProcessorFactories == null)
                {
                    _migrationProcessorFactories = new Dictionary<string, IMigrationProcessorFactory>(StringComparer.OrdinalIgnoreCase);
                }

                _migrationProcessorFactories[factory.Name] = factory;
            }
        }

        public static IEnumerable<string> ProcessorTypes
            => MigrationProcessorFactories.Keys;

        public virtual IMigrationProcessorFactory GetFactory(string name)
        {
            if (MigrationProcessorFactories.TryGetValue(name, out var result))
                return result;
            return null;
        }

        public string ListAvailableProcessorTypes()
        {
            return string.Join(", ", MigrationProcessorFactories.Keys.ToArray());
        }

        private static IDictionary<string, IMigrationProcessorFactory> FindProcessorFactories()
        {
            var availableMigrationProcessorFactories = new SortedDictionary<string, IMigrationProcessorFactory>(StringComparer.OrdinalIgnoreCase);

            foreach (var assembly in GetAssemblies())
            {
                List<Type> types = assembly
                    .GetExportedTypes()
                    .Where(type => type.IsConcrete() && type.Is<IMigrationProcessorFactory>())
                    .ToList();

                foreach (Type type in types)
                {
                    var factory = (IMigrationProcessorFactory)Activator.CreateInstance(type);
                    availableMigrationProcessorFactories.Add(factory.Name, factory);
                }
            }

            return availableMigrationProcessorFactories;
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            var initialAssemblies = RuntimeHost.Current.GetLoadedAssemblies()
                .Where(x => x.GetName().Name.StartsWith("FluentMigrator."));
            var remainingAssemblies = new Queue<Assembly>(initialAssemblies);
            var processedAssemblies = new HashSet<string>(remainingAssemblies.Select(x => x.GetName().Name), StringComparer.OrdinalIgnoreCase);

            while (remainingAssemblies.Count != 0)
            {
                var asm = remainingAssemblies.Dequeue();
                yield return asm;

                var refAsms = asm.GetReferencedAssemblies().Where(x => x.Name.StartsWith("FluentMigrator."));
                foreach (var refAsm in refAsms)
                {
                    if (processedAssemblies.Add(refAsm.Name))
                    {
                        remainingAssemblies.Enqueue(Assembly.Load(refAsm));
                    }
                }
            }
        }
    }
}
