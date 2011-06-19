using System;
using System.Diagnostics;

namespace FluentMigrator.Runner.Announcers
{
   public class DebugAnnouncer : IAnnouncer 
   {
      public void Dispose()
      {
         
      }

      public void Heading(string message)
      {
         Debug.WriteLine("--" + message);
      }

      public void Say(string message)
      {
         Debug.WriteLine(message);
      }

      public void Sql(string sql)
      {
         Debug.WriteLine(sql);
      }

      public bool AnnounceTime { get; set; }

       public void ElapsedTime(TimeSpan timeSpan)
      {
         
      }

      public void Error(string message)
      {
         Debug.WriteLine("ERROR " + message);
      }
   }
}