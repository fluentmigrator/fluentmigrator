using System.IO;
using System.Threading;
using FirebirdSql.Data.FirebirdClient;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    public class FbDatabase
    {
        public static void CreateDatabase(string connectionString)
        {
            var connectionStringBuilder = new FbConnectionStringBuilder(connectionString);
            if (File.Exists(connectionStringBuilder.Database))
                DropDatabase(connectionString);

            FbConnection.CreateDatabase(connectionString);
        }

        public static void DropDatabase(string connectionString)
        {
            FbConnection.ClearAllPools();

            // Avoid "lock time-out on wait transaction" exception
            var retries = 5;
            while (true)
            {
                try
                {
                    FbConnection.DropDatabase(connectionString);
                    break;
                }
                catch
                {
                    if (--retries == 0)
                        throw;
                    else
                        Thread.Sleep(100);
                }
            }
        }
    }
}
