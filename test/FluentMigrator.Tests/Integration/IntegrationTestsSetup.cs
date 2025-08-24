#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System.Threading.Tasks;

using FluentMigrator.Tests.Containers;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
    [SetUpFixture]
    public class IntegrationTestsSetup
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            await Task.WhenAll([
                new MySqlContainer().Start(),
                // new OracleContainer().Start(),
                new SqlServerContainer().Start(),
                new PostgresContainer().Start(),
                new FirebirdContainer().Start(),
                // new Db2Container().Start(),
            ]);
        }
    }
}
