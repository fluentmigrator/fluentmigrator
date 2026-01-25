#region License
// Copyright (c) 2026, Fluent Migrator Project
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

namespace FluentMigrator.EFCore.Example.DbContext;

/// <summary>
/// Base class for TPH (Table Per Hierarchy) inheritance demonstration.
/// All payment types stored in single table with discriminator column.
/// </summary>
public abstract class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public bool IsSuccessful { get; set; }
}

/// <summary>
/// Credit card payment - inherits from Payment (TPH).
/// </summary>
public class CreditCardPayment : Payment
{
    public string CardLastFourDigits { get; set; } = string.Empty;

    public string? CardBrand { get; set; } // Visa, MasterCard, etc.

    public string? TransactionId { get; set; }
}

/// <summary>
/// Bank transfer payment - inherits from Payment (TPH).
/// </summary>
public class BankTransferPayment : Payment
{
    public string BankAccountNumber { get; set; } = string.Empty;

    public string? BankName { get; set; }

    public string? ReferenceNumber { get; set; }
}
