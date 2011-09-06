using System;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class FunctionValueTests
    {
        const string TestValue = "{C009DE9A-4C7A-4a96-B616-D770E1054104}";

        [Test]
        public void FunctionValue_Constructor_ShouldThrowArgumentNullException_WhenValueIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FunctionValue(null));
        }

        [Test]
        public void FunctionValue_ShouldConvertFromString()
        {
            FunctionValue functionValue = (FunctionValue)TestValue;

            functionValue.ShouldNotBeNull();
            functionValue.Value.ShouldBe(TestValue);
        }

        [Test]
        public void FunctionValue_ShouldConvertToString()
        {
            var functionString = new FunctionValue(TestValue);
            string value = (string) functionString;
            value.ShouldNotBeNull();
            value.ShouldBe(TestValue);
        }

        [Test]
        public void FunctionValue_ShouldConvertFromObjectToFunctionString()
        {
            object obj = new FunctionValue(TestValue);
            string value = (FunctionValue) obj;
            value.ShouldNotBeNull();
            value.ShouldBe(TestValue);
        }

        [Test]
        public void FunctionValue_ShouldThrowInvalidCastException_WhenConvertFromObjectToString()
        {
            string value;
            object obj = new FunctionValue(TestValue);
            Assert.Throws<InvalidCastException>(() => value = (string) obj);
        }

        [Test]
        public void FunctionValue_ShouldBeFunctionString_WhenPacketIntoObject()
        {
            object obj = new FunctionValue(TestValue);
            var result = obj is FunctionValue;
            result.ShouldBeTrue();
        }

        [Test]
        public void FunctionValue_ToString_ShouldReturnValue()
        {
            object obj = new FunctionValue(TestValue);
            var result = "'" + obj + "'";
            result.ShouldBe("'" + TestValue + "'");
        }
    }
}