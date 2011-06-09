using FluentMigrator.Model;

namespace FluentMigrator.SchemaDump.SchemaMigrations
{
   public class InsertColumnReplacement
   {
      /// <summary>
      /// The column definition to be matched
      /// </summary>
      public ColumnDefinition ColumnDataToMatch { get; set; }

      /// <summary>
      /// The old value to be replaced
      /// </summary>
      public object OldValue { get; set; }

      /// <summary>
      /// The new value to be inserted
      /// </summary>
      public object NewValue { get; set; }
   }
}