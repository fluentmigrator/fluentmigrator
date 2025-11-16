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

using FluentMigrator.Generation;
using FluentMigrator.Runner.Generators;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators
{
    [Category("Generator")]
    [Category("Description")]
    public abstract class BaseDescriptionGeneratorTests
    {
        protected IDescriptionGenerator DescriptionGenerator { get; set; }

        public abstract void GenerateDescriptionStatementsForCreateTableReturnTableDescriptionStatement();
        public abstract void GenerateDescriptionStatementsForCreateTableReturnTableDescriptionAndColumnDescriptionsStatements();
        public abstract void GenerateDescriptionStatementsForCreateTableReturnTableDescriptionAndColumnDescriptionsWithAdditionalDescriptionsStatements();
        public abstract void GenerateDescriptionStatementForAlterTableReturnTableDescriptionStatement();
        public abstract void GenerateDescriptionStatementForCreateColumnReturnColumnDescriptionStatement();
        public abstract void GenerateDescriptionStatementForCreateColumnReturnColumnDescriptionStatementWithAdditionalDescriptions();
        public abstract void GenerateDescriptionStatementForAlterColumnReturnColumnDescriptionStatement();
        public abstract void GenerateDescriptionStatementForAlterColumnReturnColumnDescriptionStatementWithAdditionalDescriptions();

        [Test]
        public void GenerateDescriptionStatementsReturnEmptyForNoDescriptionsOnCreateTable()
        {
            var createTableExpression = GeneratorTestHelper.GetCreateTableExpression();
            var result = DescriptionGenerator.GenerateDescriptionStatements(createTableExpression);

            result.ShouldBe(Enumerable.Empty<string>());
        }

        [Test]
        public void GenerateDescriptionStatementReturnEmptyForNoDescriptionOnAlterTable()
        {
            var alterTableExpression = GeneratorTestHelper.GetAlterTableAutoIncrementColumnExpression();
            var result = DescriptionGenerator.GenerateDescriptionStatement(alterTableExpression);

            result.ShouldBe(string.Empty);
        }

        [Test]
        public void GenerateDescriptionStatementReturnEmptyForNoDescriptionOnCreateColumn()
        {
            var createColumnExpression = GeneratorTestHelper.GetCreateColumnExpression();
            var result = DescriptionGenerator.GenerateDescriptionStatement(createColumnExpression);

            result.ShouldBe(string.Empty);
        }

        [Test]
        public void GenerateDescriptionStatementReturnEmptyForNoDescriptionOnAlterColumn()
        {
            var alterColumnExpression = GeneratorTestHelper.GetAlterColumnExpression();
            var result = DescriptionGenerator.GenerateDescriptionStatement(alterColumnExpression);

            result.ShouldBe(string.Empty);
        }

        [Test]
        public virtual void GenerateDescriptionStatementsHaveSingleStatementForDescriptionOnCreate()
        {
            var createTableExpression = GeneratorTestHelper.GetCreateTableWithTableDescription();
            var result = DescriptionGenerator.GenerateDescriptionStatements(createTableExpression);

            result.Count().ShouldBe(1);
        }
    }
}
