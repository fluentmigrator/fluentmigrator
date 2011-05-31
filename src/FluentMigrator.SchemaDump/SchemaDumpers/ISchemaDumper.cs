using System.Collections.Generic;
using FluentMigrator.Model;

namespace FluentMigrator.SchemaDump.SchemaDumpers {
   public interface ISchemaDumper {
        IList<TableDefinition> ReadDbSchema();
    }
}
