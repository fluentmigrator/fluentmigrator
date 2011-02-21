

namespace FluentMigrator.Tests.Unit.Generators
{
    using System.Data;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.SqlServer;
    using NUnit.Framework;
    using NUnit.Should;

    public class SqlServer2008GeneratorTests
	{
		private SqlServer2008Generator generator;

		[SetUp]
		public void SetUp()
		{
			generator = new SqlServer2008Generator();
		}

		
	}
}