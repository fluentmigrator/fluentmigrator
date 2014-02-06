using System.Collections.Generic;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.SchemaGen.SchemaWriters
{
    public interface IMigrationWriter
    {
        /// <summary>
        /// Generates Fluent Migrator C# classes
        /// </summary>
        /// <returns>The paths of generated classes</returns>
        IEnumerable<string> WriteMigrationClasses();
    }
}
