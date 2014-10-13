using FluentMigrator.Runner.Generators.Hana;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Hana
{
    [TestFixture]
    public class HanaTableTests : BaseTableTests
    {
        protected HanaGenerator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new HanaGenerator();
        }

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "json";
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" json NOT NULL, PRIMARY KEY (\"TestColumn1\"))");
        }

        [Test]
        public override void CanCreateTableWithCustomColumnTypeWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.Columns[0].IsPrimaryKey = true;
            expression.Columns[1].Type = null;
            expression.Columns[1].CustomType = "json";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestTable1\" (\"TestColumn1\" NVARCHAR(255), \"TestColumn2\" json, PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanCreateTableWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestTable1\" (\"TestColumn1\" NVARCHAR(255), \"TestColumn2\" INTEGER);");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL DEFAULT NULL, \"TestColumn2\" integer NOT NULL DEFAULT 0)");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.Columns[0].DefaultValue = null;
            expression.Columns[0].TableName = expression.TableName;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestTable1\" (\"TestColumn1\" NVARCHAR(255) DEFAULT NULL, \"TestColumn2\" INTEGER DEFAULT 0);");

        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL DEFAULT 'Default', \"TestColumn2\" integer NOT NULL DEFAULT 0)");
        }

        [Test]
        public override void CanCreateTableWithIdentityWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" serial NOT NULL, \"TestColumn2\" integer NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithIdentityWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithAutoIncrementExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestTable1\" (\"TestColumn1\" INTEGER GENERATED ALWAYS AS IDENTITY, \"TestColumn2\" INTEGER);");
        }

        [Test]
        public override void CanCreateTableWithDefaultValueWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithDefaultValue();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestTable1\" (\"TestColumn1\" NVARCHAR(255) DEFAULT 'Default', \"TestColumn2\" INTEGER DEFAULT 0);");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, PRIMARY KEY (\"TestColumn1\",\"TestColumn2\"))");
        }

        [Test]
        public override void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestTable1\" (\"TestColumn1\" NVARCHAR(255), \"TestColumn2\" INTEGER," +
                            " PRIMARY KEY (\"TestColumn1\", \"TestColumn2\"));");
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\",\"TestColumn2\"))");
        }

        [Test]
        public override void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedMultiColumnPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestTable1\" (\"TestColumn1\" NVARCHAR(255), \"TestColumn2\" INTEGER," +
                            " PRIMARY KEY (\"TestColumn1\", \"TestColumn2\"));");
            //HANA no support name of primary key on composit key on create
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, CONSTRAINT \"TestKey\" PRIMARY KEY (\"TestColumn1\"))");
        }

        [Test]
        public override void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNamedPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestTable1\" (\"TestColumn1\" NVARCHAR(255), \"TestColumn2\" INTEGER, PRIMARY KEY (\"TestColumn1\"));");
            //Hana dosen't support to name primary key
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text, \"TestColumn2\" integer NOT NULL)");
        }

        [Test]
        public override void CanCreateTableWithNullableFieldWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithNullableColumn();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestTable1\" (\"TestColumn1\" NVARCHAR(255) NULL, \"TestColumn2\" INTEGER);");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestSchema\".\"TestTable1\" (\"TestColumn1\" text NOT NULL, \"TestColumn2\" integer NOT NULL, PRIMARY KEY (\"TestColumn1\"))");
        }

        [Test]
        public override void CanCreateTableWithPrimaryKeyWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateTableWithPrimaryKeyExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE COLUMN TABLE \"TestTable1\" (\"TestColumn1\" NVARCHAR(255), \"TestColumn2\" INTEGER, PRIMARY KEY (\"TestColumn1\"));");
        }

        [Test]
        public override void CanDropTableWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetDeleteTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE \"TestSchema\".\"TestTable1\"");
        }

        [Test]
        public override void CanDropTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetDeleteTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("DROP TABLE \"TestTable1\";");
        }

        [Test]
        public override void CanRenameTableWithCustomSchema()
        {
            Assert.Ignore("HANA does not support schema like us know schema in hana is a database name");

            var expression = GeneratorTestHelper.GetRenameTableExpression();
            expression.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE \"TestSchema\".\"TestTable1\" RENAME TO \"TestTable2\"");
        }

        [Test]
        public override void CanRenameTableWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetRenameTableExpression();

            var result = Generator.Generate(expression);
            result.ShouldBe("RENAME TABLE \"TestTable1\" TO \"TestTable2\";");
        }
    }
}