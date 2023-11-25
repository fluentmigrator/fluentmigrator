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

using System;

using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Snowflake
{
    [TestFixture(true)]
    [TestFixture(false)]
    [Category("Snowflake")]
    public class SnowflakeClusteredTests : BaseSqlServerClusteredTests
    {
        protected SnowflakeGenerator Generator;
        private readonly bool _quotingEnabled;

        public SnowflakeClusteredTests(bool quotingEnabled)
        {
            _quotingEnabled = quotingEnabled;
        }

        [SetUp]
        public void Setup()
        {
            var sfOptions = _quotingEnabled ? SnowflakeOptions.QuotingEnabled() : SnowflakeOptions.QuotingDisabled();
            Generator = new SnowflakeGenerator(sfOptions);
        }

        [Test]
        public override void CanCreateClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBeEmpty();
        }

        [Test]
        public override void CanCreateClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBeEmpty();
        }

        [Test]
        public override void CanCreateMultiColumnClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBeEmpty();
        }

        [Test]
        public override void CanCreateMultiColumnClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBeEmpty();
        }

        [Test]
        public override void CanCreateNamedClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedMultiColumnClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedMultiColumnClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedMultiColumnClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedMultiColumnClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedMultiColumnNonClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedMultiColumnNonClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedMultiColumnNonClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedMultiColumnNonClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedNonClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedNonClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedNonClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateNamedNonClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            var result = Generator.Generate(expression);
            // This case is covered by constraint tests.
            result.ShouldNotBeEmpty();
        }

        [Test]
        public override void CanCreateUniqueClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBeEmpty();
        }

        [Test]
        public override void CanCreateUniqueClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBeEmpty();
        }

        [Test]
        public override void CanCreateUniqueClusteredMultiColumnIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBeEmpty();
        }

        [Test]
        public override void CanCreateUniqueClusteredMultiColumnIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBeEmpty();
        }
    }
}
