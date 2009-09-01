using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Sqlite;
using Xunit.Extensions;

namespace FluentMigrator.Tests.Integration
{
    public class SqliteAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
        {
            var processor = new SqliteProcessorFactory().Create(@"Data Source=:memory:;Version=3;New=True;");
            return new[]{ new[] {processor as object}}.AsEnumerable();
        }
    }
}