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

using NUnit.Framework;
using FluentMigrator.Expressions;
using FluentMigrator.Tests.Helpers;
using FluentMigrator.Infrastructure;
using Moq;

using System;
using System.Collections.Generic;
using System.Reflection;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    [Obsolete]
    public class ObsoleteExecuteEmbeddedSqlScriptExpressionTests
    {
        private const string TestSqlScript = "embeddedtestscript.sql";
        private const string ScriptContents = "TEST SCRIPT";

        [Test]
        public void ErrorIsReturnWhenSqlScriptIsNullOrEmpty()
        {
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.SqlScriptCannotBeNullOrEmpty);
        }

        [Test]
        public void ExecutesTheStatement()
        {
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = TestSqlScript, MigrationAssemblies = new SingleAssembly(Assembly.GetExecutingAssembly()) };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(ScriptContents)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ExecutesTheStatementWithParameters()
        {
            const string scriptContentsWithParameters = "TEST SCRIPT ParameterValue $(escaped_parameter) $(missing_parameter)";
            var expression = new ExecuteEmbeddedSqlScriptExpression
            {
                SqlScript = "EmbeddedTestScriptWithParameters.sql",
                MigrationAssemblies = new SingleAssembly(Assembly.GetExecutingAssembly()),
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
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = TestSqlScript.ToUpper(), MigrationAssemblies = new SingleAssembly(Assembly.GetExecutingAssembly()) };
            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(ScriptContents)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ResourceFinderFindFileWithFullName()
        {
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = "InitialSchema.sql", MigrationAssemblies = new SingleAssembly(Assembly.GetExecutingAssembly()) };
            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute("InitialSchema")).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ResourceFinderFindFileWithFullNameAndNamespace()
        {
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = "FluentMigrator.Tests.EmbeddedResources.InitialSchema.sql", MigrationAssemblies = new SingleAssembly(Assembly.GetExecutingAssembly()) };
            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute("InitialSchema")).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ResourceFinderFindThrowsExceptionIfFoundMoreThenOneResource()
        {
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = "NotUniqueResource.sql", MigrationAssemblies = new SingleAssembly(Assembly.GetExecutingAssembly()) };
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
