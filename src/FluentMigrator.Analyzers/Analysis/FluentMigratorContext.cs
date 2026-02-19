#region License
// Copyright (c) 2024, Fluent Migrator Project
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
using System.Collections.Concurrent;

using Microsoft.CodeAnalysis;

namespace FluentMigrator.Analyzers.Analysis
{
    /// <summary>
    /// Provides context for FluentMigrator analysis operations.
    /// </summary>
    public class FluentMigratorContext
    {
        private readonly Lazy<INamedTypeSymbol> _lazyMigrationAttributeType;

        internal FluentMigratorContext(Compilation compilation)
        {
            Compilation = compilation;

            _lazyMigrationAttributeType = new Lazy<INamedTypeSymbol>(() => compilation.GetTypeByMetadataName(Constants.Types.FluentMigratorMigrationAttribute));
        }

        /// <summary>
        /// Gets the Roslyn <see cref="Compilation"/> associated with this context.
        /// </summary>
        public Compilation Compilation { get; }

        /// <summary>
        /// Gets the <see cref="INamedTypeSymbol"/> representing the Migration attribute type.
        /// </summary>
        public INamedTypeSymbol MigrationAttributeType => _lazyMigrationAttributeType?.Value;

        /// <summary>
        /// Gets the collection of migration class declarations found during analysis.
        /// </summary>
        public ConcurrentBag<MigrationClassDeclaration> Migrations { get; } = new ConcurrentBag<MigrationClassDeclaration>();
    }
}
