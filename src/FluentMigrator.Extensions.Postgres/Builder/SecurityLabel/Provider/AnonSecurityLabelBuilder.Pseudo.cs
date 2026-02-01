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
/// PSEUDO masking functions for PostgreSQL Anonymizer.
/// These functions generate pseudo-random data based on a seed value, providing reproducible but consistent masking.
/// </summary>
public partial class AnonSecurityLabelBuilder
{
    /// <summary>
    /// Masks the column with a pseudo-random first name based on a seed column.
    /// </summary>
    /// <param name="seed">The column name to use as seed for reproducible generation.</param>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seed"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithPseudoFirstName([NotNull] string seed, string locale = null)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new ArgumentException("Seed column cannot be null or whitespace.", nameof(seed));
        }

        if (locale is null)
        {
            return MaskedWithFunction("anon.pseudo_first_name", seed);
        }

        if (string.IsNullOrWhiteSpace(locale))
        {
            throw new ArgumentException("Locale cannot be null or whitespace.", nameof(locale));
        }

        return MaskedWithFunction("anon.pseudo_first_name", seed, locale);
    }

    /// <summary>
    /// Masks the column with a pseudo-random last name based on a seed column.
    /// </summary>
    /// <param name="seed">The column name to use as seed for reproducible generation.</param>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seed"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithPseudoLastName([NotNull] string seed, string locale = null)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new ArgumentException("Seed column cannot be null or whitespace.", nameof(seed));
        }

        if (locale is null)
        {
            return MaskedWithFunction("anon.pseudo_last_name", seed);
        }

        if (string.IsNullOrWhiteSpace(locale))
        {
            throw new ArgumentException("Locale cannot be null or whitespace.", nameof(locale));
        }

        return MaskedWithFunction("anon.pseudo_last_name", seed, locale);
    }

    /// <summary>
    /// Masks the column with a pseudo-random email based on a seed column.
    /// </summary>
    /// <param name="seed">The column name to use as seed for reproducible generation.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seed"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithPseudoEmail([NotNull] string seed)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new ArgumentException("Seed column cannot be null or whitespace.", nameof(seed));
        }

        return MaskedWithFunction("anon.pseudo_email", seed);
    }

    /// <summary>
    /// Masks the column with a pseudo-random company name based on a seed column.
    /// </summary>
    /// <param name="seed">The column name to use as seed for reproducible generation.</param>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seed"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithPseudoCompany([NotNull] string seed, string locale = null)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new ArgumentException("Seed column cannot be null or whitespace.", nameof(seed));
        }

        if (locale is null)
        {
            return MaskedWithFunction("anon.pseudo_company", seed);
        }

        if (string.IsNullOrWhiteSpace(locale))
        {
            throw new ArgumentException("Locale cannot be null or whitespace.", nameof(locale));
        }

        return MaskedWithFunction("anon.pseudo_company", seed, locale);
    }

    /// <summary>
    /// Masks the column with a pseudo-random address based on a seed column.
    /// </summary>
    /// <param name="seed">The column name to use as seed for reproducible generation.</param>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seed"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithPseudoAddress([NotNull] string seed, string locale = null)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new ArgumentException("Seed column cannot be null or whitespace.", nameof(seed));
        }

        if (locale is null)
        {
            return MaskedWithFunction("anon.pseudo_address", seed);
        }

        if (string.IsNullOrWhiteSpace(locale))
        {
            throw new ArgumentException("Locale cannot be null or whitespace.", nameof(locale));
        }

        return MaskedWithFunction("anon.pseudo_address", seed, locale);
    }

    /// <summary>
    /// Masks the column with a pseudo-random city name based on a seed column.
    /// </summary>
    /// <param name="seed">The column name to use as seed for reproducible generation.</param>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seed"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithPseudoCity([NotNull] string seed, string locale = null)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new ArgumentException("Seed column cannot be null or whitespace.", nameof(seed));
        }

        if (locale is null)
        {
            return MaskedWithFunction("anon.pseudo_city", seed);
        }

        if (string.IsNullOrWhiteSpace(locale))
        {
            throw new ArgumentException("Locale cannot be null or whitespace.", nameof(locale));
        }

        return MaskedWithFunction("anon.pseudo_city", seed, locale);
    }

    /// <summary>
    /// Masks the column with a pseudo-random country name based on a seed column.
    /// </summary>
    /// <param name="seed">The column name to use as seed for reproducible generation.</param>
    /// <param name="locale">Optional locale code (e.g., "en_US", "fr_FR"). If null, uses default locale.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seed"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithPseudoCountry([NotNull] string seed, string locale = null)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new ArgumentException("Seed column cannot be null or whitespace.", nameof(seed));
        }

        if (locale is null)
        {
            return MaskedWithFunction("anon.pseudo_country", seed);
        }

        if (string.IsNullOrWhiteSpace(locale))
        {
            throw new ArgumentException("Locale cannot be null or whitespace.", nameof(locale));
        }

        return MaskedWithFunction("anon.pseudo_country", seed, locale);
    }

    /// <summary>
    /// Masks the column with a pseudo-random IBAN based on a seed column.
    /// </summary>
    /// <param name="seed">The column name to use as seed for reproducible generation.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seed"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithPseudoIban([NotNull] string seed)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new ArgumentException("Seed column cannot be null or whitespace.", nameof(seed));
        }

        return MaskedWithFunction("anon.pseudo_iban", seed);
    }

    /// <summary>
    /// Masks the column with a pseudo-random SIRET number based on a seed column.
    /// </summary>
    /// <param name="seed">The column name to use as seed for reproducible generation.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seed"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithPseudoSiret([NotNull] string seed)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new ArgumentException("Seed column cannot be null or whitespace.", nameof(seed));
        }

        return MaskedWithFunction("anon.pseudo_siret", seed);
    }

    /// <summary>
    /// Masks the column with a pseudo-random SIREN number based on a seed column.
    /// </summary>
    /// <param name="seed">The column name to use as seed for reproducible generation.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="seed"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithPseudoSiren([NotNull] string seed)
    {
        if (string.IsNullOrWhiteSpace(seed))
        {
            throw new ArgumentException("Seed column cannot be null or whitespace.", nameof(seed));
        }

        return MaskedWithFunction("anon.pseudo_siren", seed);
    }
}

