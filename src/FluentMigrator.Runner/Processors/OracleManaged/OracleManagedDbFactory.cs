using System.Linq;
using System.Collections.Generic;
using System;

namespace FluentMigrator.Runner.Processors.OracleManaged
{
    public class OracleManagedDbFactory : ReflectionBasedDbFactory
    {
        public OracleManagedDbFactory()
            : base("Oracle.ManagedDataAccess", "Oracle.ManagedDataAccess.Client.OracleClientFactory")
        {
        }
    }
}