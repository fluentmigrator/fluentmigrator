using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using FluentMigrator;

namespace FluentMigrator.Runner.SchemaWriters {
    public class SchemaWriterBase : ISchemaWriter {
        public void WriteToStream(ICollection<Model.TableDefinition> tables, System.IO.StreamWriter output) {
            throw new NotImplementedException();
        }

        public abstract void WriteToFile(ICollection<Model.TableDefinition> tables, string file) {
            throw new NotImplementedException();
        }
    }
}
