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

        public INamedTypeSymbol MigrationAttributeType => _lazyMigrationAttributeType?.Value;

        public ConcurrentBag<MigrationClassDeclaration> Migrations { get; } = new ConcurrentBag<MigrationClassDeclaration>();
    }
}
