#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

using System.IO;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    public class ExecuteSqlScriptExpressionTests
    {
        private string testSqlScript = "testscript.sql";
        private string scriptContents = "TEST SCRIPT";

        [Test]
        public void ErrorIsReturnWhenSqlScriptIsNullOrEmpty()
        {
            var expression = new ExecuteSqlScriptExpression { SqlScript = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.SqlScriptCannotBeNullOrEmpty);
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
            expression.ToString().ShouldBe("ExecuteSqlScript testscript.sql");
        }

        [Test]
        [Category("NotWorkingOnMono")]
        public void CanUseScriptsOnAnotherDriveToWorkingDirectory()
        {
            var scriptOnAnotherDrive = "z" + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar + testSqlScript;
            var expression = new ExecuteSqlScriptExpression { SqlScript = scriptOnAnotherDrive };
            
            var conventions = new MigrationConventions { GetWorkingDirectory = () => "c" + Path.VolumeSeparatorChar + Path.DirectorySeparatorChar + "code" };
            expression.ApplyConventions(conventions);
            
            expression.SqlScript.ShouldBe(scriptOnAnotherDrive);
        }
    }
}
