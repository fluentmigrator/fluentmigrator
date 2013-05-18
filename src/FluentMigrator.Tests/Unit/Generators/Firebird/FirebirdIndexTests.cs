using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Generators.Firebird;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    [TestFixture]
    public class FirebirdIndexTests
    {
        protected FirebirdGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour());
        }

        [Test]
        public void CanCreateIndexWithDefaultSchema()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = "IX_TEST";
            expression.Index.TableName = "TEST_TABLE";
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = "Column1" });
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = "Column2" });

            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE ASC INDEX \"IX_TEST\" ON \"TEST_TABLE\" (\"Column1\", \"Column2\")");
        }

        [Test]
        public void CanCreateUniqueIndexWithDefaultSchema()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = "IX_TEST";
            expression.Index.TableName = "TEST_TABLE";
            expression.Index.IsUnique = true;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = "Column1" });
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = "Column2" });

            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE ASC INDEX \"IX_TEST\" ON \"TEST_TABLE\" (\"Column1\", \"Column2\")");
        }

        [Test]
        public void CanDropIndexWithDefaultSchema()
        {
            var expression = new DeleteIndexExpression();
            expression.Index.Name = "IX_TEST";
            expression.Index.TableName = "TEST_TABLE";

            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX \"IX_TEST\"");
        }
    }
}
