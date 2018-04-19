using System;

namespace FluentMigrator.Runner.Processors.Oracle
{
    public class OracleDbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _testEntries =
        {
            new TestEntry("Oracle.DataAccess", "Oracle.DataAccess.Client.OracleClientFactory"),
        };

        [Obsolete]
        public OracleDbFactory()
            : this(serviceProvider: null)
        {
        }

        public OracleDbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _testEntries)
        {
        }
    }
}
