#region License
// 
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Builders.Create
{
    using FluentMigrator.Builders.Create.Sequence;

    [TestFixture]
    public class CreateSequenceExpressionBuilderTests
    {
        [Test]
        public void CallingInSchemaSetsSchemaName()
        {
            VerifySequenceProperty(c => c.SchemaName = "Schema", b => b.InSchema("Schema"));
        }

        [Test]
        public void CallingIncrementBySetsIncrement()
        {
            VerifySequenceProperty(c => c.Increment = 10, b => b.IncrementBy(10));
        }

        [Test]
        public void CallingMinValueSetsMinValue()
        {
            VerifySequenceProperty(c => c.MinValue = 10, b => b.MinValue(10));
        }

        [Test]
        public void CallingMaxValueSetsMaxValue()
        {
            VerifySequenceProperty(c => c.MaxValue = 10, b => b.MaxValue(10));
        }

        [Test]
        public void CallingStartWithSetsStartWith()
        {
            VerifySequenceProperty(c => c.StartWith = 10, b => b.StartWith(10));
        }

        [Test]
        public void CallingCacheSetsCache()
        {
            VerifySequenceProperty(c => c.Cache = 10, b => b.Cache(10));
        }

        [Test]
        public void CallingCycleSetsCycleToTrue()
        {
            VerifySequenceProperty(c => c.Cycle = true, b => b.Cycle());
        }

        private void VerifySequenceProperty(Action<SequenceDefinition> sequenceExpression, Action<CreateSequenceExpressionBuilder> callToTest)
        {
            var sequenceMock = new Mock<SequenceDefinition>();

            var expressionMock = new Mock<CreateSequenceExpression>();
            expressionMock.SetupProperty(e => e.Sequence);

            var expression = expressionMock.Object;
            expression.Sequence = sequenceMock.Object;

            callToTest(new CreateSequenceExpressionBuilder(expression));

            sequenceMock.VerifySet(sequenceExpression);
        }
    }
}