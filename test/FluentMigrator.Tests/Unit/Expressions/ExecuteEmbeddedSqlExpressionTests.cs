#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

using FluentMigrator.Expressions;
using FluentMigrator.Tests.Helpers;
using FluentMigrator.Infrastructure;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Expressions
{

    [TestFixture]
    public class ExecuteEmbeddedSqlScriptExpressionTests
    {
        private const string TestSqlScript = "embeddedtestscript.sql";
        private const string ScriptContents = "TEST SCRIPT";

        [Test]
        public void ErrorIsReturnWhenSqlScriptIsNullOrEmpty()
        {
            var provider = new DefaultEmbeddedResourceProvider();
            var expression = new ExecuteEmbeddedSqlScriptExpression(new[] { provider }) { SqlScript = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.SqlScriptCannotBeNullOrEmpty);
        }

        [Test]
        public void ExecutesTheStatement()
        {
            var provider = new DefaultEmbeddedResourceProvider(Assembly.GetExecutingAssembly());
            var expression = new ExecuteEmbeddedSqlScriptExpression(new[] { provider }) { SqlScript = TestSqlScript };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(ScriptContents)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ExecutesTheStatementWithParameters()
        {
            const string scriptContentsWithParameters = "TEST SCRIPT ParameterValue $(escaped_parameter) $(missing_parameter)";
            var provider = new DefaultEmbeddedResourceProvider(Assembly.GetExecutingAssembly());
            var expression = new ExecuteEmbeddedSqlScriptExpression(new[] { provider })
            {
                SqlScript = "EmbeddedTestScriptWithParameters.sql",
                Parameters = new Dictionary<string, string> { { "parameter", "ParameterValue" } }
            };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(scriptContentsWithParameters)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ResourceFinderIsCaseInsensitive()
        {
            var provider = new DefaultEmbeddedResourceProvider(Assembly.GetExecutingAssembly());
            var expression = new ExecuteEmbeddedSqlScriptExpression(new[] { provider }) { SqlScript = TestSqlScript.ToUpper() };
            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(ScriptContents)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ResourceFinderFindFileWithFullName()
        {
            var provider = new DefaultEmbeddedResourceProvider(Assembly.GetExecutingAssembly());
            var expression = new ExecuteEmbeddedSqlScriptExpression(new[] { provider }) { SqlScript = "InitialSchema.sql" };
            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute("InitialSchema")).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ResourceFinderFindFileWithFullNameAndNamespace()
        {
            var provider = new DefaultEmbeddedResourceProvider(Assembly.GetExecutingAssembly());
            var expression = new ExecuteEmbeddedSqlScriptExpression(new[] { provider }) { SqlScript = "FluentMigrator.Tests.EmbeddedResources.InitialSchema.sql" };
            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute("InitialSchema")).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ResourceFinderFindThrowsExceptionIfFoundMoreThenOneResource()
        {
            var provider = new DefaultEmbeddedResourceProvider(Assembly.GetExecutingAssembly());
            var expression = new ExecuteEmbeddedSqlScriptExpression(new[] { provider }) { SqlScript = "NotUniqueResource.sql" };
            var processor = new Mock<IMigrationProcessor>();

            Assert.Throws<InvalidOperationException>(() => expression.ExecuteWith(processor.Object));
            processor.Verify(x => x.Execute("NotUniqueResource"), Times.Never());
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            var expression = new ExecuteSqlScriptExpression { SqlScript = TestSqlScript };
            expression.ToString().ShouldBe("ExecuteSqlScript embeddedtestscript.sql");
        }
    }
}
