#region License
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Infrastructure;

namespace FluentMigrator.Runner.Processors
{
    [Obsolete]
    public class MigrationProcessorFactoryProvider
    {
        private static readonly object _lock = new object();
        private static IDictionary<string, IMigrationProcessorFactory> _migrationProcessorFactories;

        [Obsolete]
        static MigrationProcessorFactoryProvider()
        { }

        [Obsolete("Ony the statically provided factories are accessed")]
        public MigrationProcessorFactoryProvider()
        {
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

        public static IEnumerable<IMigrationProcessorFactory> RegisteredFactories
            => MigrationProcessorFactories.Values;

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

        [Obsolete("Ony the statically provided factories are accessed")]
        public virtual IMigrationProcessorFactory GetFactory(string name)
        {
            if (MigrationProcessorFactories.TryGetValue(name, out var result))
                return result;
            return null;
        }

        [Obsolete]
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
