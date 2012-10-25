using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.MySql;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.MySql;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators.MySql
{
    [TestFixture]
    public class MySqlExecuteTests
    {
        [Test]
        public void CanExecuteEmbeddedResource()
        {
            Mock<IMigrationProcessor> mockProcessor = new Mock<IMigrationProcessor>();

            ExecuteEmbeddedSqlScriptExpression expression = new ExecuteEmbeddedSqlScriptExpression { MigrationAssembly = Assembly.GetExecutingAssembly(), SqlScript = "EmbeddedScript.txt" };

            expression.ExecuteWith(mockProcessor.Object);

            mockProcessor.Verify(mock => mock.Execute("TEST SQL"));
        }
    }
}
