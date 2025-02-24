using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using FluentMigrator.Builders.Create.Constraint;
using FluentMigrator.Builders.Create.ForeignKey;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Builders.Create.Sequence;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Builders.Delete.Column;
using FluentMigrator.Builders.Delete.ForeignKey;
using FluentMigrator.Builders.Delete.Sequence;
using FluentMigrator.Builders.Delete.Table;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Validation;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Validation
{
    [TestFixture]
    [Category("Validation")]
    [SetUICulture("")] // Ensure validation messages are in English
    public class DefaultMigrationExpressionValidatorTests
    {

        [Test]
        public void ValidateInsertDataExpressionWithoutTableNameShouldReturnError()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<InsertDataExpression>();

            var builder = new InsertDataExpressionBuilder(expressionMock.Object);
            builder.InSchema("dbo");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();

            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBeGreaterThan(0);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, "The TableName field is required.", StringComparison.Ordinal));

            result.TableName = "F";
            validationResults = sut.Validate(result).ToList();

            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(0);
            validationResults.ShouldNotContain(r => string.Equals(r.ErrorMessage, "The TableName field is required.", StringComparison.Ordinal));
        }

        [Test]
        public void ValidateDeleteDataExpressionWithoutTableNameShouldReturnError()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<DeleteDataExpression>();

            var builder = new DeleteDataExpressionBuilder(expressionMock.Object);
            builder.InSchema("dbo");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();

            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBeGreaterThan(0);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, "The TableName field is required.", StringComparison.Ordinal));

            result.TableName = "F";
            validationResults = sut.Validate(result).ToList();

            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(0);
            validationResults.ShouldNotContain(r => string.Equals(r.ErrorMessage, "The TableName field is required.", StringComparison.Ordinal));
        }

        [Test]
        public void ValidateCreateIndexWithoutTableNameShouldReturnError()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupProperty(p => p.Index, new IndexDefinition { Name = "TestIndex" });

            var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
            _ = builder.OnTable(null).InSchema("TestSchema");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.TableNameCannotBeNullOrEmpty, StringComparison.Ordinal));
        }


        [Test]
        public void ValidateCreateIndexWithoutColumnNameShouldReturnError()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupProperty(p => p.Index, new IndexDefinition { Name = "TestIndex" });

            var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
            _ = builder.OnTable("TestTable").InSchema("TestSchema").OnColumn(null);

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.ColumnNameCannotBeNullOrEmpty, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateCreateIndexWithPostgresIndexIncludeDefinitionWithoutColumnNameShouldReturnError()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupProperty(p => p.Index, new IndexDefinition { Name = "TestIndex" });

            var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
            var x = builder.OnTable("TestTable").InSchema("TestSchema");
            Postgres.PostgresExtensions.Include(x.OnColumn("TestColumn")
                    .Ascending()
                    .WithOptions(), null);
            
            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.IndexIncludeColumnNameMustNotBeNullOrEmpty, StringComparison.Ordinal));
        }


        [Test]
        public void ValidateCreateIndexWithSqlServerIndexIncludeDefinitionWithoutColumnNameShouldReturnError()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<CreateIndexExpression>();
            expressionMock.SetupProperty(p => p.Index, new IndexDefinition { Name = "TestIndex" });

            var builder = new CreateIndexExpressionBuilder(expressionMock.Object);
            var x = builder.OnTable("TestTable").InSchema("TestSchema");
            SqlServer.SqlServerExtensions.Include(x.OnColumn("TestColumn")
                    .Ascending()
                    .WithOptions(), null);

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.IndexIncludeColumnNameMustNotBeNullOrEmpty, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateCreateTableWithoutNameShouldReturnError()
        {
            var mockMigrationContext = new Mock<IMigrationContext>();
            mockMigrationContext.SetupProperty(p => p.Expressions, new List<IMigrationExpression>());
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<CreateTableExpression>();
            expressionMock.SetupProperty(p => p.Columns, new List<ColumnDefinition>());
            expressionMock.SetupProperty(p => p.TableName, null);

            var builder = new CreateTableExpressionBuilder(expressionMock.Object, mockMigrationContext.Object);
            _ = builder.InSchema("TestSchema")
                .WithColumn("TestColumn").AsInt32().Nullable();

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.TableNameCannotBeNullOrEmpty, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateCreateTableWithForeignKeyWithoutPrimaryColumnNameShouldReturnError()
        {
            var mockMigrationContext = new Mock<IMigrationContext>();
            mockMigrationContext.SetupProperty(p => p.Expressions, new List<IMigrationExpression>());
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<CreateTableExpression>();
            expressionMock.SetupProperty(p => p.Columns, new List<ColumnDefinition>());
            expressionMock.SetupProperty(p => p.TableName, "TestIndex");

            var builder = new CreateTableExpressionBuilder(expressionMock.Object, mockMigrationContext.Object);
            _ = builder.InSchema("TestSchema")
                .WithColumn("TestColumn").AsInt32().Nullable().ForeignKey("FK_TestColumn", "TestPrimaryTable", null);
            
            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.PrimaryKeyColumnNameReferencedByForeignKeyMustNotBeNullOrEmpty, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateCreateForeignKeyWithoutPrimaryColumnNameShouldReturn()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.SetupProperty(p => p.ForeignKey, new ForeignKeyDefinition { Name = "FK_TestTable_TestForeignColumn_TestPrimaryTable_TestPrimaryColumn"});
            
            var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
            _ = builder.FromTable("TestTable")
                .InSchema("TestSchema")
                .ForeignColumn("TestForeignColumn")
                .ToTable("TestPrimaryTable")
                .InSchema("TestPrimarySchema")
                .PrimaryColumn(null);

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.PrimaryKeyColumnNameReferencedByForeignKeyMustNotBeNullOrEmpty, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateCreateForeignKeyWithLessForeignKeyColumnsThanPrimaryKeyColumnsShouldReturn()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.SetupProperty(p => p.ForeignKey, new ForeignKeyDefinition { Name = "FK_TestTable_TestForeignColumn_TestPrimaryTable_TestPrimaryColumn" });

            var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
            _ = builder.FromTable("TestTable")
                .InSchema("TestSchema")
                .ForeignColumn("TestForeignColumn")
                .ToTable("TestPrimaryTable")
                .InSchema("TestPrimarySchema")
                .PrimaryColumns("TestPrimaryColumn1", "TestPrimaryColumn2");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.ForeignKeyColumnsCountMustMatchPrimaryKeyColumnsCount, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateCreateForeignKeyWithMoreForeignKeyColumnsThanPrimaryKeyColumnsShouldReturn()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.SetupProperty(p => p.ForeignKey, new ForeignKeyDefinition { Name = "FK_TestTable_TestForeignColumn_TestPrimaryTable_TestPrimaryColumn" });

            var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
            _ = builder.FromTable("TestTable")
                .InSchema("TestSchema")
                .ForeignColumns("TestForeignColumn1", "TestForeignColumn2", "TestForeignColumn3")
                .ToTable("TestPrimaryTable")
                .InSchema("TestPrimarySchema")
                .PrimaryColumns("TestPrimaryColumn1", "TestPrimaryColumn2");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.ForeignKeyColumnsCountMustMatchPrimaryKeyColumnsCount, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateCreateForeignKeyWithDuplicateForeignKeyColumnsShouldReturn()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.SetupProperty(p => p.ForeignKey, new ForeignKeyDefinition { Name = "FK_TestTable_TestForeignColumn_TestPrimaryTable_TestPrimaryColumn" });

            var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
            _ = builder.FromTable("TestTable")
                .InSchema("TestSchema")
                .ForeignColumns("TestForeignColumn1", "TestForeignColumn1")
                .ToTable("TestPrimaryTable")
                .InSchema("TestPrimarySchema")
                .PrimaryColumns("TestPrimaryColumn1", "TestPrimaryColumn2");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.ForeignKeyColumnNamesMustBeUnique, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateCreateForeignKeyWithDuplicatePrimaryKeyColumnsShouldReturn()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<CreateForeignKeyExpression>();
            expressionMock.SetupProperty(p => p.ForeignKey, new ForeignKeyDefinition { Name = "FK_TestTable_TestForeignColumn_TestPrimaryTable_TestPrimaryColumn" });

            var builder = new CreateForeignKeyExpressionBuilder(expressionMock.Object);
            _ = builder.FromTable("TestTable")
                .InSchema("TestSchema")
                .ForeignColumns("TestForeignColumn1", "TestForeignColumn2")
                .ToTable("TestPrimaryTable")
                .InSchema("TestPrimarySchema")
                .PrimaryColumns("TestPrimaryColumn1", "TestPrimaryColumn1");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.PrimaryKeyColumnNamesMustBeUnique, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateCreateConstraintWithoutAtLeastOneColumnShouldReturnError([Values] ConstraintType constraintType)
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<CreateConstraintExpression>(constraintType);
            expressionMock.SetupProperty(p => p.Constraint, new ConstraintDefinition(constraintType) { ConstraintName = null });

            var builder = new CreateConstraintExpressionBuilder(expressionMock.Object);
            _ = builder.OnTable("TestTable").WithSchema("TestSchema");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.ConstraintMustHaveAtLeastOneColumn, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateCreateSequenceWithoutNameShouldReturnError()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<CreateSequenceExpression>();
            expressionMock.SetupProperty(p => p.Sequence, new SequenceDefinition { Name = null });

            var builder = new CreateSequenceExpressionBuilder(expressionMock.Object);
            _ = builder.InSchema("TestSchema");
            
            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.SequenceNameCannotBeNullOrEmpty, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateDeleteTableWithoutNameShouldReturnError()
        {
            var mockMigrationContext = new Mock<IMigrationContext>();
            mockMigrationContext.SetupProperty(p => p.Expressions, new List<IMigrationExpression>());
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<DeleteTableExpression>();
            expressionMock.SetupProperty(p => p.TableName, null);

            var builder = new DeleteTableExpressionBuilder(expressionMock.Object);
            builder.InSchema("TestSchema");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.TableNameCannotBeNullOrEmpty, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateDeleteColumnWithoutColumnNameShouldReturnError()
        {
            var mockMigrationContext = new Mock<IMigrationContext>();
            mockMigrationContext.SetupProperty(p => p.Expressions, new List<IMigrationExpression>());
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<DeleteColumnExpression>();
            expressionMock.SetupProperty(p => p.TableName, "TestTable");

            var builder = new DeleteColumnExpressionBuilder(expressionMock.Object);
            builder.InSchema("TestSchema");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.ColumnNameCannotBeNullOrEmpty, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateDeleteColumnWithoutTableNameShouldReturnError()
        {
            var mockMigrationContext = new Mock<IMigrationContext>();
            mockMigrationContext.SetupProperty(p => p.Expressions, new List<IMigrationExpression>());
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<DeleteColumnExpression>();
            expressionMock.SetupProperty(p => p.TableName, null);
            var expression = expressionMock.Object;
            expression.ColumnNames = new List<string>();
            var builder = new DeleteColumnExpressionBuilder(expressionMock.Object);
            builder.InSchema("TestSchema");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.TableNameCannotBeNullOrEmpty, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateDeleteColumnWithoutAnyColumnNamesShouldReturnError()
        {
            var mockMigrationContext = new Mock<IMigrationContext>();
            mockMigrationContext.SetupProperty(p => p.Expressions, new List<IMigrationExpression>());
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<DeleteColumnExpression>();
            expressionMock.SetupProperty(p => p.TableName, "TestTable");
            var expression = expressionMock.Object;
            expression.ColumnNames = new List<string>();
            var builder = new DeleteColumnExpressionBuilder(expressionMock.Object);
            builder.InSchema("TestSchema");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.ColumnNameCannotBeNullOrEmpty, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateForeignKeyWithoutNameShouldReturnError()
        {
            var mockMigrationContext = new Mock<IMigrationContext>();
            mockMigrationContext.SetupProperty(p => p.Expressions, new List<IMigrationExpression>());
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<DeleteForeignKeyExpression>();
            expressionMock.SetupProperty(p => p.ForeignKey, new ForeignKeyDefinition { Name = null });

            var builder = new DeleteForeignKeyExpressionBuilder(expressionMock.Object);
            builder.FromTable("TestForeignTable").InSchema("TestForeignSchema");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.ForeignKeyNameCannotBeNullOrEmpty, StringComparison.Ordinal));
        }

        [Test]
        public void ValidateDeleteSequenceWithoutNameShouldReturnError()
        {
            var mockMigrationContext = new Mock<IMigrationContext>();
            mockMigrationContext.SetupProperty(p => p.Expressions, new List<IMigrationExpression>());
            var mockServiceProvider = new Mock<IServiceProvider>();
            var sut = new DefaultMigrationExpressionValidator(mockServiceProvider.Object);
            var expressionMock = new Mock<DeleteSequenceExpression>();
            expressionMock.SetupProperty(p => p.SequenceName, null);
            
            var builder = new DeleteSequenceExpressionBuilder(expressionMock.Object);
            builder.InSchema("TestSchema");

            var result = expressionMock.Object;

            var validationResults = sut.Validate(result).ToList();
            validationResults.ShouldNotBeNull();
            validationResults.Count.ShouldBe(1);
            validationResults.ShouldContain(r => string.Equals(r.ErrorMessage, ErrorMessages.SequenceNameCannotBeNullOrEmpty, StringComparison.Ordinal));
        }
    }
}
