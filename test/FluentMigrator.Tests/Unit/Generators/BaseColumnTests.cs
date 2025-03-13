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

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Generators
{
    [Category("Generator")]
    [Category("Column")]
    public abstract class BaseColumnTests
    {
        public abstract void CanAlterColumnWithCustomSchema();
        public abstract void CanAlterColumnWithDefaultSchema();
        public abstract void CanCreateAutoIncrementColumnWithCustomSchema();
        public abstract void CanCreateAutoIncrementColumnWithDefaultSchema();
        public abstract void CanCreateColumnWithCustomSchema();
        public abstract void CanCreateColumnWithDefaultSchema();
        public abstract void CanCreateColumnWithSystemMethodAndCustomSchema();
        public abstract void CanCreateColumnWithSystemMethodAndDefaultSchema();
        public abstract void CanCreateDecimalColumnWithCustomSchema();
        public abstract void CanCreateDecimalColumnWithDefaultSchema();
        public abstract void CanDropColumnWithCustomSchema();
        public abstract void CanDropColumnWithDefaultSchema();
        public abstract void CanDropMultipleColumnsWithCustomSchema();
        public abstract void CanDropMultipleColumnsWithDefaultSchema();
        public abstract void CanRenameColumnWithCustomSchema();
        public abstract void CanRenameColumnWithDefaultSchema();
        public abstract void CanCreateNullableColumnWithCustomDomainTypeAndCustomSchema();
        public abstract void CanCreateNullableColumnWithCustomDomainTypeAndDefaultSchema();
        public abstract void CanCreateColumnWithComputedExpression();
        public abstract void CanCreateColumnWithStoredComputedExpression();
        public abstract void CanAlterColumnToAddComputedExpression();
        public abstract void CanAlterColumnToAddStoredComputedExpression();
    }
}
