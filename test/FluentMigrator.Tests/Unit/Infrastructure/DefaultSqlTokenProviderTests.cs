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
using System.Linq;

using FluentMigrator.Builders.Schema;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Infrastructure;

using FluentMigrator.Runner.Generators.Generic;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Infrastructure
{
    [TestFixture]
    [Category("Infrastructure")]
    [Category("SqlScriptTokenProvider")]
    public class DefaultSqlTokenProviderTests
    {
        [Test]
        public void GetTokensReturnsDefaultSchemaFromConventionSet()
        {
            var conventionSet = ConventionSets.CreateTestSchemaName(null);
            var provider = new DefaultSqlTokenProvider(new ConventionSetDefaultSchemaNameResolver(conventionSet));

            var tokenMap = provider.GetTokens();

            tokenMap.ShouldContainKeyAndValue("DefaultSchema", "testdefault");
        }

        [Test]
        public void GetTokensDoesNotContainDefaultSchemaWhenNoDefaultSchemaConfigured()
        {
            var conventionSet = ConventionSets.CreateNoSchemaName(null);
            var provider = new DefaultSqlTokenProvider(new ConventionSetDefaultSchemaNameResolver(conventionSet));

            var tokenMap = provider.GetTokens();

            tokenMap.ShouldNotContainKey("DefaultSchema");
        }

        [Test]
        public void DefaultSchemaTokenIsReplacedForRawTokenSyntax()
        {
            var conventionSet = ConventionSets.CreateTestSchemaName(null);
            var provider = new DefaultSqlTokenProvider(new ConventionSetDefaultSchemaNameResolver(conventionSet));

            var result = SqlScriptTokenReplacer.ReplaceSqlScriptTokens(
                "ALTER TABLE $(DefaultSchema).Users ADD COLUMN Foo INT",
                ToObjectDictionary(provider.GetTokens()), null);

            result.ShouldBe("ALTER TABLE testdefault.Users ADD COLUMN Foo INT");
        }

        [Test]
        public void DefaultSchemaTokenIsReplacedAsQuotedStringLiteralForQuotedTokenSyntax()
        {
            var conventionSet = ConventionSets.CreateTestSchemaName(null);
            var provider = new DefaultSqlTokenProvider(new ConventionSetDefaultSchemaNameResolver(conventionSet));

            var result = SqlScriptTokenReplacer.ReplaceSqlScriptTokens(
                "SELECT * FROM Users WHERE SchemaName = $[DefaultSchema]",
                ToObjectDictionary(provider.GetTokens()), new GenericQuoter());

            result.ShouldBe("SELECT * FROM Users WHERE SchemaName = 'testdefault'");
        }

        [Test]
        public void DefaultSchemaTokenWithEmbeddedQuoteIsEscapedForQuotedTokenSyntax()
        {
            var conventionSet = ConventionSets.Create(new DefaultSchemaConvention("te'st"), null);
            var provider = new DefaultSqlTokenProvider(new ConventionSetDefaultSchemaNameResolver(conventionSet));

            var result = SqlScriptTokenReplacer.ReplaceSqlScriptTokens(
                "SELECT * FROM Users WHERE SchemaName = $[DefaultSchema]",
                ToObjectDictionary(provider.GetTokens()), new GenericQuoter());

            result.ShouldBe("SELECT * FROM Users WHERE SchemaName = 'te''st'");
        }

        [Test]
        public void DefaultSchemaTokenIsLeftUntouchedWhenNoDefaultSchemaConfigured()
        {
            var conventionSet = ConventionSets.CreateNoSchemaName(null);
            var provider = new DefaultSqlTokenProvider(new ConventionSetDefaultSchemaNameResolver(conventionSet));

            var result = SqlScriptTokenReplacer.ReplaceSqlScriptTokens(
                "ALTER TABLE $(DefaultSchema).Users ADD COLUMN Foo INT; SELECT $[DefaultSchema]",
                ToObjectDictionary(provider.GetTokens()), new GenericQuoter());

            result.ShouldBe("ALTER TABLE $(DefaultSchema).Users ADD COLUMN Foo INT; SELECT $[DefaultSchema]");
        }

        [Test]
        public void DefaultSchemaIsWhatTheResolverGivesForNoSchema()
        {
            var provider = new DefaultSqlTokenProvider(new FixedDefaultSchemaResolver("tenant"));

            provider.GetTokens().ShouldContainKeyAndValue("DefaultSchema", "tenant");
        }

        [Test]
        public void TheTokenAndTheSchemaChecksUseTheSameSchemaWhenTheResolverIsReplaced()
        {
            using var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .AddScoped<IDefaultSchemaNameResolver>(_ => new FixedDefaultSchemaResolver("tenant"))
                .BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var tokens = scope.ServiceProvider.GetServices<ISqlTokenProvider>().SelectMany(p => p.GetTokens());
            tokens.ShouldContain(new KeyValuePair<string, string>("DefaultSchema", "tenant"));

            var querySchema = new Mock<IQuerySchema>();
            var context = new Mock<IMigrationContext>();
            context.SetupGet(x => x.ServiceProvider).Returns(scope.ServiceProvider);
            context.SetupGet(x => x.QuerySchema).Returns(querySchema.Object);

            new SchemaExpressionRoot(context.Object).Table("Users").Exists();

            querySchema.Verify(x => x.TableExists("tenant", "Users"));
        }

        private static IDictionary<string, object> ToObjectDictionary(IDictionary<string, string> tokens)
        {
            return tokens.ToDictionary(x => x.Key, x => (object)x.Value);
        }

        /// <summary>
        /// A resolver registered in place of the convention-set one.
        /// </summary>
        private class FixedDefaultSchemaResolver : IDefaultSchemaNameResolver
        {
            private readonly string _defaultSchema;

            public FixedDefaultSchemaResolver(string defaultSchema)
            {
                _defaultSchema = defaultSchema;
            }

            public string GetSchemaName(string schemaName) => schemaName ?? _defaultSchema;
        }
    }
}
