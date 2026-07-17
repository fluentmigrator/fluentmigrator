#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using FluentMigrator.Runner;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators
{
    [Category("Generator")]
    [Category("Table")]
    public abstract class BaseTableTests
    {
        public abstract void CanCreateTableWithCustomColumnTypeWithCustomSchema();
        public abstract void CanCreateTableWithCustomColumnTypeWithDefaultSchema();
        public abstract void CanCreateTableWithCustomSchema();
        public abstract void CanCreateTableWithDefaultSchema();
        public abstract void CanCreateTableWithDefaultValueExplicitlySetToNullWithCustomSchema();
        public abstract void CanCreateTableWithDefaultValueExplicitlySetToNullWithDefaultSchema();
        public abstract void CanCreateTableWithDefaultValueWithCustomSchema();
        public abstract void CanCreateTableWithDefaultValueWithDefaultSchema();
        public abstract void CanCreateTableWithIdentityWithCustomSchema();
        public abstract void CanCreateTableWithIdentityWithDefaultSchema();
        public abstract void CanCreateTableWithMultiColumnPrimaryKeyWithCustomSchema();
        public abstract void CanCreateTableWithMultiColumnPrimaryKeyWithDefaultSchema(CompatibilityMode compatibilityMode);
        public abstract void CanCreateTableWithNamedMultiColumnPrimaryKeyWithCustomSchema();
        public abstract void CanCreateTableWithNamedMultiColumnPrimaryKeyWithDefaultSchema();
        public abstract void CanCreateTableWithNamedPrimaryKeyWithCustomSchema();
        public abstract void CanCreateTableWithNamedPrimaryKeyWithDefaultSchema();
        public abstract void CanCreateTableWithNullableFieldWithCustomSchema();
        public abstract void CanCreateTableWithNullableFieldWithDefaultSchema();
        public abstract void CanCreateTableWithPrimaryKeyWithCustomSchema();
        public abstract void CanCreateTableWithPrimaryKeyWithDefaultSchema();
        public abstract void CanDropTableWithCustomSchema();
        public abstract void CanDropTableWithDefaultSchema();
        public abstract void CanDropTableIfExistsWithDefaultSchema();
        public abstract void CanRenameTableWithCustomSchema();
        public abstract void CanRenameTableWithDefaultSchema();
        public abstract void CanCreateTableWithFluentMultiColumnForeignKey();
    }
}
