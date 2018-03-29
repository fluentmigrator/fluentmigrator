using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Definitions
{
    [TestFixture]
    public class IndexDefinitionTests
    {
        [Test]
        public void ShouldApplyIndexNameConventionWhenIndexNameIsNull()
        {
            var expr = new CreateIndexExpression()
            {
                Index =
                {
                    TableName = "Table",
                    Columns =
                    {
                        new IndexColumnDefinition() {Name = "Name"}
                    }
                }
            };

            var processed = expr.Apply(ConventionSets.NoSchemaName);

            Assert.AreEqual("IX_Table_Name", processed.Index.Name);
        }
    }
}
