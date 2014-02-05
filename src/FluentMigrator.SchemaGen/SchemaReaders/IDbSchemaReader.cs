using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator.Model;


namespace FluentMigrator.SchemaGen.SchemaReaders
{
    public interface IDbSchemaReader
    {
        IDictionary<string, int> TablesInForeignKeyOrder(bool ascending);
        IDictionary<string, int> ScriptsInDependencyOrder(bool ascending);

        IEnumerable<TableDefinition> Tables { get; }
        IEnumerable<TableDefinition> GetTables(IEnumerable<string> tableNames = null);

        IEnumerable<string> TableNames { get; }

        IEnumerable<string> UserDefinedDataTypes { get; }
        IEnumerable<string> UserDefinedFunctions { get; }
        IEnumerable<string> Views { get; }
        IEnumerable<string> StoredProcedures { get; }

        DataSet ReadTableData(string tableName);
    }
}
