using System;
using System.Data.SqlServerCe;
using System.Collections.Generic;
using System.Text;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors.SqlServer;

namespace FluentMigrator.Tests.Helpers
{

    public class SqlServerCeTestTable : IDisposable
    {
        private SqlCeConnection Connection { get; set; }
        public string Name { get; set; }
        private IQuoter Quoter { get; set; }
        private List<string> constraints = new List<string>();
        private List<string> indexies = new List<string>();

        public SqlServerCeTestTable(string table, SqlServerCeProcessor processor, params string[] columnDefinitions)
        {
            Connection = (SqlCeConnection)processor.Connection;
            Quoter = new SqlServer2000Quoter();

            Name = table;
            Create(columnDefinitions);
        }

        public SqlServerCeTestTable(SqlServerCeProcessor processor, params string[] columnDefinitions)
        {
            Connection = (SqlCeConnection)processor.Connection;
            Quoter = new SqlServer2000Quoter();

            Name = "TestTable";
            Create(columnDefinitions);
        }

        public void Dispose()
        {
            Drop();
        }

        public void Create(IEnumerable<string> columnDefinitions)
        {

            var sb = new StringBuilder();
            sb.Append("CREATE TABLE ");
            sb.Append(Quoter.QuoteTableName(Name));

            foreach (string definition in columnDefinitions)
            {
                sb.Append("(");
                sb.Append(definition);
                sb.Append("), ");
            }

            sb.Remove(sb.Length - 2, 2);

            using (var command = new SqlCeCommand(sb.ToString(), Connection))
                command.ExecuteNonQuery();
        }

        public void WithUniqueConstraintOn(string column)
        {
            WithUniqueConstraintOn(column, "UC_" + column);
        }

        public void WithUniqueConstraintOn(string column, string name)
        {
            var sb = new StringBuilder();
            var constraintName = Quoter.Quote(name);
            constraints.Add(constraintName);
            sb.Append(string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2})", Quoter.QuoteTableName(Name), constraintName, column));
            using (var command = new SqlCeCommand(sb.ToString(), Connection))
                command.ExecuteNonQuery();
        }

        public void WithIndexOn(string column)
        {
            WithIndexOn(column, "UI_" + column);
        }

        public void WithIndexOn(string column, string name)
        {
            var sb = new StringBuilder();
            var indexName = Quoter.QuoteIndexName(name);
            indexies.Add(indexName);
            sb.Append(string.Format("CREATE UNIQUE INDEX {0} ON {1} ({2})", indexName, Quoter.QuoteTableName(Name), column));
            using (var command = new SqlCeCommand(sb.ToString(), Connection))
                command.ExecuteNonQuery();
        }

        public void Drop()
        {
            foreach (var contraint in constraints)
            {
                using (var command = new SqlCeCommand("ALTER TABLE " + Quoter.QuoteTableName(Name) + " DROP CONSTRAINT " + contraint, Connection))
                    command.ExecuteNonQuery();
            }

            foreach (var index in indexies)
            {
                using (var command = new SqlCeCommand("DROP INDEX " + Quoter.QuoteTableName(Name) + "." + index, Connection))
                    command.ExecuteNonQuery();
            }

            using (var command = new SqlCeCommand("DROP TABLE " + Quoter.QuoteTableName(Name), Connection))
                command.ExecuteNonQuery();
        }
    }
}
