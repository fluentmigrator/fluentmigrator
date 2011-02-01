using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator {
    public class ISchemaWriter {
        void WriteToFile(ICollection<Model.TableDefinition> tables, string file);
    }
}
