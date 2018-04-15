namespace FluentMigrator.Runner.Processors.Oracle
{
    public class OracleDbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _testEntries =
        {
            new TestEntry("Oracle.ManagedDataAccess", "Oracle.ManagedDataAccess.Client.OracleClientFactory"),
            new TestEntry("Oracle.DataAccess", "Oracle.DataAccess.Client.OracleClientFactory"),
        };

        public OracleDbFactory()
            : base(_testEntries)
        {
        }
    }
}
