﻿

namespace FluentMigrator.Runner.Generators.Oracle
{

    using System.Data;
    using FluentMigrator.Runner.Generators.Base;

	internal class OracleTypeMap : TypeMapBase
	{
		public const int AnsiStringCapacity = 2000;
		public const int AnsiTextCapacity = 2147483647;
		public const int UnicodeStringCapacity = 2000;
		public const int UnicodeTextCapacity = int.MaxValue;
		public const int BlobCapacity = 2147483647;
      // http://download.oracle.com/docs/cd/B19306_01/server.102/b14220/datatype.htm
		public const int DecimalCapacity = 38;
		public const int XmlCapacity = 1073741823;

		protected override void SetupTypeMaps()
		{
			SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255)");
			SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size)", AnsiStringCapacity);
			SetTypeMap(DbType.AnsiString, "VARCHAR2(255)");
			SetTypeMap(DbType.AnsiString, "VARCHAR2($size)", AnsiStringCapacity);
			SetTypeMap(DbType.AnsiString, "CLOB", AnsiTextCapacity);
			SetTypeMap(DbType.Binary, "RAW(2000)");
			SetTypeMap(DbType.Binary, "RAW($size)", AnsiStringCapacity);
			SetTypeMap(DbType.Binary, "RAW(MAX)", AnsiTextCapacity);
			SetTypeMap(DbType.Binary, "BLOB", BlobCapacity);
			SetTypeMap(DbType.Boolean, "NUMBER(1,0)");
			SetTypeMap(DbType.Byte, "NUMBER(3,0)");
			SetTypeMap(DbType.Currency, "NUMBER(19,1)");
			SetTypeMap(DbType.Date, "DATE");
			SetTypeMap(DbType.DateTime, "TIMESTAMP(4)");
			SetTypeMap(DbType.Decimal, "NUMBER(19,5)");
			SetTypeMap(DbType.Decimal, "NUMBER($size,$precision)", DecimalCapacity);
			SetTypeMap(DbType.Double, "DOUBLE PRECISION");
			SetTypeMap(DbType.Guid, "RAW(16)");
			SetTypeMap(DbType.Int16, "NUMBER(5,0)");
			SetTypeMap(DbType.Int32, "NUMBER(10,0)");
			SetTypeMap(DbType.Int64, "NUMBER(20,0)");
			SetTypeMap(DbType.Single, "FLOAT(24)");
			SetTypeMap(DbType.StringFixedLength, "NCHAR(255)");
			SetTypeMap(DbType.StringFixedLength, "NCHAR($size)", UnicodeStringCapacity);
			SetTypeMap(DbType.String, "NVARCHAR2(255)");
			SetTypeMap(DbType.String, "NVARCHAR2($size)", UnicodeStringCapacity);
			SetTypeMap(DbType.String, "NCLOB", UnicodeTextCapacity);
			SetTypeMap(DbType.Time, "DATE");
			SetTypeMap(DbType.Xml, "XMLTYPE");
		}
	}
}
