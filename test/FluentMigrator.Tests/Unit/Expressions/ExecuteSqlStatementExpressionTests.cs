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

using System.Collections.Generic;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Tests.Helpers;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    [Category("Expression")]
    [Category("ExecuteSqlStatement")]
    public class ExecuteSqlStatementExpressionTests
    {
        [Test]
        public void ErrorIsReturnWhenSqlStatementIsNullOrEmpty()
        {
            var expression = new ExecuteSqlStatementExpression() { SqlStatement = null };
            var errors = ValidationHelper.CollectErrors(expression);
            errors.ShouldContain(ErrorMessages.SqlStatementCannotBeNullOrEmpty);
        }

        [Test]
        public void ExecutesTheStatement()
        {
            var expression = new ExecuteSqlStatementExpression() { SqlStatement = "INSERT INTO BLAH" };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute(expression.SqlStatement)).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ExecutesTheStatementWithWellKnownTokens()
        {
            var tokenProvider = new Mock<ISqlTokenProvider>();
            tokenProvider
                .Setup(x => x.GetTokens())
                .Returns(new Dictionary<string, string> { { "DefaultSchema", "dbo" } });

            var expression = new ExecuteSqlStatementExpression()
            {
                SqlStatement = "ALTER TABLE $(DefaultSchema).BLAH ADD COLUMN Foo INT",
                SqlScriptTokenProviders = new[] { tokenProvider.Object },
            };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute("ALTER TABLE dbo.BLAH ADD COLUMN Foo INT")).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ParametersOverrideWellKnownTokensWithTheSameName()
        {
            var tokenProvider = new Mock<ISqlTokenProvider>();
            tokenProvider
                .Setup(x => x.GetTokens())
                .Returns(new Dictionary<string, string> { { "DefaultSchema", "dbo" } });

            var expression = new ExecuteSqlStatementExpression()
            {
                SqlStatement = "ALTER TABLE $(DefaultSchema).BLAH ADD COLUMN Foo INT",
                Parameters = new Dictionary<string, string> { { "DefaultSchema", "tenant1" } },
                SqlScriptTokenProviders = new[] { tokenProvider.Object },
            };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute("ALTER TABLE tenant1.BLAH ADD COLUMN Foo INT")).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ExecutesTheStatementWithWellKnownTokensUsingQuotedTokenSyntax()
        {
            var tokenProvider = new Mock<ISqlTokenProvider>();
            tokenProvider
                .Setup(x => x.GetTokens())
                .Returns(new Dictionary<string, string> { { "DefaultSchema", "te'nant" } });

            var expression = new ExecuteSqlStatementExpression()
            {
                SqlStatement = "SELECT * FROM Users WHERE SchemaName = $[DefaultSchema]",
                SqlScriptTokenProviders = new[] { tokenProvider.Object },
            };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute("SELECT * FROM Users WHERE SchemaName = 'te''nant'")).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ParametersOverrideWellKnownTokensWithTheSameNameUsingQuotedTokenSyntax()
        {
            var tokenProvider = new Mock<ISqlTokenProvider>();
            tokenProvider
                .Setup(x => x.GetTokens())
                .Returns(new Dictionary<string, string> { { "DefaultSchema", "dbo" } });

            var expression = new ExecuteSqlStatementExpression()
            {
                SqlStatement = "SELECT * FROM Users WHERE SchemaName = $[DefaultSchema]",
                Parameters = new Dictionary<string, string> { { "DefaultSchema", "tenant1" } },
                SqlScriptTokenProviders = new[] { tokenProvider.Object },
            };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute("SELECT * FROM Users WHERE SchemaName = 'tenant1'")).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void MergesTokensFromMultipleProvidersWithLaterProvidersTakingPrecedence()
        {
            var firstTokenProvider = new Mock<ISqlTokenProvider>();
            firstTokenProvider
                .Setup(x => x.GetTokens())
                .Returns(new Dictionary<string, string> { { "DefaultSchema", "dbo" }, { "TablePrefix", "tbl_" } });

            var secondTokenProvider = new Mock<ISqlTokenProvider>();
            secondTokenProvider
                .Setup(x => x.GetTokens())
                .Returns(new Dictionary<string, string> { { "DefaultSchema", "tenant1" } });

            var expression = new ExecuteSqlStatementExpression()
            {
                SqlStatement = "ALTER TABLE $(DefaultSchema).$(TablePrefix)BLAH ADD COLUMN Foo INT",
                SqlScriptTokenProviders = new[] { firstTokenProvider.Object, secondTokenProvider.Object },
            };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute("ALTER TABLE tenant1.tbl_BLAH ADD COLUMN Foo INT")).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void IgnoresNullTokenProvidersAndNullTokenMaps()
        {
            var nullTokenMapProvider = new Mock<ISqlTokenProvider>();
            nullTokenMapProvider
                .Setup(x => x.GetTokens())
                .Returns((IDictionary<string, string>)null);

            var tokenProvider = new Mock<ISqlTokenProvider>();
            tokenProvider
                .Setup(x => x.GetTokens())
                .Returns(new Dictionary<string, string> { { "DefaultSchema", "dbo" } });

            var expression = new ExecuteSqlStatementExpression()
            {
                SqlStatement = "ALTER TABLE $(DefaultSchema).BLAH ADD COLUMN Foo INT",
                SqlScriptTokenProviders = new ISqlTokenProvider[] { null, nullTokenMapProvider.Object, tokenProvider.Object },
            };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute("ALTER TABLE dbo.BLAH ADD COLUMN Foo INT")).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ExecutesTheStatementUnchangedWhenNoTokenProvidersAndNoParameters()
        {
            var expression = new ExecuteSqlStatementExpression()
            {
                SqlStatement = "SELECT '$(NotAToken)' FROM $[AlsoNotAToken]",
                SqlScriptTokenProviders = new ISqlTokenProvider[0],
            };

            var processor = new Mock<IMigrationProcessor>();
            processor.Setup(x => x.Execute("SELECT '$(NotAToken)' FROM $[AlsoNotAToken]")).Verifiable();

            expression.ExecuteWith(processor.Object);
            processor.Verify();
        }

        [Test]
        public void ToStringIsDescriptive()
        {
            var expression = new ExecuteSqlStatementExpression() { SqlStatement = "INSERT INTO BLAH" };
            expression.ToString().ShouldBe("ExecuteSqlStatement INSERT INTO BLAH");
        }

        [Test]
        public void ToStringCanUseDescription()
        {
            var expression = new ExecuteSqlStatementExpression() { SqlStatement = "INSERT INTO BLAH", Description = "FOOBAR" };
            expression.ToString().ShouldBe("ExecuteSqlStatement FOOBAR");
        }
    }
}
