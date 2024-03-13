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
            Assert.That(instance, Is.InstanceOf<IRangeSearcher>());
            var rangeSearcher = (IRangeSearcher)instance;
            Assert.Multiple(() =>
            {
                Assert.That(rangeSearcher.StartCodeLength, Is.EqualTo(startLength));
                Assert.That(rangeSearcher.EndCodeLength, Is.EqualTo(endLength));
            });
        }

        [TestCase("  \"qweqwe\"  ", "qweqwe")]
        [TestCase(@"  ""qwe\""qweqwe""  ", "qwe\\")]
        public void TestAnsiSqlIdentifiers(string input, string expected)
        {
            var source = new LinesSource(new[] { input });
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);

            var rangeSearcher = new AnsiSqlIdentifier();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.That(startIndex, Is.Not.EqualTo(-1));

            reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
            Assert.That(reader, Is.Not.Null);

            var endInfo = rangeSearcher.FindEndCode(reader);
            Assert.That(endInfo, Is.Not.Null);
            Assert.That(endInfo.IsNestedStart, Is.False);
            var endIndex = endInfo.Index;
            var result = reader.ReadString(endIndex - reader.Index);

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("  `qweqwe`  ", "qweqwe")]
        [TestCase("  `qwe``qweqwe`  ", "qwe``qweqwe")]
        public void TestMySqlIdentifiers(string input, string expected)
        {
            var source = new LinesSource(new[] { input });
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);

            var rangeSearcher = new MySqlIdentifier();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.That(startIndex, Is.Not.EqualTo(-1));

            reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
            Assert.That(reader, Is.Not.Null);

            var endInfo = rangeSearcher.FindEndCode(reader);
            Assert.That(endInfo, Is.Not.Null);
            Assert.That(endInfo.IsNestedStart, Is.False);
            var endIndex = endInfo.Index;
            var result = reader.ReadString(endIndex - reader.Index);

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("  [qweqwe]  ", "qweqwe")]
        [TestCase("  [qwe]]qweqwe]  ", "qwe]]qweqwe")]
        public void TestSqlServerIdentifiers(string input, string expected)
        {
            var source = new LinesSource(new[] { input });
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);

            var rangeSearcher = new SqlServerIdentifier();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.That(startIndex, Is.Not.EqualTo(-1));

            reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
            Assert.That(reader, Is.Not.Null);

            var endInfo = rangeSearcher.FindEndCode(reader);
            Assert.That(endInfo, Is.Not.Null);
            Assert.That(endInfo.IsNestedStart, Is.False);
            var endIndex = endInfo.Index;
            var result = reader.ReadString(endIndex - reader.Index);

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("  'qweqwe'  ", "qweqwe")]
        [TestCase("  'qweqwe'", "qweqwe")]
        [TestCase("  'qwe''qweqwe'  ", "qwe''qweqwe")]
        public void TestSqlString(string input, string expected)
        {
            var source = new LinesSource(new[] { input });
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);

            var rangeSearcher = new SqlString();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.That(startIndex, Is.Not.EqualTo(-1));

            reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
            Assert.That(reader, Is.Not.Null);

            var endInfo = rangeSearcher.FindEndCode(reader);
            Assert.That(endInfo, Is.Not.Null);
            Assert.That(endInfo.IsNestedStart, Is.False);
            var endIndex = endInfo.Index;
            var result = reader.ReadString(endIndex - reader.Index);

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("  'qweqwe")]
        public void TestIncompleteSqlString(string input)
        {
            var source = new LinesSource(new[] { input });
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);

            var rangeSearcher = new SqlString();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.That(startIndex, Is.Not.EqualTo(-1));

            reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
            Assert.That(reader, Is.Not.Null);

            var endInfo = rangeSearcher.FindEndCode(reader);
            Assert.That(endInfo, Is.Null);
        }

        [Test]
        public void TestMissingSqlString()
        {
            var source = new LinesSource(new[] { string.Empty });
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);

            var rangeSearcher = new SqlString();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.That(startIndex, Is.EqualTo(-1));
        }

        [TestCase("  /* blah */  ", " blah ")]
        [TestCase("  /* blah /* blubb */  ", " blah /* blubb ")]
        public void TestMultiLineCommentWithSingleLine(string input, string expected)
        {
            var source = new TextReaderSource(new StringReader(input));
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);

            var rangeSearcher = new MultiLineComment();
            var startIndex = rangeSearcher.FindStartCode(reader);
            Assert.That(startIndex, Is.Not.EqualTo(-1));

            reader = reader.Advance(startIndex + rangeSearcher.StartCodeLength);
            Assert.That(reader, Is.Not.Null);

            var endInfo = rangeSearcher.FindEndCode(reader);
            Assert.That(endInfo, Is.Not.Null);
            Assert.That(endInfo.IsNestedStart, Is.False);
            var endIndex = endInfo.Index;
            var result = reader.ReadString(endIndex - reader.Index);

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("/** blah\n * blubb\n*/  ", "* blah\n * blubb\n")]
        public void TestMultiLineCommentWithMultipleLines(string input, string expected)
        {
            using (var source = new TextReaderSource(new StringReader(input), true))
            {
                var reader = source.CreateReader();
                Assert.That(reader, Is.Not.Null);

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
                        Assert.That(reader, Is.Not.Null);
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

                Assert.Multiple(() =>
                {
                    Assert.That(foundStart, Is.False);
                    Assert.That(content.ToString(), Is.EqualTo(expected));
                });
            }
        }

        [TestCase("   -- qweqwe", " qweqwe", "   ")]
        [TestCase("   -- qwe\nqwe", " qwe", "   \nqwe")]
        [TestCase("asd -- qwe\nqwe", " qwe", "asd \nqwe")]
        public void TestDoubleDashSingleLineComment(string input, string expectedComment, string expectedOther)
        {
            var source = new TextReaderSource(new StringReader(input));
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);

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
                Assert.That(reader, Is.Not.Null);

                var endInfo = rangeSearcher.FindEndCode(reader);
                Assert.That(endInfo, Is.Not.Null);

                var contentLength = endInfo.Index - reader.Index;
                commentWriter.Write(reader.ReadString(contentLength));
                addNewLine = (contentLength + rangeSearcher.EndCodeLength) == reader.Length;
                reader = reader.Advance(contentLength + rangeSearcher.EndCodeLength);
            }

            Assert.Multiple(() =>
            {
                Assert.That(commentContent.ToString(), Is.EqualTo(expectedComment));
                Assert.That(otherContent.ToString(), Is.EqualTo(expectedOther));
            });
        }

        [TestCase("   # qweqwe", " qweqwe", "   ")]
        [TestCase("   # qwe\nqwe", " qwe", "   \nqwe")]
        [TestCase("asd # qwe\nqwe", "", "asd # qwe\nqwe")]
        public void TestPoundSignSingleLineComment(string input, string expectedComment, string expectedOther)
        {
            var source = new TextReaderSource(new StringReader(input));
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);

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
                Assert.That(reader, Is.Not.Null);

                var endInfo = rangeSearcher.FindEndCode(reader);
                Assert.That(endInfo, Is.Not.Null);

                var contentLength = endInfo.Index - reader.Index;
                commentWriter.Write(reader.ReadString(contentLength));
                addNewLine = (contentLength + rangeSearcher.EndCodeLength) == reader.Length;
                reader = reader.Advance(contentLength + rangeSearcher.EndCodeLength);
            }

            Assert.Multiple(() =>
            {
                Assert.That(commentContent.ToString(), Is.EqualTo(expectedComment));
                Assert.That(otherContent.ToString(), Is.EqualTo(expectedOther));
            });
        }

        [Test]
        public void TestNestingMultiLineComment()
        {
            var searchers = new Stack<IRangeSearcher>();
            IRangeSearcher searcher = new NestingMultiLineComment();
            var source = new TextReaderSource(new StringReader("/* /* */ */"));
            var reader = source.CreateReader();
            Assert.Multiple(() =>
            {
                Assert.That(reader, Is.Not.Null);

                Assert.That(searcher.FindStartCode(reader), Is.EqualTo(0));
            });
            reader = reader.Advance(2);
            Assert.That(reader, Is.Not.Null);

            var endInfo = searcher.FindEndCode(reader);
            Assert.That(endInfo, Is.Not.Null);
            Assert.That(endInfo.IsNestedStart);

            searchers.Push(searcher);
            searcher = endInfo.NestedRangeSearcher;
            Assert.Multiple(() =>
            {
                Assert.That(searcher, Is.Not.Null);
                Assert.That(endInfo.Index, Is.EqualTo(3));
            });

            reader = reader.Advance(endInfo.Index - reader.Index + searcher.StartCodeLength);
            Assert.That(reader, Is.Not.Null);

            endInfo = searcher.FindEndCode(reader);
            Assert.That(endInfo, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(endInfo.IsNestedStart, Is.False);
                Assert.That(endInfo.Index, Is.EqualTo(6));
            });

            reader = reader.Advance(endInfo.Index - reader.Index + searcher.EndCodeLength);
            Assert.That(reader, Is.Not.Null);

            searcher = searchers.Pop();
            endInfo = searcher.FindEndCode(reader);
            Assert.That(endInfo, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(endInfo.IsNestedStart, Is.False);
                Assert.That(endInfo.Index, Is.EqualTo(9));
            });

            reader = reader.Advance(endInfo.Index - reader.Index + searcher.EndCodeLength);
            Assert.That(reader, Is.Null);
        }
    }
}
