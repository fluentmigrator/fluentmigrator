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
        public void CanBuildMaskedWithFakePostcode()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithFakePostcode();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.fake_postcode()");
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

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_first_name_locale('fr_FR')");
        }

        [Test]
        public void CanBuildMaskedWithDummyLastNameWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyLastName("en_US");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_last_name_locale('en_US')");
        }

        [Test]
        public void CanBuildMaskedWithDummyEmail()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyFreeEmail();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_free_email()");
        }

        [Test]
        public void CanBuildMaskedWithDummyCompanyName()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyCompanyName();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_company_name()");
        }

        [Test]
        public void CanBuildMaskedWithDummyCompanyNameWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyCompanyName("de_DE");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_company_name_locale('de_DE')");
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

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_address_locale('es_ES')");
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

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_city_locale('it_IT')");
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

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_country_locale('pt_PT')");
        }

        [Test]
        public void CanBuildMaskedWithDummyPhone()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyPhoneNumber();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_phone_number()");
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
            builder.MaskedWithPseudoFirstName("id", "abcd");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_first_name(id, 'abcd')");
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
            builder.MaskedWithPseudoLastName("id", "abcd");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_last_name(id, 'abcd')");
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
            builder.MaskedWithPseudoCompany("id", "abcd");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_company(id, 'abcd')");
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
            builder.MaskedWithPseudoAddress("id", "abcd");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_address(id, 'abcd')");
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
            builder.MaskedWithPseudoCity("id", "abcd");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_city(id, 'abcd')");
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
            builder.MaskedWithPseudoCountry("id", "abcd");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.pseudo_country(id, 'abcd')");
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
            builder.MaskedWithHash("foo");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.hash(foo)");
        }

        [Test]
        public void CanBuildMaskedWithHashWithAlgorithm()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDigest("foo", "'bar'", "sha256");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.digest(foo, 'bar', 'sha256')");
        }

        [Test]
        public void CanBuildMaskedWithHashWithSha512()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDigest("foo", "'bar'", AnonHashAlgorithm.Sha512);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.digest(foo, 'bar', 'sha512')");
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
        public void CanBuildMaskedWithAddNumericNoise()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithNoise("foo", 0.5);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.noise(foo, 0.5)");
        }

        [Test]
        public void CanBuildMaskedWithDateNoise()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDateNoise("foo", "2 days");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dnoise(foo, '2 days')");
        }

        [Test]
        public void ThrowsWhenNoiseRatioIsZero()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentOutOfRangeException>(() => builder.MaskedWithNoise("foo", 0));
        }

        [Test]
        public void ThrowsWhenNoiseRatioIsNegative()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentOutOfRangeException>(() => builder.MaskedWithNoise("foo", -0.5));
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
        public void CanBuildMaskedWithRandomIntBetween()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomIntBetween(10, 100);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_int_between(10, 100)");
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
            builder.MaskedWithPartialScrambling("foo", 2, '*', 2);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.partial(foo, 2, '*', 2)");
        }

        [Test]
        public void ThrowsWhenPartialScramblingPrefixIsNegative()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentOutOfRangeException>(() => builder.MaskedWithPartialScrambling("foo", -1, '*', 2));
        }

        [Test]
        public void ThrowsWhenPartialScramblingSuffixIsNegative()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentOutOfRangeException>(() => builder.MaskedWithPartialScrambling("foo", 2, '*', -1));
        }

        // ============ LOREM IPSUM FUNCTIONS ============
        [Test]
        public void CanBuildMaskedWithLoremIpsumDefaultParagraphs()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithLoremIpsum();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.lorem_ipsum( paragraphs := 5 )");
        }

        [Test]
        public void CanBuildMaskedWithLoremIpsumWithParagraphs()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithLoremIpsum(paragraphs: 3);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.lorem_ipsum( paragraphs := 3 )");
        }

        [Test]
        public void CanBuildMaskedWithLoremIpsumWithWords()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithLoremIpsum(words: 20);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.lorem_ipsum( words := 20 )");
        }

        [Test]
        public void CanBuildMaskedWithLoremIpsumWithCharacters()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithLoremIpsum(characters: "50");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.lorem_ipsum( characters := 50 )");
        }

        [Test]
        public void CanBuildMaskedWithLoremIpsumWithCharactersFromColumn()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithLoremIpsum(characters: "anon.length(table.column)");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.lorem_ipsum( characters := anon.length(table.column) )");
        }

        [Test]
        public void ThrowsWhenLoremIpsumHasMultipleParameters()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithLoremIpsum(paragraphs: 3, words: 20));
        }

        [Test]
        public void ThrowsWhenLoremIpsumHasAllParameters()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithLoremIpsum(paragraphs: 3, words: 20, characters: "50"));
        }

        // ============ EXTENDED DUMMY FUNCTIONS ============
        [Test]
        public void CanBuildMaskedWithDummyBic()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyBic();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_bic()");
        }

        [Test]
        public void CanBuildMaskedWithDummyCreditCardNumber()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyCreditCardNumber();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_credit_card_number()");
        }

        [Test]
        public void CanBuildMaskedWithDummyIpv4()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyIpv4();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_ipv4()");
        }

        [Test]
        public void CanBuildMaskedWithDummyIpv6()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyIpv6();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_ipv6()");
        }

        [Test]
        public void CanBuildMaskedWithDummyMacAddress()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyMacAddress();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_mac_address()");
        }

        [Test]
        public void CanBuildMaskedWithDummyUuidv4()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyUuidv4();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_uuidv4()");
        }

        [Test]
        public void CanBuildMaskedWithDummyName()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyName();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_name()");
        }

        [Test]
        public void CanBuildMaskedWithDummyNameWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyName("fr_FR");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_name_locale('fr_FR')");
        }

        [Test]
        public void CanBuildMaskedWithDummyWords()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyWords(3, 7);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_words('[3,7]')");
        }

        [Test]
        public void CanBuildMaskedWithDummyWordsWithLocale()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyWords(2, 5, "en_US");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_words_locale('[2,5]', 'en_US')");
        }

        [Test]
        public void ThrowsWhenDummyWordsMinIsLessThanOne()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentOutOfRangeException>(() => builder.MaskedWithDummyWords(0, 5));
        }

        [Test]
        public void ThrowsWhenDummyWordsMaxIsLessThanMin()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithDummyWords(5, 3));
        }

        [Test]
        public void CanBuildMaskedWithDummyLatitude()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyLatitude();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_latitude()");
        }

        [Test]
        public void CanBuildMaskedWithDummyLongitude()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyLongitude();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_longitude()");
        }

        [Test]
        public void CanBuildMaskedWithDummyHexColor()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyHexColor();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_hex_color()");
        }

        [Test]
        public void CanBuildMaskedWithDummyUserAgent()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithDummyUserAgent();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.dummy_user_agent()");
        }

        // ============ EXTENDED RANDOM FUNCTIONS ============
        [Test]
        public void CanBuildMaskedWithRandomDate()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomDate();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_date()");
        }

        [Test]
        public void CanBuildMaskedWithRandomZip()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomZip();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_zip()");
        }

        [Test]
        public void CanBuildMaskedWithRandomPhone()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomPhone("'01'");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_phone('01')");
        }

        [Test]
        public void CanBuildMaskedWithRandomHash()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomHash("id");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_hash(id)");
        }

        [Test]
        public void CanBuildMaskedWithRandomBigIntBetween()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomBigIntBetween(1000L, 9999L);
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_bigint_between(1000, 9999)");
        }

        [Test]
        public void CanBuildMaskedWithRandomIn()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomIn("ARRAY[1,2,3]");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_in(ARRAY[1,2,3])");
        }

        [Test]
        public void CanBuildMaskedWithRandomInString()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomIn("ARRAY['red','green','blue']");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_in(ARRAY['red','green','blue'])");
        }

        [Test]
        public void CanBuildMaskedWithRandomInEnum()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomInEnum("status");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_in_enum(status)");
        }

        [Test]
        public void CanBuildMaskedWithRandomInInt4Range()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomInInt4Range("'[5,6)'");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_in_int4range('[5,6)')");
        }

        [Test]
        public void CanBuildMaskedWithRandomInInt8Range()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomInInt8Range("'[100,200]'");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_in_int8range('[100,200]')");
        }

        [Test]
        public void CanBuildMaskedWithRandomInNumRange()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomInNumRange("'[0.1,0.9]'");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_in_numrange('[0.1,0.9]')");
        }

        [Test]
        public void CanBuildMaskedWithRandomInDateRange()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomInDateRange("'[2001-01-01, 2001-12-31)'");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_in_daterange('[2001-01-01, 2001-12-31)')");
        }

        [Test]
        public void CanBuildMaskedWithRandomInTsRange()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomInTsRange("'[2022-10-01,2022-10-31]'");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_in_tsrange('[2022-10-01,2022-10-31]')");
        }

        [Test]
        public void CanBuildMaskedWithRandomInTstzRange()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomInTstzRange("'[2022-10-01,2022-10-31]'");
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_in_tstzrange('[2022-10-01,2022-10-31]')");
        }

        [Test]
        public void CanBuildMaskedWithRandomId()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomId();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_id()");
        }

        [Test]
        public void CanBuildMaskedWithRandomIdInt()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomIdInt();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_id_int()");
        }

        [Test]
        public void CanBuildMaskedWithRandomIdSmallInt()
        {
            var builder = new AnonSecurityLabelBuilder();
            builder.MaskedWithRandomIdSmallInt();
            var result = builder.Build();

            result.ShouldBe("MASKED WITH FUNCTION anon.random_id_small_int()");
        }

        [Test]
        public void ThrowsWhenRandomPhonePrefixIsNull()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomPhone(null));
        }

        [Test]
        public void ThrowsWhenRandomHashSeedIsNull()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomHash(null));
        }

        [Test]
        public void ThrowsWhenRandomBigIntMinGreaterThanMax()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomBigIntBetween(1000L, 100L));
        }

        [Test]
        public void ThrowsWhenRandomInArrayIsNull()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomIn(null));
        }

        [Test]
        public void ThrowsWhenRandomInEnumColumnIsNull()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomInEnum(null));
        }

        [Test]
        public void ThrowsWhenRandomInRangeIsNull()
        {
            var builder = new AnonSecurityLabelBuilder();

            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomInInt4Range(null));
            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomInInt8Range(null));
            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomInNumRange(null));
            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomInDateRange(null));
            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomInTsRange(null));
            Should.Throw<ArgumentException>(() => builder.MaskedWithRandomInTstzRange(null));
        }
    }
}
