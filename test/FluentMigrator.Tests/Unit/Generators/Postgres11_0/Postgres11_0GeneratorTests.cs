#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using System.Data;

using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Tests.Unit.Generators.Postgres92;

using Microsoft.Extensions.Options;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres11_0
{
    [TestFixture]
    public class Postgres11_0GeneratorTests : Postgres92GeneratorTests
    {

        [SetUp]
        public override void Setup()
        {
            var quoter = new PostgresQuoter(new PostgresOptions());
            Generator = new Postgres11_0Generator(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()));
        }

        [Test]
        public void CanAlterColumnWithCollation()
        {
            var expression = GeneratorTestHelper.GetAlterColumnExpressionWithCollation();

            var result = Generator.Generate(expression);

            result.ShouldBe(@"ALTER TABLE ""public"".""TestTable1"" ALTER ""TestColumn1"" TYPE varchar(20) COLLATE " + GeneratorTestHelper.TestColumnCollationName + @", ALTER ""TestColumn1"" SET NOT NULL;");
        }
    }
}
