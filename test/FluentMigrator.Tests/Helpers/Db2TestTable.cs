namespace FluentMigrator.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    using FluentMigrator.Runner.Generators.DB2;
    using FluentMigrator.Runner.Processors;
    using FluentMigrator.Runner.Processors.DB2;

    public class Db2TestTable : IDisposable
    {
        #region Fields

        private readonly Db2Quoter quoter = new Db2Quoter();

        private List<string> constraints = new List<string>();
        private string _schema;

        #endregion Fields

        #region Constructors

        public Db2TestTable(Db2Processor processor, string schema, params string[] columnDefinitions)
        {
            Connection = processor.Connection;
            Transaction = processor.Transaction;
            Processor = processor;
            Factory = new Db2DbFactory();
            _schema = schema;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Name = "TestTable";
            NameWithSchema = !string.IsNullOrEmpty(_schema) ? string.Format("{0}.{1}", quoter.QuoteSchemaName(_schema), quoter.QuoteTableName(Name)) : quoter.QuoteTableName(Name);
            Create(columnDefinitions);
        }

        public Db2TestTable(string table, Db2Processor processor, string schema, params string[] columnDefinitions)
        {
            Connection = processor.Connection;
            Transaction = processor.Transaction;
            Processor = processor;
            Factory = new Db2DbFactory();
            _schema = schema;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Name = quoter.UnQuote(table);
            NameWithSchema = !string.IsNullOrEmpty(_schema) ? string.Format("{0}.{1}", quoter.QuoteSchemaName(_schema), quoter.QuoteTableName(Name)) : quoter.QuoteTableName(Name);
            Create(columnDefinitions);
        }

        #endregion Constructors

        #region Properties

        public string Name
        {
            get;
            set;
        }

        public string NameWithSchema
        {
            get;
            set;
        }

        public IDbTransaction Transaction
        {
            get;
            set;
        }

        private IDbConnection Connection
        {
            get;
            set;
        }

        private IDbFactory Factory
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        public void Create(string[] columnDefinitions)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(_schema))
            {
                sb.AppendFormat("CREATE SCHEMA {0} ", quoter.QuoteSchemaName(_schema));
            }

            var columns = string.Join(", ", columnDefinitions);
            sb.AppendFormat("CREATE TABLE {0} ({1})", NameWithSchema, columns);

            using (var command = Factory.CreateCommand(sb.ToString(), Connection, Transaction))
            {
                command.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            Drop();
        }

        public void Drop()
        {
            var tableCommand = string.Format("DROP TABLE {0}", NameWithSchema);

            using (var command = Factory.CreateCommand(tableCommand, Connection, Transaction))
            {
                command.ExecuteNonQuery();
            }

            if (!string.IsNullOrEmpty(_schema))
            {
                var schemaCommand = string.Format("DROP SCHEMA {0} RESTRICT", quoter.QuoteSchemaName(_schema));

                using (var commandToo = Factory.CreateCommand(schemaCommand, Connection, Transaction))
                {
                    commandToo.ExecuteNonQuery();
                }
            }
        }

        public void WithIndexOn(string column, string name)
        {
            var query = string.Format("CREATE UNIQUE INDEX {0} ON {1} ({2})",
                quoter.QuoteIndexName(name),
                NameWithSchema,
                quoter.QuoteColumnName(column)
                );

            using (var command = Factory.CreateCommand(query, Connection, Transaction))
            {
                command.ExecuteNonQuery();
            }
        }

        public void WithUniqueConstraintOn(string column, string name)
        {
            var constraintName = !string.IsNullOrEmpty(_schema) ? quoter.QuoteSchemaName(_schema) + "." + quoter.QuoteConstraintName(name) : quoter.QuoteConstraintName(name);

            var query = string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2})",
                NameWithSchema,
                constraintName,
                quoter.QuoteColumnName(column)
            );

            using (var command = Factory.CreateCommand(query, Connection, Transaction))
            {
                command.ExecuteNonQuery();
            }
        }

        #endregion Methods

        public Db2Processor Processor { get; set; }
    }
}