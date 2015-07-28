using System.Data;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Redshift
{
    internal class RedshiftTypeMap : TypeMapBase
    {
        private const int DecimalCapacity = 1000;
        private const int PostgresMaxVarcharSize = 10485760;

        protected override void SetupTypeMaps()
        {
            SetTypeMap(DbType.AnsiStringFixedLength, "char(255)");
            SetTypeMap(DbType.AnsiStringFixedLength, "char($size)", int.MaxValue);
            SetTypeMap(DbType.AnsiString, "text");
            SetTypeMap(DbType.AnsiString, "varchar($size)", PostgresMaxVarcharSize);
            SetTypeMap(DbType.AnsiString, "text", int.MaxValue);
            SetTypeMap(DbType.Boolean, "boolean");
            SetTypeMap(DbType.Byte, "smallint"); //no built-in support for single byte unsigned integers
            SetTypeMap(DbType.DateTime, "timestamp");
            SetTypeMap(DbType.DateTime2, "timestamp"); // timestamp columns in postgres can support a larger date range.  Source: http://www.postgresql.org/docs/9.1/static/datatype-datetime.html
            SetTypeMap(DbType.DateTimeOffset, "timestamptz");
            SetTypeMap(DbType.Decimal, "decimal(18,0)");
            SetTypeMap(DbType.Decimal, "decimal($size,$precision)", DecimalCapacity);
            SetTypeMap(DbType.Double, "float8");
            SetTypeMap(DbType.Int16, "smallint");
            SetTypeMap(DbType.Int32, "integer");
            SetTypeMap(DbType.Int64, "bigint");
            SetTypeMap(DbType.Single, "float4");
            SetTypeMap(DbType.StringFixedLength, "char(255)");
            SetTypeMap(DbType.StringFixedLength, "char($size)", int.MaxValue);
            SetTypeMap(DbType.String, "text");
            SetTypeMap(DbType.String, "varchar($size)", PostgresMaxVarcharSize);
            SetTypeMap(DbType.String, "text", int.MaxValue);
        }
    }
}