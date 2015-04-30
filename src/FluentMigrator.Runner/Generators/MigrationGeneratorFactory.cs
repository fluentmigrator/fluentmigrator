using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Generators
{ 
     public class MigrationGeneratorFactory
    {
         private static readonly IDictionary<string, IMigrationGenerator> MigrationGenerators;

        static MigrationGeneratorFactory()
        {
            Assembly assembly = typeof (IMigrationProcessorFactory).Assembly;

            List<Type> types = assembly
                .GetExportedTypes()
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
                    //can't add generators that require construtor parameters
                }
            }

            MigrationGenerators = available;
        }

        public virtual IMigrationGenerator GetGenerator(string name)
        {
            return MigrationGenerators
                .Where(pair => pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Select(pair => pair.Value)
                .FirstOrDefault();
        }

        public string ListAvailableProcessorTypes()
        {
            return string.Join(", ", MigrationGenerators.Keys.ToArray());
        }
    }
}
