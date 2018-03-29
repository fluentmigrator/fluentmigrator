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

using System.Collections.Generic;
using System.Linq;

using Autofac;

using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner
{
    public class DefaultConventionSet : IConventionSet
    {
        public DefaultConventionSet(IRunnerContext runnerContext)
        {
            var builder = new ContainerBuilder();
            builder.Register(ctx => new DefaultSchemaConvention(runnerContext?.DefaultSchemaName))
                .SingleInstance().AsImplementedInterfaces();
            builder.Register(ctx => new DefaultRootPathConvention(runnerContext?.WorkingDirectory))
                .SingleInstance().AsImplementedInterfaces();
            builder.RegisterType<DefaultConstraintNameConvention>()
                .SingleInstance().AsImplementedInterfaces();
            builder.RegisterType<DefaultForeignKeyNameConvention>()
                .SingleInstance().AsImplementedInterfaces();
            builder.RegisterType<DefaultIndexNameConvention>()
                .SingleInstance().AsImplementedInterfaces();
            builder.RegisterType<DefaultPrimaryKeyNameConvention>()
                .SingleInstance().AsImplementedInterfaces();

            var container = builder.Build();

            ColumnsConventions = container.Resolve<IEnumerable<IColumnsConvention>>().ToList();
            ConstraintConventions = container.Resolve<IEnumerable<IConstraintConvention>>().ToList();
            ForeignKeyConventions = container.Resolve<IEnumerable<IForeignKeyConvention>>().ToList();
            IndexConventions = container.Resolve<IEnumerable<IIndexConvention>>().ToList();
            RootPathConvention = container.Resolve<IRootPathConvention>();
            SchemaConvention = container.Resolve<ISchemaConvention>();
            SequenceConventions = container.Resolve<IEnumerable<ISequenceConvention>>().ToList();
        }

        public IRootPathConvention RootPathConvention { get; }
        public ISchemaConvention SchemaConvention { get; }
        public IList<IColumnsConvention> ColumnsConventions { get; }
        public IList<IConstraintConvention> ConstraintConventions { get; }
        public IList<IForeignKeyConvention> ForeignKeyConventions { get; }
        public IList<IIndexConvention> IndexConventions { get; }
        public IList<ISequenceConvention> SequenceConventions { get; }
    }
}
