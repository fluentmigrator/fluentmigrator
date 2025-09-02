using System.Data;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    /// <summary>
    /// Represents the SQL Server 2005 type mapping configuration.
    /// </summary>
    /// <remarks>
    /// This class extends the <see cref="SqlServer2000TypeMap"/> to provide additional
    /// type mappings specific to SQL Server 2005, including support for types such as
    /// <c>NVARCHAR(MAX)</c>, <c>VARCHAR(MAX)</c>, <c>VARBINARY(MAX)</c>, and <c>XML</c>.
    /// </remarks>
    public class SqlServer2005TypeMap : SqlServer2000TypeMap
    {
        /// <summary>
        /// Configures the SQL Server 2005-specific type mappings.
        /// </summary>
        /// <remarks>
        /// This method extends the base type mappings defined in <see cref="SqlServer2000TypeMap"/>
        /// by adding support for SQL Server 2005-specific types, such as <c>NVARCHAR(MAX)</c>,
        /// <c>VARCHAR(MAX)</c>, <c>VARBINARY(MAX)</c>, and <c>XML</c>.
        /// </remarks>
        protected override void SetupSqlServerTypeMaps()
        {
            base.SetupSqlServerTypeMaps();
            // Officially this is 1073741823 but we will allow the int.MaxValue Convention
            SetTypeMap(DbType.String, "NVARCHAR(MAX)", int.MaxValue);
            SetTypeMap(DbType.AnsiString, "VARCHAR(MAX)", AnsiTextCapacity);
            SetTypeMap(DbType.Binary, "VARBINARY(MAX)", ImageCapacity);

            SetTypeMap(DbType.Xml, "XML");
        }
    }
}
