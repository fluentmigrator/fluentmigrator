#region License
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using FluentMigrator.Runner.BatchParser.Sources;
using FluentMigrator.Runner.BatchParser.SpecialTokenSearchers;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.BatchParser
{
    [Category("BatchParser")]
    public class SpecialTokenTests
    {
        [TestCase(";")]
        [TestCase(" ; ")]
        [TestCase(" ;")]
        [TestCase("; ")]
        public void TestIfSemicolonExists(string input)
        {
            var source = new LinesSource(new[] { input });
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);
            var tokenSearcher = new SemicolonSearcher();
            var result = tokenSearcher.Find(reader);
            Assert.IsNotNull(result);
            Assert.Greater(result.Index, -1);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(";", result.Token);
        }

        [Test]
        public void TestIfSemicolonMissing()
        {
            var source = new LinesSource(new[] { string.Empty });
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);
            var tokenSearcher = new SemicolonSearcher();
            var result = tokenSearcher.Find(reader);
            Assert.IsNull(result);
        }

        [TestCase("GO", "GO", 1)]
        [TestCase(" GO ", "GO", 1)]
        [TestCase(" GO", "GO", 1)]
        [TestCase("GO ", "GO", 1)]
        [TestCase("GO 123", "GO 123", 123)]
        [TestCase("  GO 123  ", "GO 123", 123)]
        [TestCase("  gO 123  ", "gO 123", 123)]
        [TestCase("  gO 123", "gO 123", 123)]
        [TestCase("gO 123", "gO 123", 123)]
        public void TestIfGoExists(string input, string expected, int expectedCount)
        {
            var source = new LinesSource(new[] { input });
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);
            var tokenSearcher = new GoSearcher();
            var result = tokenSearcher.Find(reader);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Index);
            Assert.AreEqual(input.Length, result.Length);
            Assert.AreEqual(expected, result.Token);
            Assert.IsInstanceOf<GoSearcher.GoSearcherParameters>(result.Opaque);
            var goParams = (GoSearcher.GoSearcherParameters) result.Opaque;
            Assert.NotNull(goParams);
            Assert.AreEqual(expectedCount, goParams.Count);
        }

        [TestCase("x GO")]
        [TestCase("GO x")]
        [TestCase("GO 123 123")]
        public void TestIfGoMissing(string input)
        {
            var source = new LinesSource(new[] { string.Empty });
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);
            var tokenSearcher = new GoSearcher();
            var result = tokenSearcher.Find(reader);
            Assert.IsNull(result);
        }

        [Test]
        public void TestIfGoMissingIfReaderNotAtBeginOfLine()
        {
            var source = new LinesSource(new[] { " GO" });
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);
            reader = reader.Advance(1);
            Assert.IsNotNull(reader);
            var tokenSearcher = new GoSearcher();
            var result = tokenSearcher.Find(reader);
            Assert.IsNull(result);
        }
    }
}
