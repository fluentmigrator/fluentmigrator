#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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

using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;

using IoC;

namespace FluentMigrator.Runner
{
    public class DefaultConventionSet : IConventionSet
    {
        public DefaultConventionSet(IRunnerContext runnerContext)
        {
            var container = Container.Create().Using(new ContainerConfiguration(runnerContext));

            ColumnsConventions = container.Get<IEnumerable<IColumnsConvention>>().ToList();
            ConstraintConventions = container.Get<IEnumerable<IConstraintConvention>>().ToList();
            ForeignKeyConventions = container.Get<IEnumerable<IForeignKeyConvention>>().ToList();
            IndexConventions = container.Get<IEnumerable<IIndexConvention>>().ToList();
            RootPathConvention = container.Get<IRootPathConvention>();
            SchemaConvention = container.Get<ISchemaConvention>();
            SequenceConventions = container.Get<IEnumerable<ISequenceConvention>>().ToList();
        }

        public IRootPathConvention RootPathConvention { get; }
        public ISchemaConvention SchemaConvention { get; }
        public IList<IColumnsConvention> ColumnsConventions { get; }
        public IList<IConstraintConvention> ConstraintConventions { get; }
        public IList<IForeignKeyConvention> ForeignKeyConventions { get; }
        public IList<IIndexConvention> IndexConventions { get; }
        public IList<ISequenceConvention> SequenceConventions { get; }

        private class ContainerConfiguration : IConfiguration
        {
            private readonly string _rootDirectory;
            private readonly string _defaultSchema;

            public ContainerConfiguration(IRunnerContext runnerContext)
            {
                _rootDirectory = string.IsNullOrEmpty(runnerContext?.WorkingDirectory) ? null : runnerContext?.WorkingDirectory;
                _defaultSchema = runnerContext?.DefaultSchemaName ?? string.Empty;
            }

            public IEnumerable<IDisposable> Apply(IContainer container)
            {
                yield return RegisterInstance(container, new DefaultRootPathConvention(_rootDirectory));
                if (!string.IsNullOrEmpty(_defaultSchema))
                    yield return RegisterInstance(container, new DefaultSchemaConvention(_defaultSchema));
                yield return RegisterInstance(container, new DefaultConstraintNameConvention());
                yield return RegisterInstance(container, new DefaultForeignKeyNameConvention());
                yield return RegisterInstance(container, new DefaultIndexNameConvention());
                yield return RegisterInstance(container, new DefaultPrimaryKeyNameConvention());
            }

            private static IDisposable RegisterInstance(IContainer container, object convention)
            {
                var contractTypes = convention.GetType().GetInterfaces().Where(x => x != typeof(IDisposable)).ToArray();
                return container.Bind(contractTypes).Lifetime(Lifetime.Singletone).To(() => convention);
            }
        }
    }
}
