using System.Data;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.MySql
{
    internal class MySqlTypeMap : TypeMapBase
    {
        public const int AnsiTinyStringCapacity = 127;
        public const int StringCapacity = 255;
        public const int VarcharCapacity = 8192;
        public const int TextCapacity = 65535;
        public const int MediumTextCapacity = 16777215;
        public const int LongTextCapacity = int.MaxValue;
        public const int DecimalCapacity = 19;

        protected override void SetupTypeMaps()
        {
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.AnsiStringFixedLength, "CHAR($size)", StringCapacity);
            SetTypeMap(DbType.AnsiStringFixedLength, "TEXT", TextCapacity);
            SetTypeMap(DbType.AnsiStringFixedLength, "MEDIUMTEXT", MediumTextCapacity);
            SetTypeMap(DbType.AnsiStringFixedLength, "LONGTEXT", LongTextCapacity);
            SetTypeMap(DbType.AnsiString, "VARCHAR(255)");
            SetTypeMap(DbType.AnsiString, "VARCHAR($size)", VarcharCapacity);
            SetTypeMap(DbType.AnsiString, "TEXT", TextCapacity);
            SetTypeMap(DbType.AnsiString, "MEDIUMTEXT", MediumTextCapacity);
            SetTypeMap(DbType.AnsiString, "LONGTEXT", LongTextCapacity);
            SetTypeMap(DbType.Binary, "LONGBLOB");
            SetTypeMap(DbType.Binary, "TINYBLOB", AnsiTinyStringCapacity);
            SetTypeMap(DbType.Binary, "BLOB", TextCapacity);
            SetTypeMap(DbType.Binary, "MEDIUMBLOB", MediumTextCapacity);
            SetTypeMap(DbType.Boolean, "TINYINT(1)");
            SetTypeMap(DbType.Byte, "TINYINT UNSIGNED");
            SetTypeMap(DbType.Currency, "DECIMAL(19,4)");
            SetTypeMap(DbType.Date, "DATE");
            SetTypeMap(DbType.DateTime, "DATETIME");
            SetTypeMap(DbType.Decimal, "DECIMAL(19,5)");
            SetTypeMap(DbType.Decimal, "DECIMAL($size,$precision)", DecimalCapacity);
            SetTypeMap(DbType.Double, "DOUBLE");
            SetTypeMap(DbType.Guid, "CHAR(36)");
            SetTypeMap(DbType.Int16, "SMALLINT");
            SetTypeMap(DbType.Int32, "INTEGER");
            SetTypeMap(DbType.Int64, "BIGINT");
            SetTypeMap(DbType.Single, "FLOAT");
            SetTypeMap(DbType.StringFixedLength, "CHAR(255)");
            SetTypeMap(DbType.StringFixedLength, "CHAR($size)", StringCapacity);
            SetTypeMap(DbType.StringFixedLength, "TEXT", TextCapacity);
            SetTypeMap(DbType.StringFixedLength, "MEDIUMTEXT", MediumTextCapacity);
            SetTypeMap(DbType.StringFixedLength, "LONGTEXT", LongTextCapacity);
            SetTypeMap(DbType.String, "VARCHAR(255)");
            SetTypeMap(DbType.String, "VARCHAR($size)", VarcharCapacity);
            SetTypeMap(DbType.String, "TEXT", TextCapacity);
            SetTypeMap(DbType.String, "MEDIUMTEXT", MediumTextCapacity);
            SetTypeMap(DbType.String, "LONGTEXT", LongTextCapacity);
            SetTypeMap(DbType.Time, "DATETIME");
        }
    }
}
