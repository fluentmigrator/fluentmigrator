using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Model;

namespace FluentMigrator.SchemaDump.SchemaDumpers {
    interface ISchemaDumper {
        IList<TableDefinition> ReadDbSchema();
    }
}
