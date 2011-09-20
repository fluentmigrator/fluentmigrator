using System;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class ExpressionStringTests
    {
        const string TestValue = "{C009DE9A-4C7A-4a96-B616-D770E1054104}";

        [Test]
        public void ExpressionString_Constructor_ShouldThrowArgumentNullException_WhenValueIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ExpressionString(null));
        }

        [Test]
        public void ExpressionString_ShouldConvertFromString()
        {
            ExpressionString expressionString = (ExpressionString)TestValue;

            expressionString.ShouldNotBeNull();
            expressionString.Value.ShouldBe(TestValue);
        }

        [Test]
        public void ExpressionString_ShouldConvertToString()
        {
            var functionString = new ExpressionString(TestValue);
            string value = (string)functionString;
            value.ShouldNotBeNull();
            value.ShouldBe(TestValue);
        }

        [Test]
        public void ExpressionString_ShouldConvertFromObjectToFunctionString()
        {
            object obj = new ExpressionString(TestValue);
            string value = (ExpressionString)obj;
            value.ShouldNotBeNull();
            value.ShouldBe(TestValue);
        }

        [Test]
        public void ExpressionString_ShouldThrowInvalidCastException_WhenConvertFromObjectToString()
        {
            string value;
            object obj = new ExpressionString(TestValue);
            Assert.Throws<InvalidCastException>(() => value = (string)obj);
        }

        [Test]
        public void ExpressionString_ShouldBeFunctionString_WhenPacketIntoObject()
        {
            object obj = new ExpressionString(TestValue);
            var result = obj is ExpressionString;
            result.ShouldBeTrue();
        }

        [Test]
        public void ExpressionString_ToString_ShouldReturnValue()
        {
            object obj = new ExpressionString(TestValue);
            var result = "'" + obj + "'";
            result.ShouldBe("'" + TestValue + "'");
        }
    }
}