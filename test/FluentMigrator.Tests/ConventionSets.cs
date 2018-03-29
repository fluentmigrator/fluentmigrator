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
        private static readonly DefaultSchemaConvention _noSchemaNameConvention = new DefaultSchemaConvention(null);
        private static readonly DefaultSchemaConvention _testSchemaNameConvention = new DefaultSchemaConvention("testdefault");

        public static readonly ConventionSet NoSchemaName = new ConventionSet
        {
            SchemaConvention = _noSchemaNameConvention,
            ConstraintConventions =
            {
                new DefaultConstraintNameConvention(),
                _noSchemaNameConvention,
            },
            ColumnsConventions =
            {
                new DefaultPrimaryKeyNameConvention(),
            },
            ForeignKeyConventions =
            {
                new DefaultForeignKeyNameConvention(),
                _noSchemaNameConvention,
            },
            IndexConventions =
            {
                new DefaultIndexNameConvention(),
                _noSchemaNameConvention,
            }
        };

        public static readonly ConventionSet WithSchemaName = new ConventionSet
        {
            SchemaConvention = _testSchemaNameConvention,
            ConstraintConventions =
            {
                new DefaultConstraintNameConvention(),
                _testSchemaNameConvention,
            },
            ColumnsConventions =
            {
                new DefaultPrimaryKeyNameConvention(),
            },
            ForeignKeyConventions =
            {
                new DefaultForeignKeyNameConvention(),
                _testSchemaNameConvention,
            },
            IndexConventions =
            {
                new DefaultIndexNameConvention(),
                _testSchemaNameConvention,
            }
        };
    }
}
