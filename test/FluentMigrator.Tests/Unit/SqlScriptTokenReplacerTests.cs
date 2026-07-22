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

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    [Category("SqlScriptTokenReplacer")]
    public class SqlScriptTokenReplacerTests
    {
        [Test]
        public void ReturnsOriginalTextWhenParametersAreNull()
        {
            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $(Foo)", null)
                .ShouldBe("SELECT $(Foo)");
        }

        [Test]
        public void ReturnsOriginalTextWhenParametersAreEmpty()
        {
            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $(Foo)", new Dictionary<string, string>())
                .ShouldBe("SELECT $(Foo)");
        }

        [Test]
        public void ReplacesRawTokenWithValueVerbatim()
        {
            var parameters = new Dictionary<string, string> { ["TablePrefix"] = "App_" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT * FROM $(TablePrefix)Users", parameters)
                .ShouldBe("SELECT * FROM App_Users");
        }

        [Test]
        public void LeavesUnknownRawTokenUnchanged()
        {
            var parameters = new Dictionary<string, string> { ["Known"] = "Value" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $(Unknown)", parameters)
                .ShouldBe("SELECT $(Unknown)");
        }

        [Test]
        public void UnescapesDoubleDollarRawToken()
        {
            var parameters = new Dictionary<string, string> { ["Foo"] = "Bar" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT '$$((Foo))'", parameters)
                .ShouldBe("SELECT '$(Foo)'");
        }

        [Test]
        public void ReplacesSafeTokenWithQuotedAndEscapedStringLiteral()
        {
            var parameters = new Dictionary<string, string> { ["DefaultStatus"] = "isn't active" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[DefaultStatus]", parameters)
                .ShouldBe("SELECT 'isn''t active'");
        }

        [Test]
        public void ReplacesSafeTokenWithNullLiteralWhenValueIsNull()
        {
            var parameters = new Dictionary<string, string> { ["Value"] = null };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[Value]", parameters)
                .ShouldBe("SELECT NULL");
        }

        [Test]
        public void LeavesUnknownSafeTokenUnchanged()
        {
            var parameters = new Dictionary<string, string> { ["Known"] = "Value" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[Unknown]", parameters)
                .ShouldBe("SELECT $[Unknown]");
        }

        [Test]
        public void UnescapesDoubleDollarSafeToken()
        {
            var parameters = new Dictionary<string, string> { ["Foo"] = "Bar" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT '$$[[Foo]]'", parameters)
                .ShouldBe("SELECT '$[Foo]'");
        }

        [Test]
        public void SupportsMixOfRawAndSafeTokens()
        {
            var parameters = new Dictionary<string, string>
            {
                ["TablePrefix"] = "App_",
                ["DefaultStatus"] = "isn't active",
                ["CurrentDate"] = "GETDATE()",
            };

            var sql = SqlScriptTokenReplacer.ReplaceSqlScriptTokens(
                "INSERT INTO $(TablePrefix)Users (Status, CreatedAt) VALUES ($[DefaultStatus], $(CurrentDate));",
                parameters);

            sql.ShouldBe("INSERT INTO App_Users (Status, CreatedAt) VALUES ('isn''t active', GETDATE());");
        }
    }
}
