#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using System.Linq;

using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Generators
{
    [Obsolete]
    public class MigrationGeneratorFactory
    {
        private static readonly IDictionary<string, IMigrationGenerator> _migrationGenerators;

        static MigrationGeneratorFactory()
        {
            var assemblies = MigrationProcessorFactoryProvider.RegisteredFactories.Select(x => x.GetType().Assembly);

            var types = assemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(type => type.IsConcrete() && type.Is<IMigrationGenerator>())
                .ToList();

            var available = new SortedDictionary<string, IMigrationGenerator>();
            foreach (Type type in types)
            {
                try
                {
                    var factory = (IMigrationGenerator) Activator.CreateInstance(type);
                    available.Add(type.Name.Replace("Generator", ""), factory);
                }
                catch (Exception)
                {
                    //can't add generators that require constructor parameters
                }
            }

            _migrationGenerators = available;
        }

        [Obsolete("Ony the statically provided generators are accessed")]
        public MigrationGeneratorFactory()
        {
        }

        public static IEnumerable<IMigrationGenerator> RegisteredGenerators
            => _migrationGenerators.Values;

        [Obsolete("Ony the statically provided generators are accessed")]
        public virtual IMigrationGenerator GetGenerator(string name)
        {
            return _migrationGenerators
                   .Where(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
                   .Select(pair => pair.Value)
                   .FirstOrDefault();
        }

        [Obsolete("Ony the statically provided generators are accessed")]
        public string ListAvailableGeneratorTypes()
        {
            return string.Join(", ", _migrationGenerators.Keys.ToArray());
        }
    }
}
