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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.BatchParser.RangeSearchers;
using FluentMigrator.Runner.BatchParser.Sources;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.BatchParser
{
    public class RangeSearcherTests
    {
        [TestCase(typeof(AnsiSqlIdentifier), 1, 1)]
        [TestCase(typeof(MySqlIdentifier), 1, 1)]
        [TestCase(typeof(SqlServerIdentifier), 1, 1)]
        [TestCase(typeof(SqlString), 1, 1)]
        [TestCase(typeof(MultiLineComment), 2, 2)]
        [TestCase(typeof(DoubleDashSingleLineComment), 2, 0)]
        public void TestConfiguration(Type type, int startLength, int endLength)
        {
            var instance = Activator.CreateInstance(type);
            Assert.IsInstanceOf<IRangeSearcher>(instance);
            var rangeSearcher = (IRangeSearcher)instance;
            Assert.AreEqual(startLength, rangeSearcher.StartCodeLength);
            Assert.AreEqual(endLength, rangeSearcher.EndCodeLength);
        }

        [TestCase("  \"qweqwe\"  ", "qweqwe")]
        [TestCase(@"  ""qwe\""qweqwe""  ", "qwe\\")]
        public void TestAnsiSqlIdentifiers(string input, string expected)
        {
            var source = new LinesSource(new[] { input });
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);

            var rangeSearcher = new AnsiSqlIdentifier();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.AreNotEqual(-1, startIndex);

            reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
            Assert.IsNotNull(reader);

            var endInfo = rangeSearcher.FindEndCode(reader);
            Assert.IsNotNull(endInfo);
            Assert.IsFalse(endInfo.IsNestedStart);
            var endIndex = endInfo.Index;
            var result = reader.ReadString(endIndex - reader.Index);

            Assert.AreEqual(expected, result);
        }

        [TestCase("  `qweqwe`  ", "qweqwe")]
        [TestCase("  `qwe``qweqwe`  ", "qwe``qweqwe")]
        public void TestMySqlIdentifiers(string input, string expected)
        {
            var source = new LinesSource(new[] { input });
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);

            var rangeSearcher = new MySqlIdentifier();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.AreNotEqual(-1, startIndex);

            reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
            Assert.IsNotNull(reader);

            var endInfo = rangeSearcher.FindEndCode(reader);
            Assert.IsNotNull(endInfo);
            Assert.IsFalse(endInfo.IsNestedStart);
            var endIndex = endInfo.Index;
            var result = reader.ReadString(endIndex - reader.Index);

            Assert.AreEqual(expected, result);
        }

        [TestCase("  [qweqwe]  ", "qweqwe")]
        [TestCase("  [qwe]]qweqwe]  ", "qwe]]qweqwe")]
        public void TestSqlServerIdentifiers(string input, string expected)
        {
            var source = new LinesSource(new[] { input });
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);

            var rangeSearcher = new SqlServerIdentifier();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.AreNotEqual(-1, startIndex);

            reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
            Assert.IsNotNull(reader);

            var endInfo = rangeSearcher.FindEndCode(reader);
            Assert.IsNotNull(endInfo);
            Assert.IsFalse(endInfo.IsNestedStart);
            var endIndex = endInfo.Index;
            var result = reader.ReadString(endIndex - reader.Index);

            Assert.AreEqual(expected, result);
        }

        [TestCase("  'qweqwe'  ", "qweqwe")]
        [TestCase("  'qweqwe'", "qweqwe")]
        [TestCase("  'qwe''qweqwe'  ", "qwe''qweqwe")]
        public void TestSqlString(string input, string expected)
        {
            var source = new LinesSource(new[] { input });
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);

            var rangeSearcher = new SqlString();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.AreNotEqual(-1, startIndex);

            reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
            Assert.IsNotNull(reader);

            var endInfo = rangeSearcher.FindEndCode(reader);
            Assert.IsNotNull(endInfo);
            Assert.IsFalse(endInfo.IsNestedStart);
            var endIndex = endInfo.Index;
            var result = reader.ReadString(endIndex - reader.Index);

            Assert.AreEqual(expected, result);
        }

        [TestCase("  'qweqwe")]
        public void TestIncompleteSqlString(string input)
        {
            var source = new LinesSource(new[] { input });
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);

            var rangeSearcher = new SqlString();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.AreNotEqual(-1, startIndex);

            reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
            Assert.IsNotNull(reader);

            var endInfo = rangeSearcher.FindEndCode(reader);
            Assert.IsNull(endInfo);
        }

        [Test]
        public void TestMissingSqlString()
        {
            var source = new LinesSource(new[] { string.Empty });
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);

            var rangeSearcher = new SqlString();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.AreEqual(-1, startIndex);
        }

        [TestCase("  /* blah */  ", " blah ")]
        [TestCase("  /* blah /* blubb */  ", " blah /* blubb ")]
        public void TestMultiLineCommentWithSingleLine(string input, string expected)
        {
            var source = new TextReaderSource(new StringReader(input));
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);

            var rangeSearcher = new MultiLineComment();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.AreNotEqual(-1, startIndex);

            reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
            Assert.IsNotNull(reader);

            var endInfo = rangeSearcher.FindEndCode(reader);
            Assert.IsNotNull(endInfo);
            Assert.IsFalse(endInfo.IsNestedStart);
            var endIndex = endInfo.Index;
            var result = reader.ReadString(endIndex - reader.Index);

            Assert.AreEqual(expected, result);
        }

        [TestCase("/** blah\n * blubb\n*/  ", "* blah\n * blubb\n")]
        public void TestMultiLineCommentWithMultipleLines(string input, string expected)
        {
            using (var source = new TextReaderSource(new StringReader(input), true))
            {
                var reader = source.CreateReader();
                Assert.IsNotNull(reader);

                var foundStart = false;
                var content = new StringBuilder();
                var writer = new StringWriter(content)
                {
                    NewLine = "\n",
                };

                var rangeSearcher = new MultiLineComment();
                while (reader != null)
                {
                    if (!foundStart)
                    {
                        var startIndex = rangeSearcher.FindStartCode(reader);
                        if (startIndex == -1)
                        {
                            reader = reader.Advance(reader.Length);
                            continue;
                        }

                        foundStart = true;
                        reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
                        Assert.IsNotNull(reader);
                    }

                    var endInfo = rangeSearcher.FindEndCode(reader);
                    if (endInfo == null)
                    {
                        writer.WriteLine(reader.ReadString(reader.Length));
                        reader = reader.Advance(reader.Length);
                        continue;
                    }

                    var contentLength = endInfo.Index - reader.Index;
                    writer.Write(reader.ReadString(contentLength));
                    reader = reader.Advance(contentLength + rangeSearcher.EndCodeLength);
                    foundStart = false;
                }

                Assert.IsFalse(foundStart);
                Assert.AreEqual(expected, content.ToString());
            }
        }

        [TestCase("   -- qweqwe", " qweqwe", "   ")]
        [TestCase("   -- qwe\nqwe", " qwe", "   \nqwe")]
        [TestCase("asd -- qwe\nqwe", " qwe", "asd \nqwe")]
        public void TestDoubleDashSingleLineComment(string input, string expectedComment, string expectedOther)
        {
            var source = new TextReaderSource(new StringReader(input));
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);

            var commentContent = new StringBuilder();
            var commentWriter = new StringWriter(commentContent)
            {
                NewLine = "\n",
            };

            var otherContent = new StringBuilder();
            var otherWriter = new StringWriter(otherContent)
            {
                NewLine = "\n",
            };

            var addNewLine = false;
            var rangeSearcher = new DoubleDashSingleLineComment();
            while (reader != null)
            {
                if (addNewLine)
                    otherWriter.WriteLine();

                var startIndex = rangeSearcher.FindStartCode(reader);
                if (startIndex == -1)
                {
                    otherWriter.Write(reader.ReadString(reader.Length));
                    addNewLine = true;
                    reader = reader.Advance(reader.Length);
                    continue;
                }

                otherWriter.Write(reader.ReadString(startIndex));
                reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
                Assert.IsNotNull(reader);

                var endInfo = rangeSearcher.FindEndCode(reader);
                Assert.IsNotNull(endInfo);

                var contentLength = endInfo.Index - reader.Index;
                commentWriter.Write(reader.ReadString(contentLength));
                addNewLine = (contentLength + rangeSearcher.EndCodeLength) == reader.Length;
                reader = reader.Advance(contentLength + rangeSearcher.EndCodeLength);
            }

            Assert.AreEqual(expectedComment, commentContent.ToString());
            Assert.AreEqual(expectedOther, otherContent.ToString());
        }

        [TestCase("   # qweqwe", " qweqwe", "   ")]
        [TestCase("   # qwe\nqwe", " qwe", "   \nqwe")]
        [TestCase("asd # qwe\nqwe", "", "asd # qwe\nqwe")]
        public void TestPoundSignSingleLineComment(string input, string expectedComment, string expectedOther)
        {
            var source = new TextReaderSource(new StringReader(input));
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);

            var commentContent = new StringBuilder();
            var commentWriter = new StringWriter(commentContent)
            {
                NewLine = "\n",
            };

            var otherContent = new StringBuilder();
            var otherWriter = new StringWriter(otherContent)
            {
                NewLine = "\n",
            };

            var addNewLine = false;
            var rangeSearcher = new PoundSignSingleLineComment();
            while (reader != null)
            {
                if (addNewLine)
                    otherWriter.WriteLine();

                var startIndex = rangeSearcher.FindStartCode(reader);
                if (startIndex == -1)
                {
                    otherWriter.Write(reader.ReadString(reader.Length));
                    addNewLine = true;
                    reader = reader.Advance(reader.Length);
                    continue;
                }

                otherWriter.Write(reader.ReadString(startIndex));
                reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
                Assert.IsNotNull(reader);

                var endInfo = rangeSearcher.FindEndCode(reader);
                Assert.IsNotNull(endInfo);

                var contentLength = endInfo.Index - reader.Index;
                commentWriter.Write(reader.ReadString(contentLength));
                addNewLine = (contentLength + rangeSearcher.EndCodeLength) == reader.Length;
                reader = reader.Advance(contentLength + rangeSearcher.EndCodeLength);
            }

            Assert.AreEqual(expectedComment, commentContent.ToString());
            Assert.AreEqual(expectedOther, otherContent.ToString());
        }

        [Test]
        public void TestNestingMultiLineComment()
        {
            var searchers = new Stack<IRangeSearcher>();
            IRangeSearcher searcher = new NestingMultiLineComment();
            var source = new TextReaderSource(new StringReader("/* /* */ */"));
            var reader = source.CreateReader();
            Assert.IsNotNull(reader);

            Assert.AreEqual(0, searcher.FindStartCode(reader));
            reader = reader.Advance(2);
            Assert.IsNotNull(reader);

            var endInfo = searcher.FindEndCode(reader);
            Assert.NotNull(endInfo);
            Assert.True(endInfo.IsNestedStart);

            searchers.Push(searcher);
            searcher = endInfo.NestedRangeSearcher;
            Assert.NotNull(searcher);
            Assert.AreEqual(3, endInfo.Index);

            reader = reader.Advance(endInfo.Index - reader.Index + searcher.StartCodeLength);
            Assert.IsNotNull(reader);

            endInfo = searcher.FindEndCode(reader);
            Assert.NotNull(endInfo);
            Assert.IsFalse(endInfo.IsNestedStart);
            Assert.AreEqual(6, endInfo.Index);

            reader = reader.Advance(endInfo.Index - reader.Index + searcher.EndCodeLength);
            Assert.IsNotNull(reader);

            searcher = searchers.Pop();
            endInfo = searcher.FindEndCode(reader);
            Assert.NotNull(endInfo);
            Assert.IsFalse(endInfo.IsNestedStart);
            Assert.AreEqual(9, endInfo.Index);

            reader = reader.Advance(endInfo.Index - reader.Index + searcher.EndCodeLength);
            Assert.IsNull(reader);
        }
    }
}
