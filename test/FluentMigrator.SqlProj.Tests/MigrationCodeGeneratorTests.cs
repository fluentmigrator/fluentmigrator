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

using NUnit.Framework;

namespace FluentMigrator.SqlProj.Tests;

[TestFixture]
public class MigrationCodeGeneratorTests
{
    [Test]
    public Task GenerateMigration_CreatesValidMigrationCode()
    {
        var generator = new MigrationCodeGenerator("TestNamespace");
        var tables = new List<TableDefinition>
        {
            new TableDefinition
            {
                SchemaName = "dbo",
                TableName = "TestTable",
                Columns = new List<ColumnDefinition>
                {
                    new ColumnDefinition
                    {
                        Name = "Id",
                        DataType = "INT",
                        IsNullable = false,
                        IsIdentity = true
                    },
                    new ColumnDefinition
                    {
                        Name = "Name",
                        DataType = "NVARCHAR",
                        Length = 100,
                        IsNullable = false
                    }
                }
            }
        };

        var result = generator.GenerateMigration(tables, "TestMigration", 123456789);

        return Verify(result);
    }

    [Test]
    public Task GenerateMigration_HandlesCustomSchema()
    {
        var generator = new MigrationCodeGenerator();
        var tables = new List<TableDefinition>
        {
            new TableDefinition
            {
                SchemaName = "custom",
                TableName = "SchemaTable",
                Columns = new List<ColumnDefinition>
                {
                    new ColumnDefinition
                    {
                        Name = "Id",
                        DataType = "INT",
                        IsNullable = false
                    }
                }
            }
        };

        var result = generator.GenerateMigration(tables, "CustomSchemaMigration", 123);

        return Verify(result);
    }

    [Test]
    public Task GenerateMigration_HandlesDecimalWithPrecision()
    {
        var generator = new MigrationCodeGenerator();
        var tables = new List<TableDefinition>
        {
            new TableDefinition
            {
                SchemaName = "dbo",
                TableName = "PriceTable",
                Columns = new List<ColumnDefinition>
                {
                    new ColumnDefinition
                    {
                        Name = "Amount",
                        DataType = "DECIMAL",
                        Precision = 18,
                        Scale = 2,
                        IsNullable = false
                    }
                }
            }
        };

        var result = generator.GenerateMigration(tables, "DecimalMigration", 123);

        return Verify(result);
    }

    [Test]
    public void GenerateVersionNumber_ReturnsValidTimestamp()
    {
        var version = MigrationCodeGenerator.GenerateVersionNumber();

        Assert.That(version, Is.GreaterThan(20000000000000)); // After year 2000
        Assert.That(version, Is.LessThan(30000000000000)); // Before year 3000
        
        var versionStr = version.ToString();
        Assert.That(versionStr.Length, Is.EqualTo(14)); // YYYYMMDDHHmmss format
    }
}
