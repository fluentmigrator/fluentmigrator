using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;

namespace FluentMigrator.Tests.Helpers
{
    public class OracleTestTable : IDisposable
    {
        private IDbConnection Connection { get; set; }
        private IDbFactory Factory { get; set; }
        private IQuoter Quoter { get; set; }
        private string _schema;
        private List<string> constraints = new List<string>();
        private List<string> indexies = new List<string>();
        public string Name { get; set; }


        public OracleTestTable(IDbConnection connection, IQuoter quoter, string schema, params string[] columnDefinitions)
        {
            Connection = connection;
            Quoter = quoter;
            Factory = new OracleDbFactory();
            _schema = schema;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Name = "TestTable";
            Create(columnDefinitions);
        }

        public void Dispose()
        {
            Drop();
        }

        public void Create(IEnumerable<string> columnDefinitions)
        {
            var sb = CreateSchemaQuery();

            sb.Append("CREATE TABLE ");
            sb.Append(Quoter.QuoteTableName(Name));

            foreach (string definition in columnDefinitions)
            {
                sb.Append("(");
                sb.Append(definition);
                sb.Append("), ");
            }

            sb.Remove(sb.Length - 2, 2);

            using (var command = Factory.CreateCommand(sb.ToString(), Connection))
                command.ExecuteNonQuery();
        }

        private StringBuilder CreateSchemaQuery()
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(_schema))
            {
                sb.Append(string.Format("CREATE SCHEMA AUTHORIZATION {0} ", _schema));
            }
            return sb;
        }

        public void WithUniqueConstraintOn(string column)
        {
            var sb = new StringBuilder();
            var constraintName = Quoter.QuoteConstraintName("UC_" + column);
            constraints.Add(constraintName);
            sb.Append(string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2})", Quoter.QuoteTableName(Name), constraintName, Quoter.QuoteColumnName(column)));
            using (var command = Factory.CreateCommand(sb.ToString(), Connection))
                command.ExecuteNonQuery();
        }

        public void WithIndexOn(string column)
        {
            var sb = new StringBuilder();
            var indexName = Quoter.QuoteIndexName("UI_" + column);
            indexies.Add(indexName);
            sb.Append(string.Format("CREATE UNIQUE INDEX {0} ON {1} ({2})", indexName, Quoter.QuoteTableName(Name), Quoter.QuoteColumnName(column)));
            using (var command = Factory.CreateCommand(sb.ToString(), Connection))
                command.ExecuteNonQuery();
        }

        public void Drop()
        {
            foreach(var contraint in constraints)
            {
                using (var command = Factory.CreateCommand("ALTER TABLE " + Quoter.QuoteTableName(Name) + " DROP CONSTRAINT " + contraint, Connection))
                    command.ExecuteNonQuery();
            }

            foreach (var index in indexies)
            {
                using (var command = Factory.CreateCommand("DROP INDEX " + index, Connection))
                    command.ExecuteNonQuery();
            }

            using (var command = Factory.CreateCommand("DROP TABLE " + Quoter.QuoteTableName(Name), Connection))
                command.ExecuteNonQuery();
        }
    }
}