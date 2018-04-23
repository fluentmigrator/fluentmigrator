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

using FluentMigrator.Runner.Generators.SqlAnywhere;

namespace FluentMigrator.Runner.Processors.SqlAnywhere
{
    [Obsolete]
    public class SqlAnywhere16ProcessorFactory : MigrationProcessorFactory
    {
        [Obsolete]
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new SqlAnywhereDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new SqlAnywhereProcessor("SqlAnywhere16", connection, new SqlAnywhere16Generator(new SqlAnywhereQuoter()), announcer, options, factory);
        }

        [Obsolete]
        public override bool IsForProvider(string provider)
        {
            return provider.ToLower().Contains("ianywhere");
        }
    }
}
