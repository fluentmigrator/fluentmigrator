using System;
using System.Text;
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Processors.Snowflake;

namespace FluentMigrator.Tests.Helpers
{
    public class SnowflakeTestTable : IDisposable
    {
        private readonly string _schema;

        public SnowflakeProcessor Processor { get; }
        private readonly SnowflakeQuoter _quoter;

        public SnowflakeTestTable(SnowflakeProcessor processor, string schema, params string[] columnDefinitions) : this("TestTable", processor, schema, columnDefinitions) { }

        public SnowflakeTestTable(string table, SnowflakeProcessor processor, string schema, params string[] columnDefinitions)
        {
            Processor = processor;
            _quoter = Processor.Quoter;
            _schema = schema;

            Name = _quoter.UnQuote(table) + "_" + Guid.NewGuid().GetHashCode().ToString("X8");
            NameWithSchema = _quoter.QuoteTableName(Name, _schema);
            Create(columnDefinitions);
        }

        public string Name
        {
            get;
        }

        public string NameWithSchema
        {
            get;
        }

        public void Create(string[] columnDefinitions)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(_schema))
            {
                sb.AppendFormat("CREATE SCHEMA {0};", _quoter.QuoteSchemaName(_schema));
            }

            var columns = string.Join(", ", columnDefinitions);
            sb.AppendFormat("CREATE TABLE {0} ({1})", NameWithSchema, columns);

            Processor.Execute(sb.ToString());
        }

        public void Dispose()
        {
            Drop();
        }

        public void Drop()
        {
            var tableCommand = $"DROP TABLE {NameWithSchema}";
            Processor.Execute(tableCommand);

            if (!string.IsNullOrEmpty(_schema))
            {
                var schemaCommand = $"DROP SCHEMA {_quoter.QuoteSchemaName(_schema)} RESTRICT";
                Processor.Execute(schemaCommand);
            }
        }
    }
}
