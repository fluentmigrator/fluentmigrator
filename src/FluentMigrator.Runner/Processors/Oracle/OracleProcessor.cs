using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Oracle;


namespace FluentMigrator.Runner.Processors.Oracle
{
	public class OracleProcessor : ProcessorBase
	{
      /// <summary>
      /// If <c>True</c> indicates that sequences should automatically be created to Identity columns
      /// </summary>
	   private bool AutoGenerateSequenceForIdentityColumn = true;

      /// <summary>
      /// The default string.Format to apply to generate a sequence name
      /// </summary>
      private string SequenceNameFormat = "{0}SEQ";

      /// <summary>
      /// Delegate function thate allows custom generation of sequenec names based on a table name
      /// </summary>
      public Func<string,string> CustomSequenceNamer { get; set; }

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

           if (AutoGenerateSequenceForIdentityColumn && IdentityColumnExists(expression))
           {
              // Generate a sequence starting at one as default
              // ... 

              var sequenceName = string.Format(SequenceNameFormat, expression.TableName.ToUpper());
              if ( CustomSequenceNamer != null)
                 sequenceName = CustomSequenceNamer(expression.TableName);

              Process(string.Format("CREATE SEQUENCE {0} MINVALUE 1 START WITH {1} INCREMENT BY 1 CACHE 20",
                                sequenceName, 1));

           }
        }

	   private bool IdentityColumnExists(CreateTableExpression expression)
	   {
	      return expression.Columns.Where(c => c.IsIdentity).Count() == 1;
	   }

	   public override void Process(InsertDataExpression expression)
	   {
	      QuoteInsertColumnNames(expression);

	      base.Process(expression);   
           

           if (expression.WithIdentity && AutoGenerateSequenceForIdentityColumn)
           {
               // If the identity column is known then select MAX value else just assume that COUNT(*) isthe lastest value
               var count = Read(!string.IsNullOrEmpty(expression.IdentityColumn) ?
                   string.Format("SELECT COALESCE(MAX({1}),0) FROM {0}", expression.TableName, expression.IdentityColumn) 
                   : string.Format("SELECT COUNT(*) FROM {0}", expression.TableName));

               var startValue = 0;
              if (count != null && count.Tables[0].Rows.Count == 1)
                 startValue = int.Parse(count.Tables[0].Rows[0][0].ToString());

              if (startValue > 0)
              {
                 var sequenceName = string.Format(SequenceNameFormat, expression.TableName.ToUpper());
                 if (CustomSequenceNamer != null)
                    sequenceName = CustomSequenceNamer(expression.TableName);


                 // Drop the default sequence that was generated
                 Process(string.Format("DROP SEQUENCE {0}", sequenceName));

                 // And re create the sequence with the new start value
                 Process(
                    string.Format("CREATE SEQUENCE {0} MINVALUE 1 START WITH {1} INCREMENT BY 1 CACHE 20",
                                  sequenceName
                                  , startValue + 1));
              }

           }
        }

        /// <summary>
        /// Specail case for Oracle to add Unique Contraint. Required to enable foreign keys to a non primary key column
       /// See http://forums.oracle.com/forums/thread.jspa?threadID=1094544&tstart=0 for more information
        /// </summary>
        /// <param name="expression"></param>
       public override void Process(CreateIndexExpression expression)
       {
           base.Process(expression);

           // Check if we have a unique index beging created ... and we wish to create a corresponding unique constraint
           if (!string.IsNullOrEmpty(expression.Index.WithUniqueContraint) && expression.Index.IsUnique)
           {
               // 
               Process(
                    string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2}) USING INDEX {3}",
                                  expression.Index.TableName
                                  , expression.Index.WithUniqueContraint
                                  , GetIndexColumns(expression.Index.Columns)
                                  , expression.Index.Name));
           }
       }

	    private static object GetIndexColumns(IEnumerable<IndexColumnDefinition> columns)
	    {
	        var result = new StringBuilder();

	        foreach (var column in columns)
	        {
                if (result.Length > 0)
                    result.Append(", ");

	            result.Append(column.Name);
	        }

            return result.ToString();
	    }

	    private void QuoteInsertColumnNames(InsertDataExpression expression)
	   {
         if ( ! expression.CaseSensitiveColumnNames)
            return;

	      var quoter = new OracleQuoter {CaseSensitiveNames = expression.CaseSensitiveColumnNames};
	      foreach ( var row in expression.Rows )
	      {
	         var toRemove = (from nameValue in row
	                         let quotedName = quoter.QuoteColumnName(nameValue.Key)
                            where quotedName != nameValue.Key && (expression.CaseSensitiveColumns.Count == 0 || expression.CaseSensitiveColumns.Contains(nameValue.Key))
	                         select nameValue).ToList();

	         foreach (var keyValuePair in toRemove)
	         {
	            row.Remove(keyValuePair);
	            row.Add(new KeyValuePair<string, object>(quoter.QuoteColumnName(keyValuePair.Key), keyValuePair.Value));
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