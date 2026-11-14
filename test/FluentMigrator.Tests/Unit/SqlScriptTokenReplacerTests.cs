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
using System.Globalization;

using FluentMigrator.Runner.Generators.Generic;

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
            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $(Foo)", (IDictionary<string, object>)null, null)
                .ShouldBe("SELECT $(Foo)");
        }

        [Test]
        public void ReturnsOriginalTextWhenParametersAreEmpty()
        {
            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $(Foo)", new Dictionary<string, object>(), null)
                .ShouldBe("SELECT $(Foo)");
        }

        [Test]
        public void ReplacesRawTokenWithValueVerbatim()
        {
            var parameters = new Dictionary<string, object> { ["TablePrefix"] = "App_" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT * FROM $(TablePrefix)Users", parameters, null)
                .ShouldBe("SELECT * FROM App_Users");
        }

        [Test]
        public void ReplacesRawTokenWithInvariantCultureForDecimalValue()
        {
            var originalCulture = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                var parameters = new Dictionary<string, object> { ["Price"] = 12.34m };

                SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $(Price)", parameters, null)
                    .ShouldBe("SELECT 12.34");
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Test]
        public void ReplacesRawTokenWithRawSqlValue()
        {
            var parameters = new Dictionary<string, object>
            {
                ["CurrentTimestamp"] = RawSql.Insert("CURRENT_TIMESTAMP")
            };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $(CurrentTimestamp)", parameters, null)
                .ShouldBe("SELECT CURRENT_TIMESTAMP");
        }

        [Test]
        public void LeavesUnknownRawTokenUnchanged()
        {
            var parameters = new Dictionary<string, object> { ["Known"] = "Value" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $(Unknown)", parameters, null)
                .ShouldBe("SELECT $(Unknown)");
        }

        [Test]
        public void UnescapesDoubleDollarRawToken()
        {
            var parameters = new Dictionary<string, object> { ["Foo"] = "Bar" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT '$$((Foo))'", parameters, null)
                .ShouldBe("SELECT '$(Foo)'");
        }

        [Test]
        public void ReplacesSafeTokenWithQuotedAndEscapedStringLiteral()
        {
            var parameters = new Dictionary<string, object> { ["DefaultStatus"] = "isn't active" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[DefaultStatus]", parameters, new GenericQuoter())
                .ShouldBe("SELECT 'isn''t active'");
        }

        [Test]
        public void ReplacesSafeTokenWithNullLiteralWhenValueIsNull()
        {
            var parameters = new Dictionary<string, object> { ["Value"] = null };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[Value]", parameters, new GenericQuoter())
                .ShouldBe("SELECT NULL");
        }

        [Test]
        public void LeavesUnknownSafeTokenUnchanged()
        {
            var parameters = new Dictionary<string, object> { ["Known"] = "Value" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[Unknown]", parameters, new GenericQuoter())
                .ShouldBe("SELECT $[Unknown]");
        }

        [Test]
        public void UnescapesDoubleDollarSafeToken()
        {
            var parameters = new Dictionary<string, object> { ["Foo"] = "Bar" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT '$$[[Foo]]'", parameters, new GenericQuoter())
                .ShouldBe("SELECT '$[Foo]'");
        }

        [Test]
        public void SupportsMixOfRawAndSafeTokens()
        {
            var parameters = new Dictionary<string, object>
            {
                ["TablePrefix"] = "App_",
                ["DefaultStatus"] = "isn't active",
                ["CurrentDate"] = "GETDATE()",
            };

            var sql = SqlScriptTokenReplacer.ReplaceSqlScriptTokens(
                "INSERT INTO $(TablePrefix)Users (Status, CreatedAt) VALUES ($[DefaultStatus], $(CurrentDate));",
                parameters, new GenericQuoter());

            sql.ShouldBe("INSERT INTO App_Users (Status, CreatedAt) VALUES ('isn''t active', GETDATE());");
        }

        [Test]
        public void ReplacesSafeTokenWithUnquotedNumberWhenQuoterIsSupplied()
        {
            var parameters = new Dictionary<string, object> { ["Count"] = 42 };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[Count]", parameters, new GenericQuoter())
                .ShouldBe("SELECT 42");
        }

        [Test]
        public void ReplacesSafeTokenWithFormattedDateTimeWhenQuoterIsSupplied()
        {
            var value = new DateTime(2024, 1, 2, 3, 4, 5);
            var parameters = new Dictionary<string, object> { ["CreatedAt"] = value };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[CreatedAt]", parameters, new GenericQuoter())
                .ShouldBe("SELECT '2024-01-02T03:04:05'");
        }

        [Test]
        public void ReplacesSafeTokenWithBoolWhenQuoterIsSupplied()
        {
            var parameters = new Dictionary<string, object> { ["IsActive"] = true };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[IsActive]", parameters, new GenericQuoter())
                .ShouldBe("SELECT 1");
        }

        [Test]
        public void ReplacesSafeTokenWithQuotedAndEscapedStringWhenQuoterIsSupplied()
        {
            var parameters = new Dictionary<string, object> { ["DefaultStatus"] = "isn't active" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[DefaultStatus]", parameters, new GenericQuoter())
                .ShouldBe("SELECT 'isn''t active'");
        }

        [Test]
        public void ReplacesSafeTokenWithNullLiteralWhenValueIsNullAndQuoterIsSupplied()
        {
            var parameters = new Dictionary<string, object> { ["Value"] = null };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[Value]", parameters, new GenericQuoter())
                .ShouldBe("SELECT NULL");
        }

#pragma warning disable CS0618 // Type or member is obsolete
        [Test]
        public void LegacyStringDictionaryOverloadReplacesRawToken()
        {
            var parameters = new Dictionary<string, string> { ["TablePrefix"] = "App_" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT * FROM $(TablePrefix)Users", parameters)
                .ShouldBe("SELECT * FROM App_Users");
        }

        /// <summary>
        /// The obsolete overload does not expand <c>$[name]</c>, and never did: that token style
        /// arrived long after this overload, and 8.0.1 returned such text unchanged. There is also
        /// no quoter here to render it with. Restoring that is deliberate - see
        /// <c>adr/proposed/RequireQuoterForTokenSubstitution.md</c>.
        /// </summary>
        [Test]
        public void LegacyStringDictionaryOverloadLeavesSafeTokenVerbatim()
        {
            var parameters = new Dictionary<string, string> { ["Name"] = "isn't" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[Name]", parameters)
                .ShouldBe("SELECT $[Name]");
        }

        /// <summary>
        /// The escape form travels with the token it escapes. Both arrived together, so an overload
        /// that does not expand <c>$[name]</c> must not rewrite <c>$$[[name]]</c> either.
        /// </summary>
        [Test]
        public void LegacyStringDictionaryOverloadLeavesSafeTokenEscapeVerbatim()
        {
            var parameters = new Dictionary<string, string> { ["Foo"] = "Bar" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT '$$[[Foo]]'", parameters)
                .ShouldBe("SELECT '$$[[Foo]]'");
        }

        /// <summary>
        /// The raw token style is unaffected, which is what 8.0.1 callers actually use.
        /// </summary>
        [Test]
        public void LegacyStringDictionaryOverloadStillExpandsRawTokenAndItsEscape()
        {
            var parameters = new Dictionary<string, string> { ["Foo"] = "Bar" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $(Foo), '$$((Foo))'", parameters)
                .ShouldBe("SELECT Bar, '$(Foo)'");
        }

        [Test]
        public void LegacyStringDictionaryOverloadReturnsOriginalTextWhenParametersAreNull()
        {
            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $(Foo)", (IDictionary<string, string>)null)
                .ShouldBe("SELECT $(Foo)");
        }

        [Test]
        public void LegacyStringDictionaryOverloadPreservesCaseInsensitiveKeyComparer()
        {
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["TablePrefix"] = "App_",
            };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT * FROM $(tableprefix)Users", parameters)
                .ShouldBe("SELECT * FROM App_Users");
        }

        [Test]
        public void LegacyStringDictionaryOverloadKeepsCaseSensitiveLookupForDefaultComparer()
        {
            var parameters = new Dictionary<string, string> { ["TablePrefix"] = "App_" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT * FROM $(tableprefix)Users", parameters)
                .ShouldBe("SELECT * FROM $(tableprefix)Users");
        }

        [Test]
        public void LegacyStringSortedDictionaryOverloadPreservesCaseInsensitiveLookup()
        {
            IDictionary<string, string> parameters = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["TablePrefix"] = "App_",
            };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT * FROM $(tableprefix)Users", parameters)
                .ShouldBe("SELECT * FROM App_Users");
        }
#pragma warning restore CS0618 // Type or member is obsolete

        #region Quoter is required only where $[name] is expanded

        // The guard lives inside the $[name] branch rather than at method entry. These two tests
        // are the pair that makes that visible; the second is the one that regresses if someone
        // "tidies" the check up to the top of the method.
        // See adr/proposed/RequireQuoterForTokenSubstitution.md.

        [Test]
        public void ThrowsWhenSafeTokenIsExpandedWithoutAQuoter()
        {
            var parameters = new Dictionary<string, object> { ["DefaultStatus"] = "active" };

            var exception = Assert.Throws<ArgumentNullException>(
                () => SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[DefaultStatus]", parameters, quoter: null));

            Assert.Multiple(() =>
            {
                exception.ParamName.ShouldBe("quoter");

                // Names the token that forced the failure, so a long script points at one place.
                exception.Message.ShouldContain("$[DefaultStatus]");
            });
        }

        [Test]
        public void DoesNotRequireAQuoterWhenOnlyRawTokensAreExpanded()
        {
            var parameters = new Dictionary<string, object> { ["TablePrefix"] = "App_" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT * FROM $(TablePrefix)Users", parameters, quoter: null)
                .ShouldBe("SELECT * FROM App_Users");
        }

        /// <summary>
        /// An unresolved <c>$[name]</c> is left verbatim without consulting the quoter, so it never
        /// reaches the guard.
        /// </summary>
        [Test]
        public void DoesNotRequireAQuoterForAnUnknownSafeToken()
        {
            var parameters = new Dictionary<string, object> { ["Known"] = "Value" };

            SqlScriptTokenReplacer.ReplaceSqlScriptTokens("SELECT $[Unknown]", parameters, quoter: null)
                .ShouldBe("SELECT $[Unknown]");
        }

        #endregion
    }
}
