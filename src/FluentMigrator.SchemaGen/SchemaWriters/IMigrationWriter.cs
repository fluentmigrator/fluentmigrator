using System.Collections.Generic;
using System.IO;
using FluentMigrator.Model;

namespace FluentMigrator.SchemaGen.SchemaWriters
{
    public interface IMigrationWriter
    {
        /// <summary>
        /// Generates a Fluent Migrator C# class (or classes).
        /// </summary>
        void WriteMigrations();

    }
}
