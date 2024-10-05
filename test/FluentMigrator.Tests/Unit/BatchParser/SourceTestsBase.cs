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

using FluentMigrator.Runner.BatchParser;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.BatchParser
{
    [Category("BatchParser")]
    public abstract class SourceTestsBase
    {
        protected abstract ITextSource CreateSource(string content);

        [TestCase("")]
        [TestCase("a", "a")]
        [TestCase("a\n", "a")]
        [TestCase("a\nb", "a", "b")]
        [TestCase("a\nb\n", "a", "b")]
        [TestCase("a\n\nc", "a", "", "c")]
        [TestCase("\nb\n\nd", "", "b", "", "d")]
        public void TestInputs(string content, params string[] lines)
        {
            var source = CreateSource(content);
            Assert.That(source, Is.Not.Null);
            var reader = source.CreateReader();

            foreach (var line in lines)
            {
                Assert.That(reader, Is.Not.Null);
                Assert.Multiple(() =>
                {
                    Assert.That(reader.Line, Is.EqualTo(line));
                    Assert.That(reader.Index, Is.EqualTo(0));
                });

                var nextReader = reader.Advance(reader.Line.Length);
                Assert.That(nextReader, Is.Not.SameAs(reader));
                reader = nextReader;
            }

            Assert.That(reader, Is.Null);
        }

        [Test]
        public void TestReadTooMuch()
        {
            var source = CreateSource("asdasdasd");
            Assert.That(source, Is.Not.Null);
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(reader.Line, Is.EqualTo("asdasdasd"));
                Assert.That(reader.Index, Is.EqualTo(0));
                Assert.That(reader.Length, Is.EqualTo(9));
            });
            Assert.Throws<ArgumentOutOfRangeException>(() => reader.ReadString(100));
        }

        [Test]
        public void TestFullLineAdvance()
        {
            var source = CreateSource("asdasdasd");
            Assert.That(source, Is.Not.Null);
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(reader.Line, Is.EqualTo("asdasdasd"));
                Assert.That(reader.Index, Is.EqualTo(0));
            });
            var newReader = reader.Advance(reader.Line.Length);
            Assert.That(newReader, Is.Not.SameAs(reader));
            Assert.That(newReader, Is.Null);
        }

        [Test]
        public void TestPartialAdvance()
        {
            var source = CreateSource("asdasdasd");
            Assert.That(source, Is.Not.Null);
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);
            Assert.That(reader.ReadString(3), Is.EqualTo("asd"));
            var newReader = reader.Advance(1);
            Assert.That(newReader, Is.Not.SameAs(reader));
            Assert.That(newReader, Is.Not.Null);
            reader = newReader;
            Assert.Multiple(() =>
            {
                Assert.That(reader.ReadString(3), Is.EqualTo("sda"));
                Assert.That(reader.Index, Is.EqualTo(1));
                Assert.That(reader.Length, Is.EqualTo(8));
            });
        }

        [Test]
        public void TestOverlappingAdvanceOneLine()
        {
            var source = CreateSource("asd\nqwe");
            Assert.That(source, Is.Not.Null);
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);
            reader = reader.Advance(4);
            Assert.That(reader, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(reader.ReadString(2), Is.EqualTo("we"));
                Assert.That(reader.Index, Is.EqualTo(1));
            });
        }

        [Test]
        public void TestOverlappingAdvanceTwoLine()
        {
            var source = CreateSource("asd\n\nqwe");
            Assert.That(source, Is.Not.Null);
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);
            reader = reader.Advance(4);
            Assert.That(reader, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(reader.ReadString(2), Is.EqualTo("we"));
                Assert.That(reader.Index, Is.EqualTo(1));
            });
        }

        [Test]
        public void TestNonOverlappingAdvanceTwoLine()
        {
            var source = CreateSource("asd\n\nqwe");
            Assert.That(source, Is.Not.Null);
            var reader = source.CreateReader();
            Assert.That(reader, Is.Not.Null);
            reader = reader.Advance(3);
            Assert.That(reader, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(reader.Line, Is.EqualTo(string.Empty));
                Assert.That(reader.Index, Is.EqualTo(0));
            });
            reader = reader.Advance(0);
            Assert.That(reader, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(reader.Line, Is.EqualTo("qwe"));
                Assert.That(reader.Index, Is.EqualTo(0));
            });
        }
    }
}
