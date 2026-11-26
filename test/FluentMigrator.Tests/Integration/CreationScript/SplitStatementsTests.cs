#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

using System.Linq;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.CreationScript
{
    /// <summary>
    /// How <see cref="CreationScriptTarget.SplitStatements"/> splits a script into the statements it runs one by one.
    /// </summary>
    [TestFixture]
    public class SplitStatementsTests
    {
        /// <summary>
        /// A PostgreSQL dollar-quoted body is one token, whatever <c>;</c> it has. Its tag is empty, or a letter or
        /// underscore followed by letters, digits or underscores.
        /// </summary>
        [TestCase("$$")]
        [TestCase("$fluentmigrator$")]
        [TestCase("$migration1$")]
        [TestCase("$_2$")]
        public void ADollarQuotedBodyIsOneStatement(string tag)
        {
            var script = $"DO {tag} BEGIN PERFORM 1; PERFORM 2; END {tag}; SELECT 1;";

            // What follows the last ';' is an empty statement, which the targets skip.
            var statements = CreationScriptTarget.SplitStatements(script)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();

            Assert.That(statements, Is.EqualTo(new[] { $"DO {tag} BEGIN PERFORM 1; PERFORM 2; END {tag}", "SELECT 1" }));
        }

        /// <summary>
        /// PostgreSQL and MySQL allow <c>$</c> in an unquoted identifier after its first character, so <c>$b$</c> in
        /// <c>a$b$c</c> is part of the name, not a tag.
        /// </summary>
        [Test]
        public void ADollarInAnIdentifierIsNotATag()
        {
            const string script = "SELECT 1 AS a$b$c; SELECT 2;";

            var statements = CreationScriptTarget.SplitStatements(script)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();

            Assert.That(statements, Is.EqualTo(new[] { "SELECT 1 AS a$b$c", "SELECT 2" }));
        }
    }
}
