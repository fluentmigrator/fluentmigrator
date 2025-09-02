#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using System.IO;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Tests.Helpers;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    [Category("Expression")]
    [Category("ExecuteSqlScript")]
    public class ExecuteSqlScriptExpressionTests
    {
        private string testSqlScript = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testscript.sql");
        private string scriptContents = "TEST SCRIPT";

        [Test]
        public void ErrorIsReturnWhenSqlScriptIsNullOrEmpty()
        {
            var expression = new ExecuteSqlScriptExpression { SqlScript = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.SqlScriptCannotBeNullOrEmpty);
        }

        [Test]
        public void ExecutesTheStatementWithParameters()
        {
            const string scriptContentsWithParameters = "TEST SCRIPT ParameterValue $(escaped_parameter) $(missing_parameter)";
            var expression = new ExecuteSqlScriptExpression()
            {
                SqlScript = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestScriptWithParameters.sql"),
                Parameters = new Dictionary<string, string> { { "parameter", "ParameterValue" } }
            };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(scriptContentsWithParameters)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ExecutesTheStatement()
        {
            var expression = new ExecuteSqlScriptExpression { SqlScript = testSqlScript };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(scriptContents)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            var expression = new ExecuteSqlScriptExpression { SqlScript = testSqlScript };
            expression.ToString().ShouldBe($"ExecuteSqlScript {testSqlScript}");
        }

        [Test]
        [Category("NotWorkingOnMono")]
        [Platform(Exclude = "Linux,Mono", Reason = "Linux does not support different drives")]
        public void CanUseScriptsOnAnotherDriveToWorkingDirectory()
        {
            var scriptOnAnotherDrive = "z" + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar + testSqlScript;
            var expression = new ExecuteSqlScriptExpression { SqlScript = scriptOnAnotherDrive };

            var defaultRootPath = "c" + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar + "code";
            var conventionSet = ConventionSets.CreateNoSchemaName(defaultRootPath);
            var processed = expression.Apply(conventionSet);

            processed.SqlScript.ShouldBe(scriptOnAnotherDrive);
        }
    }
}
