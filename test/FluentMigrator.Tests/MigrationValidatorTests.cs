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

using FluentMigrator.Expressions;
using FluentMigrator.Runner;
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
    }
}
