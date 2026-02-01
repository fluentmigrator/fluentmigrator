#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

namespace FluentMigrator.Builder.SecurityLabel.Provider;

/// <summary>
/// DUMMY masking functions for PostgreSQL Anonymizer.
/// These functions replace data with a constant dummy value.
/// </summary>
public partial class AnonSecurityLabelBuilder
{
    /// <summary>
    /// Masks the column with a specified function and optional locale.
    /// </summary>
    /// <param name="functionName">The PG Anonymizer dummy function name</param>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    private AnonSecurityLabelBuilder MaskedWithLocale(string functionName, string locale)
    {
        if (string.IsNullOrWhiteSpace(functionName))
        {
            throw new ArgumentException("Function name cannot be null or whitespace.", nameof(functionName));
        }

        if (locale is null)
        {
            return MaskedWithFunction(functionName);
        }

        if (string.IsNullOrWhiteSpace(locale))
        {
            throw new ArgumentException("Locale cannot be null or whitespace.", nameof(locale));
        }

        return MaskedWithFunction(functionName + "_locale", BuildSqlString(locale));
    }

    /// <summary>
    /// Masks the column with a dummy first name.
    /// </summary>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyFirstName(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_first_name", locale);
    }

    /// <summary>
    /// Masks the column with a dummy last name.
    /// </summary>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyLastName(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_last_name", locale);
    }

    /// <summary>
    /// Masks the column with a dummy email address.
    /// </summary>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyFreeEmail(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_free_email", locale);
    }

    /// <summary>
    /// Masks the column with a dummy company name.
    /// </summary>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCompanyName(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_company_name", locale);
    }

    /// <summary>
    /// Masks the column with a dummy address.
    /// </summary>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyAddress(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_address", locale);
    }

    /// <summary>
    /// Masks the column with a dummy city name.
    /// </summary>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCity(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_city", locale);
    }

    /// <summary>
    /// Masks the column with a dummy country name.
    /// </summary>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCountry(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_country", locale);
    }

    /// <summary>
    /// Masks the column with a dummy phone number.
    /// </summary>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyPhoneNumber(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_phone_number", locale);
    }

    /// <summary>
    /// Masks the column with a dummy IBAN (International Bank Account Number).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyIban()
    {
        return MaskedWithFunction("anon.dummy_iban");
    }

    /// <summary>
    /// Masks the column with a dummy BIC (Bank Identifier Code).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyBic()
    {
        return MaskedWithFunction("anon.dummy_bic");
    }

    /// <summary>
    /// Masks the column with a dummy business statement.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyBs(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_bs", locale);
    }

    /// <summary>
    /// Masks the column with a dummy business statement adjective.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyBsAdj(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_bs_adj", locale);
    }

    /// <summary>
    /// Masks the column with a dummy business statement noun.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyBsNoun(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_bs_noun", locale);
    }

    /// <summary>
    /// Masks the column with a dummy business statement verb.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyBsVerb(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_bs_verb", locale);
    }

    /// <summary>
    /// Masks the column with a dummy building number.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyBuildingNumber(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_building_number", locale);
    }

    /// <summary>
    /// Masks the column with a dummy buzzword.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyBuzzword(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_buzzword", locale);
    }

    /// <summary>
    /// Masks the column with a dummy buzzword middle part.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyBuzzwordMiddle(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_buzzword_middle", locale);
    }

    /// <summary>
    /// Masks the column with a dummy buzzword tail part.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyBuzzwordTail(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_buzzword_tail", locale);
    }

    /// <summary>
    /// Masks the column with a dummy catchphrase.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCatchphrase(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_catchphrase", locale);
    }

    /// <summary>
    /// Masks the column with a dummy cell phone number.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCellNumber(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_cell_number", locale);
    }

    /// <summary>
    /// Masks the column with a dummy city name.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCityName(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_city_name", locale);
    }

    /// <summary>
    /// Masks the column with a dummy city prefix.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCityPrefix(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_city_prefix", locale);
    }

    /// <summary>
    /// Masks the column with a dummy city suffix.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCitySuffix(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_city_suffix", locale);
    }

    /// <summary>
    /// Masks the column with a dummy color name.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyColor(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_color", locale);
    }

    /// <summary>
    /// Masks the column with a dummy company suffix.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCompanySuffix(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_company_suffix", locale);
    }

    /// <summary>
    /// Masks the column with a dummy country code (ISO 3166-1 alpha-2).
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCountryCode(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_country_code", locale);
    }

    /// <summary>
    /// Masks the column with a dummy country name.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCountryName(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_country_name", locale);
    }

    /// <summary>
    /// Masks the column with a dummy credit card number.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCreditCardNumber()
    {
        return MaskedWithFunction("anon.dummy_credit_card_number");
    }

    /// <summary>
    /// Masks the column with a dummy currency code (ISO 4217).
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCurrencyCode(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_currency_code", locale);
    }

    /// <summary>
    /// Masks the column with a dummy currency name.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCurrencyName(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_currency_name", locale);
    }

    /// <summary>
    /// Masks the column with a dummy currency symbol.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyCurrencySymbol(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_currency_symbol", locale);
    }

    /// <summary>
    /// Masks the column with a dummy directory path.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyDirPath()
    {
        return MaskedWithFunction("anon.dummy_dir_path");
    }

    /// <summary>
    /// Masks the column with a dummy domain suffix (e.g., .com, .org).
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyDomainSuffix(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_domain_suffix", locale);
    }

    /// <summary>
    /// Masks the column with a dummy file extension.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyFileExtension()
    {
        return MaskedWithFunction("anon.dummy_file_extension");
    }

    /// <summary>
    /// Masks the column with a dummy file name.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyFileName()
    {
        return MaskedWithFunction("anon.dummy_file_name");
    }

    /// <summary>
    /// Masks the column with a dummy file path.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyFilePath()
    {
        return MaskedWithFunction("anon.dummy_file_path");
    }

    /// <summary>
    /// Masks the column with a dummy free email provider (e.g., gmail.com, yahoo.com).
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyFreeEmailProvider(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_free_email_provider", locale);
    }

    /// <summary>
    /// Masks the column with a dummy health insurance code.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyHealthInsuranceCode()
    {
        return MaskedWithFunction("anon.dummy_health_insurance_code");
    }

    /// <summary>
    /// Masks the column with a dummy hexadecimal color code.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyHexColor()
    {
        return MaskedWithFunction("anon.dummy_hex_color");
    }

    /// <summary>
    /// Masks the column with a dummy HSL color.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyHslColor()
    {
        return MaskedWithFunction("anon.dummy_hsl_color");
    }

    /// <summary>
    /// Masks the column with a dummy HSLA color.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyHslaColor()
    {
        return MaskedWithFunction("anon.dummy_hsla_color");
    }

    /// <summary>
    /// Masks the column with a dummy industry name.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyIndustry(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_industry", locale);
    }

    /// <summary>
    /// Masks the column with a dummy IP address (IPv4 or IPv6).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyIp()
    {
        return MaskedWithFunction("anon.dummy_ip");
    }

    /// <summary>
    /// Masks the column with a dummy IPv4 address.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyIpv4()
    {
        return MaskedWithFunction("anon.dummy_ipv4");
    }

    /// <summary>
    /// Masks the column with a dummy IPv6 address.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyIpv6()
    {
        return MaskedWithFunction("anon.dummy_ipv6");
    }

    /// <summary>
    /// Masks the column with a dummy ISBN (International Standard Book Number).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyIsbn()
    {
        return MaskedWithFunction("anon.dummy_isbn");
    }

    /// <summary>
    /// Masks the column with a dummy ISBN-13.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyIsbn13()
    {
        return MaskedWithFunction("anon.dummy_isbn13");
    }

    /// <summary>
    /// Masks the column with a dummy ISIN (International Securities Identification Number).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyIsin()
    {
        return MaskedWithFunction("anon.dummy_isin");
    }

    /// <summary>
    /// Masks the column with a dummy latitude.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyLatitude()
    {
        return MaskedWithFunction("anon.dummy_latitude");
    }

    /// <summary>
    /// Masks the column with a dummy licence plate.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyLicencePlate(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_licence_plate", locale);
    }

    /// <summary>
    /// Masks the column with a dummy longitude.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyLongitude()
    {
        return MaskedWithFunction("anon.dummy_longitude");
    }

    /// <summary>
    /// Masks the column with a dummy MAC address.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyMacAddress()
    {
        return MaskedWithFunction("anon.dummy_mac_address");
    }

    /// <summary>
    /// Masks the column with a dummy full name (first and last name).
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyName(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_name", locale);
    }

    /// <summary>
    /// Masks the column with a dummy name with title (e.g., Dr. John Doe).
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyNameWithTitle(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_name_with_title", locale);
    }

    /// <summary>
    /// Masks the column with a dummy postal code.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyPostCode(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_post_code", locale);
    }

    /// <summary>
    /// Masks the column with a dummy profession.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyProfession(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_profession", locale);
    }

    /// <summary>
    /// Masks the column with a dummy RFC status code.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyRfcStatusCode()
    {
        return MaskedWithFunction("anon.dummy_rfc_status_code");
    }

    /// <summary>
    /// Masks the column with a dummy RGB color.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyRgbColor()
    {
        return MaskedWithFunction("anon.dummy_rgb_color");
    }

    /// <summary>
    /// Masks the column with a dummy RGBA color.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyRgbaColor()
    {
        return MaskedWithFunction("anon.dummy_rgba_color");
    }

    /// <summary>
    /// Masks the column with a dummy safe email address.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummySafeEmail(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_safe_email", locale);
    }

    /// <summary>
    /// Masks the column with a dummy secondary address (e.g., Apt. 123).
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummySecondaryAddress(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_secondary_address", locale);
    }

    /// <summary>
    /// Masks the column with a dummy secondary address type (e.g., Apt., Suite).
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummySecondaryAddressType(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_secondary_address_type", locale);
    }

    /// <summary>
    /// Masks the column with a dummy state abbreviation.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyStateAbbr(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_state_abbr", locale);
    }

    /// <summary>
    /// Masks the column with a dummy state name.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyStateName(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_state_name", locale);
    }

    /// <summary>
    /// Masks the column with a dummy street name.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyStreetName(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_street_name", locale);
    }

    /// <summary>
    /// Masks the column with a dummy street suffix (e.g., St., Ave.).
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyStreetSuffix(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_street_suffix", locale);
    }

    /// <summary>
    /// Masks the column with a dummy name suffix (e.g., Jr., Sr.).
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummySuffix(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_suffix", locale);
    }

    /// <summary>
    /// Masks the column with a dummy timezone.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyTimezone()
    {
        return MaskedWithFunction("anon.dummy_timezone");
    }

    /// <summary>
    /// Masks the column with a dummy title (e.g., Dr., Mr., Ms.).
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyTitle(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_title", locale);
    }

    /// <summary>
    /// Masks the column with a dummy user agent string.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyUserAgent()
    {
        return MaskedWithFunction("anon.dummy_user_agent");
    }

    /// <summary>
    /// Masks the column with a dummy username.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyUsername(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_username", locale);
    }

    /// <summary>
    /// Masks the column with a dummy UUID v1.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyUuidv1()
    {
        return MaskedWithFunction("anon.dummy_uuidv1");
    }

    /// <summary>
    /// Masks the column with a dummy UUID v3.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyUuidv3()
    {
        return MaskedWithFunction("anon.dummy_uuidv3");
    }

    /// <summary>
    /// Masks the column with a dummy UUID v4.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyUuidv4()
    {
        return MaskedWithFunction("anon.dummy_uuidv4");
    }

    /// <summary>
    /// Masks the column with a dummy UUID v5.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyUuidv5()
    {
        return MaskedWithFunction("anon.dummy_uuidv5");
    }

    /// <summary>
    /// Masks the column with a dummy valid HTTP status code.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyValidStatusCode()
    {
        return MaskedWithFunction("anon.dummy_valid_status_code");
    }

    /// <summary>
    /// Masks the column with a dummy word.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyWord(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_word", locale);
    }

    /// <summary>
    /// Masks the column with dummy words (multiple words).
    /// </summary>
    /// <param name="minWords">Minimum number of words.</param>
    /// <param name="maxWords">Maximum number of words.</param>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyWords(int minWords, int maxWords, string locale = null)
    {
        if (minWords < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(minWords), "Minimum words must be at least 1.");
        }

        if (maxWords < minWords)
        {
            throw new ArgumentException("Maximum words must be greater than or equal to minimum words.", nameof(maxWords));
        }

        var range = $"'[{minWords},{maxWords}]'";

        if (locale is null)
        {
            return MaskedWithFunction("anon.dummy_words", range);
        }

        if (string.IsNullOrWhiteSpace(locale))
        {
            throw new ArgumentException("Locale cannot be null or whitespace.", nameof(locale));
        }

        return MaskedWithFunction("anon.dummy_words_locale", range, BuildSqlString(locale));
    }

    /// <summary>
    /// Masks the column with a dummy ZIP code.
    /// </summary>
    /// <param name="locale">Optional locale code.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyZipCode(string locale = null)
    {
        return MaskedWithLocale("anon.dummy_zip_code", locale);
    }
}
