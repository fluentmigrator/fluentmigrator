﻿using System.Data;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Firebird
{
    internal class FirebirdTypeMap : TypeMapBase
    {
        private const int DecimalCapacity = 19;
        private const int FirebirdMaxVarcharSize = 32765;
        private const int FirebirdMaxCharSize = 32767;
        // http://www.firebirdsql.org/en/firebird-technical-specifications/
        private const int FirebirdMaxTextSize = int.MaxValue;  // as close as Int32 can come to 32GB

        protected override void SetupTypeMaps()
        {
            /*
             * Values were taken from the Interbase 6 Data Definition Guide
             * 
             * */
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size)", FirebirdMaxCharSize);
            SetTypeMap(DbType.AnsiString, "BLOB SUB_TYPE TEXT");
            SetTypeMap(DbType.AnsiString, "VARCHAR($size)", FirebirdMaxVarcharSize);
            SetTypeMap(DbType.Binary, "BLOB SUB_TYPE BINARY");
            SetTypeMap(DbType.Boolean, "VARCHAR(10)"); //no direct boolean support
            SetTypeMap(DbType.Byte, "SMALLINT");
            SetTypeMap(DbType.Currency, "BIGINT");
            SetTypeMap(DbType.Date, "DATE");
            SetTypeMap(DbType.DateTime, "TIMESTAMP");
            SetTypeMap(DbType.Decimal, "DECIMAL(14,5)");
            SetTypeMap(DbType.Decimal, "DECIMAL($precision,$size)", DecimalCapacity);
            SetTypeMap(DbType.Double, "DOUBLE PRECISION"); //64 bit double precision
            SetTypeMap(DbType.Guid, "CHAR(16)"); //no guid support, "only" uuid is supported(via gen_uuid() built-in function)
            SetTypeMap(DbType.Int16, "SMALLINT");
            SetTypeMap(DbType.Int32, "INTEGER");
            SetTypeMap(DbType.Int64, "BIGINT");
            SetTypeMap(DbType.Single, "FLOAT");
            SetTypeMap(DbType.StringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.StringFixedLength, "CHAR($size)", FirebirdMaxCharSize);
            SetTypeMap(DbType.String, "VARCHAR(255)");
            SetTypeMap(DbType.String, "VARCHAR($size)", FirebirdMaxVarcharSize);
            SetTypeMap(DbType.String, "BLOB SUB_TYPE TEXT", FirebirdMaxTextSize);
            SetTypeMap(DbType.Time, "TIME");
        }
    }
}