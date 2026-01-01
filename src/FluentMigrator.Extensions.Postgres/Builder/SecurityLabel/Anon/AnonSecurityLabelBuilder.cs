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

namespace FluentMigrator.Postgres.Builder.SecurityLabel.Anon
{
    /// <summary>
    /// Implementation of the PostgreSQL Anonymizer security label builder.
    /// </summary>
    public class AnonSecurityLabelBuilder : IAnonSecurityLabelBuilder
    {
        private string _label;

        /// <summary>
        /// Gets the provider name for PostgreSQL Anonymizer.
        /// </summary>
        public const string ProviderName = "anon";

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(value));
            }
            // Escape single quotes in the value
            var escapedValue = value.Replace("'", "''");
            _label = $"MASKED WITH VALUE ''{escapedValue}''";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithFunction(string functionCall)
        {
            if (string.IsNullOrWhiteSpace(functionCall))
            {
                throw new ArgumentException("Function call cannot be null or empty.", nameof(functionCall));
            }
            _label = $"MASKED WITH FUNCTION {functionCall}";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithFakeFirstName()
        {
            _label = "MASKED WITH FUNCTION anon.fake_first_name()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithFakeLastName()
        {
            _label = "MASKED WITH FUNCTION anon.fake_last_name()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithDummyLastName()
        {
            _label = "MASKED WITH FUNCTION anon.dummy_last_name()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithFakeEmail()
        {
            _label = "MASKED WITH FUNCTION anon.fake_email()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithPseudoEmail(string usernameColumn)
        {
            if (string.IsNullOrWhiteSpace(usernameColumn))
            {
                throw new ArgumentException("Username column cannot be null or empty.", nameof(usernameColumn));
            }
            _label = $"MASKED WITH FUNCTION anon.pseudo_email({usernameColumn})";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithFakeCompany()
        {
            _label = "MASKED WITH FUNCTION anon.fake_company()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithFakeCity()
        {
            _label = "MASKED WITH FUNCTION anon.fake_city()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithFakeCountry()
        {
            _label = "MASKED WITH FUNCTION anon.fake_country()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithFakeAddress()
        {
            _label = "MASKED WITH FUNCTION anon.fake_address()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithFakePhone()
        {
            _label = "MASKED WITH FUNCTION anon.fake_phone()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithFakeIban()
        {
            _label = "MASKED WITH FUNCTION anon.fake_iban()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithFakeSiret()
        {
            _label = "MASKED WITH FUNCTION anon.fake_siret()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithFakeSiren()
        {
            _label = "MASKED WITH FUNCTION anon.fake_siren()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithRandomString()
        {
            _label = "MASKED WITH FUNCTION anon.random_string(12)";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithRandomString(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than 0.");
            }
            _label = $"MASKED WITH FUNCTION anon.random_string({length})";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithRandomInt()
        {
            _label = "MASKED WITH FUNCTION anon.random_int()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithRandomInt(int min, int max)
        {
            if (min > max)
            {
                throw new ArgumentException("Min value cannot be greater than max value.");
            }
            _label = $"MASKED WITH FUNCTION anon.random_int_between({min}, {max})";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithRandomDate()
        {
            _label = "MASKED WITH FUNCTION anon.random_date()";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithRandomDateBetween(string startDate, string endDate)
        {
            if (string.IsNullOrWhiteSpace(startDate))
            {
                throw new ArgumentException("Start date cannot be null or empty.", nameof(startDate));
            }
            if (string.IsNullOrWhiteSpace(endDate))
            {
                throw new ArgumentException("End date cannot be null or empty.", nameof(endDate));
            }
            _label = $"MASKED WITH FUNCTION anon.random_date_between(''{startDate}'', ''{endDate}'')";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder MaskedWithPartialScrambling(int prefix, char padding, int suffix)
        {
            if (prefix < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(prefix), "Prefix must be non-negative.");
            }
            if (suffix < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(suffix), "Suffix must be non-negative.");
            }
            _label = $"MASKED WITH FUNCTION anon.partial({prefix}, ''{padding}'', {suffix})";
            return this;
        }

        /// <inheritdoc />
        public IAnonSecurityLabelBuilder Masked()
        {
            _label = "MASKED";
            return this;
        }

        /// <inheritdoc />
        public string Build()
        {
            if (string.IsNullOrWhiteSpace(_label))
            {
                throw new InvalidOperationException("No masking rule has been specified. Call one of the Masked* methods first.");
            }
            return _label;
        }
    }
}
