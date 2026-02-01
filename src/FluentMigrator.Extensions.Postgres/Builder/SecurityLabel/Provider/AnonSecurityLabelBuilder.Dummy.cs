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
    /// Masks the column with a dummy SIRET number (French establishment identifier).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummySiret()
    {
        return MaskedWithFunction("anon.dummy_siret");
    }

    /// <summary>
    /// Masks the column with a dummy SIREN number (French business identifier).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummySiren()
    {
        return MaskedWithFunction("anon.dummy_siren");
    }
}

