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
/// GENERALIZE masking functions for PostgreSQL Anonymizer.
/// These functions generalize or mask sensitive parts of data while preserving structure.
/// </summary>
public partial class AnonSecurityLabelBuilder
{
    /// <summary>
    /// Generalizes an IBAN by masking the account details while preserving country and bank codes.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithGeneralizeIban()
    {
        return MaskedWithFunction("anon.generalize_iban");
    }

    /// <summary>
    /// Generalizes a phone number by masking the significant digits while preserving format.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithGeneralizePhoneNumber()
    {
        return MaskedWithFunction("anon.generalize_phone_number");
    }

    /// <summary>
    /// Generalizes an email address by masking the local part while preserving domain information.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithGeneralizeEmail()
    {
        return MaskedWithFunction("anon.generalize_email");
    }
}

