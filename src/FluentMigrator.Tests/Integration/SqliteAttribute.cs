using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using Xunit.Extensions;

namespace FluentMigrator.Tests.Integration
{
    public class SqliteAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
        {
            var connection = new SQLiteConnection(@"Data Source=:memory:;Version=3;New=True;");
            connection.Open();
            return new[]{ new[] {new SqliteProcessor(connection, new SqliteGenerator()) as object}}.AsEnumerable();
        }
    }
}