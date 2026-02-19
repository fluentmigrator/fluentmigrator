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

using Microsoft.CodeAnalysis;

namespace FluentMigrator.Analyzers
{
    /// <summary>
    /// Represents a migration class declaration with its associated metadata.
    /// </summary>
    public struct MigrationClassDeclaration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationClassDeclaration"/> struct.
        /// </summary>
        /// <param name="typeSymbol">The type symbol representing the migration class.</param>
        /// <param name="migrationVersion">The version number of the migration.</param>
        public MigrationClassDeclaration(INamedTypeSymbol typeSymbol, long migrationVersion)
        {
            Location = typeSymbol.Locations[0];
            Name = typeSymbol.Name;
            Version = migrationVersion;
        }

        /// <summary>
        /// Gets the name of the migration class.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the source location of the migration class declaration.
        /// </summary>
        public Location Location { get; init; }

        /// <summary>
        /// Gets the version number of the migration.
        /// </summary>
        public long Version { get; init; }
    }
}
