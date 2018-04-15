using System.Linq;
using System.Collections.Generic;
using System;

namespace FluentMigrator.Runner.Processors.Oracle
{
    public class OracleManagedDbFactory : ReflectionBasedDbFactory
    {
        public OracleManagedDbFactory()
            : base("Oracle.ManagedDataAccess", "Oracle.ManagedDataAccess.Client.OracleClientFactory")
        {
        }
    }
}