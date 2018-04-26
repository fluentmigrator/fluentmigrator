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

namespace FluentMigrator.Runner.Processors.SqlAnywhere
{
    public class SqlAnywhereDbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _testEntries =
        {
            new TestEntry("iAnywhere.Data.SQLAnywhere.SAFactory", "iAnywhere.Data.SQLAnywhere.v4.5"),
            new TestEntry("iAnywhere.Data.SQLAnywhere.SAFactory", "iAnywhere.Data.SQLAnywhere.EF6"),
            new TestEntry("iAnywhere.Data.SQLAnywhere.SAFactory", "iAnywhere.Data.SQLAnywhere.v4.0"),
            new TestEntry("iAnywhere.Data.SQLAnywhere.SAFactory", "iAnywhere.Data.SQLAnywhere.v3.5"),
        };

        public SqlAnywhereDbFactory()
            : base(serviceProvider: null, _testEntries)
        {
        }
    }
}
