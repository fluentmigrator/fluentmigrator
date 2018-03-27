using System.Data;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.Jet
{
    internal class JetTypeMap : TypeMapBase
    {
        public const int AnsiStringCapacity = 255;
        public const int AnsiTextCapacity = 1073741823;
        public const int UnicodeStringCapacity = 255;
        public const int UnicodeTextCapacity = 1073741823;
        public const int ImageCapacity = 2147483647;
        public const int DecimalCapacity = 19;

        protected override void SetupTypeMaps()
        {
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size)", AnsiStringCapacity);
            SetTypeMap(DbType.AnsiString, "VARCHAR(255)");
            SetTypeMap(DbType.AnsiString, "VARCHAR($size)", AnsiStringCapacity);
            SetTypeMap(DbType.AnsiString, "TEXT", AnsiTextCapacity);
            SetTypeMap(DbType.Binary, "VARBINARY(8000)");
            SetTypeMap(DbType.Binary, "VARBINARY($size)", ImageCapacity);
            SetTypeMap(DbType.Binary, "IMAGE", ImageCapacity);
            SetTypeMap(DbType.Boolean, "BIT");
            SetTypeMap(DbType.Byte, "TINYINT");
            SetTypeMap(DbType.Currency, "MONEY");
            SetTypeMap(DbType.Date, "DATETIME");
            SetTypeMap(DbType.DateTime, "DATETIME");
            SetTypeMap(DbType.Decimal, "DECIMAL(19,5)");
            SetTypeMap(DbType.Decimal, "DECIMAL($size,$precision)", DecimalCapacity);
            SetTypeMap(DbType.Double, "FLOAT");
            SetTypeMap(DbType.Guid, "UNIQUEIDENTIFIER");
            SetTypeMap(DbType.Int16, "SMALLINT");
            SetTypeMap(DbType.Int32, "INTEGER");
            // Jet does not have a 64-bit integer type, thus mapping it to a decimal
            SetTypeMap(DbType.Int64, "DECIMAL(20,0)");
            SetTypeMap(DbType.Single, "REAL");
            SetTypeMap(DbType.StringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.StringFixedLength, "CHAR($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.String, "VARCHAR(255)");
            SetTypeMap(DbType.String, "VARCHAR($size)", UnicodeStringCapacity);
            SetTypeMap(DbType.String, "TEXT", UnicodeTextCapacity);
            SetTypeMap(DbType.Time, "DATETIME");
        }
    }
}
