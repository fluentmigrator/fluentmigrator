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

namespace FluentMigrator.Postgres.Builder.SecurityLabel.Anon
{
    /// <summary>
    /// Provides a fluent interface for building PostgreSQL Anonymizer security labels.
    /// </summary>
    /// <remarks>
    /// This builder allows strongly-typed construction of PostgreSQL Anonymizer (anon) 
    /// security labels for data masking. For more information, see:
    /// https://postgresql-anonymizer.readthedocs.io/
    /// </remarks>
    public interface IAnonSecurityLabelBuilder
    {
        /// <summary>
        /// Marks the column as masked with a static value.
        /// </summary>
        /// <param name="value">The static value to use for masking.</param>
        /// <returns>The builder for chaining.</returns>
        /// <example>
        /// <code>
        /// .WithLabel(label => label.MaskedWithValue("CONFIDENTIAL"))
        /// </code>
        /// </example>
        IAnonSecurityLabelBuilder MaskedWithValue(string value);

        /// <summary>
        /// Marks the column as masked with a custom function call.
        /// </summary>
        /// <param name="functionCall">The function call (e.g., "anon.fake_email()").</param>
        /// <returns>The builder for chaining.</returns>
        /// <example>
        /// <code>
        /// .WithLabel(label => label.MaskedWithFunction("anon.fake_email()"))
        /// </code>
        /// </example>
        IAnonSecurityLabelBuilder MaskedWithFunction(string functionCall);

        /// <summary>
        /// Marks the column as masked with a fake first name.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithFakeFirstName();

        /// <summary>
        /// Marks the column as masked with a fake last name.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithFakeLastName();

        /// <summary>
        /// Marks the column as masked with a dummy/random last name.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithDummyLastName();

        /// <summary>
        /// Marks the column as masked with a fake email address.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithFakeEmail();

        /// <summary>
        /// Marks the column as masked with a pseudo email address using the provided username column.
        /// </summary>
        /// <param name="usernameColumn">The column name to use as the email username part.</param>
        /// <returns>The builder for chaining.</returns>
        /// <example>
        /// <code>
        /// .WithLabel(label => label.MaskedWithPseudoEmail("username"))
        /// </code>
        /// </example>
        IAnonSecurityLabelBuilder MaskedWithPseudoEmail(string usernameColumn);

        /// <summary>
        /// Marks the column as masked with a fake company name.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithFakeCompany();

        /// <summary>
        /// Marks the column as masked with a fake city name.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithFakeCity();

        /// <summary>
        /// Marks the column as masked with a fake country name.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithFakeCountry();

        /// <summary>
        /// Marks the column as masked with a fake address.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithFakeAddress();

        /// <summary>
        /// Marks the column as masked with a fake phone number.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithFakePhone();

        /// <summary>
        /// Marks the column as masked with a fake IBAN.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithFakeIban();

        /// <summary>
        /// Marks the column as masked with a fake SIRET (French business registration number).
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithFakeSiret();

        /// <summary>
        /// Marks the column as masked with a fake SIREN (French company registration number).
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithFakeSiren();

        /// <summary>
        /// Marks the column as masked with random characters.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithRandomString();

        /// <summary>
        /// Marks the column as masked with random characters of a specific length.
        /// </summary>
        /// <param name="length">The length of the random string.</param>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithRandomString(int length);

        /// <summary>
        /// Marks the column as masked with a random integer.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithRandomInt();

        /// <summary>
        /// Marks the column as masked with a random integer within a range.
        /// </summary>
        /// <param name="min">The minimum value (inclusive).</param>
        /// <param name="max">The maximum value (inclusive).</param>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithRandomInt(int min, int max);

        /// <summary>
        /// Marks the column as masked with a random date.
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithRandomDate();

        /// <summary>
        /// Marks the column as masked with a random date within a range.
        /// </summary>
        /// <param name="startDate">The start date (format: 'YYYY-MM-DD').</param>
        /// <param name="endDate">The end date (format: 'YYYY-MM-DD').</param>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder MaskedWithRandomDateBetween(string startDate, string endDate);

        /// <summary>
        /// Marks the column as masked with partial scrambling, keeping the first and last characters.
        /// </summary>
        /// <param name="prefix">Number of characters to keep at the beginning.</param>
        /// <param name="padding">The padding character to use.</param>
        /// <param name="suffix">Number of characters to keep at the end.</param>
        /// <returns>The builder for chaining.</returns>
        /// <example>
        /// <code>
        /// // "John Doe" becomes "Jo***oe"
        /// .WithLabel(label => label.MaskedWithPartialScrambling(2, '*', 2))
        /// </code>
        /// </example>
        IAnonSecurityLabelBuilder MaskedWithPartialScrambling(int prefix, char padding, int suffix);

        /// <summary>
        /// Marks the role as a masked user (for role-based masking).
        /// </summary>
        /// <returns>The builder for chaining.</returns>
        IAnonSecurityLabelBuilder Masked();

        /// <summary>
        /// Gets the built label string.
        /// </summary>
        /// <returns>The security label string.</returns>
        string Build();
    }
}
