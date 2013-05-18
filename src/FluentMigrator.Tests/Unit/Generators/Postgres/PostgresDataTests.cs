using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Postgres;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    public class PostgresDataTests
    {
        protected PostgresGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new PostgresGenerator();
        }

        [Test]
        public void CanDeleteAllData()
        {
            var expression = new DeleteDataExpression
            {
                IsAllRows = true,
                TableName = "Table1"
            };

            var sql = generator.Generate(expression);
            sql.ShouldBe("DELETE FROM \"public\".\"Table1\";");

        }

        [Test]
        public void CanDeleteAllDataWithMultipleConditions()
        {
            var expression = new DeleteDataExpression
            {
                IsAllRows = false,
                SchemaName = "public",
                TableName = "Table1"
            };
            expression.Rows.Add(new DeletionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("description", null),
                                        new KeyValuePair<string, object>("id", 10)
                                    });

            var sql = generator.Generate(expression);
            sql.ShouldBe("DELETE FROM \"public\".\"Table1\" WHERE \"description\" IS NULL AND \"id\" = 10;");
        }

        [Test]
        public void CanInsertData()
        {
            var expression = new InsertDataExpression();
            expression.TableName = "TestTable";
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Id", 1),
                                        new KeyValuePair<string, object>("Name", "Just'in"),
                                        new KeyValuePair<string, object>("Website", "codethinked.com")
                                    });
            expression.Rows.Add(new InsertionDataDefinition
                                    {
                                        new KeyValuePair<string, object>("Id", 2),
                                        new KeyValuePair<string, object>("Name", "Na\\te"),
                                        new KeyValuePair<string, object>("Website", "kohari.org")
                                    });

            var sql = generator.Generate(expression);

            var expected = "INSERT INTO \"public\".\"TestTable\" (\"Id\",\"Name\",\"Website\") VALUES (1,'Just''in','codethinked.com');";
            expected += "INSERT INTO \"public\".\"TestTable\" (\"Id\",\"Name\",\"Website\") VALUES (2,'Na\\te','kohari.org');";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanInsertGuidData()
        {
            var gid = Guid.NewGuid();
            var expression = new InsertDataExpression { TableName = "TestTable" };
            expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("guid", gid) });

            var sql = generator.Generate(expression);

            var expected = String.Format("INSERT INTO \"public\".\"TestTable\" (\"guid\") VALUES ('{0}');", gid);

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanUpdateDataForAllRows()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpressionWithAllRows();

            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE \"public\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE 1 = 1");
        }

        [Test]
        public void CanUpdateDataWithCustomSchema()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();
            expression.SchemaName = "TestSchema";
            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE \"TestSchema\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE \"Id\" = 9 AND \"Homepage\" IS NULL");
        }

        [Test]
        public void CanUpdateData()
        {
            var expression = GeneratorTestHelper.GetUpdateDataExpression();

            var sql = generator.Generate(expression);
            sql.ShouldBe("UPDATE \"public\".\"TestTable1\" SET \"Name\" = 'Just''in', \"Age\" = 25 WHERE \"Id\" = 9 AND \"Homepage\" IS NULL");
        }
    }
}