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

using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Exceptions;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

namespace FluentMigrator.Tests
{
    [TestFixture]
    [Category("Validation")]
    public class MigrationValidatorTests
    {
        [SetUp]
        public void Setup()
        {
            _migration = Mock.Of<IMigration>();
            _migrationValidator = new MigrationValidator(Mock.Of<ILogger<MigrationValidator>>(), new DefaultConventionSet());
        }

        private MigrationValidator _migrationValidator;

        private IMigration _migration;

        private IMigrationExpression BuildInvalidExpression()
        {
            return new CreateTableExpression();
        }

        private IMigrationExpression BuildValidExpression()
        {
            var expression = new CreateTableExpression { TableName = "Foo" };
            return expression;
        }

        [Test]
        public void ItDoesNotThrowIfExpressionsAreValid()
        {
            Assert.DoesNotThrow(
                () => _migrationValidator.ApplyConventionsToAndValidateExpressions(
                    _migration,
                    new[] { BuildValidExpression() }));
        }

        [Test]
        public void ItThrowsInvalidMigrationExceptionIfExpressionsAreInvalid()
        {
            Assert.Throws<InvalidMigrationException>(
                () => _migrationValidator.ApplyConventionsToAndValidateExpressions(
                    _migration,
                    new[] { BuildInvalidExpression() }));
        }

        [Test]
        public void ItThrowsInvalidMigrationExceptionIfExpressionsContainsMultipleInvalidOfSameType()
        {
            Assert.Throws<InvalidMigrationException>(
                () => _migrationValidator.ApplyConventionsToAndValidateExpressions(
                    _migration,
                    new[] { BuildInvalidExpression(), BuildInvalidExpression() }));
        }

        [Test]
        public void ItReturnsExpressionsReplacedByConventions()
        {
            var original = new CreateIndexExpression
            {
                Index = new IndexDefinition
                {
                    Name = "ix_original",
                    TableName = "items",
                    Columns = { new IndexColumnDefinition { Name = "qty" } },
                },
            };
            var replacement = new CreateIndexExpression
            {
                Index = new IndexDefinition
                {
                    Name = "ix_from_convention",
                    TableName = "items",
                    Columns = { new IndexColumnDefinition { Name = "qty" } },
                },
            };

            var convention = new Mock<IIndexConvention>();
            convention.Setup(c => c.Apply(original)).Returns(replacement);

            var conventions = new ConventionSet { IndexConventions = { convention.Object } };
            var validator = new MigrationValidator(Mock.Of<ILogger<MigrationValidator>>(), conventions);

            var result = validator.ApplyConventionsAndValidateExpressions(_migration, new IMigrationExpression[] { original });

            Assert.That(result.Single(), Is.SameAs(replacement));
        }
    }
}
