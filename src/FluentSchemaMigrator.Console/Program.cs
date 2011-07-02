using System;

namespace FluentSchemaMigrator.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new SchemaMigratorConsole(args);
            }
            catch (ArgumentException ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }
    }
}
