using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using FluentMigrator.Builders.Execute;


namespace FluentMigrator.Runner.Processors.Oracle
{
	public class OracleProcessor : ProcessorBase
	{
		public virtual IDbConnection Connection { get; set; }
		

		public OracleProcessor(IDbConnection connection, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options)
			: base(generator, announcer, options)
		{
			Connection = connection;


			//oracle does not support ddl transactions
			//this.Transaction = this.Connection.BeginTransaction();
		}

		public override bool SchemaExists(string schemaName)
		{
			throw new NotImplementedException();
		}

        public override bool TableExists(string schemaName, string tableName)
		{
			return Exists("SELECT TABLE_NAME FROM USER_TABLES WHERE LOWER(TABLE_NAME)='{0}'", tableName.ToLower());
		}

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
		{
			return Exists("SELECT COLUMN_NAME FROM USER_TAB_COLUMNS WHERE LOWER(TABLE_NAME) = '{0}' AND LOWER(COLUMN_NAME) = '{1}'", tableName.ToLower(), columnName.ToLower());
		}

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
		{
			const string sql = @"S'";
			return Exists(sql, tableName.ToLower(), constraintName.ToLower());
		}

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return Exists("SELECT INDEX_NAME FROM ALL_INDEXES WHERE LOWER(TABLE_NAME) = '{0}' AND LOWER(INDEX_NAME) = '{1}'", tableName.ToLower(), indexName.ToLower());
        }

		public override void Execute(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open)
				Connection.Open();

			using (var command = OracleFactory.GetCommand(Connection,String.Format(template, args)))
			{
				command.ExecuteNonQuery();
			}
		}

		public override bool Exists(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open)
				Connection.Open();

			using (var command = OracleFactory.GetCommand(Connection,String.Format(template, args)))
			using (var reader = command.ExecuteReader())
			{
				return reader.Read();
			}
		}

        public override DataSet ReadTableData(string schemaName, string tableName)
		{
			return Read("SELECT * FROM {0}", tableName);
		}

		public override DataSet Read(string template, params object[] args)
		{
			if (Connection.State != ConnectionState.Open) Connection.Open();

			DataSet ds = new DataSet();
			using (var command = OracleFactory.GetCommand(Connection,String.Format(template, args)))
			using (DbDataAdapter adapter = OracleFactory.GetDataAdapter(command))
			{
				adapter.Fill(ds);
				return ds;
			}
		}

		public override void Process(PerformDBOperationExpression expression)
		{
			throw new NotImplementedException();
		}

		protected override void Process(string sql)
		{
			Announcer.Sql(sql);

			if (Options.PreviewOnly || string.IsNullOrEmpty(sql))
				return;

			if (Connection.State != ConnectionState.Open)
				Connection.Open();

			using (var command = OracleFactory.GetCommand(Connection,sql))
				command.ExecuteNonQuery();
		}
	}
}