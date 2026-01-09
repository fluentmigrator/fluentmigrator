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
/// Implementation of the PostgreSQL Anonymizer security label builder.
/// Provides strongly-typed methods for creating masking rules using PostgreSQL Anonymizer extension.
/// </summary>
public class AnonSecurityLabelBuilder : SecurityLabelSyntaxBuilderBase
{
    /// <inheritdoc/>
    public override string ProviderName => "anon";

    /// <summary>
    /// Masks the column with a static value.
    /// </summary>
    /// <param name="value">The static value to use for masking.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
        }
        // Escape single quotes in the value
        var escapedValue = value.Replace("'", "''");
        RawLabel($"MASKED WITH VALUE '{escapedValue}'");
        return this;
    }

    /// <summary>
    /// Masks the column with a custom function call.
    /// </summary>
    /// <param name="functionCall">The function call expression (e.g., "anon.my_function()").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="functionCall"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithFunction(string functionCall)
    {
        if (string.IsNullOrWhiteSpace(functionCall))
        {
            throw new ArgumentException("Function call cannot be null or whitespace.", nameof(functionCall));
        }
        RawLabel($"MASKED WITH FUNCTION {functionCall}");
        return this;
    }

    /// <summary>
    /// Masks the column with a fake first name.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeFirstName()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_first_name()");
        return this;
    }

    /// <summary>
    /// Masks the column with a fake last name.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeLastName()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_last_name()");
        return this;
    }

    /// <summary>
    /// Masks the column with a dummy last name.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyLastName()
    {
        RawLabel("MASKED WITH FUNCTION anon.dummy_last_name()");
        return this;
    }

    /// <summary>
    /// Masks the column with a fake email address.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeEmail()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_email()");
        return this;
    }

    /// <summary>
    /// Masks the column with a pseudo email based on another column.
    /// </summary>
    /// <param name="usernameColumn">The column containing the username to base the pseudo email on.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="usernameColumn"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithPseudoEmail(string usernameColumn)
    {
        if (string.IsNullOrWhiteSpace(usernameColumn))
        {
            throw new ArgumentException("Username column cannot be null or whitespace.", nameof(usernameColumn));
        }
        RawLabel($"MASKED WITH FUNCTION anon.pseudo_email({usernameColumn})");
        return this;
    }

    /// <summary>
    /// Masks the column with a fake company name.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeCompany()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_company()");
        return this;
    }

    /// <summary>
    /// Masks the column with a fake city name.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeCity()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_city()");
        return this;
    }

    /// <summary>
    /// Masks the column with a fake country name.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeCountry()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_country()");
        return this;
    }

    /// <summary>
    /// Masks the column with a fake address.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeAddress()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_address()");
        return this;
    }

    /// <summary>
    /// Masks the column with a fake phone number.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakePhone()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_phone()");
        return this;
    }

    /// <summary>
    /// Masks the column with a fake IBAN.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeIban()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_iban()");
        return this;
    }

    /// <summary>
    /// Masks the column with a fake SIRET number (French business identifier).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeSiret()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_siret()");
        return this;
    }

    /// <summary>
    /// Masks the column with a fake SIREN number (French business identifier).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeSiren()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_siren()");
        return this;
    }

    /// <summary>
    /// Masks the column with a random string of specified length.
    /// </summary>
    /// <param name="length">The length of the random string. Default is 12.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is less than or equal to 0.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomString(int length = 12)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than 0.");
        }
        RawLabel($"MASKED WITH FUNCTION anon.random_string({length})");
        return this;
    }

    /// <summary>
    /// Masks the column with a random integer.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithRandomInt()
    {
        RawLabel("MASKED WITH FUNCTION anon.random_int()");
        return this;
    }

    /// <summary>
    /// Masks the column with a random integer within the specified range.
    /// </summary>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (inclusive).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomInt(int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentException("Min value cannot be greater than max value.", nameof(min));
        }
        RawLabel($"MASKED WITH FUNCTION anon.random_int_between({min}, {max})");
        return this;
    }

    /// <summary>
    /// Masks the column with a random date.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithRandomDate()
    {
        RawLabel("MASKED WITH FUNCTION anon.random_date()");
        return this;
    }

    /// <summary>
    /// Masks the column with a random date within the specified range.
    /// </summary>
    /// <param name="startDate">The start date (inclusive).</param>
    /// <param name="endDate">The end date (inclusive).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="startDate"/> or <paramref name="endDate"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomDateBetween(string startDate, string endDate)
    {
        if (string.IsNullOrWhiteSpace(startDate))
        {
            throw new ArgumentException("Start date cannot be null or whitespace.", nameof(startDate));
        }
        if (string.IsNullOrWhiteSpace(endDate))
        {
            throw new ArgumentException("End date cannot be null or whitespace.", nameof(endDate));
        }
        RawLabel($"MASKED WITH FUNCTION anon.random_date_between('{startDate}', '{endDate}')");
        return this;
    }

    /// <summary>
    /// Masks the column with partial scrambling, preserving a prefix and suffix.
    /// </summary>
    /// <param name="prefix">The number of characters to preserve at the beginning.</param>
    /// <param name="padding">The padding character to use for the scrambled portion.</param>
    /// <param name="suffix">The number of characters to preserve at the end.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="prefix"/> or <paramref name="suffix"/> is negative.</exception>
    public AnonSecurityLabelBuilder MaskedWithPartialScrambling(int prefix, char padding, int suffix)
    {
        if (prefix < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(prefix), "Prefix cannot be negative.");
        }
        if (suffix < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(suffix), "Suffix cannot be negative.");
        }
        RawLabel($"MASKED WITH FUNCTION anon.partial({prefix}, '{padding}', {suffix})");
        return this;
    }

    /// <summary>
    /// Marks the column as masked without specifying a masking function.
    /// This is typically used for dynamic masking where the function is determined at runtime.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder Masked()
    {
        RawLabel("MASKED");
        return this;
    }
}
