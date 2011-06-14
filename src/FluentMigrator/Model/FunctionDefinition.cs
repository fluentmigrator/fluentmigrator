namespace FluentMigrator.Model
{
   ///<summary>
   /// Defines the sql statement used to generate a function
   ///</summary>
   public class FunctionDefinition
   {
      /// <summary>
      /// The schema name of the function
      /// </summary>
      public string SchemaName { get; set; }

      /// <summary>
      /// The name of the function
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// The sql used to create the function
      /// </summary>
      public string Sql { get; set; }
   }
}