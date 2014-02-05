using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Model;


namespace FluentMigrator.SchemaGen.SchemaReaders
{
    public interface IDbSchemaReader
    {
        IDictionary<string, int> TableFkDependencyOrder(bool ascending);

        IEnumerable<TableDefinition> Tables { get; }
        IEnumerable<TableDefinition> GetTables(IEnumerable<string> tableNames);

        IEnumerable<string> TableNames { get; }
        IEnumerable<string> UserDefinedDataTypes { get; }
        IEnumerable<string> UserDefinedFunctions { get; }
        IEnumerable<string> Views { get; }
        IEnumerable<string> StoredProcedures { get; }
    }
}
