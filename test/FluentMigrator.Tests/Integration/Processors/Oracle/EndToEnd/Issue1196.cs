#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.Processors.Oracle.EndToEnd
{
    [TestFixture]
    [Category("Integration")]
    [Category("OracleManaged")]
    public class Issue1196 : OracleManagedEndToEndFixture
    {
        [SetUp]
        public void SetUp()
        {
            IntegrationTestOptions.Oracle.IgnoreIfNotEnabled();
        }

        [Test]
        public void Insert_LongStringLiteral_ShouldNotFail()
        {
            var ns = typeof(FluentMigrator.Tests.Integration.Migrations.Oracle.Issue1196.Migration_v100_AddSimpleTable).Namespace;

            Migrate(ns);
            Rollback(ns);
        }
    }
}
