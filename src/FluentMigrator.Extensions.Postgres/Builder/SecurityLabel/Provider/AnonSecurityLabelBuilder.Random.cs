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
/// RANDOM masking functions for PostgreSQL Anonymizer.
/// These functions generate random values without requiring a seed.
/// </summary>
public partial class AnonSecurityLabelBuilder
{
    /// <summary>
    /// Masks the column with a random date.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithRandomDate()
    {
        return MaskedWithFunction("anon.random_date");
    }

    /// <summary>
    /// Masks the column with a random string of specified length.
    /// </summary>
    /// <param name="length">The length of the random string. Default is 12. Must be greater than 0.</param>
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
    /// Masks the column with a random 5-digit ZIP code.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithRandomZip()
    {
        return MaskedWithFunction("anon.random_zip");
    }

    /// <summary>
    /// Masks the column with a random 8-digit phone number with the specified prefix.
    /// </summary>
    /// <param name="prefix">The phone number prefix as a SQL expression (e.g., "'01'", "area_code").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="prefix"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomPhone([NotNull] string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentException("Prefix cannot be null or whitespace.", nameof(prefix));
        }

        return MaskedWithFunction("anon.random_phone", prefix);
    }

    /// <summary>
    /// Masks the column with a hash of a random string for a given seed.
    /// </summary>
    /// <param name="seed">The seed value as a SQL expression (e.g., "id", "'some_value'").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seed"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomHash([NotNull] string seed)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new ArgumentException("Seed cannot be null or whitespace.", nameof(seed));
        }

        return MaskedWithFunction("anon.random_hash", seed);
    }

    /// <summary>
    /// Masks the column with a random integer within the specified range (inclusive).
    /// </summary>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (inclusive).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomIntBetween(int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentException("Min value cannot be greater than max value.", nameof(min));
        }

        return MaskedWithFunction("anon.random_int_between", min, max);
    }

    /// <summary>
    /// Masks the column with a random bigint within the specified range (inclusive).
    /// </summary>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (inclusive).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomBigIntBetween(long min, long max)
    {
        if (min > max)
        {
            throw new ArgumentException("Min value cannot be greater than max value.", nameof(min));
        }

        return MaskedWithFunction("anon.random_bigint_between", min, max);
    }

    /// <summary>
    /// Masks the column with a random date within the specified range (inclusive).
    /// </summary>
    /// <param name="startDate">The start date (inclusive) as a SQL-formatted string (e.g., "'2020-01-01'").</param>
    /// <param name="endDate">The end date (inclusive) as a SQL-formatted string (e.g., "'2025-12-31'").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="startDate"/> or <paramref name="endDate"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomDateBetween([NotNull] string startDate, [NotNull] string endDate)
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
    /// Masks the column with a random element from the specified array.
    /// </summary>
    /// <param name="arrayExpression">The PostgreSQL array expression (e.g., "ARRAY[1,2,3]", "ARRAY['red','green','blue']").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="arrayExpression"/> is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// // For integers
    /// builder.MaskedWithRandomIn("ARRAY[1,2,3]");
    ///
    /// // For strings
    /// builder.MaskedWithRandomIn("ARRAY['red','green','blue']");
    /// </code>
    /// </example>
    public AnonSecurityLabelBuilder MaskedWithRandomIn([NotNull] string arrayExpression)
    {
        if (string.IsNullOrWhiteSpace(arrayExpression))
        {
            throw new ArgumentException("Array expression cannot be null or whitespace.", nameof(arrayExpression));
        }

        return MaskedWithFunction("anon.random_in", arrayExpression);
    }

    /// <summary>
    /// Masks the column with a random value from the column's ENUM type.
    /// </summary>
    /// <param name="columnName">The name of the column (used to determine the ENUM type).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="columnName"/> is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// // For a column 'status' of type 'order_status' ENUM
    /// builder.MaskedWithRandomInEnum("status");
    /// </code>
    /// </example>
    public AnonSecurityLabelBuilder MaskedWithRandomInEnum([NotNull] string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
        {
            throw new ArgumentException("Column name cannot be null or whitespace.", nameof(columnName));
        }

        return MaskedWithFunction("anon.random_in_enum", columnName);
    }

    /// <summary>
    /// Masks the column with a random INT from the specified int4range.
    /// </summary>
    /// <param name="range">The PostgreSQL int4range expression (e.g., "'[5,6)'", "'(6,7]'").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="range"/> is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// builder.MaskedWithRandomInInt4Range("'[5,6)'"); // Returns 5
    /// builder.MaskedWithRandomInInt4Range("'(6,7]'"); // Returns 7
    /// </code>
    /// </example>
    public AnonSecurityLabelBuilder MaskedWithRandomInInt4Range([NotNull] string range)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            throw new ArgumentException("Range cannot be null or whitespace.", nameof(range));
        }

        return MaskedWithFunction("anon.random_in_int4range", range);
    }

    /// <summary>
    /// Masks the column with a random BIGINT from the specified int8range.
    /// </summary>
    /// <param name="range">The PostgreSQL int8range expression (e.g., "'[100,200]'").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="range"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomInInt8Range([NotNull] string range)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            throw new ArgumentException("Range cannot be null or whitespace.", nameof(range));
        }

        return MaskedWithFunction("anon.random_in_int8range", range);
    }

    /// <summary>
    /// Masks the column with a random NUMERIC from the specified numrange.
    /// </summary>
    /// <param name="range">The PostgreSQL numrange expression (e.g., "'[0.1,0.9]'").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="range"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomInNumRange([NotNull] string range)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            throw new ArgumentException("Range cannot be null or whitespace.", nameof(range));
        }

        return MaskedWithFunction("anon.random_in_numrange", range);
    }

    /// <summary>
    /// Masks the column with a random DATE from the specified daterange.
    /// </summary>
    /// <param name="range">The PostgreSQL daterange expression (e.g., "'[2001-01-01, 2001-12-31)'").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="range"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomInDateRange([NotNull] string range)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            throw new ArgumentException("Range cannot be null or whitespace.", nameof(range));
        }

        return MaskedWithFunction("anon.random_in_daterange", range);
    }

    /// <summary>
    /// Masks the column with a random TIMESTAMP from the specified tsrange.
    /// </summary>
    /// <param name="range">The PostgreSQL tsrange expression (e.g., "'[2022-10-01,2022-10-31]'").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="range"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomInTsRange([NotNull] string range)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            throw new ArgumentException("Range cannot be null or whitespace.", nameof(range));
        }

        return MaskedWithFunction("anon.random_in_tsrange", range);
    }

    /// <summary>
    /// Masks the column with a random TIMESTAMP WITH TIMEZONE from the specified tstzrange.
    /// </summary>
    /// <param name="range">The PostgreSQL tstzrange expression (e.g., "'[2022-10-01,2022-10-31]'").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="range"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomInTstzRange([NotNull] string range)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            throw new ArgumentException("Range cannot be null or whitespace.", nameof(range));
        }

        return MaskedWithFunction("anon.random_in_tstzrange", range);
    }

    /// <summary>
    /// Masks the column with a unique BIGINT value based on a sequence.
    /// Each call returns an incremented value much like the nextval() function.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithRandomId()
    {
        return MaskedWithFunction("anon.random_id");
    }

    /// <summary>
    /// Masks the column with a unique INT value based on a sequence.
    /// Each call returns an incremented value much like the nextval() function.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithRandomIdInt()
    {
        return MaskedWithFunction("anon.random_id_int");
    }

    /// <summary>
    /// Masks the column with a unique SMALLINT value based on a sequence.
    /// Each call returns an incremented value much like the nextval() function.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithRandomIdSmallInt()
    {
        return MaskedWithFunction("anon.random_id_small_int");
    }
}
