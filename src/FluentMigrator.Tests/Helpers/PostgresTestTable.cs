﻿using System;
using System.Collections.Generic;
using System.Text;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;
using Npgsql;

namespace FluentMigrator.Tests.Helpers
{
    public class PostgresTestTable : IDisposable
    {
        private readonly PostgresQuoter quoter = new PostgresQuoter();
        private readonly string _schemaName;
        public NpgsqlConnection Connection { get; private set; }
        public string Name { get; set; }
        public string NameWithSchema { get; set; }
        public NpgsqlTransaction Transaction { get; private set; }

        public PostgresTestTable(PostgresProcessor processor, string schemaName, params string[] columnDefinitions) 
        {
            _schemaName = schemaName;
            Name = "\"Table" + Guid.NewGuid().ToString("N") + "\"";
            Init(processor, columnDefinitions);

        }

        public PostgresTestTable(string schemaName, string tableName, PostgresProcessor processor, params string[] columnDefinitions)
        {
            _schemaName = schemaName;
            
            Name = quoter.QuoteTableName(tableName);
            Init(processor, columnDefinitions);
        }

        private void Init(PostgresProcessor processor, IEnumerable<string> columnDefinitions)
        {      
            Connection = (NpgsqlConnection) processor.Connection;
            Transaction = (NpgsqlTransaction) processor.Transaction;
                
            NameWithSchema = string.IsNullOrEmpty(_schemaName) ? Name : string.Format("\"{0}\".{1}", _schemaName, Name);
            Create(columnDefinitions);
        }

        public void Dispose()
        {
            Drop();
        }

        public void Create(IEnumerable<string> columnDefinitions)
        {
            var sb = new StringBuilder();

			if(!string.IsNullOrEmpty(_schemaName))
				sb.AppendFormat("CREATE SCHEMA \"{0}\";",_schemaName);

            sb.Append("CREATE TABLE ");

            sb.Append(NameWithSchema);
            sb.Append(" (");

            foreach (string definition in columnDefinitions)
            {
                sb.Append(definition);
                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(")");

            var s = sb.ToString();
            using (var command = new NpgsqlCommand(s, Connection, Transaction))
                command.ExecuteNonQuery();
        }

        public void Drop()
        {
        	var sb = new StringBuilder();
        	sb.AppendFormat("DROP TABLE {0}", NameWithSchema);
			if (!string.IsNullOrEmpty(_schemaName))
				sb.AppendFormat(";DROP SCHEMA \"{0}\"", _schemaName);

            using (var command = new NpgsqlCommand(sb.ToString(), Connection, Transaction))
                command.ExecuteNonQuery();
        }
    }
}