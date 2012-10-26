using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System;

namespace FluentMigrator.T4
{
    class SqlServerSchemaReader : SchemaReader
    {
        // SchemaReader.ReadSchema
        public override Tables ReadSchema(DbConnection connection, DbProviderFactory factory)
        {
            var result=new Tables();
        
            this._connection=connection;
            this._factory=factory;

            var cmd=this._factory.CreateCommand();
            cmd.Connection=connection;
            cmd.CommandText=TABLE_SQL;

            //pull the tables in a reader
            using(cmd)
            {

                using (var rdr=cmd.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        Table tbl=new Table();
                        tbl.Name=rdr["TABLE_NAME"].ToString();
                        tbl.Schema=rdr["TABLE_SCHEMA"].ToString();
                        tbl.IsView=String.Compare(rdr["TABLE_TYPE"].ToString(), "View", true)==0;
                        tbl.CleanName=CleanUp(tbl.Name);
                        tbl.ClassName=Inflector.MakeSingular(tbl.CleanName);

                        result.Add(tbl);
                    }
                }
            }

            foreach (var tbl in result)
            {
                tbl.Columns=this.LoadColumns(tbl);
                tbl.Indices = this.LoadIndices(tbl);
                tbl.FKeys = this.LoadFKeys(tbl);			
                    
                // Mark the primary key
                var primaryKey = this.GetPrimaryKey(tbl.Name);
                var primaryKeyColumns = tbl.Columns.Where(c => primaryKey.Contains(c.Name.ToLowerInvariant()));

                foreach (var column in primaryKeyColumns)
                    column.IsPK = true;
            }
        

            return result;
        }
    
        DbConnection _connection;
        DbProviderFactory _factory;
    

