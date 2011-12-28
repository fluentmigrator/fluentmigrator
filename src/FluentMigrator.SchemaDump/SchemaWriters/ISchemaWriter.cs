using System.Collections.Generic;
using System.IO;

namespace FluentMigrator.SchemaDump.SchemaWriters
{
    public interface ISchemaWriter
    {
        void WriteToStream(ICollection<Model.TableDefinition> tables, StreamWriter output);
        void WriteToFile(ICollection<Model.TableDefinition> tables, string file);
    }
}
