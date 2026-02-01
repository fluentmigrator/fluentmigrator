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

using JetBrains.Annotations;

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
    /// Builds an array of SQL-formatted argument strings from the provided values.
    /// </summary>
    private static string[] BuildArgsArray(params object[] args)
    {
        if (args is null || args.Length == 0)
        {
            return [];
        }

        var argsArray = new string[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            argsArray[i] = BuildSqlValue(args[i]);

        }
        return argsArray;
    }

    /// <summary>
    /// Builds the SQL representation of a value for use in function calls.
    /// </summary>
    private static string BuildSqlValue(object value) => value switch
    {
        null => "NULL",
        RawSql sql => sql.Value,
        string str => str, // Strings are handled as-is to allow column names
        char c => $"'{c}'",
        bool b => b ? "TRUE" : "FALSE",
        DateTime dt => BuildSqlValue(dt.ToString("yyyy-MM-dd HH:mm:ss")),
        DateTimeOffset dto => BuildSqlValue(dto.ToString("yyyy-MM-dd HH:mm:ss zzz")),
        int or long or short or byte or uint or ulong or ushort or sbyte or float or double or decimal => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture),
        _ => BuildSqlValue(value.ToString()),
    };

    /// <summary>
    /// Escapes and formats a string for SQL usage.
    /// </summary>
    private static string MakeSqlString(string value) => $"'{value.Replace("'", "''")}'";

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

        RawLabel($"MASKED WITH VALUE {MakeSqlString(value)}");
        return this;
    }

    /// <summary>
    /// Masks the column with a custom function call.
    /// </summary>
    /// <param name="functionName">The function name, or call expression (e.g., "anon.my_function()").</param>
    /// <param name="args">Function params, it no parenthesis is provided in functionName</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="functionName"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithFunction([NotNull] string functionName, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(functionName))
        {
            throw new ArgumentException("Function name cannot be null or whitespace.", nameof(functionName));
        }

        // If parentheses already present, use as-is; otherwise ensure parentheses.
        if (functionName.IndexOf('(') >= 0)
        {
            RawLabel($"MASKED WITH FUNCTION {functionName}");
            return this;
        }

        // No parameters
        if (args is null || args.Length == 0)
        {
            RawLabel($"MASKED WITH FUNCTION {functionName}()");
            return this;
        }

        // Build anon.<functionName>(args...)
        var argsPart = "(" + string.Join(", ", BuildArgsArray(args)) + ")";
        RawLabel($"MASKED WITH FUNCTION {functionName}{argsPart}");
        return this;
    }

    /// <summary>
    /// Masks the column with a fake first name.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeFirstName()
    {
        return MaskedWithFunction("anon.fake_first_name");
    }

    /// <summary>
    /// Masks the column with a fake last name.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeLastName()
    {
        return MaskedWithFunction("anon.fake_last_name");
    }

    /// <summary>
    /// Masks the column with a dummy last name.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithDummyLastName()
    {
        return MaskedWithFunction("anon.dummy_last_name");
    }

    /// <summary>
    /// Masks the column with a fake email address.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeEmail()
    {
        return MaskedWithFunction("anon.fake_email");
    }

    /// <summary>
    /// Masks the column with a pseudo email based on another column.
    /// </summary>
    /// <param name="seed">The column containing the username to base the pseudo email on.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seed"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithPseudoEmail(string seed)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new ArgumentException("Username column cannot be null or whitespace.", nameof(seed));
        }
        return MaskedWithFunction("anon.pseudo_email", seed);
    }

    /// <summary>
    /// Masks the column with a fake company name.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeCompany()
    {
        return MaskedWithFunction("anon.fake_company");
    }

    /// <summary>
    /// Masks the column with a fake city name.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeCity()
    {
        return MaskedWithFunction("anon.fake_city");
    }

    /// <summary>
    /// Masks the column with a fake country name.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeCountry()
    {
        return MaskedWithFunction("anon.fake_country");
    }

    /// <summary>
    /// Masks the column with a fake address.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeAddress()
    {
        return MaskedWithFunction("anon.fake_address");
    }

    /// <summary>
    /// Masks the column with a fake phone number.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakePhone()
    {
        return MaskedWithFunction("anon.fake_phone");
    }

    /// <summary>
    /// Masks the column with a fake IBAN.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeIban()
    {
        return MaskedWithFunction("anon.fake_iban");
    }

    /// <summary>
    /// Masks the column with a fake SIRET number (French business identifier).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeSiret()
    {
        return MaskedWithFunction("anon.fake_siret");
    }

    /// <summary>
    /// Masks the column with a fake SIREN number (French business identifier).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeSiren()
    {
        return MaskedWithFunction("anon.fake_siren");
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
        return MaskedWithFunction("anon.random_string", length);
    }

    /// <summary>
    /// Masks the column with a random integer.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithRandomInt()
    {
        return MaskedWithFunction("anon.random_int");
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
        return MaskedWithFunction("anon.random_int_between", min, max);
    }

    /// <summary>
    /// Masks the column with a random date.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithRandomDate()
    {
        return MaskedWithFunction("anon.random_date");
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

        return MaskedWithFunction("anon.random_date_between", startDate, endDate);
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
        return MaskedWithFunction("anon.partial", prefix, BuildSqlValue(padding), suffix);
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
