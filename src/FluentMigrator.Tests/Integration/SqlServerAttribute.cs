using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using Xunit.Extensions;

namespace FluentMigrator.Tests.Integration
{
    public class SqlServerAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
        {
            var connection = new SqlConnection(@"server=(local)\SQLEXPRESS;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator");
            connection.Open();
            return new[] { new[] { new SqlServerProcessor(connection, new SqlServerGenerator()) as object } }.AsEnumerable();
        }
    }
}