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

using FluentMigrator.Runner.Conventions;

namespace FluentMigrator.Tests
{
    public static class ConventionSets
    {
        private static readonly DefaultSchemaConvention _noSchemaNameConvention = new DefaultSchemaConvention();
        private static readonly DefaultSchemaConvention _testSchemaNameConvention = new DefaultSchemaConvention("testdefault");

        public static readonly IConventionSet NoSchemaName = CreateNoSchemaName(null);

        public static readonly IConventionSet WithSchemaName = CreateTestSchemaName(null);

        public static IConventionSet CreateNoSchemaName(string rootPath)
            => Create(_noSchemaNameConvention, rootPath);

        public static IConventionSet CreateTestSchemaName(string rootPath)
            => Create(_testSchemaNameConvention, rootPath);

        public static IConventionSet Create(DefaultSchemaConvention schemaConvention, string rootPath)
        {
            return new ConventionSet
            {
                SchemaConvention = schemaConvention,
                RootPathConvention = new DefaultRootPathConvention(rootPath),
                ConstraintConventions =
                {
                    new DefaultConstraintNameConvention(),
                    schemaConvention,
                },
                ColumnsConventions =
                {
                    new DefaultPrimaryKeyNameConvention(),
                },
                ForeignKeyConventions =
                {
                    new DefaultForeignKeyNameConvention(),
                    schemaConvention,
                },
                IndexConventions =
                {
                    new DefaultIndexNameConvention(),
                    schemaConvention,
                },
                SequenceConventions =
                {
                    schemaConvention,
                },
                AutoNameConventions =
                {
                    new DefaultAutoNameConvention(),
                }
            };
        }
    }
}
