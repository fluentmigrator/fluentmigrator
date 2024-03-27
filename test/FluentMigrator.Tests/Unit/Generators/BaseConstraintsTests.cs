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
    [Category("Constraint")]
    public abstract class BaseConstraintsTests
    {
        public abstract void CanCreateForeignKeyWithCustomSchema();
        public abstract void CanCreateForeignKeyWithDefaultSchema();
        public abstract void CanCreateForeignKeyWithDifferentSchemas();
        public abstract void CanCreateMultiColumnForeignKeyWithCustomSchema();
        public abstract void CanCreateMultiColumnForeignKeyWithDefaultSchema();
        public abstract void CanCreateMultiColumnForeignKeyWithDifferentSchemas();
        public abstract void CanCreateMultiColumnPrimaryKeyConstraintWithCustomSchema([Values] CompatibilityMode compatibilityMode);
        public abstract void CanCreateMultiColumnPrimaryKeyConstraintWithDefaultSchema([Values] CompatibilityMode compatibilityMode);
        public abstract void CanCreateMultiColumnUniqueConstraintWithCustomSchema();
        public abstract void CanCreateMultiColumnUniqueConstraintWithDefaultSchema();
        public abstract void CanCreateNamedForeignKeyWithCustomSchema();
        public abstract void CanCreateNamedForeignKeyWithDefaultSchema();
        public abstract void CanCreateNamedForeignKeyWithDifferentSchemas();
        public abstract void CanCreateNamedForeignKeyWithOnDeleteAndOnUpdateOptions();
        public abstract void CanCreateNamedForeignKeyWithOnDeleteOptions(System.Data.Rule rule, string output);
        public abstract void CanCreateNamedForeignKeyWithOnUpdateOptions(System.Data.Rule rule, string output);
        public abstract void CanCreateNamedMultiColumnForeignKeyWithCustomSchema();
        public abstract void CanCreateNamedMultiColumnForeignKeyWithDefaultSchema();
        public abstract void CanCreateNamedMultiColumnForeignKeyWithDifferentSchemas();
        public abstract void CanCreateNamedMultiColumnPrimaryKeyConstraintWithCustomSchema([Values] CompatibilityMode compatibilityMode);
        public abstract void CanCreateNamedMultiColumnPrimaryKeyConstraintWithDefaultSchema([Values] CompatibilityMode compatibilityMode);
        public abstract void CanCreateNamedMultiColumnUniqueConstraintWithCustomSchema();
        public abstract void CanCreateNamedMultiColumnUniqueConstraintWithDefaultSchema();
        public abstract void CanCreateNamedPrimaryKeyConstraintWithCustomSchema([Values] CompatibilityMode compatibilityMode);
        public abstract void CanCreateNamedPrimaryKeyConstraintWithDefaultSchema([Values] CompatibilityMode compatibilityMode);
        public abstract void CanCreateNamedUniqueConstraintWithCustomSchema();
        public abstract void CanCreateNamedUniqueConstraintWithDefaultSchema();
        public abstract void CanCreatePrimaryKeyConstraintWithCustomSchema([Values] CompatibilityMode compatibilityMode);
        public abstract void CanCreatePrimaryKeyConstraintWithDefaultSchema([Values] CompatibilityMode compatibilityMode);
        public abstract void CanCreateUniqueConstraintWithCustomSchema();
        public abstract void CanCreateUniqueConstraintWithDefaultSchema();
        public abstract void CanDropForeignKeyWithCustomSchema();
        public abstract void CanDropForeignKeyWithDefaultSchema();
        public abstract void CanDropPrimaryKeyConstraintWithCustomSchema();
        public abstract void CanDropPrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanDropUniqueConstraintWithCustomSchema();
        public abstract void CanDropUniqueConstraintWithDefaultSchema();
    }
}
