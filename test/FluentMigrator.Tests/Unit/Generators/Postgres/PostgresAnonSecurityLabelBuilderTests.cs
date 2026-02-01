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

using FluentMigrator.Builder.SecurityLabel.Provider;

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

            result.ShouldBe("MASKED WITH VALUE 'foo'");
        }

        [Test]
        public void CanBuildMaskedWithValueEscapesSingleQuotes()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithValue("it's a test");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH VALUE 'it''s a test'");
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
        public void ProviderNameIsAnon()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.ProviderName.ShouldBe("anon");
        }

        // ============ FAKE FUNCTIONS ============
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

        // ============ DUMMY FUNCTIONS ============
        [Test]
        public void CanBuildMaskedWithDummyFirstName()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyFirstName();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_first_name()");
        }

        [Test]
        public void CanBuildMaskedWithDummyFirstNameWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyFirstName("fr_FR");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_first_name(fr_FR)");
        }

        [Test]
        public void CanBuildMaskedWithDummyLastNameWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyLastName("en_US");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_last_name(en_US)");
        }

        [Test]
        public void CanBuildMaskedWithDummyEmail()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyEmail();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_email()");
        }

        [Test]
        public void CanBuildMaskedWithDummyCompany()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyCompany();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_company()");
        }

        [Test]
        public void CanBuildMaskedWithDummyCompanyWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyCompany("de_DE");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_company(de_DE)");
        }

        [Test]
        public void CanBuildMaskedWithDummyAddress()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyAddress();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_address()");
        }

        [Test]
        public void CanBuildMaskedWithDummyAddressWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyAddress("es_ES");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_address(es_ES)");
        }

        [Test]
        public void CanBuildMaskedWithDummyCity()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyCity();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_city()");
        }

        [Test]
        public void CanBuildMaskedWithDummyCityWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyCity("it_IT");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_city(it_IT)");
        }

        [Test]
        public void CanBuildMaskedWithDummyCountry()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyCountry();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_country()");
        }

        [Test]
        public void CanBuildMaskedWithDummyCountryWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyCountry("pt_PT");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_country(pt_PT)");
        }

        [Test]
        public void CanBuildMaskedWithDummyPhone()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyPhone();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_phone()");
        }

        [Test]
        public void CanBuildMaskedWithDummyIban()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyIban();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_iban()");
        }

        [Test]
        public void CanBuildMaskedWithDummySiret()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummySiret();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_siret()");
        }

        [Test]
        public void CanBuildMaskedWithDummySiren()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummySiren();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_siren()");
        }

        [Test]
        public void ThrowsWhenDummyFirstNameLocaleIsEmpty()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithDummyFirstName(""));
        }

        // ============ PSEUDO FUNCTIONS ============
        [Test]
        public void CanBuildMaskedWithPseudoFirstName()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoFirstName("id");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_first_name(id)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoFirstNameWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoFirstName("id", "fr_FR");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_first_name(id, fr_FR)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoLastName()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoLastName("id");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_last_name(id)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoLastNameWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoLastName("id", "en_US");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_last_name(id, en_US)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoCompany()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoCompany("id");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_company(id)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoCompanyWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoCompany("id", "de_DE");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_company(id, de_DE)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoAddress()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoAddress("id");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_address(id)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoAddressWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoAddress("id", "es_ES");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_address(id, es_ES)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoCity()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoCity("id");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_city(id)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoCityWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoCity("id", "it_IT");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_city(id, it_IT)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoCountry()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoCountry("id");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_country(id)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoCountryWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoCountry("id", "pt_PT");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_country(id, pt_PT)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoPhone()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoPhone("id");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_phone(id)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoIban()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoIban("id");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_iban(id)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoSiret()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoSiret("id");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_siret(id)");
        }

        [Test]
        public void CanBuildMaskedWithPseudoSiren()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPseudoSiren("id");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_siren(id)");
        }

        [Test]
        public void ThrowsWhenPseudoFirstNameSeedIsNull()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithPseudoFirstName(null));
        }

        [Test]
        public void ThrowsWhenPseudoFirstNameSeedIsEmpty()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithPseudoFirstName(""));
        }

        // ============ HASHING FUNCTIONS ============
        [Test]
        public void CanBuildMaskedWithHash()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithHash();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.hash()");
        }

        [Test]
        public void CanBuildMaskedWithHashWithAlgorithm()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithHash("sha256");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.hash(sha256)");
        }

        [Test]
        public void CanBuildMaskedWithHashWithSha512()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithHash(AnonHashAlgorithm.Sha512);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.hash(sha512)");
        }

        [Test]
        public void CanBuildMaskedWithHmacHash()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithHmacHash();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.hmac_hash()");
        }

        [Test]
        public void CanBuildMaskedWithHmacHashWithAlgorithm()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithHmacHash("sha1");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.hmac_hash(sha1)");
        }

        [Test]
        public void ThrowsWhenHashAlgorithmIsNull()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithHash(null));
        }

        [Test]
        public void ThrowsWhenHashAlgorithmIsEmpty()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithHash(""));
        }

        // ============ NOISE FUNCTIONS ============
        [Test]
        public void CanBuildMaskedWithAddNoiseToInt()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithAddNoiseToInt(0.5);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.add_noise_to_int(0.5)");
        }

        [Test]
        public void CanBuildMaskedWithAddNoiseToNumeric()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithAddNoiseToNumeric(0.3);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.add_noise_to_numeric(0.3)");
        }

        [Test]
        public void ThrowsWhenNoiseRatioIsZero()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentOutOfRangeException>(() => builder.MaskedWithAddNoiseToInt(0));
        }

        [Test]
        public void ThrowsWhenNoiseRatioIsNegative()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentOutOfRangeException>(() => builder.MaskedWithAddNoiseToNumeric(-0.5));
        }

        // ============ GENERALIZE FUNCTIONS ============
        [Test]
        public void CanBuildMaskedWithGeneralizeIban()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithGeneralizeIban();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.generalize_iban()");
        }

        [Test]
        public void CanBuildMaskedWithGeneralizePhoneNumber()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithGeneralizePhoneNumber();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.generalize_phone_number()");
        }

        [Test]
        public void CanBuildMaskedWithGeneralizeEmail()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithGeneralizeEmail();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.generalize_email()");
        }

        // ============ RANDOM FUNCTIONS ============
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
        public void CanBuildMaskedWithRandomIntBetween()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomIntBetween(10, 100);
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
            builder.MaskedWithRandomDateBetween("'2020-01-01'", "'2025-12-31'");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_date_between('2020-01-01', '2025-12-31')");
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

            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomIntBetween(100, 10));
        }

        [Test]
        public void ThrowsWhenRandomDateStartIsNull()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomDateBetween(null, "'2025-12-31'"));
        }

        [Test]
        public void ThrowsWhenRandomDateEndIsNull()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomDateBetween("'2020-01-01'", null));
        }

        // ============ PARTIAL FUNCTIONS ============
        [Test]
        public void CanBuildMaskedWithPartialScrambling()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithPartialScrambling(2, '*', 2);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.partial(2, '*', 2)");
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
    }
}
