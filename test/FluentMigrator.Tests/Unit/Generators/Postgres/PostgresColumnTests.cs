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

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    [Category("Generator")]
    [Category("Postgres")]
    public class PostgresColumnTests : PostgresBaseColumnTests<PostgresGenerator>
    {
        protected override PostgresGenerator ConstructGenerator()
        {
            var quoter = new PostgresQuoter(new PostgresOptions());
            return new PostgresGenerator(quoter);
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ALTER \"TestColumn1\" TYPE serial;");
        }

        [Test]
        public override void CanCreateAutoIncrementColumnWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterColumnAddAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ALTER \"TestColumn1\" TYPE serial;");
        }

        [Test]
        public override void CanCreateTableWithIdentityWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" serial NOT NULL, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateTableWithIdentityWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE TABLE \"public\".\"TestTable1\" (\"TestColumn1\" serial NOT NULL, \"TestColumn2\" integer NOT NULL);");
        }

        [Test]
        public override void CanCreateColumnWithAutoIncrementAndCustomSchema()
        {
            var expression = GeneratorTestHelper.GetAlterTableAutoIncrementColumnExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" ADD \"TestColumn1\" serial NOT NULL;");
        }

        [Test]
        public override void CanCreateColumnWithAutoIncrementAndDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetAlterTableAutoIncrementColumnExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD \"TestColumn1\" serial NOT NULL;");
        }

        [Test]
        public void CanCreateJsonColumnWithDefaultSchema()
        {
            var column = new ColumnDefinition { Name = "TestColumn1", DefaultValue = "{}", CustomType = "json"};
            var expression =  new CreateColumnExpression { TableName = "TestTable1", Column = column };


            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"public\".\"TestTable1\" ADD \"TestColumn1\" json NOT NULL DEFAULT '{}';");
        }
    }
}
