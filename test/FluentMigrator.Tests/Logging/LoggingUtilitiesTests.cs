#region License
// Copyright (c) 2020, FluentMigrator Project
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

using FluentMigrator.Runner;

using NUnit.Framework;

namespace FluentMigrator.Tests.Logging
{
    [TestFixture]
    public class LoggingUtilitiesTests
    {
        private readonly TextWriter _writer;

        public LoggingUtilitiesTests()
        {
            _writer = TextWriter.Null;
        }

        [Test]
        public void Write_exception() =>
            _writer.WriteException(new Exception("ex"));

        [Test]
        public void Write_exception_with_inner_exception() =>
            _writer.WriteException(new Exception("ex", new Exception("inner")));

        [Test]
        public void Write_aggregate_exception_of_two_exceptions_with_no_inner_exceptions() =>
            _writer.WriteException(new AggregateException(new List<Exception>
            {
                new Exception("ex1"),
                new Exception("ex2")
            }));

        [Test]
        public void Write_aggregate_exception_of_two_exceptions_each_with_inner_exceptions() =>
            _writer.WriteException(new AggregateException(new List<Exception>
            {
                new Exception("ex1", new Exception("innerEx1")),
                new Exception("ex2", new Exception("innerEx2"))
            }));

        [Test]
        public void Write_aggregate_exception_of_two_exceptions_each_with_inner_exceptions_one_of_which_is_an_aggregate() =>
            _writer.WriteException(new AggregateException(new List<Exception>
            {
                new Exception("ex1", new AggregateException(new List<Exception>
                {
                    new Exception("ex1.1", new Exception("innerEx1.1")),
                    new Exception("ex1.2", new Exception("innerEx1.2"))
                })),
                new Exception("ex2", new Exception("innerEx2"))
            }));
    }
}
