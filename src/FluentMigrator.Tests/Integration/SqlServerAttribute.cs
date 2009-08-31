using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentMigrator.Runner.Processors;
using Xunit.Extensions;

namespace FluentMigrator.Tests.Integration
{
    public class SqlServerAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
        {
            var processor = new SqlServerProcessorFactory().Create(@"server=(local)\SQLEXPRESS;uid=;pwd=;Trusted_Connection=yes;database=FluentMigrator");
            
            return new[] { new[] { processor as object } }.AsEnumerable();
        }
    }
}