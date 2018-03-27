

namespace FluentMigrator.Runner.Generators.SQLite
{
    using System.Data;
    using FluentMigrator.Runner.Generators.Base;

    internal class SQLiteTypeMap : TypeMapBase
    {
        public const int AnsiStringCapacity = 8000;
        public const int AnsiTextCapacity = 2147483647;
        public const int UnicodeStringCapacity = 4000;
        public const int UnicodeTextCapacity = 1073741823;
        public const int ImageCapacity = 2147483647;
        public const int DecimalCapacity = 19;
        public const int XmlCapacity = 1073741823;

        protected override void SetupTypeMaps()
        {
            SetTypeMap(DbType.Binary, "BLOB");
            SetTypeMap(DbType.Byte, "INTEGER");
            SetTypeMap(DbType.Int16, "INTEGER");
            SetTypeMap(DbType.Int32, "INTEGER");
            SetTypeMap(DbType.Int64, "INTEGER");
            SetTypeMap(DbType.SByte, "INTEGER");
            SetTypeMap(DbType.UInt16, "INTEGER");
            SetTypeMap(DbType.UInt32, "INTEGER");
            SetTypeMap(DbType.UInt64, "INTEGER");
            SetTypeMap(DbType.Currency, "NUMERIC");
            SetTypeMap(DbType.Decimal, "NUMERIC");
            SetTypeMap(DbType.Double, "NUMERIC");
            SetTypeMap(DbType.Single, "NUMERIC");
            SetTypeMap(DbType.VarNumeric, "NUMERIC");
            SetTypeMap(DbType.AnsiString, "TEXT");
            SetTypeMap(DbType.String, "TEXT");
            SetTypeMap(DbType.AnsiStringFixedLength, "TEXT");
            SetTypeMap(DbType.StringFixedLength, "TEXT");

            SetTypeMap(DbType.Date, "DATETIME");
            SetTypeMap(DbType.DateTime, "DATETIME");
            SetTypeMap(DbType.Time, "DATETIME");
            SetTypeMap(DbType.Boolean, "INTEGER");
            SetTypeMap(DbType.Guid, "UNIQUEIDENTIFIER");
        }

        public override string GetTypeMap(DbType type, int size, int precision)
        {
            return base.GetTypeMap(type, 0, 0);
        }
    }
}