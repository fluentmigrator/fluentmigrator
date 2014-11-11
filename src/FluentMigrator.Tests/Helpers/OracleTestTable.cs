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
        private readonly IQuoter quoter = new OracleQuoterQuotedIdentifier();

        private IDbConnection Connection { get; set; }
        private IDbFactory Factory { get; set; }
        private string _schema;
        private List<string> constraints = new List<string>();
        private List<string> indexies = new List<string>();
        public string Name { get; set; }


        public OracleTestTable(IDbConnection connection, string schema, IDbFactory factory, params string[] columnDefinitions)
        {
            Connection = connection;
            Factory = factory;
            _schema = schema;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Name = "TestTable";
            Create(columnDefinitions);
        }

        public OracleTestTable(string table, IDbConnection connection, string schema, IDbFactory factory, params string[] columnDefinitions)
        {
            Connection = connection;
            Factory = factory;
            _schema = schema;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Name = table;
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
            sb.Append(quoter.QuoteTableName(Name));

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
            WithUniqueConstraintOn(column, "UC_" + column);
        }

        public void WithUniqueConstraintOn(string column, string name)
        {
            var sb = new StringBuilder();
            sb.Append(string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2})", quoter.QuoteTableName(Name), quoter.QuoteConstraintName(name), quoter.QuoteColumnName(column)));
            using (var command = Factory.CreateCommand(sb.ToString(), Connection))
                command.ExecuteNonQuery();
			constraints.Add(name);
       }

        public void WithIndexOn(string column)
        {
            WithIndexOn(column, "UI_" + column);
        }

        public void WithIndexOn(string column, string name)
        {
            var sb = new StringBuilder();
            sb.Append(string.Format("CREATE UNIQUE INDEX {0} ON {1} ({2})", quoter.QuoteIndexName(name), quoter.QuoteTableName(Name), quoter.QuoteColumnName(column)));
            using (var command = Factory.CreateCommand(sb.ToString(), Connection))
                command.ExecuteNonQuery();
            indexies.Add(name);
        }

        public void Drop()
        {
            foreach(var constraint in constraints)
            {
                using (var command = Factory.CreateCommand(string.Format( "ALTER TABLE {0} DROP CONSTRAINT {1}", quoter.QuoteTableName(this.Name), quoter.QuoteConstraintName(constraint) ), Connection))
                    command.ExecuteNonQuery();
            }

            foreach (var index in indexies)
            {
                using (var command = Factory.CreateCommand(string.Format( "DROP INDEX {0}", this.quoter.QuoteIndexName( index ) ), Connection))
                    command.ExecuteNonQuery();
            }

            using (var command = Factory.CreateCommand("DROP TABLE " + quoter.QuoteTableName(Name), Connection))
                command.ExecuteNonQuery();
        }
    }
}