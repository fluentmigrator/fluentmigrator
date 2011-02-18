

namespace FluentMigrator.Tests.Unit.Generators
{
    using FluentMigrator.Runner.Generators.SQLite;
using NUnit.Framework;

    public class SQLiteTestBase : GeneratorTestBase
    {
        protected SqliteGenerator generator;

        [SetUp]
        public void Setup()
		{
			generator = new SqliteGenerator();
		}

    }
}
