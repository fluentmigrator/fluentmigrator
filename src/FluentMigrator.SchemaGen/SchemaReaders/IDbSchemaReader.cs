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

        IDictionary<string, TableDefinition> Tables { get; }

        IEnumerable<string> UserDefinedDataTypes { get; }
        IEnumerable<string> UserDefinedFunctions { get; }
        IEnumerable<string> Views { get; }
        IEnumerable<string> StoredProcedures { get; }

        DataSet ReadTableData(string tableName);
    }
}
