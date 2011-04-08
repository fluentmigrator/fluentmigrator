using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using FluentMigrator;

namespace FluentMigrator.SchemaDump.SchemaWriters {
    public abstract class SchemaWriterBase : ISchemaWriter {
        public abstract void WriteToStream(ICollection<Model.TableDefinition> tables, System.IO.StreamWriter output);

        public virtual void WriteToFile(ICollection<Model.TableDefinition> tables, string file) {
            FileStream fs = new FileStream(file, FileMode.CreateNew);
            StreamWriter sr = new StreamWriter(fs);

            WriteToStream(tables, sr);

            sr.Flush();
            sr.Close();
            fs.Close();
        }
    }
}
