using FluentMigrator.Runner.Logging;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Loggers
{
    [TestFixture]
    public class PasswordMaskUtilityTests
    {
        private IPasswordMaskUtility _passwordMaskUtility;

        [SetUp]
        public void Setup()
        {
            _passwordMaskUtility = new PasswordMaskUtility();
        }
        public class TestInputExpected
        {
            public string Input { get; set; }
            public string Expected { get; set; }

            public override string ToString()
            {
                return $"{nameof(Input)} [{Input}] {nameof(Expected)} [{Expected}]";
            }
        }

        [DatapointSource]
        protected TestInputExpected[] GetTestInputExpected()
        {
            var mask = "********";
            return new TestInputExpected[]
            {
                new TestInputExpected() { Input = @"Password=pass@123", Expected = $@"Password={mask}" },
                new TestInputExpected() { Input = @"Password = pass@123", Expected = $@"Password = {mask}" },
                new TestInputExpected() { Input = @"Password=pass@123;", Expected = $@"Password={mask};" },
                new TestInputExpected() { Input = @"Password = pass@123;", Expected = $@"Password = {mask};" },
                new TestInputExpected() { Input = @"Password=pass@123;;", Expected = $@"Password={mask};;" },
                new TestInputExpected() { Input = @"Password = pass@123;;", Expected = $@"Password = {mask};;" },
                new TestInputExpected() { Input = @"Password=", Expected = $@"Password={mask}" },
                new TestInputExpected() { Input = @"Password = ", Expected = $@"Password = {mask}" },
                new TestInputExpected() { Input = @"Password=;", Expected = $@"Password={mask};" },
                new TestInputExpected() { Input = @"Password = ;", Expected = $@"Password = {mask};" },
                new TestInputExpected() { Input = @"Password=;;", Expected = $@"Password={mask};;" },
                new TestInputExpected() { Input = @"Password = ;;", Expected = $@"Password = {mask};;" },
                new TestInputExpected() { Input = @"Password=;User Id =;", Expected = $@"Password={mask};User Id =;" },
                new TestInputExpected() { Input = @"Password = ;User Id =;", Expected = $@"Password = {mask};User Id =;" },
                new TestInputExpected() { Input = @"Server=127.0.0.1;Database=FluentMigrator;User Id=MyUser;Password=pass@123", Expected = $@"Server=127.0.0.1;Database=FluentMigrator;User Id=MyUser;Password={mask}" },
                new TestInputExpected() { Input = @"Server = 127.0.0.1;Database = FluentMigrator;User Id = MyUser;Password = pass@123", Expected = $@"Server = 127.0.0.1;Database = FluentMigrator;User Id = MyUser;Password = {mask}" },
                new TestInputExpected() { Input = @"Server=127.0.0.1;Database=FluentMigrator;Password=pass@123;User Id=MyUser", Expected = $@"Server=127.0.0.1;Database=FluentMigrator;Password={mask};User Id=MyUser" },
                new TestInputExpected() { Input = @"Server = 127.0.0.1;Database = FluentMigrator;Password = pass@123;User Id = MyUser", Expected = $@"Server = 127.0.0.1;Database = FluentMigrator;Password = {mask};User Id = MyUser" }
            };
        }

        [Theory]
        public void PasswordShouldBeMaskedWhenInConnectionString(TestInputExpected testInputExpected)
        {
            var actual = _passwordMaskUtility.ApplyMask(testInputExpected.Input);
            Assert.That(actual, Is.EqualTo(testInputExpected.Expected), $"{nameof(testInputExpected.Input)} [{testInputExpected.Input}]");
        }
    }
}
