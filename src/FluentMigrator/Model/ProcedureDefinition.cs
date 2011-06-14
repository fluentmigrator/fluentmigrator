namespace FluentMigrator.Model
{
   ///<summary>
   /// Defines the sql statement used to generate a procedure
   ///</summary>
   public class ProcedureDefinition
   {
      /// <summary>
      /// The schema that the procedure belongs to
      /// </summary>
      public string SchemaName;

      /// <summary>
      /// The name of the procedure
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// The sql used to create the procedure
      /// </summary>
      public string Sql { get; set; }
   }
}