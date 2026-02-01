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

namespace FluentMigrator.Builder.SecurityLabel.Provider;

/// <summary>
/// FAKE masking functions for PostgreSQL Anonymizer.
/// These functions generate realistic but fake data that looks similar to the original data type.
/// </summary>
public partial class AnonSecurityLabelBuilder
{
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
    /// Masks the column with a fake email address.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeEmail()
    {
        return MaskedWithFunction("anon.fake_email");
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
    /// Masks the column with a fake address.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeAddress()
    {
        return MaskedWithFunction("anon.fake_address");
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
    /// Masks the column with a fake IBAN (International Bank Account Number).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeIban()
    {
        return MaskedWithFunction("anon.fake_iban");
    }

    /// <summary>
    /// Masks the column with a fake SIRET number (French establishment identifier).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithFakeSiret()
    {
        return MaskedWithFunction("anon.fake_siret");
    }
}

