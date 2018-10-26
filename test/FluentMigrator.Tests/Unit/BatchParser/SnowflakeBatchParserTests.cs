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

using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.BatchParser.Sources;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.BatchParser
{
    [Category("BatchParser")]
    public class SnowflakeBatchParserTests
    {
        [Test]
        public void TestNothing()
        {
            var output = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { Assert.Fail("No special token expected"); };
            batchParser.Process(new LinesSource(new string[0]));
            Assert.AreEqual(0, output.Count);
        }

        [Test]
        public void TestSqlWithoutGo()
        {
            var output = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { Assert.Fail("No special token expected"); };
            batchParser.Process(new LinesSource(new[] { "asd" }));
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("asd\n", output[0]);
        }

        [Test]
        public void TestEmptySqlWithoutGo()
        {
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { Assert.Fail("No SQL text expected"); };
            batchParser.SpecialToken += (sender, evt) => { Assert.Fail("No special token expected"); };
            batchParser.Process(new LinesSource(new[] { string.Empty }));
        }

        [Test]
        public void TestEmptyLineWithoutGo()
        {
            var output = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { Assert.Fail("No special token expected"); };
            batchParser.Process(new TextReaderSource(new StringReader("\n")));
            Assert.AreEqual(0, output.Count);
        }

        [Test]
        public void TestSqlAfterEmptyLineWithoutGo()
        {
            var output = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { Assert.Fail("No special token expected"); };
            batchParser.Process(new TextReaderSource(new StringReader("\nasd")));
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("\nasd\n", output[0]);
        }

        [Test]
        public void TestSqlWithGo()
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            batchParser.Process(new TextReaderSource(new StringReader("asd\ngo")));
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("asd\ngo\n", output[0]);
            Assert.AreEqual(0, specialTokens.Count);
        }

        [Test]
        public void TestEmptySqlWithGo()
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            batchParser.Process(new TextReaderSource(new StringReader("gO")));
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("gO\n", output[0]);
            Assert.AreEqual(0, specialTokens.Count);
        }

        [Test]
        public void TestEmptyLineWithGo()
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            batchParser.Process(new TextReaderSource(new StringReader("\n  go")));
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("\n  go\n", output[0]);
            Assert.AreEqual(0, specialTokens.Count);
        }

        [Test]
        public void TestSqlMultiBatch()
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            batchParser.Process(new TextReaderSource(new StringReader(@"select ""foo;bar"" from ""asd;"";select 1;")));
            Assert.AreEqual(2, output.Count);
            CollectionAssert.AreEqual(new [] { @"select ""foo;bar"" from ""asd;"";", "select 1;\n" }, output);
            Assert.AreEqual(2, specialTokens.Count);
            CollectionAssert.AreEqual(new [] {";", ";"}, specialTokens);
        }

        [Test]
        public void TestSqlComment()
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            batchParser.Process(new TextReaderSource(new StringReader("/* test */\nasd")));
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("/* test */\nasd\n", output[0]);
            Assert.AreEqual(0, specialTokens.Count);
        }

        [Test]
        public void TestSqlMultiLineCommentWithGo()
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            batchParser.Process(new TextReaderSource(new StringReader("/* test\n * blah */\nasd\ngo")));
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("/* test\n * blah */\nasd\ngo\n", output[0]);
            Assert.AreEqual(0, specialTokens.Count);
        }

        [Test]
        public void TestSqlMultiLineCommentWithoutGo()
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            batchParser.Process(new TextReaderSource(new StringReader("/* test */")));
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("/* test */\n", output[0]);
            Assert.AreEqual(0, specialTokens.Count);
        }

        [Test]
        public void TestSqlStrippedCommentWithoutGo()
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            var source = new TextReaderSource(new StringReader("/* test */"));
            batchParser.Process(source, true);
            Assert.AreEqual(0, output.Count);
            Assert.AreEqual(0, specialTokens.Count);
        }

        [Test]
        public void TestSqlStrippedMultiLineCommentWithoutGo()
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            var source = new TextReaderSource(new StringReader("/* t \n est */"));
            batchParser.Process(source, true);
            Assert.AreEqual(0, output.Count);
            Assert.AreEqual(0, specialTokens.Count);
        }

        [TestCase("/* t \n est */qweqwe", "qweqwe\n")]
        [TestCase("/* t \n est */\nqweqwe", "\nqweqwe\n")]
        public void TestSqlStrippedMultiLineCommentAndSql(string input, string expected)
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            var source = new TextReaderSource(new StringReader(input));
            batchParser.Process(source, true);
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual(expected, output[0]);
            Assert.AreEqual(0, specialTokens.Count);
        }

        [TestCase("-- blah\nqweqwe", "\nqweqwe\n")]
        [TestCase("# blah\nqweqwe", "\nqweqwe\n")]
        [TestCase("qwe # blah\nqweqwe", "qwe # blah\nqweqwe\n")]
        public void TestSqlStrippedSingleLineCommentAndSql(string input, string expected)
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            var source = new TextReaderSource(new StringReader(input));
            batchParser.Process(source, true);
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual(expected, output[0]);
            Assert.AreEqual(0, specialTokens.Count);
        }

        [Test]
        public void TestSqlUnclosedMultiLineComment()
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            var source = new TextReaderSource(new StringReader("/* test\n * blah"));
            Assert.Throws<InvalidOperationException>(() => batchParser.Process(source));
            Assert.AreEqual(0, output.Count);
            Assert.AreEqual(0, specialTokens.Count);
        }

        [Test]
        public void TestSqlMultipleGo()
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SnowflakeBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            batchParser.Process(new TextReaderSource(new StringReader("go\nGO")));
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("go\nGO\n", output[0]);
            Assert.AreEqual(0, specialTokens.Count);
        }
    }
}