        List<Column> LoadColumns(Table tbl)
        {
    
            using (var cmd=this._factory.CreateCommand())
            {
                cmd.Connection=this._connection;
                cmd.CommandText=COLUMN_SQL;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tableName";
                p.Value=tbl.Name;
                cmd.Parameters.Add(p);

                p = cmd.CreateParameter();
                p.ParameterName = "@schemaName";
                p.Value=tbl.Schema;
                cmd.Parameters.Add(p);

                var result=new List<Column>();
                using (IDataReader rdr=cmd.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        Column col=new Column();
                        col.Name=rdr["ColumnName"].ToString();
                        col.PropertyName=CleanUp(col.Name);
                        col.PropertyType=GetPropertyType(rdr["DataType"].ToString());
                        col.Size=GetDatatypeSize(rdr["DataType"].ToString());
                        col.Precision=GetDatatypePrecision(rdr["DataType"].ToString());					
                        col.IsNullable=rdr["IsNullable"].ToString()=="YES";
                        col.IsAutoIncrement=((int)rdr["IsIdentity"])==1;
                        result.Add(col);
                    }
                }

                return result;
            }
        }

        List<TableIndex> LoadIndices(Table tbl)
        {
            string sql = @"SELECT DISTINCT OBJECT_SCHEMA_NAME(T.[object_id],DB_ID()) AS [Schema],  
              T.[name] AS [table_name], I.[name] AS [index_name], AC.[name] AS [column_name],  
              I.[type_desc], I.[is_unique], I.[data_space_id], I.[ignore_dup_key], I.[is_primary_key], 
              I.[is_unique_constraint], I.[fill_factor],    I.[is_padded], I.[is_disabled], I.[is_hypothetical], 
              I.[allow_row_locks], I.[allow_page_locks], IC.[is_descending_key], IC.[is_included_column] 
            FROM sys.[tables] AS T  
              INNER JOIN sys.[indexes] I ON T.[object_id] = I.[object_id]  
              INNER JOIN sys.[index_columns] IC ON I.[object_id] = IC.[object_id] 
              INNER JOIN sys.[all_columns] AC ON T.[object_id] = AC.[object_id] AND IC.[column_id] = AC.[column_id] 
            WHERE T.[is_ms_shipped] = 0 AND I.[type_desc] <> 'HEAP' 
            AND I.is_primary_key = 0 AND T.[name] = @tableName and OBJECT_SCHEMA_NAME(T.[object_id],DB_ID()) = @schemaName";
		
            using (var cmd=this._factory.CreateCommand())
            {
                cmd.Connection=this._connection;
                cmd.CommandText=sql;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tableName";
                p.Value=tbl.Name;
                cmd.Parameters.Add(p);

                p = cmd.CreateParameter();
                p.ParameterName = "@schemaName";
                p.Value=tbl.Schema;
                cmd.Parameters.Add(p);

                var result=new List<TableIndex>();

                using (IDataReader rdr=cmd.ExecuteReader())
                {
                    while(rdr.Read()){
					
                        string thisTable=rdr["table_name"].ToString();
            
                        if(tbl.Name.ToLower()==thisTable.ToLower()){
                            var indexName = rdr["index_name"].ToString();
                            if(!result.Exists(i => i.Name == indexName))
                            {
                                TableIndex index=new TableIndex();
                                index.Name= indexName;
                                index.IsUnique=rdr.GetBoolean(rdr.GetOrdinal("is_unique"));
                                index.IndexColumns = new List<IndexColumn>();
                                index.IndexColumns.Add(new IndexColumn { Name = rdr["column_name"].ToString(), IsAsc = !rdr.GetBoolean(rdr.GetOrdinal("is_descending_key"))});
                                result.Add(index);
                            }
                            else
                            {
                                result.Single(i => i.Name == indexName).IndexColumns.Add(new IndexColumn { Name = rdr["column_name"].ToString(), IsAsc = !rdr.GetBoolean(rdr.GetOrdinal("is_descending_key"))});
                            }
                        }
            
                    }
                }
                return result;	
            }	
        }

        List<ForeignKey> LoadFKeys(Table tbl)
        {
            using (var cmd=this._factory.CreateCommand())
            {
                cmd.Connection=this._connection;
                cmd.CommandText=FKSql;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tableName";
                p.Value=tbl.Name;
                cmd.Parameters.Add(p);

                p = cmd.CreateParameter();
                p.ParameterName = "@schemaName";
                p.Value=tbl.Schema;
                cmd.Parameters.Add(p);

                var result=new List<ForeignKey>();

                using (IDataReader rdr=cmd.ExecuteReader())
                {
                    while(rdr.Read()){
                        var fk = new ForeignKey();
                        string thisTable=rdr["ThisTable"].ToString();
            
                        if(tbl.Name.ToLower()==thisTable.ToLower())
                        {
                            fk.ThisTable=rdr["ThisTable"].ToString();
                            fk.ThisColumn=rdr["ThisColumn"].ToString();
                            fk.OtherTable=rdr["OtherTable"].ToString();
                            fk.OtherColumn=rdr["OtherColumn"].ToString();
                        }
                        else
                        {
                            fk.ThisTable=rdr["OtherTable"].ToString();
                            fk.ThisColumn=rdr["OtherColumn"].ToString();
                            fk.OtherTable=rdr["ThisTable"].ToString();
                            fk.OtherColumn=rdr["ThisColumn"].ToString();
                        }
            
                        fk.OtherClass=Inflector.MakeSingular(CleanUp(fk.OtherTable));
            
                        result.Add(fk);
                    }
                }
                return result;	
            }	
        }
	
        IList<string> GetPrimaryKey(string table)
        {
            const string sql = @"SELECT c.name AS ColumnName
                FROM sys.indexes AS i 
                INNER JOIN sys.index_columns AS ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id 
                INNER JOIN sys.objects AS o ON i.object_id = o.object_id 
                LEFT OUTER JOIN sys.columns AS c ON ic.object_id = c.object_id AND c.column_id = ic.column_id
                WHERE (i.type = 1) AND (o.name = @tableName)";

            using (var cmd=this._factory.CreateCommand())
            {
                cmd.Connection=this._connection;
                cmd.CommandText=sql;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tableName";
                p.Value=table;
                cmd.Parameters.Add(p);

                using (var result = cmd.ExecuteReader())
                {
                    var primaryKey = result
                        .Select(c => 
                            ((string)c["ColumnName"]).ToLowerInvariant())
                        .ToList();
                    return primaryKey;
                }
            }
        }

        static string GetPropertyType(string sqlType)
        {
            string sysType="string";
            switch (sqlType) 
            {
                case "bigint":
                    sysType = "long";
                    break;
                case "smallint":
                    sysType= "short";
                    break;
                case "int":
                    sysType= "int";
                    break;
                case "uniqueidentifier":
                    sysType=  "Guid";
                    break;
                case "smalldatetime":
                case "datetime":
                case "date":
                case "time":
                    sysType=  "DateTime";
                    break;
                case "float":
                    sysType="double";
                    break;
                case "real":
                    sysType="float";
                    break;
                case "numeric":
                case "smallmoney":
                case "decimal":
                case "money":
                    sysType=  "decimal";
                    break;
                case "tinyint":
                    sysType = "byte";
                    break;
                case "bit":
                    sysType=  "bool";
                    break;
                case "image":
                case "binary":
                case "varbinary":
                case "timestamp":
                    sysType=  "byte[]";
                    break;
                case "geography":
                    sysType = "Microsoft.SqlServer.Types.SqlGeography";
                    break;
                case "geometry":
                    sysType = "Microsoft.SqlServer.Types.SqlGeometry";
                    break;
            }
            return sysType;
        }



        const string TABLE_SQL=@"SELECT *
        FROM  INFORMATION_SCHEMA.TABLES
        WHERE TABLE_TYPE='BASE TABLE' OR TABLE_TYPE='VIEW'";

        const string COLUMN_SQL=@"SELECT 
            TABLE_CATALOG AS [Database],
            TABLE_SCHEMA AS Owner, 
            TABLE_NAME AS TableName, 
            COLUMN_NAME AS ColumnName, 
            ORDINAL_POSITION AS OrdinalPosition, 
            COLUMN_DEFAULT AS DefaultSetting, 
            IS_NULLABLE AS IsNullable, DATA_TYPE AS DataType, 
            CHARACTER_MAXIMUM_LENGTH AS MaxLength, 
            DATETIME_PRECISION AS DatePrecision,
            COLUMNPROPERTY(object_id('[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']'), COLUMN_NAME, 'IsIdentity') AS IsIdentity,
            COLUMNPROPERTY(object_id('[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']'), COLUMN_NAME, 'IsComputed') as IsComputed
        FROM  INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME=@tableName AND TABLE_SCHEMA=@schemaName
        ORDER BY OrdinalPosition ASC";

        const string FKSql=@"SELECT
        ThisTable  = FK.TABLE_NAME,
        ThisColumn = CU.COLUMN_NAME,
        OtherTable  = PK.TABLE_NAME,
        OtherColumn = PT.COLUMN_NAME, 
        Constraint_Name = C.CONSTRAINT_NAME,
        Owner = FK.TABLE_SCHEMA
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
    INNER JOIN
        (	
            SELECT i1.TABLE_NAME, i2.COLUMN_NAME
            FROM  INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
            WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
        ) 
    PT ON PT.TABLE_NAME = PK.TABLE_NAME
    WHERE (FK.Table_NAME=@tableName OR PK.Table_NAME=@tableName)  AND (FK.TABLE_SCHEMA=@schemaName OR PK.TABLE_SCHEMA=@schemaName)";
      
    }

    public static class DbReaderExtensions 
    { 
        public static IEnumerable<T> Select<T>(this DbDataReader reader, Func<DbDataReader, T> selector)
        {
            while (reader.Read())
                yield return selector(reader);
        }   
    }
}