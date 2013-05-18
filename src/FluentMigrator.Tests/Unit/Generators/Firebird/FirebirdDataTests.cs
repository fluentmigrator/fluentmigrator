using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Tests.Unit.Generators.Firebird
{
    [TestFixture]
    public class FirebirdDataTests
    {
        protected FirebirdGenerator generator;

        [SetUp]
        public void Setup()
        {
            generator = new FirebirdGenerator(FirebirdOptions.StandardBehaviour());
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
            sql.ShouldBe("DELETE FROM \"Table1\" WHERE 1 = 1");

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
            sql.ShouldBe("DELETE FROM \"Table1\" WHERE \"description\" IS NULL AND \"id\" = 10");
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

            var expected = "INSERT INTO \"TestTable\" (\"Id\", \"Name\", \"Website\") VALUES (1, 'Just''in', 'codethinked.com');";
            expected += " INSERT INTO \"TestTable\" (\"Id\", \"Name\", \"Website\") VALUES (2, 'Na\\te', 'kohari.org')";

            sql.ShouldBe(expected);
        }

        [Test]
        public void CanInsertGuidData()
        {
            var gid = Guid.NewGuid();
            var expression = new InsertDataExpression { TableName = "TestTable" };
            expression.Rows.Add(new InsertionDataDefinition { new KeyValuePair<string, object>("guid", gid) });

            var sql = generator.Generate(expression);

            var expected = String.Format("INSERT INTO \"TestTable\" (\"guid\") VALUES ('{0}')", gid);

            sql.ShouldBe(expected);
        }
    }
}
