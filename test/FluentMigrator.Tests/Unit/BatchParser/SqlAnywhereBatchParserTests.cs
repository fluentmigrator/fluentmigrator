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
    public class SqlAnywhereBatchParserTests
    {
        [TestCase("-- blah\nqweqwe", "\nqweqwe\n")]
        [TestCase("// blah\nqweqwe", "\nqweqwe\n")]
        [TestCase("qwe # blah\nqweqwe", "qwe # blah\nqweqwe\n")]
        [TestCase("# blah\nqweqwe", "# blah\nqweqwe\n")] // #'s do not indicate comments. Leave as is
        public void TestSqlStrippedSingleLineCommentAndSqlWithoutGo(string input, string expected)
        {
            var output = new List<string>();
            var specialTokens = new List<string>();
            var batchParser = new SqlAnywhereBatchParser("\n");
            batchParser.SqlText += (sender, evt) => { output.Add(evt.SqlText); };
            batchParser.SpecialToken += (sender, evt) => { specialTokens.Add(evt.Token); };
            var source = new TextReaderSource(new StringReader(input));
            batchParser.Process(source, true);
            Assert.AreEqual(1, output.Count);
            Assert.AreEqual(expected, output[0]);
            Assert.AreEqual(0, specialTokens.Count);
        }
    }
}
