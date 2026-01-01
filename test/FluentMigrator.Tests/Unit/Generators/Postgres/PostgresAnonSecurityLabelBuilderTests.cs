#region License
// Copyright (c) 2018, Fluent Migrator Project
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
#endregion

using System;

using FluentMigrator.Builder.SecurityLabel.Anon;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    [Category("Generator")]
    [Category("Postgres")]
    public class PostgresAnonSecurityLabelBuilderTests
    {
        [Test]
        public void CanBuildMaskedWithValue()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithValue("foo");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH VALUE ''foo''");
        }

        [Test]
        public void CanBuildMaskedWithValueEscapesSingleQuotes()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithValue("it's a test");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH VALUE ''it''s a test''");
        }

        [Test]
        public void CanBuildMaskedWithFunction()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithFunction("anon.fake_email()");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.fake_email()");
        }

        [Test]
        public void CanBuildMaskedWithFakeFirstName()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithFakeFirstName();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.fake_first_name()");
        }

        [Test]
        public void CanBuildMaskedWithFakeLastName()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithFakeLastName();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.fake_last_name()");
        }

        [Test]
        public void CanBuildMaskedWithDummyLastName()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyLastName();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_last_name()");
        }

        [Test]
        public void CanBuildMaskedWithFakeEmail()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithFakeEmail();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.fake_email()");
        }

        [Test]
        public void CanBuildMaskedWithPseudoEmail()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoEmail("username");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_email(username)");
        }

        [Test]
        public void CanBuildMaskedWithFakeCompany()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithFakeCompany();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.fake_company()");
        }

        [Test]
        public void CanBuildMaskedWithFakeCity()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithFakeCity();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.fake_city()");
        }

        [Test]
        public void CanBuildMaskedWithFakeCountry()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithFakeCountry();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.fake_country()");
        }

        [Test]
        public void CanBuildMaskedWithFakeAddress()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithFakeAddress();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.fake_address()");
        }

        [Test]
        public void CanBuildMaskedWithFakePhone()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithFakePhone();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.fake_phone()");
        }

        [Test]
        public void CanBuildMaskedWithFakeIban()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithFakeIban();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.fake_iban()");
        }

        [Test]
        public void CanBuildMaskedWithFakeSiret()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithFakeSiret();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.fake_siret()");
        }

        [Test]
        public void CanBuildMaskedWithFakeSiren()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithFakeSiren();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.fake_siren()");
        }

        [Test]
        public void CanBuildMaskedWithRandomStringDefault()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomString();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_string(12)");
        }

        [Test]
        public void CanBuildMaskedWithRandomStringWithLength()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomString(20);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_string(20)");
        }

        [Test]
        public void CanBuildMaskedWithRandomIntDefault()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomInt();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_int()");
        }

        [Test]
        public void CanBuildMaskedWithRandomIntWithRange()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomInt(10, 100);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_int_between(10, 100)");
        }

        [Test]
        public void CanBuildMaskedWithRandomDateDefault()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomDate();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_date()");
        }

        [Test]
        public void CanBuildMaskedWithRandomDateBetween()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomDateBetween("2020-01-01", "2025-12-31");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_date_between(''2020-01-01'', ''2025-12-31'')");
        }

        [Test]
        public void CanBuildMaskedWithPartialScrambling()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPartialScrambling(2, '*', 2);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.partial(2, ''*'', 2)");
        }

        [Test]
        public void CanBuildMasked()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.Masked();
            var result = builder.Build();

            result.ShouldBe("MASKED");
        }

        [Test]
        public void ThrowsWhenBuildCalledWithoutConfiguration()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<InvalidOperationException>(() => builder.Build());
        }

        [Test]
        public void ThrowsWhenMaskedWithValueIsNull()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithValue(null));
        }

        [Test]
        public void ThrowsWhenMaskedWithValueIsEmpty()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithValue(""));
        }

        [Test]
        public void ThrowsWhenMaskedWithFunctionIsNull()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithFunction(null));
        }

        [Test]
        public void ThrowsWhenMaskedWithPseudoEmailColumnIsNull()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithPseudoEmail(null));
        }

        [Test]
        public void ThrowsWhenRandomStringLengthIsZero()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentOutOfRangeException>(() => builder.MaskedWithRandomString(0));
        }

        [Test]
        public void ThrowsWhenRandomStringLengthIsNegative()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentOutOfRangeException>(() => builder.MaskedWithRandomString(-1));
        }

        [Test]
        public void ThrowsWhenRandomIntMinGreaterThanMax()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomInt(100, 10));
        }

        [Test]
        public void ThrowsWhenPartialScramblingPrefixIsNegative()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentOutOfRangeException>(() => builder.MaskedWithPartialScrambling(-1, '*', 2));
        }

        [Test]
        public void ThrowsWhenPartialScramblingSuffixIsNegative()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentOutOfRangeException>(() => builder.MaskedWithPartialScrambling(2, '*', -1));
        }

        [Test]
        public void ProviderNameIsAnon()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.ProviderName.ShouldBe("anon");
        }
    }
}
