#region License
// Copyright (c) 2021, Fluent Migrator Project
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

using FluentMigrator.Builders.Create.Index;

using JetBrains.Annotations;

namespace FluentMigrator.Builder.Create.Index
{
    /// <summary>
    /// Provides syntax for creating a hash index with specific options in a fluent manner.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="FluentMigrator.Builder.Create.Index.AbstractCreateIndexMethodOptionsSyntax"/> 
    /// and implements <see cref="FluentMigrator.Builder.Create.Index.ICreateHashIndexOptionSyntax"/> 
    /// to support hash index-specific configurations.
    /// </remarks>
    public class CreateHashIndexOptionSyntax : AbstractCreateIndexMethodOptionsSyntax, ICreateHashIndexOptionSyntax
    {
        /// <inheritdoc />
        public CreateHashIndexOptionSyntax([NotNull] ICreateIndexOptionsSyntax createIndexOptionsSyntax)
            : base(createIndexOptionsSyntax)
        {
        }
    }
}
