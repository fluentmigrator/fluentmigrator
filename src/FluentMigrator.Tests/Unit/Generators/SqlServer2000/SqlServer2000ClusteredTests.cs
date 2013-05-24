using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2000
{
    [TestFixture]
    public class SqlServer2000ClusteredTests
    {
        protected SqlServer2000Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2000Generator();
        }

        [Test]
        public void CanCreateClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.IsClustered = true;
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateMultiColumnClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.IsClustered = true;
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateMultiColumnClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateNamedClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY CLUSTERED ([TestColumn1])");
        }

        [Test]
        public void CanCreateNamedClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY CLUSTERED ([TestColumn1])");
        }

        [Test]
        public void CanCreateNamedClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE CLUSTERED ([TestColumn1])");
        }

        [Test]
        public void CanCreateNamedClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE CLUSTERED ([TestColumn1])");
        }

        [Test]
        public void CanCreateNamedMultiColumnClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY CLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateNamedMultiColumnClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY CLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateNamedMultiColumnClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE CLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateNamedMultiColumnClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE CLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateNamedMultiColumnNonClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY NONCLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateNamedMultiColumnNonClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY NONCLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateNamedMultiColumnNonClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE NONCLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateNamedMultiColumnNonClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE NONCLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public void CanCreateNamedNonClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY NONCLUSTERED ([TestColumn1])");
        }

        [Test]
        public void CanCreateNamedNonClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY NONCLUSTERED ([TestColumn1])");
        }

        [Test]
        public void CanCreateNamedNonClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE NONCLUSTERED ([TestColumn1])");
        }

        [Test]
        public void CanCreateNamedNonClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE NONCLUSTERED ([TestColumn1])");
        }

        [Test]
        public void CanCreateUniqueClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.IsClustered = true;
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateUniqueClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public void CanCreateUniqueClusteredMultiColumnIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.IsClustered = true;
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public void CanCreateUniqueClusteredMultiColumnIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }
    }
}