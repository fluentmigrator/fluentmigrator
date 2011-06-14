using System;

namespace FluentMigrator.SchemaDump.SchemaMigrations
{
   [Flags]
   public enum MigrationType
   {
      Tables = 2,
      Views = 4,
      Data = 8,
      Indexes = 16,
      Procedures = 32,
      Functions = 64,
      ForeignKeys = 128,

      All = Tables | Views | Data | Indexes | Procedures | Functions | ForeignKeys
   }
}