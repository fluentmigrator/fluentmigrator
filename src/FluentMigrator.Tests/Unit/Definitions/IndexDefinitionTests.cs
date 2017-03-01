﻿using FluentMigrator.Model;
using Xunit;

namespace FluentMigrator.Tests.Unit.Definitions
{
    public class IndexDefinitionTests
    {
        [Fact]
        public void ShouldApplyIndexNameConventionWhenIndexNameIsNull()
        {
            var indexDefinition = new IndexDefinition();
            var conventions = new MigrationConventions { GetIndexName = definition => "IX_Table_Name" };

            indexDefinition.ApplyConventions(conventions);

            Assert.AreEqual("IX_Table_Name", indexDefinition.Name);
        }
    }
}