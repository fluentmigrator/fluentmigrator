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

using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner
{
    public class DefaultConventionSet : IConventionSet
    {
        public DefaultConventionSet(IRunnerContext runnerContext)
        {
            var schemaConvention =
                new DefaultSchemaConvention(new DefaultSchemaNameConvention(runnerContext?.DefaultSchemaName));

            ColumnsConventions = new List<IColumnsConvention>()
            {
                new DefaultPrimaryKeyNameConvention(),
            };

            ConstraintConventions = new List<IConstraintConvention>()
            {
                new DefaultConstraintNameConvention(),
                schemaConvention,
            };

            ForeignKeyConventions = new List<IForeignKeyConvention>()
            {
                new DefaultForeignKeyNameConvention(),
                schemaConvention,
            };

            IndexConventions = new List<IIndexConvention>()
            {
                new DefaultIndexNameConvention(),
                schemaConvention,
            };

            SequenceConventions = new List<ISequenceConvention>()
            {
                schemaConvention,
            };

            SchemaConvention = schemaConvention;
            RootPathConvention = new DefaultRootPathConvention(runnerContext?.WorkingDirectory);
        }

        public IRootPathConvention RootPathConvention { get; }
        public DefaultSchemaConvention SchemaConvention { get; }
        public IList<IColumnsConvention> ColumnsConventions { get; }
        public IList<IConstraintConvention> ConstraintConventions { get; }
        public IList<IForeignKeyConvention> ForeignKeyConventions { get; }
        public IList<IIndexConvention> IndexConventions { get; }
        public IList<ISequenceConvention> SequenceConventions { get; }
    }
}
