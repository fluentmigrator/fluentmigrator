using FluentMigrator.Runner.Generators.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FluentMigrator.Runner.Generators.DB2
{
    class Db2TypeMap : TypeMapBase
    {
        protected override void SetupTypeMaps()
        {
            SetTypeMap(DbType.AnsiStringFixedLength, "CHARACTER(255)");
            SetTypeMap(DbType.AnsiStringFixedLength, "CHARACTER($size)", 255);
            SetTypeMap(DbType.AnsiString, "VARCHAR(255)");
            SetTypeMap(DbType.AnsiString, "VARCHAR($size)", 32704);
            SetTypeMap(DbType.AnsiString, "CLOB(1048576)");
            SetTypeMap(DbType.AnsiString, "CLOB($size)", Int32.MaxValue);
            SetTypeMap(DbType.Binary, "BINARY(255)");
            SetTypeMap(DbType.Binary, "BINARY($size)", 255);
            SetTypeMap(DbType.Binary, "VARBINARY(8000)");
            SetTypeMap(DbType.Binary, "VARBINARY($size)", 32704);
            SetTypeMap(DbType.Binary, "BLOB(1048576)");
            SetTypeMap(DbType.Binary, "BLOB($size)", 2147483647);
            SetTypeMap(DbType.Boolean, "CHAR(1)");
            SetTypeMap(DbType.Byte, "SMALLINT");
            SetTypeMap(DbType.Time, "TIME");
            SetTypeMap(DbType.Date, "DATE");
            SetTypeMap(DbType.DateTime, "TIMESTAMP");
            SetTypeMap(DbType.Decimal, "NUMERIC(19,5)");
            SetTypeMap(DbType.Decimal, "NUMERIC($size,$precision)", 31);
            SetTypeMap(DbType.Decimal, "DECIMAL(19,5)");
            SetTypeMap(DbType.Decimal, "DECIMAL($size,$precision)", 31);
            SetTypeMap(DbType.Double, "DOUBLE");
            SetTypeMap(DbType.Int16, "SMALLINT");
            SetTypeMap(DbType.Int32, "INT");
            SetTypeMap(DbType.Int32, "INTEGER");
            SetTypeMap(DbType.Int64, "BIGINT");
            SetTypeMap(DbType.Single, "REAL");
            SetTypeMap(DbType.Single, "DECFLOAT", 34);
            SetTypeMap(DbType.StringFixedLength, "GRAPHIC(128)");
            SetTypeMap(DbType.StringFixedLength, "GRAPHIC($size)", 128);
            SetTypeMap(DbType.String, "VARGRAPHIC(8000)");
            SetTypeMap(DbType.String, "VARGRAPHIC($size)", 16352);
            SetTypeMap(DbType.String, "DBCLOB(1048576)");
            SetTypeMap(DbType.String, "DBCLOB($size)", 1073741824);
            SetTypeMap(DbType.Xml, "XML");
        }
    }
}
