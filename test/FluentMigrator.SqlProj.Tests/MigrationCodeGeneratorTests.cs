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

namespace FluentMigrator.SqlProj.Tests;

public class MigrationCodeGeneratorTests
{
    [Fact]
    public void GenerateMigration_CreatesValidMigrationCode()
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

        Assert.Contains("namespace TestNamespace", result);
        Assert.Contains("[Migration(123456789)]", result);
        Assert.Contains("public class TestMigration : Migration", result);
        Assert.Contains("Create.Table(\"TestTable\")", result);
        Assert.Contains(".WithColumn(\"Id\").AsInt32().NotNullable().Identity()", result);
        Assert.Contains(".WithColumn(\"Name\").AsString(100).NotNullable()", result);
        Assert.Contains("public override void Up()", result);
        Assert.Contains("public override void Down()", result);
        Assert.Contains("Delete.Table(\"TestTable\");", result);
    }

    [Fact]
    public void GenerateMigration_HandlesCustomSchema()
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

        Assert.Contains(".InSchema(\"custom\")", result);
    }

    [Fact]
    public void GenerateMigration_HandlesDecimalWithPrecision()
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

        Assert.Contains(".AsDecimal(18, 2)", result);
    }

    [Fact]
    public void GenerateVersionNumber_ReturnsValidTimestamp()
    {
        var version = MigrationCodeGenerator.GenerateVersionNumber();

        Assert.True(version > 20000000000000); // After year 2000
        Assert.True(version < 30000000000000); // Before year 3000
        
        var versionStr = version.ToString();
        Assert.Equal(14, versionStr.Length); // YYYYMMDDHHmmss format
    }
}
