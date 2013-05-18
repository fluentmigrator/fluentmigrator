using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresIndexTests
    {
        protected PostgresGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new PostgresGenerator();
        }

        [Test]
        public void CanCreateIndex()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = "IX_TEST";
            expression.Index.TableName = "TEST_TABLE";
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = "Column1" });
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = "Column2" });

            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE INDEX \"IX_TEST\" ON \"public\".\"TEST_TABLE\" (\"Column1\" ASC,\"Column2\" DESC)");
        }

        [Test]
        public void CanCreateUniqueIndexWithSchema()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = "IX_TEST";
            expression.Index.TableName = "TEST_TABLE";
            expression.Index.SchemaName = "TEST_SCHEMA";
            expression.Index.IsUnique = true;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = "Column1" });
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = "Column2" });

            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX \"IX_TEST\" ON \"TEST_SCHEMA\".\"TEST_TABLE\" (\"Column1\" ASC,\"Column2\" DESC)");
        }

        [Test]
        public void CanCreateUniqueIndex()
        {
            var expression = new CreateIndexExpression();
            expression.Index.Name = "IX_TEST";
            expression.Index.TableName = "TEST_TABLE";
            expression.Index.IsUnique = true;
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Ascending, Name = "Column1" });
            expression.Index.Columns.Add(new IndexColumnDefinition { Direction = Direction.Descending, Name = "Column2" });

            string sql = generator.Generate(expression);
            sql.ShouldBe("CREATE UNIQUE INDEX \"IX_TEST\" ON \"public\".\"TEST_TABLE\" (\"Column1\" ASC,\"Column2\" DESC)");
        }

        [Test]
        public void CanDropIndex()
        {
            var expression = new DeleteIndexExpression();
            expression.Index.Name = "IX_TEST";
            expression.Index.TableName = "TEST_TABLE";

            string sql = generator.Generate(expression);
            sql.ShouldBe("DROP INDEX \"public\".\"IX_TEST\"");
        }
    }
}