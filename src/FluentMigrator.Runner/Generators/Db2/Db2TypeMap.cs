namespace FluentMigrator.Runner.Generators.DB2
{
    using System.Data;

    using FluentMigrator.Runner.Generators.Base;

    internal class Db2TypeMap : TypeMapBase
    {
        #region Methods

        /// <summary>
        /// The setup type maps.
        /// </summary>
        protected override void SetupTypeMaps()
        {
            this.SetTypeMap(DbType.AnsiStringFixedLength, "CHARACTER(255)");
            this.SetTypeMap(DbType.AnsiStringFixedLength, "CHARACTER($size)", 255);
            this.SetTypeMap(DbType.AnsiString, "VARCHAR(255)");
            this.SetTypeMap(DbType.AnsiString, "VARCHAR($size)", 32704);
            this.SetTypeMap(DbType.AnsiString, "CLOB(1048576)");
            this.SetTypeMap(DbType.AnsiString, "CLOB($size)", int.MaxValue);
            this.SetTypeMap(DbType.Binary, "BINARY(255)");
            this.SetTypeMap(DbType.Binary, "BINARY($size)", 255);
            this.SetTypeMap(DbType.Binary, "VARBINARY(8000)");
            this.SetTypeMap(DbType.Binary, "VARBINARY($size)", 32704);
            this.SetTypeMap(DbType.Binary, "BLOB(1048576)");
            this.SetTypeMap(DbType.Binary, "BLOB($size)", 2147483647);
            this.SetTypeMap(DbType.Boolean, "CHAR(1)");
            this.SetTypeMap(DbType.Byte, "SMALLINT");
            this.SetTypeMap(DbType.Time, "TIME");
            this.SetTypeMap(DbType.Date, "DATE");
            this.SetTypeMap(DbType.DateTime, "TIMESTAMP");
            this.SetTypeMap(DbType.Decimal, "NUMERIC(19,5)");
            this.SetTypeMap(DbType.Decimal, "NUMERIC($size,$precision)", 31);
            this.SetTypeMap(DbType.Decimal, "DECIMAL(19,5)");
            this.SetTypeMap(DbType.Decimal, "DECIMAL($size,$precision)", 31);
            this.SetTypeMap(DbType.Double, "DOUBLE");
            this.SetTypeMap(DbType.Int16, "SMALLINT");
            this.SetTypeMap(DbType.Int32, "INT");
            this.SetTypeMap(DbType.Int32, "INTEGER");
            this.SetTypeMap(DbType.Int64, "BIGINT");
            this.SetTypeMap(DbType.Single, "REAL");
            this.SetTypeMap(DbType.Single, "DECFLOAT", 34);
            this.SetTypeMap(DbType.StringFixedLength, "GRAPHIC(128)");
            this.SetTypeMap(DbType.StringFixedLength, "GRAPHIC($size)", 128);
            this.SetTypeMap(DbType.String, "VARGRAPHIC(8000)");
            this.SetTypeMap(DbType.String, "VARGRAPHIC($size)", 16352);
            this.SetTypeMap(DbType.String, "DBCLOB(1048576)");
            this.SetTypeMap(DbType.String, "DBCLOB($size)", 1073741824);
            this.SetTypeMap(DbType.Xml, "XML");
        }

        #endregion
    }
}