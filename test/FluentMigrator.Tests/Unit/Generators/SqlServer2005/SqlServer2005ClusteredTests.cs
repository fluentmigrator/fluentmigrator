using FluentMigrator.Runner.Extensions;
using FluentMigrator.Runner.Generators.SqlServer;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.SqlServer2005
{
    [TestFixture]
    public class SqlServer2005ClusteredTests : BaseSqlServerClusteredTests
    {
        protected SqlServer2005Generator Generator;

        [SetUp]
        public void Setup()
        {
            Generator = new SqlServer2005Generator();
        }

        [Test]
        public override void CanCreateClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public override void CanCreateClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public override void CanCreateMultiColumnClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public override void CanCreateMultiColumnClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateMultiColumnCreateIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE CLUSTERED INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public override void CanCreateNamedClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY CLUSTERED ([TestColumn1])");
        }

        [Test]
        public override void CanCreateNamedClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY CLUSTERED ([TestColumn1])");
        }

        [Test]
        public override void CanCreateNamedClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE CLUSTERED ([TestColumn1])");
        }

        [Test]
        public override void CanCreateNamedClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE CLUSTERED ([TestColumn1])");
        }

        [Test]
        public override void CanCreateNamedMultiColumnClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY CLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public override void CanCreateNamedMultiColumnClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY CLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public override void CanCreateNamedMultiColumnClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE CLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public override void CanCreateNamedMultiColumnClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.Clustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE CLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public override void CanCreateNamedMultiColumnNonClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY NONCLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public override void CanCreateNamedMultiColumnNonClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY NONCLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public override void CanCreateNamedMultiColumnNonClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE NONCLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public override void CanCreateNamedMultiColumnNonClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedMultiColumnUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE NONCLUSTERED ([TestColumn1], [TestColumn2])");
        }

        [Test]
        public override void CanCreateNamedNonClusteredPrimaryKeyConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY NONCLUSTERED ([TestColumn1])");
        }

        [Test]
        public override void CanCreateNamedNonClusteredPrimaryKeyConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedPrimaryKeyExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTPRIMARYKEY] PRIMARY KEY NONCLUSTERED ([TestColumn1])");
        }

        [Test]
        public override void CanCreateNamedNonClusteredUniqueConstraintWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);
            expression.Constraint.SchemaName = "TestSchema";

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [TestSchema].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE NONCLUSTERED ([TestColumn1])");
        }

        [Test]
        public override void CanCreateNamedNonClusteredUniqueConstraintWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateNamedUniqueConstraintExpression();
            expression.Constraint.AdditionalFeatures.Add(SqlServerExtensions.ConstraintType, SqlServerConstraintType.NonClustered);

            var result = Generator.Generate(expression);
            result.ShouldBe("ALTER TABLE [dbo].[TestTable1] ADD CONSTRAINT [TESTUNIQUECONSTRAINT] UNIQUE NONCLUSTERED ([TestColumn1])");
        }

        [Test]
        public override void CanCreateUniqueClusteredIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public override void CanCreateUniqueClusteredIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC)");
        }

        [Test]
        public override void CanCreateUniqueClusteredMultiColumnIndexWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.SchemaName = "TestSchema";
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [TestSchema].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }

        [Test]
        public override void CanCreateUniqueClusteredMultiColumnIndexWithDefaultSchema()
        {
            var expression = GeneratorTestHelper.GetCreateUniqueMultiColumnIndexExpression();
            expression.Index.IsClustered = true;

            var result = Generator.Generate(expression);
            result.ShouldBe("CREATE UNIQUE CLUSTERED INDEX [TestIndex] ON [dbo].[TestTable1] ([TestColumn1] ASC, [TestColumn2] DESC)");
        }
    }
}
