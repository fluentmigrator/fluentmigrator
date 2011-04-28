using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentMigrator.Model;

namespace FluentMigrator.SchemaDump.SchemaWriters {
    public interface ISchemaWriter {
        void WriteToStream(ICollection<Model.TableDefinition> tables, StreamWriter output);
        void WriteToFile(ICollection<Model.TableDefinition> tables, string file);
    }
}
