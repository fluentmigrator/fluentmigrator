using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Generators.SqlServer;
using Xunit;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2000
{
    public class SqlServer2000ClusteredTests : BaseSqlServerClusteredTests
    {
        protected SqlServer2000Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2000Generator();
        }

        [Fact]
        public override void CanCreateClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.IsClustered = true;
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Fact]
        public override void CanCreateClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Fact]
        public override void CanCreateMultiColumnClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.IsClustered = true;
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Fact]
        public override void CanCreateMultiColumnClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Fact]
        public override void CanCreateNamedClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY CLUSTERED ([TestColumn1])");
        }

        [Fact]
        public override void CanCreateNamedClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY CLUSTERED ([TestColumn1])");
        }

        [Fact]
        public override void CanCreateNamedClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE CLUSTERED ([TestColumn1])");
        }

        [Fact]
        public override void CanCreateNamedClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE CLUSTERED ([TestColumn1])");
        }

        [Fact]
        public override void CanCreateNamedMultiColumnClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY CLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Fact]
        public override void CanCreateNamedMultiColumnClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY CLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Fact]
        public override void CanCreateNamedMultiColumnClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE CLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Fact]
        public override void CanCreateNamedMultiColumnClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE CLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Fact]
        public override void CanCreateNamedMultiColumnNonClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY NONCLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Fact]
        public override void CanCreateNamedMultiColumnNonClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY NONCLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Fact]
        public override void CanCreateNamedMultiColumnNonClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE NONCLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Fact]
        public override void CanCreateNamedMultiColumnNonClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE NONCLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Fact]
        public override void CanCreateNamedNonClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY NONCLUSTERED ([TestColumn1])");
        }

        [Fact]
        public override void CanCreateNamedNonClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY NONCLUSTERED ([TestColumn1])");
        }

        [Fact]
        public override void CanCreateNamedNonClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE NONCLUSTERED ([TestColumn1])");
        }

        [Fact]
        public override void CanCreateNamedNonClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE NONCLUSTERED ([TestColumn1])");
        }

        [Fact]
        public override void CanCreateUniqueClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.IsClustered = true;
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Fact]
        public override void CanCreateUniqueClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC)");
        }

        [Fact]
        public override void CanCreateUniqueClusteredMultiColumnIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.IsClustered = true;
            expression.Index.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Fact]
        public override void CanCreateUniqueClusteredMultiColumnIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }
    }
}