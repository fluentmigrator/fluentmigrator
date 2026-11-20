#region License
// Copyright (c) 2026, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion


using FluentMigrator.Expressions;

namespace FluentMigrator.Builder.CheckConstraint;

internal sealed class CreateCheckConstraintExpression : MigrationExpressionBase
{
    private readonly string _constraintName;
    private readonly string _tableName;
    private readonly string _schemaName;
    private readonly string _condition;

    public CreateCheckConstraintExpression(string constraintName, string tableName, string schemaName, string condition)
    {
        _constraintName = constraintName;
        _tableName = tableName;
        _schemaName = schemaName;
        _condition = condition;
    }

    public override void ExecuteWith(IMigrationProcessor processor)
    {
        var table = processor.Quoter.QuoteTableName(_tableName, _schemaName);
        var constraint = processor.Quoter.QuoteConstraintName(_constraintName);
        processor.Execute($"ALTER TABLE {table} ADD CONSTRAINT {constraint} CHECK ({_condition});");
    }

    public override string ToString()
    {
        return base.ToString() + _constraintName;
    }
}
