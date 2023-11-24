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
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Jet;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.Jet
{
    [TestFixture]
    [Category("Generator")]
    [Category("Jet")]
    public class JetGeneratorTests
    {
        protected JetGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new JetGenerator();
        }

        [Test]
        public void CanAlterSchemaInStrictMode()
        {
            Generator.CompatibilityMode = Runner.CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanCreateSchemaInStrictMode()
        {
            Generator.CompatibilityMode = Runner.CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new CreateSchemaExpression()));
        }

        [Test]
        public void CanDropSchemaInStrictMode()
        {
            Generator.CompatibilityMode = Runner.CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(new DeleteSchemaExpression()));
        }

        [Test]
        public void CanRenameColumnInStrictMode()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            Generator.CompatibilityMode = Runner.CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }

        [Test]
        public void CanRenameTableInStrictMode()
        {
            var expression = GeneratorTestHelper.GetRenameColumnExpression();
            Generator.CompatibilityMode = Runner.CompatibilityMode.STRICT;

            Assert.Throws<DatabaseOperationNotSupportedException>(() => Generator.Generate(expression));
        }
    }
}
