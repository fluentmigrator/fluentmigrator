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

using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Generators.Hana;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Hana
{
    [TestFixture]
    [Category("Hana")]
    public class HanaSequenceTests : BaseSequenceTests
    {
        protected HanaGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new HanaGenerator()
            {
                CompatibilityMode = Runner.CompatibilityMode.STRICT,
            };
        }

        [Test]
        public override void CanCreateSequenceWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSequenceExpression();
            expression.Sequence.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SEQUENCE \"Sequence\" INCREMENT BY 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE;");
        }

        [Test]
        public override void CanCreateSequenceWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateSequenceExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SEQUENCE \"Sequence\" INCREMENT BY 2 MINVALUE 0 MAXVALUE 100 START WITH 2 CACHE 10 CYCLE;");
        }

        [Test]
        public void CanCreateSequenceWithNocache()
        {
            var expression = GeneratorTestHelper.GetCreateSequenceExpression();
            expression.Sequence.Cache = null;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE SEQUENCE \"Sequence\" INCREMENT BY 2 MINVALUE 0 MAXVALUE 100 START WITH 2 NO CACHE CYCLE;");
        }

        [Test]
        public void CanNotCreateSequenceWithCacheOne()
        {
            var expression = GeneratorTestHelper.GetCreateSequenceExpression();
            expression.Sequence.Cache = 1;

            Should.Throw<DatabaseOperationNotSupportedException>(
                () => Generator.Generate(expression),
                "Cache size must be greater than 1; if you intended to disable caching, set Cache to null."
            );
        }

        [Test]
        public override void CanDropSequenceWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSequenceExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP SEQUENCE \"Sequence\";");
        }

        [Test]
        public override void CanDropSequenceWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteSequenceExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP SEQUENCE \"Sequence\";");
        }
    }
}
