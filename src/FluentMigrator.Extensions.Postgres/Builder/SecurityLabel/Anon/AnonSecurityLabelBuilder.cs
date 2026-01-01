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

namespace FluentMigrator.Builder.SecurityLabel.Anon;

/// <summary>
/// Implementation of the PostgreSQL Anonymizer security label builder.
/// </summary>
public class AnonSecurityLabelBuilder : SecurityLabelSyntaxBuilderBase
{
    // <inheritdoc/>
    public override string ProviderName => "anon";

    public AnonSecurityLabelBuilder MaskedWithValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(nameof(value));
        }
        // Escape single quotes in the value
        var escapedValue = value.Replace("'", "''");
        RawLabel($"MASKED WITH VALUE ''{escapedValue}''");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithFunction(string functionCall)
    {
        if (string.IsNullOrWhiteSpace(functionCall))
        {
            throw new ArgumentException(nameof(functionCall));
        }
        RawLabel($"MASKED WITH FUNCTION {functionCall}");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithFakeFirstName()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_first_name()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithFakeLastName()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_last_name()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithDummyLastName()
    {
        RawLabel("MASKED WITH FUNCTION anon.dummy_last_name()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithFakeEmail()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_email()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithPseudoEmail(string usernameColumn)
    {
        if (string.IsNullOrWhiteSpace(usernameColumn))
        {
            throw new ArgumentException(nameof(usernameColumn));
        }
        RawLabel($"MASKED WITH FUNCTION anon.pseudo_email({usernameColumn})");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithFakeCompany()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_company()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithFakeCity()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_city()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithFakeCountry()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_country()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithFakeAddress()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_address()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithFakePhone()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_phone()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithFakeIban()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_iban()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithFakeSiret()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_siret()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithFakeSiren()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_siren()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithRandomString(int length = 12)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }
        RawLabel($"MASKED WITH FUNCTION anon.random_string({length})");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithRandomInt()
    {
        RawLabel("MASKED WITH FUNCTION anon.random_int()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithRandomInt(int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentException("Min value cannot be greater than max value.");
        }
        RawLabel($"MASKED WITH FUNCTION anon.random_int_between({min}, {max})");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithRandomDate()
    {
        RawLabel("MASKED WITH FUNCTION anon.random_date()");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithRandomDateBetween(string startDate, string endDate)
    {
        if (string.IsNullOrWhiteSpace(startDate))
        {
            throw new ArgumentException(nameof(startDate));
        }
        if (string.IsNullOrWhiteSpace(endDate))
        {
            throw new ArgumentException(nameof(endDate));
        }
        RawLabel($"MASKED WITH FUNCTION anon.random_date_between(''{startDate}'', ''{endDate}'')");
        return this;
    }

    public AnonSecurityLabelBuilder MaskedWithPartialScrambling(int prefix, char padding, int suffix)
    {
        if (prefix < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(prefix));
        }
        if (suffix < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(suffix));
        }
        RawLabel($"MASKED WITH FUNCTION anon.partial({prefix}, ''{padding}'', {suffix})");
        return this;
    }

    public AnonSecurityLabelBuilder Masked()
    {
        RawLabel("MASKED");
        return this;
    }
}
