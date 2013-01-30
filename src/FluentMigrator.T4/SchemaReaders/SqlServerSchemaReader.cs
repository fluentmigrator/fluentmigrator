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
            var result = new Tables();

            this._connection = connection;
            this._factory = factory;

            //pull the tables in a reader
            using (var cmd = this._factory.CreateCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = TableSql;

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        Table tbl = new Table();
                        tbl.Name = rdr["TABLE_NAME"].ToString();
                        tbl.Schema = rdr["TABLE_SCHEMA"].ToString();
                        tbl.IsView = String.Compare(rdr["TABLE_TYPE"].ToString(), "View", true) == 0;
                        tbl.CleanName = CleanUp(tbl.Name);
                        tbl.ClassName = Inflector.MakeSingular(tbl.CleanName);

                        result.Add(tbl);
                    }
                }
            }

            foreach (var tbl in result)
            {
                tbl.Columns = this.LoadColumns(tbl);
                tbl.Indexes = this.LoadIndices(tbl);
                tbl.ForeignKeys = this.LoadForeignKeys(tbl);

                // Mark the primary key
                var primaryKey = this.GetPrimaryKey(tbl.Name).Select(c => c.ToLowerInvariant());
                var primaryKeyColumns = tbl.Columns.Where(c => primaryKey.Contains(c.Name.ToLowerInvariant()));

                foreach (var column in primaryKeyColumns)
                    column.IsPrimaryKey = true;
            }


            return result;
        }

        DbConnection _connection;
        DbProviderFactory _factory;


        List<Column> LoadColumns(Table tbl)
        {

            using (var cmd = this._factory.CreateCommand())
            {
                cmd.Connection = this._connection;
                cmd.CommandText = ColumnSql;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tableName";
                p.Value = tbl.Name;
                cmd.Parameters.Add(p);

                p = cmd.CreateParameter();
                p.ParameterName = "@schemaName";
                p.Value = tbl.Schema;
                cmd.Parameters.Add(p);

                var result = new List<Column>();
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var type = GetPropertyType(rdr["DataType"].ToString());
                        Column col = new Column();
                        col.Name = rdr["ColumnName"].ToString();
                        col.PropertyName = CleanUp(col.Name);
                        col.PropertyType = type;
                        col.CustomType = type == null
                            ? rdr["DataType"].ToString().ToLowerInvariant()
                            : null;
                        col.Size = rdr.Get("MaxLength", -1);
                        col.Precision = rdr.Get("Precision", -1);
                        col.IsNullable = rdr["IsNullable"].ToString() == "YES";
                        col.IsAutoIncrement = ((int)rdr["IsIdentity"]) == 1;
                        col.DefaultValue = rdr.IsDBNull(5) ? null : rdr["DefaultSetting"].ToString();
                        result.Add(col);
                    }
                }

                return result;
            }
        }

        List<TableIndex> LoadIndices(Table tbl)
        {
            const string sql = @"SELECT DISTINCT OBJECT_SCHEMA_NAME(T.[object_id],DB_ID()) AS [Schema],  
              T.[name] AS [table_name], I.[name] AS [index_name], AC.[name] AS [column_name],  
              I.[type_desc], I.[is_unique], I.[data_space_id], I.[ignore_dup_key], I.[is_primary_key], 
              I.[is_unique_constraint], I.[fill_factor],    I.[is_padded], I.[is_disabled], I.[is_hypothetical], 
              I.[allow_row_locks], I.[allow_page_locks], IC.[is_descending_key], IC.[is_included_column] 
            FROM sys.[tables] AS T  
              INNER JOIN sys.[indexes] I ON T.[object_id] = I.[object_id] 
              INNER JOIN sys.[index_columns] IC ON I.[object_id] = IC.[object_id] AND I.index_id = IC.index_id 
              INNER JOIN sys.[all_columns] AC ON T.[object_id] = AC.[object_id] AND IC.[column_id] = AC.[column_id] 
            WHERE T.[is_ms_shipped] = 0 AND I.[type_desc] <> 'HEAP' 
            AND I.is_primary_key = 0 AND T.[name] = @tableName and OBJECT_SCHEMA_NAME(T.[object_id],DB_ID()) = @schemaName";

            using (var cmd = this._factory.CreateCommand())
            {
                cmd.Connection = this._connection;
                cmd.CommandText = sql;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tableName";
                p.Value = tbl.Name;
                cmd.Parameters.Add(p);

                p = cmd.CreateParameter();
                p.ParameterName = "@schemaName";
                p.Value = tbl.Schema;
                cmd.Parameters.Add(p);

                var result = new List<TableIndex>();

                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {

                        string thisTable = rdr["table_name"].ToString();

                        if (tbl.Name.ToLower() == thisTable.ToLower())
                        {
                            var indexName = rdr["index_name"].ToString();
                            if (!result.Exists(i => i.Name == indexName))
                            {
                                TableIndex index = new TableIndex();
                                index.Name = indexName;
                                index.IsUnique = rdr.GetBoolean(rdr.GetOrdinal("is_unique"));
                                index.IndexColumns = new List<IndexColumn>();
                                index.IndexColumns.Add(new IndexColumn { Name = rdr["column_name"].ToString(), IsAsc = !rdr.GetBoolean(rdr.GetOrdinal("is_descending_key")) });
                                result.Add(index);
                            }
                            else
                            {
                                result.Single(i => i.Name == indexName).IndexColumns.Add(new IndexColumn { Name = rdr["column_name"].ToString(), IsAsc = !rdr.GetBoolean(rdr.GetOrdinal("is_descending_key")) });
                            }
                        }

                    }
                }
                return result;
            }
        }

        List<ForeignKey> LoadForeignKeys(Table tbl)
        {
            using (var cmd = this._factory.CreateCommand())
            {
                cmd.Connection = this._connection;
                cmd.CommandText = ForeignKeySql;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tableName";
                p.Value = tbl.Name;
                cmd.Parameters.Add(p);

                p = cmd.CreateParameter();
                p.ParameterName = "@schemaName";
                p.Value = tbl.Schema;
                cmd.Parameters.Add(p);

                var keys = cmd.Select(reader => new
                {
                    ForeignConstraintName = reader["ForeignConstraintName"].ToString(),
                    ForeignTableSchema = reader["ForeignTableSchema"].ToString(),
                    ForeignTable = reader["ForeignTable"].ToString(),
                    ForeignColumn = reader["ForeignColumn"].ToString(),
                    PrimaryConstraintName = reader["PrimaryConstraintName"].ToString(),
                    PrimaryTableSchema = reader["PrimaryTableSchema"].ToString(),
                    PrimaryTable = reader["PrimaryTable"].ToString(),
                    PrimaryColumn = reader["PrimaryColumn"].ToString(),
                    UpdateRule = DecodeRule(reader.Get<string>("UpdateRule")),
                    DeleteRule = DecodeRule(reader.Get<string>("DeleteRule")),
                });

                return keys.GroupBy(key => new
                {
                    key.ForeignConstraintName,
                    key.ForeignTableSchema,
                    key.ForeignTable,
                    key.PrimaryTableSchema,
                    key.PrimaryTable,
                    key.UpdateRule,
                    key.DeleteRule
                }).Select(foreignKeyGrouping => new ForeignKey
                {
                    Name = foreignKeyGrouping.Key.ForeignConstraintName,
                    ForeignTable = foreignKeyGrouping.Key.ForeignTable,
                    ForeignTableSchema = foreignKeyGrouping.Key.ForeignTableSchema,
                    ForeignColumns = foreignKeyGrouping.Select(f => f.ForeignColumn).ToList(),
                    PrimaryTable = foreignKeyGrouping.Key.PrimaryTable,
                    PrimaryTableSchema = foreignKeyGrouping.Key.PrimaryTableSchema,
                    PrimaryColumns = foreignKeyGrouping.Select(f => f.PrimaryColumn).ToList(),
                    PrimaryClass = Inflector.MakeSingular(CleanUp(foreignKeyGrouping.Key.PrimaryTable)),
                    UpdateRule = foreignKeyGrouping.Key.UpdateRule,
                    DeleteRule = foreignKeyGrouping.Key.DeleteRule
                }).ToList();
            }
        }

        IEnumerable<string> GetPrimaryKey(string table)
        {
            using (var cmd = this._factory.CreateCommand())
            {
                cmd.Connection = this._connection;
                cmd.CommandText = PrimaryKeySql;

                var p = cmd.CreateParameter();
                p.ParameterName = "@tableName";
                p.Value = table;
                cmd.Parameters.Add(p);

                return cmd.Select(c => (string)c["ColumnName"]).ToList();
            }
        }

        private static readonly IDictionary<string, DbType?> _typeMap =
            new Dictionary<string, DbType?>()
            {
                {"bigint", DbType.Int64}
                ,{"smallint",DbType.Int16}
                ,{"int",DbType.Int32}
                ,{"uniqueidentifier",DbType.Guid}
                ,{"smalldatetime",DbType.DateTime}
                ,{"datetime",DbType.DateTime}
                ,{"datetime2",DbType.DateTime2}
                ,{"date",DbType.Date}
                ,{"time",DbType.Time}
                ,{"float",DbType.Double}
                ,{"real",DbType.Single}
                ,{"numeric",DbType.Decimal}
                ,{"smallmoney",DbType.Currency}
                ,{"money",DbType.Currency}
                ,{"decimal",DbType.Decimal}
                ,{"tinyint",DbType.Byte}
                ,{"image",DbType.Binary}
                ,{"binary",DbType.Binary}
                ,{"varbinary",DbType.Binary}
                ,{"bit",DbType.Boolean}
                ,{"datetimeoffset",DbType.DateTimeOffset}
                ,{"char",DbType.AnsiStringFixedLength}
                ,{"varchar",DbType.AnsiString}
                ,{"text",DbType.AnsiString}
                ,{"nchar",DbType.StringFixedLength}
                ,{"nvarchar",DbType.String}
                ,{"ntext",DbType.String}

            };
        static DbType? GetPropertyType(string sqlType)
        {
            var sysType = default(DbType?);
            _typeMap.TryGetValue(sqlType, out sysType);
            return sysType;
        }

        private static System.Data.Rule DecodeRule(string rule)
        {
            return new Switch<string, System.Data.Rule>(rule)
                .Case("CASCADE", System.Data.Rule.Cascade)
                .Case("SET NULL",System.Data.Rule.SetDefault)
                .Case("SET DEFAULT", System.Data.Rule.SetNull)
                .Default(System.Data.Rule.None);
        }

        const string PrimaryKeySql = @"SELECT c.name AS ColumnName
                FROM sys.indexes AS i 
                INNER JOIN sys.index_columns AS ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id 
                INNER JOIN sys.objects AS o ON i.object_id = o.object_id 
                LEFT OUTER JOIN sys.columns AS c ON ic.object_id = c.object_id AND c.column_id = ic.column_id
                WHERE (i.type = 1) AND (o.name = @tableName)";

        const string TableSql = @"SELECT *
        FROM  INFORMATION_SCHEMA.TABLES
        WHERE TABLE_TYPE='BASE TABLE' OR TABLE_TYPE='VIEW'";

        const string ColumnSql = @"SELECT 
            TABLE_CATALOG AS [Database],
            TABLE_SCHEMA AS Owner, 
            TABLE_NAME AS TableName, 
            COLUMN_NAME AS ColumnName, 
            ORDINAL_POSITION AS OrdinalPosition, 
            COLUMN_DEFAULT AS DefaultSetting, 
            IS_NULLABLE AS IsNullable, DATA_TYPE AS DataType, 
            ISNULL(CHARACTER_MAXIMUM_LENGTH,CAST(NUMERIC_PRECISION as Int)) AS MaxLength, 
            NUMERIC_SCALE AS Precision,
            COLUMNPROPERTY(object_id('[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']'), COLUMN_NAME, 'IsIdentity') AS IsIdentity,
            COLUMNPROPERTY(object_id('[' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']'), COLUMN_NAME, 'IsComputed') as IsComputed
        FROM  INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME=@tableName AND TABLE_SCHEMA=@schemaName
        ORDER BY OrdinalPosition ASC";

        const string ForeignKeySql = @"
        SELECT 
            FK.CONSTRAINT_NAME AS ForeignConstraintName, 
            FK.TABLE_SCHEMA AS ForeignTableSchema, 
            FK.TABLE_NAME AS ForeignTable, 
            FKC.COLUMN_NAME AS ForeignColumn, 
            PK.CONSTRAINT_NAME AS PrimaryConstraintName, 
            PK.TABLE_SCHEMA AS PrimaryTableSchema, 
            PK.TABLE_NAME AS PrimaryTable, 
            PKC.COLUMN_NAME AS PrimaryColumn, 
            RC.UPDATE_RULE AS UpdateRule, 
            RC.DELETE_RULE AS DeleteRule
        FROM         
            INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS FK INNER JOIN
            INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS FKC ON FK.CONSTRAINT_SCHEMA = FKC.CONSTRAINT_SCHEMA AND 
            FK.CONSTRAINT_NAME = FKC.CONSTRAINT_NAME INNER JOIN
            INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS RC ON FK.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA AND 
            FK.CONSTRAINT_NAME = RC.CONSTRAINT_NAME INNER JOIN
            INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS PK ON RC.UNIQUE_CONSTRAINT_SCHEMA = PK.CONSTRAINT_SCHEMA AND 
            RC.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME INNER JOIN
            INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS PKC ON PK.CONSTRAINT_SCHEMA = PKC.CONSTRAINT_SCHEMA AND 
            PK.CONSTRAINT_NAME = PKC.CONSTRAINT_NAME AND FKC.ORDINAL_POSITION = PKC.ORDINAL_POSITION
        WHERE     
            (FK.CONSTRAINT_TYPE = 'FOREIGN KEY') 
            AND (FK.TABLE_NAME=@tableName) 
            AND (FK.TABLE_SCHEMA=@schemaName)
        ORDER BY ForeignConstraintName, ForeignTableSchema, ForeignTable, FKC.ORDINAL_POSITION";
    }

    public static class DbDataReaderExtensions
    {
        public static IEnumerable<T> Select<T>(this DbDataReader reader, Func<DbDataReader, T> selector)
        {
            while (reader.Read())
                yield return selector(reader);
        }

        public static IEnumerable<T> Select<T>(this DbCommand command, Func<DbDataReader, T> selector)
        {
            using (var reader = command.ExecuteReader())
                return reader.Select(selector).ToList();
        }
    }
}