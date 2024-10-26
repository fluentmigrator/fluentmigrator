#region License
// Copyright (c) 2021, Fluent Migrator Project
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

using FluentMigrator.Postgres;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Tests.Unit.Generators.Postgres;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres10_0
{
    [TestFixture]
    public class Postgres10_0DataTests : PostgresDataTests
    {
        /// <inheritdoc />
        protected override PostgresGenerator CreateGenerator(PostgresQuoter quoter)
        {
            return new Postgres10_0Generator(quoter);
        }

        [Test]
        public override void CanInsertWithOverridingSystemValue()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.AdditionalFeatures[PostgresExtensions.OverridingIdentityValues] = PostgresOverridingIdentityValuesType.System;

            var expected = "INSERT INTO \"public\".\"TestTable1\" (\"Id\",\"Name\",\"Website\") OVERRIDING SYSTEM VALUE VALUES (1,'Just''in','codethinked.com');";
            expected += "INSERT INTO \"public\".\"TestTable1\" (\"Id\",\"Name\",\"Website\") OVERRIDING SYSTEM VALUE VALUES (2,'Na\\te','kohari.org');";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }

        [Test]
        public override void CanInsertWithOverridingUserValue()
        {
            var expression = GeneratorTestHelper.GetInsertDataExpression();
            expression.AdditionalFeatures[PostgresExtensions.OverridingIdentityValues] = PostgresOverridingIdentityValuesType.User;

            var expected = "INSERT INTO \"public\".\"TestTable1\" (\"Id\",\"Name\",\"Website\") OVERRIDING USER VALUE VALUES (1,'Just''in','codethinked.com');";
            expected += "INSERT INTO \"public\".\"TestTable1\" (\"Id\",\"Name\",\"Website\") OVERRIDING USER VALUE VALUES (2,'Na\\te','kohari.org');";

            var result = Generator.Generate(expression);
            result.ShouldBe(expected);
        }
    }
}
