using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Expressions;


namespace FluentMigrator.Runner.Processors.Oracle
{
	public class OracleProcessor : ProcessorBase
	{
	   private bool AutoGenerateSequenceForIdentityColumn = true;
      private string SequenceNameFormat = "{0}SEQ";

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
		    return true;
		}

        public override bool TableExists(string schemaName, string tableName)
		{
			return Exists("SELECT TABLE_NAME FROM USER_TABLES WHERE LOWER(TABLE_NAME)='{0}'", tableName.ToLower());
		}

        public override void Process(CreateTableExpression expression)
        {
           Process(Generator.Generate(expression));

           if (AutoGenerateSequenceForIdentityColumn)
           {
              // Generate a sequence starting at one
              // ... 
              Process(
                 string.Format("CREATE SEQUENCE {0} MINVALUE 1 START WITH {1} INCREMENT BY 1 CACHE 20",
                               string.Format(SequenceNameFormat, expression.TableName.ToUpper())
                               , 1));

           }
        }

        public override void Process(InsertDataExpression expression)
        {
           base.Process(expression);

           if (expression.WithIdentity && AutoGenerateSequenceForIdentityColumn)
           {
              // Select the current number of rows from the table
              var count = Read(string.Format("SELECT COUNT(*) FROM {0}", expression.TableName));
              var startValue = 0;
              if (count != null && count.Tables[0].Rows.Count == 1)
                 startValue = int.Parse(count.Tables[0].Rows[0][0].ToString());

              if (startValue > 0)
              {
                 // Drop the default sequence that was generated
                 Process(string.Format("DROP SEQUENCE {0}",
                                       string.Format(SequenceNameFormat, expression.TableName.ToUpper())));

                 // And re create the sequence with the new start value
                 Process(
                    string.Format("CREATE SEQUENCE {0} MINVALUE 1 START WITH {1} INCREMENT BY 1 CACHE 20",
                                  string.Format(SequenceNameFormat, expression.TableName.ToUpper())
                                  , startValue + 1));
              }

           }
        }


        public override bool ColumnExists(string schemaName, string tableName, string columnName)
		{
			return Exists("SELECT COLUMN_NAME FROM USER_TAB_COLUMNS WHERE LOWER(TABLE_NAME) = '{0}' AND LOWER(COLUMN_NAME) = '{1}'", tableName.ToLower(), columnName.ToLower());
		}

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
		{
         const string sql = @"SELECT * FROM USER_CONSTRAINTS WHERE LOWER(TABLE_NAME) = '{0}' AND LOWER(CONSTRAINT_NAME) = '{1}'";
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

      /// <summary>
      /// Override the alter column to handle special case where NOT NULL specified on existing NOT NULL column
      /// </summary>
      /// <param name="expression"></param>
      public override void Process(AlterColumnExpression expression)
      {
         try
         {
            Process(Generator.Generate(expression));
         }
         catch (Exception ex)
         {
            // Check if we have the special case
            if ( ex.Message.StartsWith("ORA-01442: column to be modified to NOT NULL is already NOT NULL") )
            {
               // Remove the NOT NULL as it has already been set
               Process(Generator.Generate(expression).Replace("NOT NULL",""));
            }
            else
               throw;
         }
         
      }
	}
}